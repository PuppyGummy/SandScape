using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EditorUI
{
    public class MiniatureCreator : EditorWindow
    {
        [MenuItem("SandScape Tools/Miniature Creator")]
        public static void ShowExample()
        {
            MiniatureCreator wnd = GetWindow<MiniatureCreator>();
            wnd.titleContent = new GUIContent("Miniature Creator");
            wnd.maxSize = new Vector2(380.0f, 160.0f);
            wnd.minSize = wnd.maxSize;
        }

        private TextField nameField;
        private ObjectField meshField;
        private DropdownField categoryField;
        private Button createButton;
        private readonly Color outlineColor = new(78.0f, 186.0f, 204.0f, 255.0f);

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy
            Label mainLabel = new Label("Miniature Creator")
            {
                style =
                {
                    fontSize = 24,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            root.Add(mainLabel);

            //Miniature name field
            nameField = new TextField
            {
                label = "Miniature name",
                value = "Enter name here...",
                style =
                {
                    marginLeft = 10,
                    marginRight = 10
                }
            };
            root.Add(nameField);

            //TODO: Change from mesh to prefab.
            //Miniature model field
            meshField = new ObjectField
            {
                label = "Model",
                tooltip = "The prefab model, containing both mesh and materials",
                objectType = typeof(GameObject),
                style = { marginLeft =  10, marginRight = 10}
            };
            root.Add(meshField);
        
            //Category field
            categoryField = new DropdownField
            {
                label = "Category",
                choices = new List<string>{"Avatar", "Animal", "Nature", "Building", "Monster"}
            };
            categoryField.value = categoryField.choices[0];
            categoryField.style.marginLeft = 10;
            categoryField.style.marginRight = 10;
            root.Add(categoryField);

            Label finalizeLabel = new Label("Finalize")
            {
                style =
                {
                    fontSize = 16,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5
                }
            };
            root.Add(finalizeLabel);
        
            //Create miniature button
            createButton = new Button
            {
                name = "button",
                text = "Create!"
            };
            createButton.clicked += OnClick();
            root.Add(createButton);
        }

        private Action OnClick()
        {
            return CreationButtonPressed;
        }

        private void CreationButtonPressed()
        {
            CreatePrefab();
            ResetUI();
        }

        void ResetUI()
        {
            nameField.value = "Enter name here...";
            meshField.value = null;
            categoryField.value = categoryField.choices[0];
        }

        private void CreatePrefab()
        {
            string rootPath = "Assets/Prefabs/Miniatures/";
            string categoryPath = categoryField.value + "/";
            string localPath;
            
            GameObject prefabObject = new GameObject();
            prefabObject = BuildPrefab(prefabObject);
            
            if (!Directory.Exists(rootPath))
                Debug.LogError("Failed to create asset! Are we missing a folder?");

            localPath = rootPath + categoryPath + nameField.value + ".prefab";

            PrefabUtility.SaveAsPrefabAsset(prefabObject, localPath);

            DestroyImmediate(prefabObject);
        }

        private GameObject BuildPrefab(GameObject prefabObject)
        {
            MeshFilter meshFilterComponent = prefabObject.AddComponent<MeshFilter>();
            MeshRenderer meshRendererComponent = prefabObject.AddComponent<MeshRenderer>();
            Rigidbody rigidbodyComponent = prefabObject.AddComponent<Rigidbody>();
            MeshCollider meshColliderComponent = prefabObject.AddComponent<MeshCollider>();
            Outline outlineComponent = prefabObject.AddComponent<Outline>();
            prefabObject.AddComponent<ObjectController>();

            //TODO: Mesh filter and renderer settings should be copied from prefab, not model
            meshFilterComponent.sharedMesh = meshField.value.GetComponent<MeshFilter>().sharedMesh;
            meshRendererComponent.materials = meshField.value.GetComponent<MeshRenderer>().sharedMaterials;
            meshColliderComponent.convex = true;
            meshColliderComponent.sharedMesh = meshFilterComponent.sharedMesh;
            outlineComponent.enabled = false;

            prefabObject.tag = "Interactable";
            prefabObject.layer = LayerMask.NameToLayer("Objects");

            return prefabObject;
        }
    }
}