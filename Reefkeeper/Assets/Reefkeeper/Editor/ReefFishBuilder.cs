using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reefkeeper.Editor
{
    public static class ReefFishBuilder
    {
        const string Source = "Assets/ThirdParty/QuaterniusAnimatedFish";
        const string Output = "Assets/Reefkeeper/Generated/AnimatedFish";
        static readonly string[] Species = { "Fish1", "Fish2", "Fish3", "Manta ray", "Dolphin", "Shark", "Whale" };

        public static void InstallAndBuild()
        {
            Directory.CreateDirectory(Output);
            AssetDatabase.Refresh();
            foreach (string species in Species) CreatePrefab(species);
            EditorSceneManager.OpenScene(ReefSceneBuilder.ScenePath);
            if (!GameObject.Find("REEF / Quaternius animated fish"))
            {
                var oldSchool = GameObject.Find("School of reef fish");
                if (oldSchool) oldSchool.SetActive(false);
                var root = new GameObject("REEF / Quaternius animated fish");
                var ground = GameObject.Find("Rippling sand seabed").GetComponent<Collider>();
                AddSchool(root.transform, ground, "Fish1", 10, new Vector3(-0.5f, 3.9f, -5), new Vector2(3.5f, 2), 0.18f, 0.55f, 0);
                AddSchool(root.transform, ground, "Fish2", 10, new Vector3(3, 4.8f, 3), new Vector2(4, 2.6f), 0.14f, 0.65f, 1.5f);
                AddSchool(root.transform, ground, "Fish3", 6, new Vector3(-4, 3.3f, -3), new Vector2(2.2f, 1.4f), 0.13f, 0.48f, 0.8f);
                AddSchool(root.transform, ground, "Manta ray", 1, new Vector3(0, 7.3f, 4), new Vector2(10, 6), 0.11f, 3.4f, 2.4f);
                root.AddComponent<ReefFishSmokeCheck>();
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("REEFKEEPER_ANIMATED_FISH_READY");
            ReefSceneBuilder.BuildWindows();
        }

        static void CreatePrefab(string species)
        {
            string path = Source + "/Models/" + species + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips) { clip.loopTime = true; clip.loopPose = true; }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var animation = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            if (!source || !animation) throw new Exception("Missing model or swimming clip: " + species);

            var wrapper = new GameObject(species);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
            visual.transform.SetParent(wrapper.transform, false);
            var bones = visual.GetComponentsInChildren<Transform>();
            var head = bones.FirstOrDefault(t => t.name == "Face") ?? bones.FirstOrDefault(t => t.name == "Spine1");
            var tail = bones.FirstOrDefault(t => t.name == "Tail");
            if (!head || !tail) throw new Exception("Cannot establish swimming direction: " + species);
            Vector3 forward = head.position - tail.position; forward.y = 0;
            visual.transform.rotation = Quaternion.FromToRotation(forward.normalized, Vector3.forward) * visual.transform.rotation;
            var renderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length == 0) throw new Exception("No skinned mesh: " + species);
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float scale = 1f / bounds.size.z;
            visual.transform.localScale *= scale;
            visual.transform.position -= bounds.center * scale;
            foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.updateWhenOffscreen = true;
                r.sharedMaterials = r.sharedMaterials.Select(m => ConvertMaterial(species, m)).ToArray();
            }
            string controllerPath = Output + "/" + species + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (!controller)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddMotion(animation);
                controller.layers[0].stateMachine.states[0].state.name = "Swim";
            }
            var animator = visual.GetComponent<Animator>();
            if (!animator) animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            PrefabUtility.SaveAsPrefabAsset(wrapper, Output + "/" + species + ".prefab");
            Debug.Log("REEF_FISH_IMPORTED " + species + " clip=" + animation.name + " duration=" + animation.length + " originalBounds=" + bounds.size + " forward=" + forward);
            UnityEngine.Object.DestroyImmediate(wrapper);
        }

        static Material ConvertMaterial(string species, Material original)
        {
            string path = Output + "/" + species + "-" + original.name + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result) return result;
            Color color = original.HasProperty("_Color") ? original.GetColor("_Color") : Color.white;
            result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = species + " " + original.name };
            result.SetColor("_BaseColor", color);
            result.SetFloat("_Smoothness", 0.22f);
            result.SetFloat("_Metallic", 0);
            AssetDatabase.CreateAsset(result, path);
            return result;
        }

        static void AddSchool(Transform parent, Collider ground, string species, int count, Vector3 center, Vector2 orbit, float speed, float length, float phase)
        {
            var go = new GameObject(species + " / ambient school");
            go.transform.SetParent(parent, false); go.transform.position = center;
            var school = go.AddComponent<ReefFishSchool>();
            school.orbit = orbit; school.angularSpeed = speed; school.phase = phase; school.seabed = ground;
            school.fish = new Transform[count];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Output + "/" + species + ".prefab");
            for (int i = 0; i < count; i++)
            {
                var fish = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                fish.transform.SetParent(go.transform, false);
                fish.transform.localScale = Vector3.one * length * (1 + Mathf.Sin(i * 2.1f) * 0.12f);
                school.fish[i] = fish.transform;
            }
            school.Evaluate(0);
        }
    }
}
