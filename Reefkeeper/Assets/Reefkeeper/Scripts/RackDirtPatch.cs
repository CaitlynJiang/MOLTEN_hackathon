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
        Color dirtColor;

        void Awake()
        {
            originalScale = dirtVisual.transform.localScale;
            dirtColor = dirtVisual.sharedMaterial.GetColor("_BaseColor");
            properties = new MaterialPropertyBlock();
        }
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
            properties.SetColor("_BaseColor", Color.Lerp(dirtColor, dirtColor * 1.15f, Cleaned));
            dirtVisual.SetPropertyBlock(properties);
        }
    }
}
