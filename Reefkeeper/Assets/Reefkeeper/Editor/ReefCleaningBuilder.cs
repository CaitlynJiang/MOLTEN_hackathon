using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Object = UnityEngine.Object;

namespace Reefkeeper.Editor
{
    public static class ReefCleaningBuilder
    {
        const string Generated = "Assets/Reefkeeper/Generated/";
        const string Maps = "Assets/ThirdParty/PolyHavenMossWood/";
        public static void InstallAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ReefSceneBuilder.ScenePath);
            var workshop = Object.FindFirstObjectByType<ReefWorkshop>();
            var pickup = workshop.GetComponent<ReefBrushPickup>();
            if (!pickup) pickup = workshop.gameObject.AddComponent<ReefBrushPickup>();
            pickup.workshop = workshop; pickup.corals = workshop.GetComponent<ReefCoralPickup>();
            var oldGrab = workshop.brush.GetComponent<XRGrabInteractable>();
            var attach = oldGrab.attachTransform;
            var grab = oldGrab as ReefBrushInteractable;
            if (!grab)
            {
                var tool = workshop.brush.gameObject; var bristles = workshop.brush.bristles; float radius = workshop.brush.contactRadius;
                Object.DestroyImmediate(workshop.brush); Object.DestroyImmediate(oldGrab);
                grab = tool.AddComponent<ReefBrushInteractable>(); workshop.brush = tool.AddComponent<ReefBrush>();
                workshop.brush.bristles = bristles; workshop.brush.contactRadius = radius;
            }
            grab.owner = pickup;
            grab.outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(Generated + "CoralPickupOutline.mat");
            grab.interactionManager = workshop.GetComponent<XRInteractionManager>();
            grab.attachTransform = attach; grab.throwOnDetach = false; grab.useDynamicAttach = false;
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous; grab.attachEaseInTime = 0.12f;
            grab.colliders.Clear(); grab.colliders.AddRange(grab.GetComponentsInChildren<Collider>());
            foreach (var collider in grab.colliders) collider.isTrigger = false;
            pickup.brush = grab;
            if (!pickup.hand)
            {
                var hand = new GameObject("Persistent brush hand"); hand.transform.SetParent(workshop.transform, false);
                pickup.hand = hand.AddComponent<ReefPickupInteractor>(); pickup.hand.attachTransform = hand.transform;
            }
            pickup.hand.interactionManager = grab.interactionManager; pickup.hand.keepSelectedTargetValid = true;
            // One practice area on the middle front rim; keep the surrounding nursery intact.
            var patch = workshop.patches[0];
            foreach (var other in workshop.patches.Skip(1)) Object.DestroyImmediate(other.gameObject);
            patch.name = "Practice spot / attached algae";
            patch.transform.localPosition = new Vector3(-0.28f, 2.138f, -0.55f);
            patch.transform.localRotation = Quaternion.identity; patch.requiredTravel = 2.8f;
            var contact = patch.GetComponent<BoxCollider>(); contact.isTrigger = true;
            contact.center = new Vector3(0, 0.025f, -0.012f); contact.size = new Vector3(0.72f, 0.095f, 0.19f);
            var renderer = patch.dirtVisual;
            renderer.transform.localScale = new Vector3(0.72f, 0.085f, 0.18f);
            renderer.GetComponent<MeshFilter>().sharedMesh = CrustMesh(); renderer.sharedMaterial = AlgaeMaterial();
            foreach (var old in patch.growth) if (old) Object.DestroyImmediate(old.gameObject);
            var growth = new List<Transform>(); var random = new System.Random(1017);
            var frondMesh = FrondMesh(); var frondMaterial = Material("AlgaeFronds", new Color(0.27f, 0.34f, 0.095f));
            frondMaterial.SetFloat("_Cull", 0); EditorUtility.SetDirty(frondMaterial);
            for (int i = 0; i < 22; i++)
            {
                var tuft = new GameObject("Attached algae tuft " + i); tuft.transform.SetParent(patch.transform, false);
                tuft.transform.localPosition = new Vector3((float)random.NextDouble() * 0.64f - 0.32f, 0.018f, (float)random.NextDouble() * 0.09f - 0.045f);
                tuft.transform.localRotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
                tuft.transform.localScale = Vector3.one * (0.7f + (float)random.NextDouble() * 0.7f);
                tuft.AddComponent<MeshFilter>().sharedMesh = frondMesh; tuft.AddComponent<MeshRenderer>().sharedMaterial = frondMaterial;
                growth.Add(tuft.transform);
            }
            patch.growth = growth.ToArray(); workshop.patches = new[] { patch }; pickup.practiceSpot = patch;
            var hud = workshop.GetComponent<ReefCleaningHUD>(); if (!hud) hud = workshop.gameObject.AddComponent<ReefCleaningHUD>(); hud.pickup = pickup;
            if (!workshop.GetComponent<ReefCleaningSmokeCheck>()) workshop.gameObject.AddComponent<ReefCleaningSmokeCheck>();
            var audio = workshop.GetComponent<ReefSoundscape>(); if (!audio) audio = workshop.gameObject.AddComponent<ReefSoundscape>();
            audio.ambience = Audio("Assets/Audio/Ambience/amb_underwater_loop.wav");
            audio.music = Audio("Assets/Audio/Music/bgm_dream_culture.mp3");
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("REEFKEEPER_CLEANING_READY: one algae practice spot, persistent brush and scrub animation");
            ReefSceneBuilder.BuildWindows();
        }
        static AudioClip Audio(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) throw new Exception("Audio asset missing: " + path);
            var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.Streaming;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        static Material AlgaeMaterial()
        {
            var normal = AssetImporter.GetAtPath(Maps + "moss_wood_nor_gl_1k.jpg") as TextureImporter;
            if (!normal) throw new Exception("Moss Wood normal texture missing");
            normal.textureType = TextureImporterType.NormalMap; normal.maxTextureSize = 1024; normal.SaveAndReimport();
            var material = Material("AttachedAlgae", new Color(0.8f, 0.9f, 0.55f));
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Maps + "moss_wood_diff_1k.jpg"));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Maps + "moss_wood_nor_gl_1k.jpg"));
            material.EnableKeyword("_NORMALMAP"); material.SetFloat("_BumpScale", 0.6f);
            material.SetFloat("_Cull", 0); EditorUtility.SetDirty(material); return material;
        }
        static Material Material(string name, Color color)
        {
            string path = Generated + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(material, path); }
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.16f); EditorUtility.SetDirty(material); return material;
        }
        static Mesh CrustMesh()
        {
            string path = Generated + "TexturedAlgaeCrust.asset"; var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh) return mesh;
            mesh = Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mesh>(Generated + "NurseryAlgaeCrust.asset")); mesh.name = "Textured attached algae";
            mesh.uv = mesh.vertices.Select(v => new Vector2(v.x + 0.5f, v.z + 0.5f)).ToArray(); mesh.RecalculateTangents(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        static Mesh FrondMesh()
        {
            string path = Generated + "AlgaeTuft.asset"; var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh) return mesh;
            var vertices = new List<Vector3>(); var triangles = new List<int>(); var uv = new List<Vector2>();
            for (int blade = 0; blade < 5; blade++)
            {
                var rotation = Quaternion.Euler(0, blade * 137, 0); int start = vertices.Count;
                for (int row = 0; row < 5; row++)
                {
                    float t = row / 4f, width = Mathf.Lerp(0.013f, 0.001f, t);
                    var center = new Vector3(Mathf.Sin(t * 4 + blade) * 0.018f, t * (0.075f + blade * 0.009f), t * t * 0.045f);
                    vertices.Add(rotation * (center + Vector3.left * width)); vertices.Add(rotation * (center + Vector3.right * width));
                    uv.Add(new Vector2(0, t)); uv.Add(new Vector2(1, t));
                    if (row == 0) continue;
                    int n = start + row * 2; triangles.AddRange(new[] { n - 2, n, n - 1, n - 1, n, n + 1 });
                }
            }
            mesh = new Mesh { name = "Short attached algae fronds" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
    }
}
