using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Reefkeeper
{
    // Run before the simulator; it still owns head/hand tracking and grip input.
    [DefaultExecutionOrder(-32000)]
    [RequireComponent(typeof(CharacterController))]
    public sealed class ReefXRSwim : MonoBehaviour
    {
        public Camera view;
        public XRInteractionSimulator simulator;
        public bool Automated;
        public Vector3 Velocity { get; private set; }
        CharacterController body;

        void Awake()
        {
            body = GetComponent<CharacterController>();
            // The starter rig's 1 mm cutoff discards slow movement at high frame rates.
            body.minMoveDistance = 0;
        }
        void Update()
        {
            if (!view || !simulator) return;
            bool swimming = simulator.manipulatingFPS;
            simulator.bodyTranslateMultiplier = swimming ? 0 : 0.65f;
            if (Automated) return;
            var k = Keyboard.current;
            if (k == null) return;
            Vector3 axes = swimming ? new Vector3((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
                (k.eKey.isPressed ? 1 : 0) - (k.qKey.isPressed ? 1 : 0),
                (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)) : Vector3.zero;
            if (!swimming) { Velocity = Vector3.zero; return; }
            var direction = view.transform.right * axes.x + Vector3.up * axes.y + view.transform.forward * axes.z;
            Tick(direction, k.leftShiftKey.isPressed, k.bKey.isPressed, Time.deltaTime);
        }
        public void Tick(Vector3 worldDirection, bool boost, bool brake, float dt)
        {
            dt = Mathf.Clamp(dt, 0, 0.05f);
            body.center = transform.InverseTransformPoint(view.transform.position);
            Velocity = DiveMotor.IntegrateVelocity(Velocity, worldDirection, boost, brake, dt, 2.1f, 4, 3.5f, 1.1f, 12);
            body.Move(Velocity * dt);
            if (view.transform.position.y > 14.2f)
            {
                body.Move(Vector3.down * (view.transform.position.y - 14.2f));
                Velocity = Vector3.zero;
            }
            var p = view.transform.position;
            var radial = new Vector3(p.x, 0, p.z);
            if (radial.magnitude > 44) { body.Move(radial.normalized * (44 - radial.magnitude)); Velocity = Vector3.zero; }
        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            float into = Vector3.Dot(Velocity, hit.normal);
            if (into < 0) Velocity -= hit.normal * into;
        }
    }
}
