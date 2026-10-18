using UnityEngine;

namespace Reefkeeper
{
    public sealed class RackDirtPatch : MonoBehaviour
    {
        public float requiredTravel = 0.38f;
        public Renderer dirtVisual;
        public Transform[] growth = new Transform[0];
        public float Cleaned { get; private set; }
        public bool Complete => Cleaned >= 0.999f;
        MaterialPropertyBlock properties;
        Vector3 originalScale;
        Color dirtColor;
        Vector3[] growthScale;

        void Awake()
        {
            originalScale = dirtVisual.transform.localScale;
            dirtColor = dirtVisual.sharedMaterial.GetColor("_BaseColor");
            properties = new MaterialPropertyBlock();
            growthScale = new Vector3[growth.Length];
            for (int i = 0; i < growth.Length; i++) growthScale[i] = growth[i].localScale;
        }
        public void ApplyStroke(float tangentDistance)
        {
            if (tangentDistance <= 0 || Complete) return;
            Cleaned = Mathf.Clamp01(Cleaned + tangentDistance / Mathf.Max(0.05f, requiredTravel));
            if (Complete) Cleaned = 1;
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
            for (int i = 0; i < growth.Length; i++)
            {
                float remaining = Mathf.Clamp01((1 - Cleaned) * growth.Length - i);
                growth[i].gameObject.SetActive(remaining > 0.01f);
                growth[i].localScale = growthScale[i] * Mathf.Lerp(0.15f, 1, remaining);
            }
        }
    }
}
