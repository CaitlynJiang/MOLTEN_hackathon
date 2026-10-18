using UnityEngine;
using UnityEngine.UI;

namespace Reefkeeper
{
    [DefaultExecutionOrder(200)]
    public sealed class ReefCleaningHUD : MonoBehaviour
    {
        public ReefBrushPickup pickup;
        Canvas canvas;
        RectTransform panel, prompt;
        Image fill;
        Text title, status, promptLabel;
        public float DisplayedProgress => fill ? fill.fillAmount : 0;
        public string Status => status ? status.text : "";
        void Awake()
        {
            var go = new GameObject("Cleaning HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 0.2f; canvas.sortingOrder = 80;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 1;
            panel = Rect("Cleaning progress", go.transform, new Vector2(0.5f, 0), new Vector2(0, 126), new Vector2(420, 94));
            panel.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.09f, 0.1f, 0.92f);
            title = Label(panel, "Title", new Vector2(0, 27), new Vector2(392, 24), 17);
            status = Label(panel, "Instruction", new Vector2(0, -30), new Vector2(400, 22), 14);
            var track = Rect("Track", panel, Vector2.one * 0.5f, new Vector2(0, 1), new Vector2(380, 10));
            track.gameObject.AddComponent<Image>().color = new Color(0.19f, 0.27f, 0.28f);
            var bar = Rect("Fill", track, Vector2.one * 0.5f, Vector2.zero, new Vector2(380, 10));
            fill = bar.gameObject.AddComponent<Image>();
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f);
            fill.sprite = sprite; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0; fill.color = new Color(0.59f, 0.91f, 0.75f);
            prompt = Rect("Brush prompt", go.transform, Vector2.one * 0.5f, new Vector2(0, -58), new Vector2(280, 38));
            prompt.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.09f, 0.1f, 0.92f);
            promptLabel = Label(prompt, "Prompt", Vector2.zero, new Vector2(270, 34), 19);
            promptLabel.text = "[G] Pick up brush";
        }
        void LateUpdate()
        {
            if (!pickup.View) return;
            canvas.worldCamera = pickup.View;
            fill.fillAmount = pickup.Progress;
            bool complete = pickup.practiceSpot.Complete;
            title.text = complete ? "NURSERY SPOT  /  CLEAN" : "REMOVE ALGAE  /  " + (pickup.Progress * 100).ToString("0") + "%";
            status.text = complete ? "Restoration complete  |  F5 to reset" : pickup.IsScrubbing ? "Scrubbing..." : pickup.Carrying
                ? pickup.SpotInReach ? "Hold LMB to scrub  |  H return brush" : "Move closer and aim at the algae  |  H return"
                : pickup.workshop.brush.Held ? "Sweep the bristles over the algae" : "Pick up the brush from the tool tray";
            panel.gameObject.SetActive(pickup.Carrying || pickup.workshop.brush.Held || pickup.SpotInReach || (complete && Vector3.Distance(pickup.View.transform.position, pickup.practiceSpot.transform.position) < 4));
            prompt.gameObject.SetActive(pickup.AimingBrush);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize)
        {
            var text = Rect(name, parent, Vector2.one * 0.5f, position, size).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
    }
}
