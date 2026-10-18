using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reefkeeper
{
    [DefaultExecutionOrder(200)]
    public sealed class ReefCleaningHUD : MonoBehaviour
    {
        public ReefBrushPickup pickup;
        public Font font, boldFont;
        public Sprite panelSprite, arcTrack, arcFill;
        public Material arcMaterial;
        Canvas canvas;
        RectTransform progress, prompt, helpPanel, objectives;
        CanvasGroup progressGroup;
        Material fillMaterial;
        Text progressLabel, promptLabel, helpText, helpHint, objectiveBody;
        float completedAt = -1, displayedProgress;
        bool collapsedOnComplete;
        const float CompletionHold = 1.2f, CompletionFade = 0.6f;
        static readonly Color Cream = new Color(0.97f, 0.93f, 0.81f);
        static readonly Color Navy = new Color(0.15f, 0.22f, 0.28f, 0.94f);
        public float DisplayedProgress => displayedProgress;
        public string Status => pickup.practiceSpot.Complete ? "Restoration complete" : promptLabel.text;
        public bool ProgressVisible => progress && progress.gameObject.activeInHierarchy && progressGroup.alpha > 0;
        public bool HelpVisible { get; private set; }
        public bool ObjectivesExpanded { get; private set; } = true;

        void Awake()
        {
            var go = new GameObject("Reef HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.layer = 5;
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 1;

            objectives = Rect("Objectives", go.transform, new Vector2(0, 1), new Vector2(178, -64), new Vector2(308, 86));
            Background(objectives);
            var objectiveTitle = Label(objectives, "Objectives title", new Vector2(0, 19), new Vector2(260, 22), 16, true);
            objectiveTitle.text = "Objectives   [O]"; objectiveTitle.alignment = TextAnchor.MiddleLeft;
            objectiveBody = Label(objectives, "Objective", new Vector2(0, -13), new Vector2(260, 28), 18);
            objectiveBody.alignment = TextAnchor.MiddleLeft;

            helpHint = Label(go.transform, "Help hint", new Vector2(104, 26), new Vector2(156, 24), 14, false, Vector2.zero);
            helpHint.alignment = TextAnchor.MiddleLeft;
            var dot = Rect("Aim dot", go.transform, Vector2.one * 0.5f, Vector2.zero, new Vector2(3, 3));
            dot.gameObject.AddComponent<Image>().color = Cream;

            progress = Rect("Arc progress", go.transform, Vector2.one * 0.5f, new Vector2(60, 0), new Vector2(220, 220));
            progressGroup = progress.gameObject.AddComponent<CanvasGroup>();
            progressGroup.interactable = false; progressGroup.blocksRaycasts = false;
            ArcLayer("Track", arcTrack, Color.white);
            var fill = ArcLayer("Fill", arcFill, new Color(0.68f, 0.79f, 0.36f));
            fillMaterial = new Material(arcMaterial); fill.material = fillMaterial;
            progressLabel = Label(progress, "Progress", new Vector2(74, -83), new Vector2(100, 26), 17, true);

            prompt = Rect("Context hint", go.transform, new Vector2(0.5f, 0), new Vector2(0, 74), new Vector2(352, 62));
            Background(prompt);
            promptLabel = Label(prompt, "Hint text", Vector2.zero, new Vector2(300, 38), 19);

            helpPanel = Rect("Foldable controls", go.transform, new Vector2(0, 1), new Vector2(228, -320), new Vector2(408, 400));
            Background(helpPanel);
            var helpTitle = Label(helpPanel, "Controls title", new Vector2(0, 164), new Vector2(344, 28), 21, true);
            helpTitle.text = "Controls   [F1]"; helpTitle.alignment = TextAnchor.MiddleLeft;
            helpText = Label(helpPanel, "Controls", new Vector2(0, -12), new Vector2(344, 320), 16);
            helpText.alignment = TextAnchor.UpperLeft; helpText.lineSpacing = 1.2f;
            helpPanel.gameObject.SetActive(false); progress.gameObject.SetActive(false); prompt.gameObject.SetActive(false);
        }
        Image ArcLayer(string name, Sprite sprite, Color tint)
        {
            // Same square rect, pivot and untrimmed sprite canvas for both supplied layers.
            var rect = Rect(name, progress, Vector2.one * 0.5f, Vector2.zero, new Vector2(220, 220));
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.color = tint;
            image.type = Image.Type.Simple; image.preserveAspect = true; image.useSpriteMesh = false; image.raycastTarget = false;
            return image;
        }
        void Update()
        {
            var keyboard = Keyboard.current; if (keyboard == null) return;
            if (keyboard.f1Key.wasPressedThisFrame) HelpVisible = !HelpVisible;
            if (keyboard.oKey.wasPressedThisFrame) ObjectivesExpanded = !ObjectivesExpanded;
        }
        void LateUpdate()
        {
            if (!pickup.View) return;
            helpPanel.gameObject.SetActive(HelpVisible);
            helpHint.text = HelpVisible ? "F1  Close controls" : "F1  Controls";
            if (HelpVisible) UpdateHelp();

            displayedProgress = pickup.Progress;
            fillMaterial.SetFloat("_Progress", displayedProgress);
            bool complete = pickup.practiceSpot.Complete;
            if (!complete) { completedAt = -1; collapsedOnComplete = false; }
            else if (completedAt < 0) completedAt = Time.unscaledTime;
            float completionAge = complete ? Time.unscaledTime - completedAt : 0;
            progressGroup.alpha = complete ? Mathf.Clamp01(1 - (completionAge - CompletionHold) / CompletionFade) : 1;
            progressLabel.text = complete ? "Clean!" : (displayedProgress * 100).ToString("0") + "%";
            progress.gameObject.SetActive(complete ? completionAge < CompletionHold + CompletionFade
                : pickup.SpotInReach || (pickup.workshop.brush.Held && !pickup.CenterMode));
            if (complete && completionAge >= CompletionHold + CompletionFade && !collapsedOnComplete)
            { ObjectivesExpanded = false; collapsedOnComplete = true; }
            objectiveBody.text = complete ? "Nursery restored" : "Clean the nursery frame";
            objectiveBody.gameObject.SetActive(ObjectivesExpanded);
            objectives.sizeDelta = new Vector2(308, ObjectivesExpanded ? 86 : 48);
            objectives.anchoredPosition = new Vector2(178, ObjectivesExpanded ? -64 : -45);
            var objectiveTitle = objectives.GetChild(0) as RectTransform;
            objectiveTitle.anchoredPosition = new Vector2(0, ObjectivesExpanded ? 19 : 0);

            string context = pickup.AimingBrush ? "G   Pick up brush" : pickup.corals.HasPrompt
                ? pickup.corals.Held ? "G   Replace coral" : "G   Pick up coral" : "";
            if (pickup.SpotInReach && pickup.Carrying)
                context = complete || pickup.IsScrubbing ? "" : "Hold LMB to scrub";
            else if (!complete && pickup.SpotInReach && context.Length == 0)
                context = pickup.workshop.brush.Held ? "Sweep over the algae" : "Pick up the brush from the tray";
            if (!pickup.workshop.xrGroup.activeSelf && Cursor.lockState != CursorLockMode.Locked && !pickup.Automated && !pickup.corals.Automated)
                context = "Click to dive";
            promptLabel.text = context; prompt.gameObject.SetActive(context.Length > 0);
        }
        void UpdateHelp()
        {
            bool xr = pickup.workshop.xrGroup.activeSelf;
            helpText.text = (xr ? pickup.CenterMode ? "XR body mode\n\nWASD  Swim     Q / E  Down / up\nShift  Boost     B  Brake\n" : "XR controller mode\n\nWASD + Q / E  Move controller\nHold G  Grip brush; sweep to clean\n" : "Desktop swimming\n\nWASD  Swim     Space / C  Up / down\nShift  Boost     RMB / X  Brake\n")
                + (pickup.CenterMode ? "G  Pick up     Hold LMB  Scrub\nH  Return brush\n" : "Release grip  Keep coral in hand\n")
                + (xr ? "Tab  Body / devices     [ / ]  Hand\nRMB  Toggle mouse look\n" : "Esc  Cursor     Click  Resume\nR  Return to start\n")
                + "\nF2  Desktop / XR     O  Objectives\nF5  Reset nursery     F11  Fullscreen";
        }
        void Background(RectTransform rect)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = panelSprite;
            image.type = Image.Type.Sliced; image.color = Navy; image.raycastTarget = false;
            // The 1024px source border stays modest at the final HUD size.
            image.pixelsPerUnitMultiplier = 2;
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.gameObject.layer = 5;
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        Text Label(Transform parent, string name, Vector2 position, Vector2 size, int sizeInPixels, bool bold = false, Vector2? anchor = null)
        {
            var text = Rect(name, parent, anchor ?? Vector2.one * 0.5f, position, size).gameObject.AddComponent<Text>();
            text.font = bold ? boldFont : font; text.fontSize = sizeInPixels;
            text.alignment = TextAnchor.MiddleCenter; text.color = Cream; text.raycastTarget = false;
            var shadow = text.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0.04f, 0.05f, 0.35f); shadow.effectDistance = new Vector2(0, -1);
            return text;
        }
        void OnDestroy() { if (fillMaterial) Destroy(fillMaterial); }
    }
}
