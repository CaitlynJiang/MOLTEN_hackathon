using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Reefkeeper
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class ReefBrush : MonoBehaviour
    {
        public Transform bristles;
        public float contactRadius = 0.075f;
        public bool InContact { get; private set; }
        public bool Held => grab && grab.isSelected;
        XRGrabInteractable grab;
        Vector3 lastTip;
        bool wasHeld;
        readonly Collider[] contacts = new Collider[32];
        readonly HashSet<RackDirtPatch> visited = new HashSet<RackDirtPatch>();

        void Awake() { grab = GetComponent<XRGrabInteractable>(); }
        void LateUpdate()
        {
            Vector3 tip = bristles.position;
            InContact = false;
            if (Held && wasHeld) Sweep(lastTip, tip);
            lastTip = tip;
            wasHeld = Held;
        }
        void Sweep(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            // Ignore pickup snapping and teleports; touching without motion never cleans.
            if (delta.magnitude > 0.25f) return;
            int steps = Mathf.Clamp(Mathf.CeilToInt(delta.magnitude / (contactRadius * 0.5f)), 1, 10);
            visited.Clear();
            for (int step = 0; step <= steps; step++)
            {
                Vector3 point = Vector3.Lerp(from, to, step / (float)steps);
                int count = Physics.OverlapSphereNonAlloc(point, contactRadius, contacts, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < count; i++)
                {
                    var patch = contacts[i].GetComponent<RackDirtPatch>();
                    if (!patch || patch.Complete || !visited.Add(patch)) continue;
                    Vector3 normal = patch.transform.up;
                    // Bristles face the surface, and their center stays on its exposed side.
                    if (Vector3.Dot(bristles.up, normal) < 0.55f || Vector3.Dot(point - patch.transform.position, normal) < -0.015f) continue;
                    InContact = true;
                    float travel = Vector3.ProjectOnPlane(delta, normal).magnitude;
                    if (travel > 0.0005f) patch.ApplyStroke(travel);
                }
            }
        }
        void OnDisable() { wasHeld = false; }
    }
}
