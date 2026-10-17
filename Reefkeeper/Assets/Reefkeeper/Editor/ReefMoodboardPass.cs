using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Reefkeeper.Editor
{
    /// <summary>Applies the supplied photographic palette and nursery structure to the saved scene.</summary>
    public static class ReefMoodboardPass
    {
        const string Generated = "Assets/Reefkeeper/Generated/";
        const string TreeName = "Nursery tree / interactive";
        static Material pipe, rope, coral, coralTips, floatMaterial, anchorMaterial;

        public static void ApplyAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ReefSceneBuilder.ScenePath);
            ApplyWater();
            var workshop = Object.FindFirstObjectByType<ReefWorkshop>();
            if (!workshop.transform.Cast<Transform>().Any(child => child.name == TreeName)) ReplaceRack(workshop);
            if (!workshop.GetComponent<ReefArtCapture>()) workshop.gameObject.AddComponent<ReefArtCapture>();
            Physics.SyncTransforms();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("REEFKEEPER_MOODBOARD_READY: blue water, tiered coral nursery, original fish preserved");
            ReefSceneBuilder.BuildWindows();
        }

        static void ApplyWater()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.055f, 0.36f, 0.46f);
            RenderSettings.fogStartDistance = 3;
            RenderSettings.fogEndDistance = 48;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.43f, 0.63f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.17f, 0.37f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.09f, 0.23f, 0.19f);
            RenderSettings.sun.color = new Color(1, 0.96f, 0.86f);
            RenderSettings.sun.intensity = 2;
            RenderSettings.sun.shadowStrength = 0.58f;
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                camera.backgroundColor = RenderSettings.fogColor;
            Surface("Sand", new Color(0.64f, 0.72f, 0.63f), 0.9f);
            AddSandGrain();
            Surface("Limestone", new Color(0.37f, 0.45f, 0.41f), 0.28f);
            Surface("FireCoral", new Color(0.61f, 0.32f, 0.18f), 0.08f);
            Surface("LavenderCoral", new Color(0.39f, 0.30f, 0.40f), 0.08f);
            Surface("PlateCoral", new Color(0.40f, 0.43f, 0.34f), 0.10f);
            SetTint("Seagrass", new Color(0.22f, 0.34f, 0.27f));
            SetTint("Water", new Color(0.06f, 0.36f, 0.56f));
            var water = AssetDatabase.LoadAssetAtPath<Material>(Generated + "Water.mat");
            water.SetColor("_TransmissionColor", new Color(0.28f, 0.59f, 0.78f));
            water.SetColor("_ReflectionColor", new Color(0.85f, 1.05f, 1.15f));
            water.SetFloat("_ReflectionStrength", 1.4f);
            water.SetFloat("_WaveScale", 1.15f);
            water.SetVector("_SunDirection", -RenderSettings.sun.transform.forward);
            EditorUtility.SetDirty(water);
            SetTint("Sunbeams", new Color(0.68f, 0.85f, 0.82f, 0.045f));
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Generated + "UnderwaterGrade.asset");
            if (profile.TryGet<ColorAdjustments>(out var grading))
            {
                grading.postExposure.Override(0.35f);
                grading.contrast.Override(7);
                grading.saturation.Override(0);
                EditorUtility.SetDirty(grading);
            }
            if (profile.TryGet<Bloom>(out var bloom)) { bloom.intensity.Override(0.13f); EditorUtility.SetDirty(bloom); }
            if (profile.TryGet<Vignette>(out var vignette)) { vignette.intensity.Override(0.12f); EditorUtility.SetDirty(vignette); }
        }

        static void Surface(string name, Color tint, float caustics)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Generated + name + ".mat");
            material.SetColor("_BaseColor", tint);
            material.SetColor("Color_DB714CC1", tint);
            material.SetColor("Color_2B7C42F7", new Color(0.78f, 1, 0.93f) * caustics);
            EditorUtility.SetDirty(material);
        }
        static void SetTint(string name, Color tint)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Generated + name + ".mat");
            material.SetColor("_BaseColor", tint); EditorUtility.SetDirty(material);
        }

        static void AddSandGrain()
        {
            const string path = Generated + "NurserySandGrain.png";
            if (!File.Exists(path))
            {
                const int size = 512;
                var texture = new Texture2D(size, size, TextureFormat.RGB24, false);
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float ripple = Mathf.Sin((v * 14 + Mathf.Sin(u * Mathf.PI * 2) * 0.24f) * Mathf.PI * 2);
                    float broad = TileNoise(u, v, 8) * 0.6f + TileNoise(u, v, 32) * 0.4f;
                    float grain = Hash(x, y);
                    float value = 0.70f + broad * 0.18f + grain * 0.08f + ripple * 0.035f;
                    pixels[y * size + x] = new Color(value, value, value);
                }
                texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.wrapMode = TextureWrapMode.Repeat; importer.mipmapEnabled = true; importer.anisoLevel = 8;
                importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
            }
            var sand = AssetDatabase.LoadAssetAtPath<Material>(Generated + "Sand.mat");
            sand.SetTexture("Texture2D_D71B5CBC", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            sand.SetVector("Vector4_47405F4E", new Vector4(44, 44, 0, 0));
            EditorUtility.SetDirty(sand);
        }
        static float TileNoise(float u, float v, int cells)
        {
            float x = u * cells, y = v * cells;
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
            float tx = Mathf.SmoothStep(0, 1, x - ix), ty = Mathf.SmoothStep(0, 1, y - iy);
            return Mathf.Lerp(Mathf.Lerp(Hash(ix % cells, iy % cells), Hash((ix + 1) % cells, iy % cells), tx),
                Mathf.Lerp(Hash(ix % cells, (iy + 1) % cells), Hash((ix + 1) % cells, (iy + 1) % cells), tx), ty);
        }
        static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)y * 668265263u + 417u;
                h = (h ^ (h >> 13)) * 1274126177u;
                return (h & 0x00ffffffu) / 16777215f;
            }
        }

        static void ReplaceRack(ReefWorkshop workshop)
        {
            pipe = Lit("NurseryAgedFrame", new Color(0.48f, 0.46f, 0.37f));
            rope = Lit("NurseryRope", new Color(0.21f, 0.24f, 0.22f));
            coral = Lit("NurseryCoral", new Color(0.52f, 0.35f, 0.19f));
            coralTips = Lit("NurseryCoralTips", new Color(0.68f, 0.54f, 0.32f));
            floatMaterial = Lit("NurseryFloat", new Color(0.67f, 0.62f, 0.44f));
            anchorMaterial = Lit("NurseryAnchor", new Color(0.34f, 0.35f, 0.32f));
            var algae = AssetDatabase.LoadAssetAtPath<Material>(Generated + "RackAlgae.mat");
            algae.SetColor("_BaseColor", new Color(0.25f, 0.23f, 0.13f)); EditorUtility.SetDirty(algae);
            var rack = workshop.transform.Find("Nursery rack - replaceable placeholder");
            if (!rack) throw new Exception("Expected existing nursery rack.");
            foreach (Transform child in rack.Cast<Transform>().ToArray())
                if (!child.GetComponent<RackDirtPatch>()) Object.DestroyImmediate(child.gameObject);
            rack.name = TreeName;
            BuildTree(rack, 0, true);
            var crust = MakeCrust();
            for (int i = 0; i < workshop.patches.Length; i++)
            {
                var patch = workshop.patches[i];
                patch.transform.localPosition = new Vector3(-0.9625f + (i % 8) * 0.275f, 1.05f + (i / 8) * 1.05f + 0.038f, -0.55f);
                var visual = patch.dirtVisual;
                visual.GetComponent<MeshFilter>().sharedMesh = crust;
                visual.transform.localScale = new Vector3(0.26f, 0.038f, 0.09f);
            }
            // Keep an obvious place to find and return the existing interactive brush.
            Box("Tool rest", rack, new Vector3(0.68f, 1.35f, -0.78f), new Vector3(0.55f, 0.035f, 0.32f), pipe, true);
            Rod("Tool rest support", rack, new Vector3(0.7f, 1.02f, -0.55f), new Vector3(0.7f, 1.35f, -0.78f), 0.02f, pipe);
            Vector3 home = rack.TransformPoint(new Vector3(0.68f, 1.48f, -0.89f));
            workshop.brush.transform.SetPositionAndRotation(home, Quaternion.identity);
            workshop.brushHome.SetPositionAndRotation(home, Quaternion.identity);
            var body = workshop.brush.GetComponent<Rigidbody>(); body.position = home; body.rotation = Quaternion.identity;

            var root = new GameObject("NURSERY / suspended coral trees").transform;
            var seabed = Object.FindFirstObjectByType<ReefSeabed>();
            var ground = seabed.GetComponent<Collider>();
            var anchored = new List<Transform>(seabed.groundedProps);
            var sites = new[] { new Vector3(-3.5f, 0, -8), new Vector3(3.8f, 0, -8.7f), new Vector3(0.2f, 0, -1.6f) };
            for (int i = 0; i < sites.Length; i++)
            {
                var tree = new GameObject("Nursery tree " + (i + 2) + " / display").transform;
                tree.SetParent(root, false);
                var p = sites[i];
                if (ground.Raycast(new Ray(p + Vector3.up * 30, Vector3.down), out var hit, 60)) p.y = hit.point.y;
                tree.position = p;
                tree.rotation = Quaternion.Euler(0, new[] { -12, 18, -7 }[i], 0);
                tree.localScale = Vector3.one * new[] { 0.9f, 1.04f, 0.86f }[i];
                BuildTree(tree, i + 1, false); anchored.Add(tree);
            }
            seabed.groundedProps = anchored.ToArray();
        }

        static void BuildTree(Transform parent, int seed, bool interactive)
        {
            // Two slender stems, open tray frames and suspended fragments follow the supplied photos.
            foreach (float x in new[] { -0.09f, 0.09f })
                Rod("Central stem", parent, new Vector3(x, 0.62f, 0), new Vector3(x, 3.75f, 0), 0.025f, pipe, interactive);
            Box("Seabed anchor", parent, new Vector3(0, 0.08f, 0), new Vector3(0.38f, 0.16f, 0.30f), anchorMaterial, interactive);
            Rod("Lower tether", parent, new Vector3(0, 0.16f, 0), new Vector3(0, 0.8f, 0), 0.009f, rope);
            Rod("Buoy tether", parent, new Vector3(0, 3.65f, 0), new Vector3(0.06f, 4.8f, 0.04f), 0.01f, rope);
            for (int i = 0; i < 3; i++)
            {
                var buoy = Primitive(PrimitiveType.Sphere, "Buoyancy float", parent, floatMaterial, false);
                buoy.transform.localPosition = new Vector3((i - 1) * 0.21f, 4.75f + (i == 1 ? 0.13f : 0), 0.04f);
                buoy.transform.localScale = new Vector3(0.24f, 0.30f, 0.24f);
            }
            for (int level = 0; level < 3; level++)
            {
                float y = 1.05f + level * 1.05f;
                foreach (float z in new[] { -0.55f, 0.55f })
                    Rod("Tray rim / level " + (level + 1), parent, new Vector3(-1.1f, y, z), new Vector3(1.1f, y, z), 0.026f, pipe, interactive);
                foreach (float x in new[] { -1.1f, 1.1f })
                    Rod("Tray edge", parent, new Vector3(x, y, -0.55f), new Vector3(x, y, 0.55f), 0.022f, pipe, interactive);
                foreach (float x in new[] { -0.8f, -0.4f, 0, 0.4f, 0.8f })
                    Rod("Open tray lattice", parent, new Vector3(x, y - 0.007f, -0.55f), new Vector3(x, y - 0.007f, 0.55f), 0.009f, rope);
                Rod("Cross tie", parent, new Vector3(-1.1f, y, 0), new Vector3(1.1f, y, 0), 0.012f, pipe);
                foreach (float x in new[] { -0.94f, 0.94f })
                    Rod("Diagonal tray brace", parent, new Vector3(0, y + 0.32f, 0), new Vector3(x, y, 0), 0.013f, pipe);
                for (int fragment = 0; fragment < 7; fragment++)
                {
                    float x = -0.90f + fragment * 0.30f;
                    float z = fragment % 2 == 0 ? 0.30f : -0.12f;
                    float variation = Mathf.Sin(fragment * 3.1f + seed * 1.8f + level);
                    float drop = 0.17f + 0.10f * (variation + 1);
                    Rod("Fragment suspension line", parent, new Vector3(x, y, z), new Vector3(x, y - drop, z), 0.0028f, rope);
                    Fragment(parent, new Vector3(x, y - drop - 0.12f, z), 0.8f + 0.15f * variation, fragment * 39 + level * 11 + seed * 17);
                }
            }
        }

        static void Fragment(Transform parent, Vector3 position, float scale, float angle)
        {
            var root = new GameObject("Hanging staghorn fragment").transform;
            root.SetParent(parent, false); root.localPosition = position;
            root.localRotation = Quaternion.Euler(8, angle, -9); root.localScale = Vector3.one * scale;
            Rod("Main branch", root, Vector3.zero, new Vector3(0.01f, 0.21f, 0), 0.014f, coral);
            Rod("Branch", root, new Vector3(0, 0.05f, 0), new Vector3(-0.09f, 0.16f, 0.035f), 0.011f, coral);
            Rod("Branch", root, new Vector3(0, 0.10f, 0), new Vector3(0.10f, 0.19f, -0.045f), 0.010f, coral);
            Rod("Growing tip", root, new Vector3(0.01f, 0.18f, 0), new Vector3(0.01f, 0.22f, 0), 0.012f, coralTips);
        }

        static Mesh MakeCrust()
        {
            const string path = Generated + "NurseryAlgaeCrust.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (existing) return existing;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int ring = 0; ring <= 8; ring++)
                for (int side = 0; side < 8; side++)
                {
                    float a = side * Mathf.PI / 4;
                    float radius = 0.35f + 0.15f * Mathf.Sin(ring * 4.7f + side * 2.3f);
                    vertices.Add(new Vector3(ring / 8f - 0.5f, Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
                    if (ring == 0) continue;
                    int b = (ring - 1) * 8 + side, c = (ring - 1) * 8 + (side + 1) % 8;
                    triangles.AddRange(new[] { b, c, b + 8, c, c + 8, b + 8 });
                }
            var mesh = new Mesh { name = "Irregular algae coating" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        static Material Lit(string name, Color color)
        {
            string path = Generated + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", 0.12f); EditorUtility.SetDirty(mat); return mat;
        }
        static GameObject Primitive(PrimitiveType kind, string name, Transform parent, Material mat, bool solid)
        {
            var go = GameObject.CreatePrimitive(kind); go.name = name; go.transform.SetParent(parent, false);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        static void Box(string name, Transform parent, Vector3 p, Vector3 size, Material mat, bool solid)
        {
            var go = Primitive(PrimitiveType.Cube, name, parent, mat, solid);
            go.transform.localPosition = p; go.transform.localScale = size;
        }
        static void Rod(string name, Transform parent, Vector3 a, Vector3 b, float radius, Material mat, bool solid = false)
        {
            var go = Primitive(PrimitiveType.Cylinder, name, parent, mat, solid);
            go.transform.localPosition = (a + b) * 0.5f;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
            go.transform.localScale = new Vector3(radius * 2, Vector3.Distance(a, b) * 0.5f, radius * 2);
        }
    }
}
