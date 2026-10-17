using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Reefkeeper
{
    /// <summary>Only runs with -reef-smoke OUTPUT_DIRECTORY. Normal play is unaffected.</summary>
    public sealed class PrototypeSmokeCheck : MonoBehaviour
    {
        IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-reef-smoke");
            if (index < 0 || index + 1 >= args.Length) yield break;
            string output = args[index + 1];
            Directory.CreateDirectory(output);
            var motor = GetComponent<DiveMotor>();
            motor.Automated = true;
            motor.SetControl(false);
            yield return new WaitForSeconds(2);
            string error = null;
            try { CheckMotion(motor); }
            catch (Exception e) { error = e.ToString(); }
            if (error != null)
            {
                File.WriteAllText(Path.Combine(output, "smoke-result.txt"), "FAIL\n" + error);
                Debug.LogError(error);
                Application.Quit(1);
                yield break;
            }
            motor.ResetDive();
            yield return new WaitForSeconds(1);
            Capture(motor.view, Path.Combine(output, "reef-spawn.png"));
            yield return new WaitForSeconds(1);
            var controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = new Vector3(0, 3.5f, 3);
            transform.rotation = Quaternion.Euler(0, 6, 0);
            controller.enabled = true;
            yield return new WaitForSeconds(1);
            Capture(motor.view, Path.Combine(output, "reef-landmark.png"));
            yield return new WaitForSeconds(1);
            controller.enabled = false;
            transform.position = new Vector3(5, 2, -11);
            transform.rotation = Quaternion.Euler(0, 18, 0);
            motor.view.transform.localRotation = Quaternion.Euler(10, 0, 0);
            controller.enabled = true;
            yield return new WaitForSeconds(1);
            Capture(motor.view, Path.Combine(output, "reef-coral.png"));
            yield return new WaitForSeconds(1);
            Require(File.Exists(Path.Combine(output, "reef-spawn.png"))
                && File.Exists(Path.Combine(output, "reef-landmark.png"))
                && File.Exists(Path.Combine(output, "reef-coral.png")), "Rendered screenshots exist");
            File.WriteAllText(Path.Combine(output, "smoke-result.txt"),
                "PASS: forward movement, sprint, coast, active brake, solid collision, surface limit, reset.\n"
                + "Rendered spawn, landmark and coral screenshots. Desktop prototype; XR and Rokoko are not connected.\n");
            Application.Quit(0);
        }

        static void Capture(Camera camera, string path)
        {
            // Explicit render target also works when Windows keeps the test window hidden.
            var target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            target.Create();
            var previous = RenderTexture.active;
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                RenderPipeline.SubmitRenderRequest(camera,
                    new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                target.Release();
                Destroy(target);
                Destroy(image);
            }
        }

        void CheckMotion(DiveMotor m)
        {
            m.ResetDive();
            Vector3 start = transform.position;
            for (int i = 0; i < 180; i++) m.Tick(Vector3.forward, false, false, 1f / 60);
            Require(transform.position.z > start.z + 6, "Forward movement");
            for (int i = 0; i < 60; i++) m.Tick(Vector3.forward, true, false, 1f / 60);
            Require(m.Velocity.magnitude > 5, "Sprint speed");
            float speed = m.Velocity.magnitude;
            m.Tick(Vector3.zero, false, false, 1f / 60);
            Require(m.Velocity.magnitude < speed && m.Velocity.magnitude > 0, "Coast");
            for (int i = 0; i < 60; i++) m.Tick(Vector3.forward, true, true, 1f / 60);
            Require(m.Velocity == Vector3.zero, "Brake overrides sprint");

            m.ResetDive();
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Temporary smoke-test obstacle";
            wall.transform.position = start + Vector3.forward * 4;
            wall.transform.localScale = new Vector3(8, 8, 1);
            Physics.SyncTransforms();
            for (int i = 0; i < 300; i++) m.Tick(Vector3.forward, true, false, 1f / 60);
            Require(transform.position.z < wall.transform.position.z - 0.65f, "Solid collider prevents tunnelling");
            wall.SetActive(false);
            Destroy(wall);
            m.ResetDive();
            for (int i = 0; i < 600; i++) m.Tick(Vector3.up, true, false, 1f / 60);
            Require(transform.position.y <= m.surfaceHeight - 0.79f, "Surface boundary");
            m.ResetDive();
            Require(Vector3.Distance(transform.position, start) < 0.01f && m.Velocity == Vector3.zero, "Reset");
        }
        static void Require(bool condition, string name)
        {
            if (!condition) throw new Exception("Runtime check failed: " + name);
        }
    }
}
