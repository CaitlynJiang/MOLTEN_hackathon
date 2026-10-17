using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reefkeeper
{
    /// <summary>Optional engine renders of the saved art pass; does nothing in normal play.</summary>
    public sealed class ReefArtCapture : MonoBehaviour
    {
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-reef-art-capture");
            if (index < 0 || index + 1 >= args.Length) yield break;
            string output = args[index + 1]; Directory.CreateDirectory(output);
            yield return new WaitForSeconds(3);
            var source = Camera.main;
            // Offscreen art views omit the simulator's floating controller models and rays.
            foreach (var renderer in GetComponent<ReefWorkshop>().xrGroup.GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            Vector3 center = transform.position;
            Capture(source, center + new Vector3(-5.5f, 3.4f, -5.8f), center + new Vector3(0.7f, 2.3f, 1.3f), 58, Path.Combine(output, "nursery-overview.png"));
            Capture(source, center + new Vector3(2.8f, 2.6f, -3.6f), center + new Vector3(0, 2, 0), 55, Path.Combine(output, "nursery-detail.png"));
            Capture(source, new Vector3(-1, 4.8f, -11), new Vector3(0, 4.3f, -3), 52, Path.Combine(output, "blue-reef-fish.png"));
            File.WriteAllText(Path.Combine(output, "capture-result.txt"), "Saved three actual Unity renders of the mood-board art pass.\n");
            Application.Quit(0);
        }
        static void Capture(Camera source, Vector3 position, Vector3 target, float fov, string path)
        {
            var go = new GameObject("Art capture camera");
            var camera = go.AddComponent<Camera>(); camera.CopyFrom(source); camera.enabled = false; camera.fieldOfView = fov;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            go.transform.position = position; go.transform.LookAt(target);
            PrototypeSmokeCheck.Capture(camera, path); Destroy(go);
        }
    }
}
