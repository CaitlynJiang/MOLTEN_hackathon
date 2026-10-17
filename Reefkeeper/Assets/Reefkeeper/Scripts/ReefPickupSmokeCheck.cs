using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;

namespace Reefkeeper
{
    public sealed class ReefPickupSmokeCheck : MonoBehaviour
    {
        Keyboard keyboard;
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-reef-pickup-smoke");
            if (index < 0 || index + 1 >= args.Length) yield break;
            string output = args[index + 1]; Directory.CreateDirectory(output);
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
            var routine = Check(output);
            while (true)
            {
                bool next = false; object current = null; Exception error = null;
                try { next = routine.MoveNext(); if (next) current = routine.Current; }
                catch (Exception e) { error = e; }
                if (error != null)
                {
                    File.WriteAllText(Path.Combine(output, "pickup-result.txt"), "FAIL\n" + error);
                    Debug.LogException(error); Application.Quit(1); yield break;
                }
                if (!next) break;
                yield return current;
            }
            File.WriteAllText(Path.Combine(output, "pickup-result.txt"),
                "PASS: 84 XR grab fragments; aim outline/prompt; occlusion and range; keyboard G pickup; persistent hold; " +
                "three pickups leave one selected coral and destroy predecessors; empty aim preserves held coral; " +
                "F5 restores nursery; FPS aim has exclusive G input; native simulated grip and persistent hand transfer; mode switch; F11 fullscreen/window restore.\n");
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            Application.Quit(0);
        }
        IEnumerator Check(string output)
        {
            yield return new WaitForSeconds(3);
            var workshop = GetComponent<ReefWorkshop>();
            var pickup = GetComponent<ReefCoralPickup>();
            var diver = workshop.desktopPlayer.GetComponent<DiveMotor>();
            diver.Automated = true; pickup.Automated = true;
            keyboard = InputSystem.AddDevice<Keyboard>("Reef pickup check keyboard");
            Require(workshop.desktopPlayer.activeSelf && pickup.fragments.Length == 84, "Desktop mode with 84 fragments");
            var candidates = pickup.fragments.Where(f => f.transform.parent.name == "Nursery tree / interactive" && f.transform.localPosition.z < 0).Take(3).ToArray();
            Require(candidates.Length == 3, "Three reachable front fragments");
            var view = pickup.View;
            Aim(view, candidates[0]); yield return new WaitForSeconds(0.2f);
            Require(pickup.Aimed == candidates[0] && pickup.HasPrompt && candidates[0].Highlighted, "Aim highlights edges and offers G prompt");
            PrototypeSmokeCheck.Capture(view, Path.Combine(output, "coral-highlight.png"));
            yield return new WaitForSeconds(0.2f);

            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.transform.position = view.transform.position + view.transform.forward * 0.3f;
            blocker.transform.localScale = new Vector3(0.35f, 0.35f, 0.04f);
            Physics.SyncTransforms(); yield return new WaitForSeconds(0.1f);
            Require(!pickup.Aimed && !candidates[0].Highlighted, "Solid occluder blocks aim and clears outline");
            Keys(Key.G); yield return null; Keys(); yield return null;
            Require(!pickup.Held, "Cannot pick through solid objects");
            blocker.SetActive(false); Destroy(blocker);
            pickup.reach = 0.2f; yield return new WaitForSeconds(0.1f);
            Require(!pickup.Aimed, "Reach limit prevents remote pickup");
            pickup.reach = 3;

            ReefCoralFragment previous = null;
            for (int i = 0; i < candidates.Length; i++)
            {
                Aim(view, candidates[i]); yield return new WaitForSeconds(0.15f);
                Require(pickup.Aimed == candidates[i], "Aim at next fragment " + i);
                Keys(Key.G); yield return new WaitForSeconds(0.2f); Keys(); yield return new WaitForSeconds(0.2f);
                Require(pickup.Held == candidates[i] && candidates[i].isSelected, "G selects and retains fragment " + i);
                Require(pickup.hand.interactablesSelected.Count == 1, "Single selected coral in hand");
                Require(!candidates[i].Highlighted && !pickup.HasPrompt, "Held fragment loses outline and prompt");
                Require(!candidates[i].suspensionLine.activeSelf, "Picked coral detaches its hanging line");
                if (i > 0) Require(!previous, "Previous held object was destroyed");
                Vector3 start = candidates[i].transform.position;
                view.transform.position += Vector3.right * 0.15f;
                yield return new WaitForSeconds(0.2f);
                Require(Vector3.Distance(start, candidates[i].transform.position) > 0.1f, "Held coral follows hand slot");
                previous = candidates[i];
            }
            PrototypeSmokeCheck.Capture(view, Path.Combine(output, "coral-held.png"));
            view.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            yield return new WaitForSeconds(0.1f);
            Keys(Key.G); yield return null; Keys(); yield return null;
            Require(pickup.Held == previous, "Empty aim does not delete held coral");
            Keys(Key.F5); yield return null; Keys(); yield return new WaitForSeconds(0.3f);
            Require(!pickup.Held && pickup.fragments.Length == 84 && pickup.fragments.All(f => f && f.gameObject.activeInHierarchy && f.suspensionLine.activeSelf), "F5 restores every fragment and line");
            Require(pickup.fragments.All(f => !f.isSelected && !f.Highlighted), "Reset leaves no stale selections or outlines");

            Keys(Key.F2); yield return new WaitForSeconds(0.1f); Keys(); yield return new WaitForSeconds(0.3f);
            Require(workshop.xrGroup.activeSelf, "F2 activates XR group; current keyboard=" + Keyboard.current?.name);
            var right = workshop.xrGroup.GetComponentsInChildren<Transform>(true).First(t => t.name == "Right Controller");
            // Simulator devices and controller visuals become tracked over several input frames.
            for (int i = 0; i < 120 && !right.gameObject.activeInHierarchy; i++) yield return null;
            Require(right.gameObject.activeInHierarchy, "Simulated right controller becomes active");
            var xrView = pickup.View;
            var headDriver = xrView.GetComponent<TrackedPoseDriver>();
            headDriver.enabled = false;
            var fpsTarget = pickup.fragments.First(f => f.transform.parent.name == "Nursery tree / interactive" && f.transform.localPosition.z < 0);
            var gripTarget = pickup.fragments.First(f => f != fpsTarget);
            gripTarget.transform.SetPositionAndRotation(right.position, right.rotation);
            gripTarget.GetComponent<Rigidbody>().position = right.position;
            Aim(xrView, fpsTarget); yield return new WaitForSeconds(0.2f);
            Require(pickup.CenterAimMode && pickup.Aimed == fpsTarget, "FPS center aim selects its own target");
            Keys(Key.G); yield return new WaitForSeconds(0.3f); Keys(); yield return new WaitForSeconds(0.2f);
            Require(pickup.Held == fpsTarget && !gripTarget.isSelected && gripTarget.suspensionLine.activeSelf,
                "One FPS G press cannot also grab the controller's different target");
            workshop.ResetWorkshop(); headDriver.enabled = true; yield return new WaitForSeconds(0.2f);
            Keys(Key.RightBracket); yield return null; Keys(); yield return new WaitForSeconds(0.1f);
            var native = pickup.fragments[0];
            native.transform.SetPositionAndRotation(right.position, right.rotation);
            native.GetComponent<Rigidbody>().position = right.position;
            Physics.SyncTransforms(); yield return new WaitForSeconds(0.2f);
            Keys(Key.G); yield return new WaitForSeconds(0.35f);
            Require(pickup.Held == native && native.isSelected && native.interactorsSelecting[0] != pickup.hand, "Simulated controller grip uses native XR coral grab");
            Keys(); yield return new WaitForSeconds(0.3f);
            Require(pickup.Held == native && native.isSelected && pickup.hand.IsSelecting(native), "Released grip retains coral in persistent slot");
            Keys(Key.F2); yield return null; Keys(); yield return new WaitForSeconds(0.25f);
            Require(pickup.Held == native && native.isSelected && workshop.desktopPlayer.activeSelf, "Mode switching retains one held coral");

            int width = Screen.width, height = Screen.height;
            Require(!Screen.fullScreen, "Test begins windowed");
            Keys(Key.F11); yield return null; Keys(); yield return new WaitForSeconds(1);
            Require(Screen.fullScreen && Screen.fullScreenMode == FullScreenMode.FullScreenWindow, "F11 enters borderless fullscreen");
            Keys(Key.F11); yield return null; Keys(); yield return new WaitForSeconds(1);
            Require(!Screen.fullScreen && Screen.width == width && Screen.height == height, "F11 restores original window size");
        }
        static void Aim(Camera view, ReefCoralFragment fragment)
        {
            var target = fragment.transform.TransformPoint(new Vector3(0, 0.11f, 0));
            view.transform.position = target + new Vector3(0, 0, -0.7f);
            view.transform.LookAt(target); Physics.SyncTransforms();
        }
        void Keys(params Key[] keys) { keyboard.MakeCurrent(); InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        static void Require(bool valid, string message) { if (!valid) throw new Exception("Pickup check: " + message); }
    }
}
