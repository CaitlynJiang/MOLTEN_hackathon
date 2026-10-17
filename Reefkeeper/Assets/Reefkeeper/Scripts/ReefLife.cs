using UnityEngine;

namespace Reefkeeper
{
    public sealed class ReefLife : MonoBehaviour
    {
        public Transform[] fish;
        public Vector3 center = new Vector3(0, 6, 14);
        public float radius = 7;
        public float speed = 0.15f;
        void Update()
        {
            if (fish == null) return;
            for (int i = 0; i < fish.Length; i++)
            {
                if (!fish[i]) continue;
                float angle = Time.time * speed + i * 0.16f;
                float r = radius + Mathf.Sin(i * 13.3f) * 1.5f;
                Vector3 p = center + new Vector3(Mathf.Cos(angle) * r,
                    Mathf.Sin(angle * 2 + i) * 0.4f + Mathf.Sin(i * 3.1f), Mathf.Sin(angle) * r * 0.6f);
                Vector3 forward = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle * 2 + i) * 0.1f, Mathf.Cos(angle) * 0.6f);
                fish[i].SetPositionAndRotation(p, Quaternion.LookRotation(forward));
            }
        }
    }
}
