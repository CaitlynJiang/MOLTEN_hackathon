using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Object = UnityEngine.Object;

namespace Reefkeeper.Editor
{
    public static class ReefPickupBuilder
    {
        public static void InstallAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ReefSceneBuilder.ScenePath);
            var workshop = Object.FindFirstObjectByType<ReefWorkshop>();
            var pickup = workshop.GetComponent<ReefCoralPickup>();
            if (!pickup) pickup = workshop.gameObject.AddComponent<ReefCoralPickup>();
            pickup.workshop = workshop;
            var manager = workshop.GetComponent<XRInteractionManager>();
            if (!manager) manager = workshop.gameObject.AddComponent<XRInteractionManager>();
            // Both camera modes use the same manager, which must survive disabling the XR group.
            foreach (var interactor in Object.FindObjectsByType<XRBaseInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                interactor.interactionManager = manager;
            foreach (var interactable in Object.FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                interactable.interactionManager = manager;
            foreach (var old in Object.FindObjectsByType<XRInteractionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (old != manager) Object.DestroyImmediate(old);
            if (!pickup.hand)
            {
                var slot = new GameObject("Single coral hand slot"); slot.transform.SetParent(workshop.transform, false);
                pickup.hand = slot.AddComponent<ReefPickupInteractor>();
                pickup.hand.attachTransform = slot.transform;
            }
            pickup.hand.interactionManager = manager;
            pickup.hand.keepSelectedTargetValid = true;
            const string path = "Assets/Reefkeeper/Generated/CoralPickupOutline.mat";
            var outline = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!outline)
            {
                outline = new Material(Shader.Find("Reefkeeper/Pickup Outline"));
                AssetDatabase.CreateAsset(outline, path);
            }
            var fragments = new List<ReefCoralFragment>();
            foreach (var root in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (root.name != "Hanging staghorn fragment") continue;
                var body = root.GetComponent<Rigidbody>();
                if (!body) body = root.gameObject.AddComponent<Rigidbody>();
                body.useGravity = false; body.isKinematic = true; body.mass = 0.04f;
                var collider = root.GetComponent<BoxCollider>();
                if (!collider) collider = root.gameObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0, 0.11f, 0); collider.size = new Vector3(0.25f, 0.26f, 0.14f); collider.isTrigger = false;
                var fragment = root.GetComponent<ReefCoralFragment>();
                if (!fragment) fragment = root.gameObject.AddComponent<ReefCoralFragment>();
                fragment.owner = pickup; fragment.outlineMaterial = outline; fragment.interactionManager = manager;
                fragment.movementType = XRBaseInteractable.MovementType.Instantaneous;
                fragment.throwOnDetach = false; fragment.useDynamicAttach = false; fragment.attachEaseInTime = 0.12f;
                fragment.selectMode = InteractableSelectMode.Single;
                fragment.colliders.Clear(); fragment.colliders.Add(collider);
                if (!fragment.attachTransform)
                {
                    var grip = new GameObject("Coral grip pose").transform;
                    grip.SetParent(root, false); grip.localPosition = new Vector3(0, 0.09f, 0);
                    fragment.attachTransform = grip;
                }
                int previous = root.GetSiblingIndex() - 1;
                if (previous >= 0 && root.parent.GetChild(previous).name == "Fragment suspension line")
                    fragment.suspensionLine = root.parent.GetChild(previous).gameObject;
                fragments.Add(fragment);
            }
            if (fragments.Count != 84) throw new Exception("Expected 84 nursery fragments; found " + fragments.Count);
            pickup.fragments = fragments.ToArray();
            if (!workshop.GetComponent<ReefDisplayControls>()) workshop.gameObject.AddComponent<ReefDisplayControls>();
            if (!workshop.GetComponent<ReefPickupSmokeCheck>()) workshop.gameObject.AddComponent<ReefPickupSmokeCheck>();
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("REEFKEEPER_PICKUP_READY: 84 grab interactables, outline, single held coral, fullscreen toggle");
            ReefSceneBuilder.BuildWindows();
        }
    }
}
