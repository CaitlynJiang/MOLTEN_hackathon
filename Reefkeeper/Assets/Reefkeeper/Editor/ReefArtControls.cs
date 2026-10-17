using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Reefkeeper.Editor
{
    public sealed class ReefArtControls : EditorWindow
    {
        const string Generated = "Assets/Reefkeeper/Generated/";
        Vector2 scroll;
        UnityEditor.Editor materialEditor;
        Material selectedMaterial;
        readonly string[] materialNames = { "Sand", "Limestone", "FireCoral", "LavenderCoral", "PlateCoral", "Water", "UnderwaterDistortion" };
        readonly string[] materialLabels = { "沙地 Sand", "岩石 Rock", "枝状珊瑚", "管状海绵", "桌形珊瑚", "水面底部", "水下折射" };

        [MenuItem("Reefkeeper/Art controls - 美术调节")]
        public static void Open() => GetWindow<ReefArtControls>("Reef art / 美术");
        void OnEnable() { Undo.undoRedoPerformed += RefreshTerrainCollider; }
        void OnDisable()
        {
            Undo.undoRedoPerformed -= RefreshTerrainCollider;
            if (materialEditor) DestroyImmediate(materialEditor);
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox("在停止 Play 的状态下调节。颜色即时预览；地形按按钮应用。Ctrl+S 保存场景。材质属于共享资源，修改会影响所有使用它的物体。", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                EditorGUILayout.LabelField("水体与光线", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                Color fog = EditorGUILayout.ColorField("远处水色 / Fog", RenderSettings.fogColor);
                float visibility = EditorGUILayout.Slider("能见度 / metres", RenderSettings.fogEndDistance, 18, 90);
                if (EditorGUI.EndChangeCheck())
                {
                    var settings = Resources.FindObjectsOfTypeAll<RenderSettings>();
                    if (settings.Length > 0) Undo.RecordObject(settings[0], "Change water atmosphere");
                    RenderSettings.fogColor = fog;
                    RenderSettings.fogEndDistance = visibility;
                    foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                    {
                        Undo.RecordObject(camera, "Change water background"); camera.backgroundColor = fog;
                    }
                    MarkScene();
                }
                var sun = RenderSettings.sun;
                if (sun)
                {
                    EditorGUI.BeginChangeCheck();
                    Color color = EditorGUILayout.ColorField("日光颜色", sun.color);
                    float strength = EditorGUILayout.Slider("日光强度", sun.intensity, 0, 3);
                    if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(sun, "Change sunlight"); sun.color = color; sun.intensity = strength; MarkScene(); }
                }
                var volume = Object.FindFirstObjectByType<Volume>();
                if (volume && volume.sharedProfile && volume.sharedProfile.TryGet<ColorAdjustments>(out var grading))
                {
                    EditorGUI.BeginChangeCheck();
                    float exposure = EditorGUILayout.Slider("曝光 / stops", grading.postExposure.value, -2, 2);
                    float saturation = EditorGUILayout.Slider("饱和度", grading.saturation.value, -70, 30);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(grading, "Change color grade");
                        grading.postExposure.Override(exposure); grading.saturation.Override(saturation);
                        EditorUtility.SetDirty(grading); SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("海床起伏", EditorStyles.boldLabel);
                var seabed = Object.FindFirstObjectByType<ReefSeabed>();
                if (seabed)
                {
                    var serialized = new SerializedObject(seabed);
                    serialized.Update();
                    EditorGUILayout.PropertyField(serialized.FindProperty("bankHeight"), new GUIContent("两侧坡地高度"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("duneHeight"), new GUIContent("沙丘起伏"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("channelDepth"), new GUIContent("中央浅沟深度"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("detailHeight"), new GUIContent("细小起伏"));
                    serialized.ApplyModifiedProperties();
                    if (GUILayout.Button("应用地形 · 保留物件与材质")) ApplyTerrain(seabed);
                    if (GUILayout.Button("选中海床 / 管理随地形移动的物件")) Selection.activeGameObject = seabed.gameObject;
                }
                else EditorGUILayout.HelpBox("请打开 ShallowReef 场景。", MessageType.None);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("材质 · 颜色 / 贴图 / 焦散", EditorStyles.boldLabel);
                for (int i = 0; i < materialNames.Length; i++)
                    if (GUILayout.Button(materialLabels[i])) SelectMaterial(materialNames[i]);
                if (materialEditor)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Tint = 材质颜色；Main Texture = 贴图；Caustic Color = 水纹亮度。先选场景物件再替换 Mesh / Material，可单独更换 asset。", MessageType.None);
                    materialEditor.OnInspectorGUI();
                }
                if (GUILayout.Button("保存材质与场景"))
                {
                    AssetDatabase.SaveAssets();
                    EditorSceneManager.SaveOpenScenes();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void SelectMaterial(string name)
        {
            selectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Generated + name + ".mat");
            if (materialEditor) DestroyImmediate(materialEditor);
            materialEditor = UnityEditor.Editor.CreateEditor(selectedMaterial);
            EditorGUIUtility.PingObject(selectedMaterial);
        }
        static void MarkScene()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            SceneView.RepaintAll();
        }
        static void RefreshTerrainCollider()
        {
            var seabed = Object.FindFirstObjectByType<ReefSeabed>();
            if (!seabed) return;
            var collider = seabed.GetComponent<MeshCollider>();
            collider.sharedMesh = null;
            collider.sharedMesh = seabed.GetComponent<MeshFilter>().sharedMesh;
            Physics.SyncTransforms();
            SceneView.RepaintAll();
        }

        public static void ApplyTerrain(ReefSeabed seabed)
        {
            var collider = seabed.GetComponent<MeshCollider>();
            var mesh = seabed.GetComponent<MeshFilter>().sharedMesh;
            var offsets = new Dictionary<Transform, float>();
            // Sample the old collider before replacing it; repeated edits preserve prop placement.
            Physics.SyncTransforms();
            foreach (var prop in seabed.groundedProps)
                if (prop && !offsets.ContainsKey(prop) && collider.Raycast(new Ray(prop.position + Vector3.up * 100, Vector3.down), out var hit, 200))
                    offsets.Add(prop, prop.position.y - hit.point.y);
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Reshape reef seabed");
            Undo.RegisterCompleteObjectUndo(mesh, "Reshape reef seabed");
            var generated = seabed.BuildMesh();
            EditorUtility.CopySerialized(generated, mesh);
            DestroyImmediate(generated);
            EditorUtility.SetDirty(mesh);
            RefreshTerrainCollider();
            foreach (var pair in offsets)
            {
                var p = pair.Key.position;
                if (!collider.Raycast(new Ray(p + Vector3.up * 100, Vector3.down), out var hit, 200)) continue;
                Undo.RecordObject(pair.Key, "Move reef prop with seabed");
                p.y = hit.point.y + pair.Value;
                pair.Key.position = p;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Physics.SyncTransforms();
            MarkScene();
        }

        // One-time upgrade of the existing scene, preserving object identities and custom placements.
        public static void UpgradeTerrainAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ReefSceneBuilder.ScenePath);
            var ground = GameObject.Find("Rippling sand seabed");
            var seabed = ground.GetComponent<ReefSeabed>();
            if (!seabed)
            {
                seabed = ground.AddComponent<ReefSeabed>();
                var anchors = new List<Transform>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "REEF / corals and seagrass")
                        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>()) anchors.Add(renderer.transform);
                    if (root.name == "REEF / environment")
                        foreach (Transform child in root.transform)
                            if (child.name == "Reef limestone") anchors.Add(child);
                }
                seabed.groundedProps = anchors.ToArray();
                ApplyTerrain(seabed);
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("REEFKEEPER_TERRAIN_READY height range: " + ground.GetComponent<MeshFilter>().sharedMesh.bounds.size.y);
            ReefSceneBuilder.BuildWindows();
        }
    }
}
