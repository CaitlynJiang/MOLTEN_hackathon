using UnityEngine;
using UnityEngine.InputSystem;

namespace Reefkeeper
{
    public sealed class ReefDisplayControls : MonoBehaviour
    {
        int windowWidth = 1280, windowHeight = 720;
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f11Key.wasPressedThisFrame) ToggleFullscreen();
        }
        public void ToggleFullscreen()
        {
            if (Screen.fullScreen)
                Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
            else
            {
                windowWidth = Screen.width; windowHeight = Screen.height;
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
            }
        }
    }
}
