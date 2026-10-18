using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Reefkeeper
{
    public sealed class ReefCleaningSmokeCheck : MonoBehaviour
    {
        Keyboard keyboard;
        Mouse mouse;
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-reef-cleaning-smoke");
            if (index < 0 || index + 1 >= args.Length) yield break;
            string output = args[index + 1]; Directory.CreateDirectory(output);
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
            var routine = Check(output);
            while (true)
            {
                bool next = false; object current = null; Exception error = null;
                try { next = routine.MoveNext(); if (next) current = routine.Current; } catch (Exception e) { error = e; }
                if (error != null) { File.WriteAllText(Path.Combine(output, "cleaning-result.txt"), "FAIL\n" + error); Debug.LogException(error); Application.Quit(1); yield break; }
                if (!next) break; yield return current;
            }
            File.WriteAllText(Path.Combine(output, "cleaning-result.txt"),
                "PASS: single practice spot, brush aim/white outline, G persistent XRI pickup, occlusion and range rejection, no idle cleaning, LMB physical brush animation, progressive algae removal, UI progress, pause/resume, completion, H return, coral/brush exclusion and F5 reset.\n");
            InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); Application.Quit(0);
        }
        IEnumerator Check(string output)
        {
            yield return new WaitForSeconds(3);
            var pickup = GetComponent<ReefBrushPickup>(); var workshop = pickup.workshop;
            workshop.desktopPlayer.GetComponent<DiveMotor>().Automated = true;
            pickup.Automated = true; pickup.corals.Automated = true;
            keyboard = InputSystem.AddDevice<Keyboard>("Cleaning check keyboard"); mouse = InputSystem.AddDevice<Mouse>("Cleaning check mouse");
            var view = pickup.View; var patch = pickup.practiceSpot; var hud = GetComponent<ReefCleaningHUD>();
            var audio = GetComponent<ReefSoundscape>();
            Require(audio && audio.AmbienceSource.isPlaying && audio.MusicSource.isPlaying && audio.AmbienceSource.loop && audio.MusicSource.loop
                && audio.musicVolume < audio.ambienceVolume, "Both user audio clips loop with quieter music");
            Require(workshop.patches.Length == 1 && patch.growth.Length > 0, "One attached algae target");
            Vector3 brushTarget = workshop.brush.transform.TransformPoint(new Vector3(0, 0, 0.2f));
            view.transform.position = brushTarget + new Vector3(0, 0.22f, -0.75f); view.transform.LookAt(brushTarget);
            Physics.SyncTransforms(); yield return new WaitForSeconds(0.3f);
            Require(pickup.AimingBrush && pickup.brush.Highlighted, "Brush white highlight and pickup prompt");
            Capture(view, output, "brush-pickup.png");
            Keys(Key.G); yield return new WaitForSeconds(0.15f); Keys(); yield return new WaitForSeconds(0.3f);
            Require(pickup.Carrying && pickup.brush.isSelected && !pickup.brush.Highlighted, "Tap G holds brush after release");
            AimSpot(view, patch); yield return new WaitForSeconds(0.5f);
            Require(pickup.SpotInReach && patch.Cleaned == 0, "Aiming with idle brush does not clean");
            Capture(view, output, "cleaning-before.png");
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.transform.position = view.transform.position + view.transform.forward * 0.35f;
            blocker.transform.localScale = new Vector3(0.3f, 0.3f, 0.06f); Physics.SyncTransforms();
            Click(true); yield return new WaitForSeconds(0.4f);
            Require(!pickup.IsScrubbing && patch.Cleaned == 0, "Cannot scrub through obstruction");
            Click(false); blocker.SetActive(false); Destroy(blocker); yield return null;
            view.transform.position -= view.transform.forward * 2; Click(true); yield return new WaitForSeconds(0.35f);
            Require(!pickup.IsScrubbing && patch.Cleaned == 0, "Cannot scrub from out of reach");
            Click(false); AimSpot(view, patch); yield return new WaitForSeconds(0.3f);
            Click(true); yield return new WaitForSeconds(1.5f);
            Require(pickup.IsScrubbing && patch.Cleaned > 0.05f && !patch.Complete, "LMB animates physical contact and partial progress; progress=" + patch.Cleaned);
            Require(Mathf.Abs(hud.DisplayedProgress - patch.Cleaned) < 0.03f, "UI matches cleaning progress");
            Require(patch.growth.Any(t => !t.gameObject.activeSelf) && patch.dirtVisual.enabled, "Algae is removed gradually");
            Capture(view, output, "cleaning-progress.png");
            Vector3 tip = workshop.brush.bristles.position; yield return new WaitForSeconds(0.12f);
            Require(Vector3.Distance(tip, workshop.brush.bristles.position) > 0.005f, "Visible brush movement");
            Click(false); yield return new WaitForSeconds(0.2f); float paused = patch.Cleaned;
            yield return new WaitForSeconds(0.5f);
            Require(Mathf.Abs(paused - patch.Cleaned) < 0.001f, "Release pauses cleaning");
            Click(true);
            for (int i = 0; i < 600 && !patch.Complete; i++) yield return null;
            Click(false); yield return new WaitForSeconds(0.3f);
            Require(patch.Complete && !patch.dirtVisual.enabled && patch.growth.All(t => !t.gameObject.activeSelf), "Complete removes attached growth; progress=" + patch.Cleaned + " activeTufts=" + patch.growth.Count(t => t.gameObject.activeSelf));
            Require(hud.DisplayedProgress > 0.999f && hud.Status.Contains("complete"), "UI confirms completion");
            Capture(view, output, "cleaning-complete.png");
            Keys(Key.H); yield return new WaitForSeconds(0.1f); Keys(); yield return new WaitForSeconds(0.3f);
            Require(!pickup.Carrying && !pickup.brush.isSelected && Vector3.Distance(workshop.brush.transform.position, workshop.brushHome.position) < 0.02f, "H returns brush");
            // Re-grab then switch to a coral; the brush must return and leave only one held item.
            view.transform.position = brushTarget + new Vector3(0, 0.22f, -0.75f); view.transform.LookAt(brushTarget); yield return new WaitForSeconds(0.2f);
            Keys(Key.G); yield return new WaitForSeconds(0.15f); Keys(); yield return new WaitForSeconds(0.2f);
            Require(pickup.Carrying, "Brush can be picked again");
            var coral = pickup.corals.fragments.First(f => f.transform.parent.name == "Nursery tree / interactive" && f.transform.localPosition.z < 0);
            var center = coral.GetComponent<Collider>().bounds.center; view.transform.position = center + Vector3.back * 0.7f; view.transform.LookAt(center);
            yield return new WaitForSeconds(0.2f); Keys(Key.G); yield return new WaitForSeconds(0.15f); Keys(); yield return new WaitForSeconds(0.2f);
            Require(pickup.corals.Held == coral && !pickup.Carrying && !pickup.brush.isSelected, "Coral returns brush");
            view.transform.position = brushTarget + new Vector3(0, 0.22f, -0.75f); view.transform.LookAt(brushTarget); yield return new WaitForSeconds(0.2f);
            Keys(Key.G); yield return new WaitForSeconds(0.15f); Keys(); yield return new WaitForSeconds(0.2f);
            Require(pickup.Carrying && !pickup.corals.Held && !coral, "Brush clears held coral");
            Keys(Key.F5); yield return new WaitForSeconds(0.15f); Keys(); yield return new WaitForSeconds(0.2f);
            Require(patch.Cleaned == 0 && patch.dirtVisual.enabled && patch.growth.All(t => t.gameObject.activeSelf) && !pickup.Carrying, "F5 restores dirt, fronds and tool");
        }
        static void AimSpot(Camera view, RackDirtPatch patch)
        {
            view.transform.position = patch.transform.position + new Vector3(0, 0.35f, -0.85f);
            view.transform.LookAt(patch.transform.position + Vector3.up * 0.025f); Physics.SyncTransforms();
        }
        static void Capture(Camera view, string output, string file) { Canvas.ForceUpdateCanvases(); PrototypeSmokeCheck.Capture(view, Path.Combine(output, file)); }
        void Keys(params Key[] keys) { keyboard.MakeCurrent(); InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        void Click(bool down) { mouse.MakeCurrent(); InputSystem.QueueStateEvent(mouse, new MouseState { buttons = (ushort)(down ? 1 : 0) }); }
        static void Require(bool valid, string message) { if (!valid) throw new Exception("Cleaning check: " + message); }
    }
}
