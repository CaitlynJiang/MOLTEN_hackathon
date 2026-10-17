using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Reefkeeper
{
    [DefaultExecutionOrder(100)]
    public sealed class ReefCoralPickup : MonoBehaviour
    {
        public ReefWorkshop workshop;
        public ReefPickupInteractor hand;
        public ReefCoralFragment[] fragments;
        public float reach = 3;
        public bool Automated { get; set; }
        public ReefCoralFragment Held { get; private set; }
        public ReefCoralFragment Aimed { get; private set; }
        public Camera View => Camera.main;
        public bool HasPrompt => Aimed && Aimed != Held;
        readonly RaycastHit[] hits = new RaycastHit[64];
        readonly List<Home> homes = new List<Home>();
        XRInteractionSimulator simulator;
        Transform holdParent;
        Camera previousView;
        bool clearing, pendingHold;
        GUIStyle prompt, hint;
        struct Home
        {
            public ReefCoralFragment template;
            public Transform parent;
            public Vector3 position, scale;
            public Quaternion rotation;
        }

        void Start()
        {
            simulator = workshop.xrGroup.GetComponentInChildren<XRInteractionSimulator>(true);
            var templates = new GameObject("Coral reset templates");
            templates.transform.SetParent(transform, false); templates.SetActive(false);
            foreach (var fragment in fragments)
                homes.Add(new Home { template = Instantiate(fragment, templates.transform), parent = fragment.transform.parent,
                    position = fragment.transform.localPosition, rotation = fragment.transform.localRotation, scale = fragment.transform.localScale });
            workshop.brush.GetComponent<XRGrabInteractable>().selectEntered.AddListener(OnBrushGrabbed);
        }
        void OnBrushGrabbed(SelectEnterEventArgs args) { ClearHeld(); }

        void Update()
        {
            var view = View;
            if (!view) return;
            if (view != previousView)
            {
                holdParent = null; previousView = view;
            }
            UpdateHand(view);
            bool centerAim = !workshop.xrGroup.activeSelf || (simulator && simulator.manipulatingFPS);
            bool canAim = Automated || workshop.xrGroup.activeSelf || Cursor.lockState == CursorLockMode.Locked;
            ReefCoralFragment target = null;
            if (centerAim && canAim) target = FindTarget(new Ray(view.transform.position, view.transform.forward));
            else if (workshop.xrGroup.activeSelf)
                foreach (var fragment in fragments)
                    if (fragment && fragment != Held && fragment.isHovered) { target = fragment; break; }
            SetAim(target);
            if (centerAim && canAim && Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
                TryGrabAimed();
        }
        void LateUpdate()
        {
            if (View) UpdateHand(View);
            if (pendingHold && Held && !Held.isSelected)
            {
                pendingHold = false;
                hand.Requested = Held;
                hand.StartManualInteraction((IXRSelectInteractable)Held);
            }
        }
        void UpdateHand(Camera view)
        {
            bool controller = holdParent && holdParent.gameObject.activeInHierarchy;
            var parent = controller ? holdParent : view.transform;
            if (hand.transform.parent != parent) hand.transform.SetParent(parent, false);
            hand.transform.localPosition = controller ? new Vector3(0.02f, -0.02f, 0.08f) : new Vector3(0.28f, -0.20f, 0.48f);
            hand.transform.localRotation = Quaternion.Euler(0, 0, -12);
        }
        public ReefCoralFragment FindTarget(Ray ray)
        {
            int count = Physics.RaycastNonAlloc(ray, hits, reach, ~0, QueryTriggerInteraction.Collide);
            float nearest = float.PositiveInfinity;
            ReefCoralFragment target = null;
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (collider.transform.IsChildOf(workshop.desktopPlayer.transform) || collider.transform.IsChildOf(workshop.xrGroup.transform)) continue;
                var fragment = collider.GetComponentInParent<ReefCoralFragment>();
                if (fragment && fragment == Held) continue;
                if (collider.isTrigger && !fragment) continue;
                if (hits[i].distance >= nearest) continue;
                nearest = hits[i].distance;
                target = fragment && !fragment.isSelected ? fragment : null;
            }
            return target;
        }
        void SetAim(ReefCoralFragment target)
        {
            if (Aimed == target) return;
            if (Aimed) Aimed.SetAim(false);
            Aimed = target;
            if (Aimed) Aimed.SetAim(true);
        }
        public void TryGrabAimed()
        {
            if (!Aimed || Aimed == Held || Aimed.isSelected) return;
            var target = Aimed;
            ClearHeld();
            holdParent = null;
            UpdateHand(View);
            hand.Requested = target;
            hand.StartManualInteraction((IXRSelectInteractable)target);
            SetAim(null);
        }
        public void Claim(ReefCoralFragment fragment)
        {
            if (Held == fragment) return;
            if (Held) ClearHeld();
            workshop.ReturnBrush();
            Held = fragment;
            pendingHold = false;
        }
        public void KeepInHand(ReefCoralFragment fragment, Transform controller)
        {
            if (clearing || Held != fragment) return;
            holdParent = controller == hand.transform ? null : controller;
            pendingHold = true;
        }
        public void ClearHeld()
        {
            clearing = true; pendingHold = false;
            if (hand.isPerformingManualInteraction) hand.EndManualInteraction();
            hand.Requested = null;
            if (Held)
            {
                if (Held.isSelected) Held.interactionManager.CancelInteractableSelection((IXRSelectInteractable)Held);
                Held.gameObject.SetActive(false);
                Destroy(Held.gameObject);
            }
            Held = null; clearing = false;
        }
        public void ResetCorals()
        {
            SetAim(null); ClearHeld();
            foreach (var fragment in fragments)
                if (fragment) { fragment.gameObject.SetActive(false); Destroy(fragment.gameObject); }
            fragments = new ReefCoralFragment[homes.Count];
            for (int i = 0; i < homes.Count; i++)
            {
                var home = homes[i];
                var fragment = Instantiate(home.template, home.parent);
                fragment.name = "Hanging staghorn fragment";
                fragment.transform.SetLocalPositionAndRotation(home.position, home.rotation);
                fragment.transform.localScale = home.scale;
                if (fragment.suspensionLine) fragment.suspensionLine.SetActive(true);
                fragments[i] = fragment;
            }
        }
        void OnGUI()
        {
            prompt ??= new GUIStyle(GUI.skin.box) { fontSize = 19, alignment = TextAnchor.MiddleCenter };
            hint ??= new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            float x = Screen.width * 0.5f, y = Screen.height * 0.5f;
            if (HasPrompt) GUI.Box(new Rect(x - 150, y + 36, 300, 38), Held ? "[G] Grab coral / replace held" : "[G] Grab coral", prompt);
            if (Held) GUI.Label(new Rect(x - 200, Screen.height - 100, 400, 26), "Coral held 1 / 1  |  Next grab replaces it  |  F5 reset", hint);
            if (workshop.xrGroup.activeSelf && simulator && simulator.manipulatingFPS)
                GUI.DrawTexture(new Rect(x - 2, y - 2, 4, 4), Texture2D.whiteTexture);
        }
    }
}
