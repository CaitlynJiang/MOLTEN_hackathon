using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Reefkeeper
{
    public sealed class ReefWorkshop : MonoBehaviour
    {
        public GameObject desktopPlayer, xrGroup;
        public ReefBrush brush;
        public RackDirtPatch[] patches;
        public Transform brushHome;
        GUIStyle text, title;
        public float Progress
        {
            get { float total = 0; foreach (var patch in patches) total += patch.Cleaned; return patches.Length > 0 ? total / patches.Length : 0; }
        }
        void Awake()
        {
            // Existing desktop checks can still run on the same scene.
            var args = System.Environment.GetCommandLineArgs();
            bool desktopSmoke = System.Array.IndexOf(args, "-reef-smoke") >= 0 || System.Array.IndexOf(args, "-reef-pickup-smoke") >= 0 || System.Array.IndexOf(args, "-reef-cleaning-smoke") >= 0;
            xrGroup.SetActive(!desktopSmoke); desktopPlayer.SetActive(desktopSmoke);
        }
        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (k.f2Key.wasPressedThisFrame)
            {
                ReturnBrush();
                bool xr = !xrGroup.activeSelf;
                xrGroup.SetActive(xr); desktopPlayer.SetActive(!xr);
                Cursor.lockState = xr ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = xr;
            }
            if (k.f5Key.wasPressedThisFrame) ResetWorkshop();
        }
        public void ResetWorkshop()
        {
            foreach (var patch in patches) patch.ResetDirt();
            ReturnBrush();
            var corals = GetComponent<ReefCoralPickup>();
            if (corals) corals.ResetCorals();
        }
        public void ReturnBrush()
        {
            var pickup = GetComponent<ReefBrushPickup>();
            if (pickup) pickup.Release();
            ReleaseBrush();
            var body = brush.GetComponent<Rigidbody>();
            body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            brush.transform.SetPositionAndRotation(brushHome.position, brushHome.rotation);
            body.position = brushHome.position; body.rotation = brushHome.rotation;
            Physics.SyncTransforms();
        }
        void ReleaseBrush()
        {
            var grab = brush.GetComponent<XRGrabInteractable>();
            if (grab.isSelected) grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
        }
        void OnGUI()
        {
            if (!xrGroup.activeSelf) return;
            text ??= new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = Color.white } };
            title ??= new GUIStyle(text) { fontSize = 23, fontStyle = FontStyle.Bold };
            GUI.Box(new Rect(18, 16, 600, 175), GUIContent.none);
            GUI.Label(new Rect(32, 27, 570, 32), "REEFKEEPER / NURSERY MAINTENANCE", title);
            GUI.Label(new Rect(32, 63, 570, 24), "Practice spot: " + (Progress * 100).ToString("0") + "%   |   " +
                (Progress > 0.999f ? "Clean! F5 to practice again" : brush.Held ? "Aim at the algae + hold LMB to scrub" : "Aim at the brush + tap G"), text);
            GUI.Label(new Rect(32, 91, 570, 24), "TAB: body / devices    [ / ]: left / right    G: hold grip    RMB: mouse", text);
            GUI.Label(new Rect(32, 116, 570, 24), "WASD + Q/E: move body or hand    B: brake    F2: desktop    F5: reset", text);
            GUI.Label(new Rect(32, 141, 570, 24), "Coral: aim + tap G, one held at a time    F11: full screen", text);
        }
    }
}
