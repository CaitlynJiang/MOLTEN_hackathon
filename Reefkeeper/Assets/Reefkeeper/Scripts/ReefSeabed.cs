using UnityEngine;

namespace Reefkeeper
{
    /// <summary>Authoring parameters; the generated mesh is saved, not regenerated during play.</summary>
    public sealed class ReefSeabed : MonoBehaviour
    {
        [Range(0, 8)] public float bankHeight = 4.5f;
        [Range(0, 3)] public float duneHeight = 1.15f;
        [Range(0, 4)] public float channelDepth = 1.35f;
        [Range(0, 0.6f)] public float detailHeight = 0.18f;
        [Tooltip("Objects that move with the seabed when Apply terrain is pressed. Add your own grounded prop roots here.")]
        public Transform[] groundedProps = new Transform[0];

        public float Height(float x, float z) => Height(x, z, bankHeight, duneHeight, channelDepth, detailHeight);

        public static float Height(float x, float z, float banks = 4.5f, float dunes = 1.15f,
            float channel = 1.35f, float detail = 0.18f)
        {
            float center = 2.4f * Mathf.Sin(z * 0.075f);
            float distance = x - center;
            float shelf = Mathf.SmoothStep(0, 1, Mathf.Abs(distance) / 23) * banks;
            float trough = -Mathf.Exp(-distance * distance / 48) * channel;
            float ridge = Mathf.Sin(z * 0.24f + x * 0.12f + Mathf.Sin(x * 0.13f));
            float swell = (Mathf.PerlinNoise(x * 0.065f + 11, z * 0.065f + 19) - 0.5f) * 2;
            float fine = (Mathf.PerlinNoise(x * 0.44f + 31, z * 0.44f + 13) - 0.5f) * 2;
            return -0.25f + shelf + trough + dunes * (ridge * 0.65f + swell * 0.6f) + fine * detail;
        }

        public Mesh BuildMesh()
        {
            const int n = 192;
            var vertices = new Vector3[(n + 1) * (n + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[n * n * 6];
            for (int z = 0; z <= n; z++) for (int x = 0; x <= n; x++)
            {
                float px = (x / (float)n - 0.5f) * 110, pz = (z / (float)n - 0.5f) * 110;
                int i = z * (n + 1) + x;
                vertices[i] = new Vector3(px, Height(px, pz), pz);
                uv[i] = new Vector2(x / (float)n, z / (float)n);
                if (x == n || z == n) continue;
                int t = (z * n + x) * 6;
                triangles[t] = i; triangles[t + 1] = i + n + 1; triangles[t + 2] = i + 1;
                triangles[t + 3] = i + 1; triangles[t + 4] = i + n + 1; triangles[t + 5] = i + n + 2;
            }
            var mesh = new Mesh { name = "Seabed" };
            mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
