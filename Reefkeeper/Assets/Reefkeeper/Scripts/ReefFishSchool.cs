using UnityEngine;

namespace Reefkeeper
{
    /// <summary>Small ambient schools following editable loops; not a full Boids simulation.</summary>
    public sealed class ReefFishSchool : MonoBehaviour
    {
        public Transform[] fish;
        public Vector2 orbit = new Vector2(4, 2);
        public float angularSpeed = 0.16f;
        public float phase;
        public float heightVariation = 0.35f;
        public float seabedClearance = 0.9f;
        public Collider seabed;
        public bool animatePath = true;

        void Start()
        {
            for (int i = 0; i < fish.Length; i++)
            {
                var animator = fish[i].GetComponentInChildren<Animator>();
                animator.speed = 0.85f + Mathf.Repeat(i * 0.137f, 0.35f);
                animator.Play("Swim", 0, Mathf.Repeat(i * 0.271f, 1));
            }
        }
        void Update() { if (animatePath) Evaluate(Time.time); }

        public void Evaluate(float time)
        {
            for (int i = 0; i < fish.Length; i++)
            {
                if (!fish[i]) continue;
                Vector3 position = PositionAt(time, i);
                Vector3 next = PositionAt(time + 0.08f, i);
                fish[i].SetPositionAndRotation(position, Quaternion.LookRotation(next - position, Vector3.up));
            }
        }
        Vector3 PositionAt(float time, int i)
        {
            float a = phase + time * angularSpeed + i * 0.21f;
            float spread = fish.Length == 1 ? 0 : Mathf.Sin(i * 2.4f) * 0.55f;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(a) * (orbit.x + spread),
                Mathf.Sin(a * 1.7f + i) * heightVariation, Mathf.Sin(a) * (orbit.y + spread * 0.5f));
            if (seabed && seabed.Raycast(new Ray(p + Vector3.up * 40, Vector3.down), out var hit, 80))
                p.y = Mathf.Max(p.y, hit.point.y + seabedClearance);
            return p;
        }
    }
}
