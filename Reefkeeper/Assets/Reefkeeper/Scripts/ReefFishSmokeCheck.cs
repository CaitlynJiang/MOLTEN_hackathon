using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reefkeeper
{
    /// <summary>Opt-in check of real imported skin deformation, swim paths and rendered fish.</summary>
    public sealed class ReefFishSmokeCheck : MonoBehaviour
    {
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-reef-fish-smoke");
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
                    File.WriteAllText(Path.Combine(output, "fish-result.txt"), "FAIL\n" + error);
                    Debug.LogException(error); Application.Quit(1); yield break;
                }
                if (!next) break;
                yield return current;
            }
            File.WriteAllText(Path.Combine(output, "fish-result.txt"),
                "PASS: 27 animated animals, 4 groups, actual skinned vertex deformation, moving paths, URP materials, looping clips, seabed clearance.\n"
                + "Three fish portraits, manta and habitat renders captured. Ambient loops; no full Boids/obstacle or predator simulation.\n");
            Application.Quit(0);
        }
        IEnumerator Check(string output)
        {
            yield return new WaitForSeconds(3);
            var schools = GetComponentsInChildren<ReefFishSchool>();
            Require(schools.Length == 4 && schools.Sum(s => s.fish.Length) == 27, "All placed animals exist");
            var source = Camera.main;
            foreach (var school in schools)
            {
                Transform fish = school.fish[0];
                var skin = fish.GetComponentInChildren<SkinnedMeshRenderer>();
                var animator = fish.GetComponentInChildren<Animator>();
                Require(animator && animator.GetCurrentAnimatorStateInfo(0).loop, "Swim animation loops: " + school.name);
                foreach (var material in skin.sharedMaterials)
                    Require(material && material.shader.name == "Universal Render Pipeline/Lit" && material.shader.isSupported, "URP fish material");
                var sample = new Mesh(); skin.BakeMesh(sample);
                var before = sample.vertices; Vector3 start = fish.position;
                yield return new WaitForSeconds(0.37f);
                skin.BakeMesh(sample);
                var after = sample.vertices;
                Require(before.Length == after.Length && before.Where((v, i) => (v - after[i]).sqrMagnitude > 0.000001f).Any(), "Actual mesh deforms: " + school.name);
                Require(Vector3.Distance(start, fish.position) > 0.015f, "Fish swims through scene");
                Destroy(sample);
                school.animatePath = false;
                float length = fish.localScale.x;
                Vector3 center = skin.bounds.center;
                Vector3 cameraOffset = fish.right * length * 1.65f + fish.forward * length * 0.45f + Vector3.up * length * 0.35f;
                Capture(source, center + cameraOffset, center, Path.Combine(output, school.name.Split('/')[0].Trim() + ".png"));
                school.animatePath = true;
            }
            foreach (var school in schools)
                for (int t = 0; t <= 60; t += 2)
                {
                    school.Evaluate(t);
                    foreach (var fish in school.fish)
                    {
                        Vector3 p = fish.position;
                        Require(!float.IsNaN(p.x) && p.y < 14.5f, "Finite position below surface");
                        if (school.seabed.Raycast(new Ray(p + Vector3.up * 40, Vector3.down), out var hit, 80))
                            Require(p.y >= hit.point.y + school.seabedClearance - 0.01f, "Fish clears seabed");
                    }
                }
            foreach (var school in schools) { school.animatePath = false; school.Evaluate(2); }
            Capture(source, new Vector3(-1, 4.8f, -11), new Vector3(0, 4.3f, -3), Path.Combine(output, "reef-fish-habitat.png"));
        }
        static void Capture(Camera source, Vector3 position, Vector3 target, string path)
        {
            var go = new GameObject("Fish screenshot camera");
            var camera = go.AddComponent<Camera>(); camera.CopyFrom(source); camera.enabled = false;
            camera.fieldOfView = 52;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            go.transform.position = position; go.transform.LookAt(target);
            PrototypeSmokeCheck.Capture(camera, path); Destroy(go);
        }
        static void Require(bool condition, string name) { if (!condition) throw new Exception(name); }
    }
}
