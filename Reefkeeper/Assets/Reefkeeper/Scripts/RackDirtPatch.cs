using UnityEngine;

namespace Reefkeeper
{
    public sealed class RackDirtPatch : MonoBehaviour
    {
        public float requiredTravel = 0.38f;
        public Renderer dirtVisual;
        public float Cleaned { get; private set; }
        public bool Complete => Cleaned >= 0.999f;
        MaterialPropertyBlock properties;
        Vector3 originalScale;

        void Awake() { originalScale = dirtVisual.transform.localScale; properties = new MaterialPropertyBlock(); }
        public void ApplyStroke(float tangentDistance)
        {
            if (tangentDistance <= 0 || Complete) return;
            Cleaned = Mathf.Clamp01(Cleaned + tangentDistance / Mathf.Max(0.05f, requiredTravel));
            Refresh();
        }
        public void ResetDirt() { Cleaned = 0; Refresh(); }
        void Refresh()
        {
            if (!dirtVisual) return;
            dirtVisual.enabled = !Complete;
            var scale = originalScale;
            scale.x *= Mathf.Lerp(1, 0.08f, Cleaned);
            scale.z *= Mathf.Lerp(1, 0.25f, Cleaned);
            dirtVisual.transform.localScale = scale;
            properties ??= new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", Color.Lerp(new Color(0.2f, 0.25f, 0.075f), new Color(0.35f, 0.30f, 0.16f), Cleaned));
            dirtVisual.SetPropertyBlock(properties);
        }
    }
}
