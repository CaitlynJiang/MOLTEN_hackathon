using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Reefkeeper
{
    /// <summary>Opt-in end-to-end checks, using simulated keyboard grip and tracked-pose sweeps.</summary>
    public sealed class ReefXRSmokeCheck : MonoBehaviour
    {
        Keyboard keyboard;
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-reef-xr-smoke");
            if (index < 0 || index + 1 >= args.Length) yield break;
            string output = args[index + 1]; Directory.CreateDirectory(output);
            var routine = Check(output);
            while (true)
            {
                bool next = false; object current = null; Exception error = null;
                try { next = routine.MoveNext(); if (next) current = routine.Current; }
                catch (Exception e) { error = e; }
                if (error != null)
                {
                    File.WriteAllText(Path.Combine(output, "xr-result.txt"), "FAIL\n" + error);
                    Debug.LogException(error); Application.Quit(1); yield break;
                }
                if (!next) break;
                yield return current;
            }
            File.WriteAllText(Path.Combine(output, "xr-result.txt"),
                "PASS: XR swimming and brake, simulated hand translation, keyboard grip -> XR selection, held brush follows hand, release, "
                + "stationary/off-surface/unheld rejection, contact strokes on all 24 patches, completion, reset.\n"
                + "Synthetic tracked-pose sweeps were used for precise contact. No physical headset or Rokoko validation.\n");
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            Application.Quit(0);
        }

        IEnumerator Check(string output)
        {
            yield return new WaitForSeconds(3);
            var workshop = GetComponent<ReefWorkshop>();
            var swim = workshop.xrGroup.GetComponentInChildren<ReefXRSwim>();
            var brush = workshop.brush;
            var grab = brush.GetComponent<XRGrabInteractable>();
            var body = brush.GetComponent<Rigidbody>();
            Transform right = null;
            foreach (Transform item in workshop.xrGroup.GetComponentsInChildren<Transform>())
                if (item.name == "Right Controller") right = item;
            Require(right && swim.simulator.isActiveAndEnabled, "XR rig and simulator active");
            keyboard = InputSystem.AddDevice<Keyboard>("Reef smoke keyboard");
            Vector3 originalPosition = swim.transform.position;
            Keys(Key.W); yield return new WaitForSeconds(0.35f);
            Require(Vector3.Distance(originalPosition, swim.transform.position) > 0.03f, "FPS mode swims the XR Origin");
            Keys(Key.B); yield return new WaitForSeconds(0.35f); Keys(); yield return null;
            Require(swim.Velocity.magnitude < 0.01f, "XR active brake stops motion");
            var capsule = swim.GetComponent<CharacterController>();
            capsule.enabled = false; swim.transform.position = originalPosition; capsule.enabled = true;
            Log("XR swimming and brake");
            PrototypeSmokeCheck.Capture(swim.view, Path.Combine(output, "rack-before.png"));
            Vector3 rackCenter = workshop.transform.position + Vector3.up * 1.2f;
            CaptureAngle(swim.view, rackCenter + new Vector3(-4, 5, -5), rackCenter + Vector3.forward * 3,
                Path.Combine(output, "reef-workshop-overview.png"));
            Keys(Key.RightBracket); yield return null; Keys(); yield return null;
            Vector3 handStart = right.position;
            Keys(Key.D); yield return new WaitForSeconds(0.25f); Keys(); yield return null;
            Require(Vector3.Distance(handStart, right.position) > 0.025f, "Keyboard moves the simulated right controller");
            Log("simulated hand translation");
            body.position = right.position; body.rotation = right.rotation;
            brush.transform.SetPositionAndRotation(right.position, right.rotation);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.25f);
            Keys(Key.G); yield return new WaitForSeconds(0.4f);
            Require(grab.isSelected, "Keyboard G reaches XR grab selection");
            Vector3 brushStart = brush.transform.position;
            Keys(Key.G, Key.D); yield return new WaitForSeconds(0.3f); Keys(Key.G); yield return null;
            Require(Vector3.Distance(brushStart, brush.transform.position) > 0.025f, "Held brush follows simulated hand");
            Keys(); yield return new WaitForSeconds(0.2f);
            Require(!grab.isSelected, "Releasing G releases brush");
            Log("keyboard grip, following and release");

            // For repeatable contact paths, replay controller poses while preserving the actual grip action.
            var poseDriver = right.GetComponent<TrackedPoseDriver>();
            Require(poseDriver, "Right controller uses tracked poses");
            poseDriver.enabled = false;
            swim.Automated = true;
            right.rotation = Quaternion.identity;
            body.position = right.position; body.rotation = right.rotation;
            brush.transform.SetPositionAndRotation(right.position, right.rotation);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.2f);
            Keys(Key.G); yield return new WaitForSeconds(0.3f);
            Require(grab.isSelected, "Re-grab for tracked-pose replay");
            for (int i = 0; i < 10; i++) { right.position += Vector3.right * 0.02f; yield return null; }
            Require(workshop.Progress == 0, "Air strokes do not clean rack");
            foreach (var patch in workshop.patches)
            {
                Vector3 center = patch.transform.position + patch.transform.up * 0.04f;
                right.position += center - patch.transform.right * 0.09f - brush.bristles.position;
                for (int i = 0; i < 8; i++) yield return null;
                float stationary = patch.Cleaned;
                for (int i = 0; i < 8; i++) yield return null;
                Require(Mathf.Abs(stationary - patch.Cleaned) < 0.001f, "Stationary contact does not clean");
                for (int sweep = 0; sweep < 4; sweep++)
                    for (int step = 0; step <= 10; step++)
                    {
                        float x = Mathf.Lerp(-0.09f, 0.09f, step / 10f) * (sweep % 2 == 0 ? 1 : -1);
                        Vector3 desired = center + patch.transform.right * x;
                        right.position += desired - brush.bristles.position;
                        yield return null; yield return null;
                    }
                Require(patch.Complete, "Sweeping cleans " + patch.name + " (" + patch.Cleaned + ")");
                if (patch == workshop.patches[7])
                    CaptureAngle(swim.view, rackCenter + new Vector3(1.7f, 1.15f, -1.7f), rackCenter,
                        Path.Combine(output, "rack-brushing.png"));
            }
            Require(workshop.Progress > 0.999f, "Rack completion");
            PrototypeSmokeCheck.Capture(swim.view, Path.Combine(output, "rack-after.png"));
            Log("contact strokes and complete rack");
            Keys(); yield return new WaitForSeconds(0.2f);
            workshop.ResetWorkshop(); yield return null;
            Require(workshop.Progress == 0 && !grab.isSelected, "Reset restores dirty rack and releases tool");
            var first = workshop.patches[0];
            for (int i = 0; i < 20; i++)
            {
                Vector3 desired = first.transform.position + Vector3.up * 0.04f + Vector3.right * Mathf.Sin(i) * 0.06f;
                brush.transform.position += desired - brush.bristles.position;
                body.position = brush.transform.position;
                yield return null;
            }
            Require(workshop.Progress == 0, "Unheld brush cannot clean");
            Log("unheld rejection and reset");
            Keys(Key.F2); yield return null; Keys(); yield return null;
            Require(workshop.desktopPlayer.activeSelf && !workshop.xrGroup.activeSelf, "Switch to desktop");
            Keys(Key.F2); yield return null; Keys(); yield return null;
            Require(!workshop.desktopPlayer.activeSelf && workshop.xrGroup.activeSelf, "Switch back to XR simulator");
            Log("desktop and XR mode switching");
        }
        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        static void CaptureAngle(Camera source, Vector3 position, Vector3 target, string path)
        {
            var cameraObject = new GameObject("Temporary screenshot camera");
            var camera = cameraObject.AddComponent<Camera>(); camera.CopyFrom(source); camera.enabled = false;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.transform.position = position; camera.transform.LookAt(target);
            PrototypeSmokeCheck.Capture(camera, path);
            Destroy(cameraObject);
        }
        static void Log(string step) { Debug.Log("REEF_XR_CHECK " + step); }
        static void Require(bool condition, string message) { if (!condition) throw new Exception("XR check failed: " + message); }
    }
}
