using UnityEngine;

namespace Reefkeeper
{
    public sealed class DiveHUD : MonoBehaviour
    {
        public DiveMotor diver;
        GUIStyle title, small, number, hint;
        Texture2D pixel;
        void OnGUI()
        {
            if (!diver) return;
            if (title == null)
            {
                title = MakeStyle(29, new Color(0.91f, 0.98f, 0.94f));
                small = MakeStyle(11, new Color(0.7f, 0.9f, 0.85f));
                number = MakeStyle(24, new Color(0.9f, 1, 0.95f));
                hint = MakeStyle(12, new Color(0.83f, 0.94f, 0.9f));
                pixel = Texture2D.whiteTexture;
            }
            Matrix4x4 original = GUI.matrix;
            float scale = Mathf.Max(0.5f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float w = Screen.width / scale, h = Screen.height / scale;
            GUI.Label(new Rect(38, 30, 400, 22), "FIELD NOTES    /    01", small);
            GUI.Label(new Rect(36, 51, 400, 45), "Reefkeeper", title);
            GUI.Label(new Rect(38, 98, 460, 24), "SHALLOW REEF     /     FREE EXPLORATION", small);
            GUI.color = new Color(0.55f, 0.89f, 0.8f, 0.5f);
            GUI.DrawTexture(new Rect(38, 132, 42, 2), pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(w - 204, 34, 170, 24), "DEPTH", small);
            GUI.Label(new Rect(w - 204, 55, 170, 34), diver.Depth.ToString("00.0") + " m", number);
            GUI.Label(new Rect(w - 204, 100, 170, 24), "SPEED  " + diver.Velocity.magnitude.ToString("0.0") + " m/s", small);
            string state = diver.Braking ? "BRAKING" : diver.Sprinting ? "FIN KICK / BOOST" : "NEUTRAL BUOYANCY";
            GUI.Label(new Rect(38, h - 78, 400, 24), state, small);
            GUI.Label(new Rect(38, h - 48, w - 60, 26),
                "WASD swim   SPACE / C rise / descend   SHIFT boost   RMB / X brake   R reset   ESC cursor   F2 XR workshop", hint);
            GUI.color = new Color(0.85f, 1, 0.94f, 0.6f);
            GUI.DrawTexture(new Rect(w / 2 - 1.5f, h / 2 - 1.5f, 3, 3), pixel);
            GUI.color = Color.white;
            if (Cursor.lockState != CursorLockMode.Locked && !diver.Automated)
            {
                GUIStyle centered = new GUIStyle(number) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(w / 2 - 260, h / 2 + 30, 520, 50), "Click to dive", centered);
            }
            GUI.matrix = original;
        }
        static GUIStyle MakeStyle(int size, Color color)
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Normal };
            s.normal.textColor = color;
            return s;
        }
    }
}
