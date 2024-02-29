using System;
using System.Collections.Generic;
using System.IO;
using RapidIcon_1_6_2;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        private Button refreshInventoryButton;
        private Button clearInventoryButton;
        private TextField miniPath;
        private TextField neededIconPath;
        private Button openIconToolButton;

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

            //Miniature model field
            meshField = new ObjectField
            {
                label = "Model",
                tooltip = "The prefab model, containing both mesh and materials",
                objectType = typeof(GameObject),
                style = { marginLeft = 10, marginRight = 10 }
            };
            root.Add(meshField);

            //Category field
            categoryField = new DropdownField
            {
                label = "Category",
                choices = new List<string> { "Avatar", "Animal", "Nature", "Building", "Monster" }
            };
            categoryField.value = categoryField.choices[0];
            categoryField.style.marginLeft = 10;
            categoryField.style.marginRight = 10;
            root.Add(categoryField);

            Label finalizeLabel = new Label("Create Miniature")
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
            createButton.clicked += OnCreateClicked();
            root.Add(createButton);

            //Miniature path
            miniPath = new TextField
            {
                label = "Miniature path",
                value = "none...",
                style = 
                {
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5
                }
                
            };
            root.Add(miniPath);
            
            //Icon path
            neededIconPath = new TextField
            {
                label = "Icon path",
                value = "none...",
                style = 
                {
                    // marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5
                }
            };
            root.Add(neededIconPath);
            
            /*Label iconLabel = new Label("Icon")
            {
                style =
                {
                    fontSize = 16,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5
                }
            };
            root.Add(iconLabel);

            openIconToolButton = new Button
            {
                name = "button",
                text = "Open Icon Tool"
            };
            openIconToolButton.clicked += OnIconToolClicked();
            root.Add(openIconToolButton);*/
            
            Label inventoryLabel = new Label("Inventory")
            {
                style =
                {
                    fontSize = 16,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5
                }
            };
            root.Add(inventoryLabel);
            
            //Refresh miniature inventory
            refreshInventoryButton = new Button
            {
                name = "button",
                text = "Refresh all"
            };
            refreshInventoryButton.clicked += OnRefreshClicked();
            root.Add(refreshInventoryButton);

            clearInventoryButton = new Button
            {
                name = "button",
                text = "clear all"
            };
            clearInventoryButton.clicked += OnClearClicked();
            root.Add(clearInventoryButton);
        }

        #region Methods
        
        private System.Action OnCreateClicked()
        {
            return CreationButtonPressed;
        }
        
        private System.Action OnRefreshClicked()
        {
            return RefreshAll;
        }

        private System.Action OnClearClicked()
        {
            return ClearAll;
        }

        private void ClearAll()
        {
            MiniatureManager.Instance.ClearAllMiniatures();
        }

        private void CreationButtonPressed()
        {
            CreatePrefab();
            ResetUI();
        }

        private void RefreshAll()
        {
            if (!MiniatureManager.Instance)
            {
                Debug.LogError("No miniature manager! Are you in the main scene?");
            }
            
            MiniatureManager.Instance.RefreshAllMiniatures();
        }

        void ResetUI()
        {
            nameField.value = "Enter name here...";
            meshField.value = null;
            categoryField.value = categoryField.choices[0];
        }

        private void CreatePrefab()
        {
            const string rootPath = "Assets/Prefabs/Miniatures/";
            string categoryPath = categoryField.value + "/";
            string localPath;

            GameObject prefabObject = new GameObject();

            //Let the new prefab to inherit the original prefab's scale
            GameObject originalPrefab = meshField.value as GameObject;
            if (originalPrefab == null)
            {
                Debug.LogError("No original prefab selected!");
                return;
            }
            GameObject prefabInstance = Instantiate(originalPrefab);
            Vector3 originalScale = prefabInstance.transform.localScale;
            DestroyImmediate(prefabInstance);
            prefabObject.transform.localScale = originalScale;

            prefabObject = BuildPrefab(prefabObject, originalPrefab);

            if (!Directory.Exists(rootPath))
                Debug.LogError("Failed to create asset! Are we missing a folder?");

            localPath = rootPath + categoryPath + nameField.value + ".prefab";
            
            miniPath.value = localPath;
            neededIconPath.value = "Assets/UI_Assets/Icons/" + categoryPath;

            PrefabUtility.SaveAsPrefabAsset(prefabObject, localPath);

            DestroyImmediate(prefabObject);
        }

        private GameObject BuildPrefab(GameObject prefabObject, GameObject originalPrefab)
        {
            //Add and save components
            MeshFilter meshFilterComponent = prefabObject.AddComponent<MeshFilter>();
            MeshRenderer meshRendererComponent = prefabObject.AddComponent<MeshRenderer>();
            Rigidbody rigidbodyComponent = prefabObject.AddComponent<Rigidbody>();
            // MeshCollider meshColliderComponent = prefabObject.AddComponent<MeshCollider>();
            Outline outlineComponent = prefabObject.AddComponent<Outline>();
            prefabObject.AddComponent<ObjectController>();

            //Add movement controller to avatars
            if (categoryField.value == categoryField.choices[0])
            {
                PlayerMovementController playerMovementController = prefabObject.AddComponent<PlayerMovementController>();
                playerMovementController.enabled = false;

                GameObject orientationObject = new GameObject();
                orientationObject.name = "Orientation";
                orientationObject.transform.parent = prefabObject.transform;
            }

            //Setup all component values
            meshFilterComponent.sharedMesh = meshField.value.GetComponent<MeshFilter>().sharedMesh;
            meshRendererComponent.materials = meshField.value.GetComponent<MeshRenderer>().sharedMaterials;
            outlineComponent.enabled = false;

            //Copy colliders
            CopyColliders(originalPrefab, prefabObject);

            //If no colliders were copied, add a convex mesh collider
            if (prefabObject.GetComponents<Collider>().Length < 1)
            {
                MeshCollider collider = prefabObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilterComponent.sharedMesh;
                collider.convex = true;
            }

            //Setup tags and layers
            prefabObject.tag = "Interactable";
            prefabObject.layer = LayerMask.NameToLayer("Objects");

            return prefabObject;
        }

        private void CopyColliders(GameObject source, GameObject destination)
        {
            // Copy BoxCollider
            foreach (var originalCollider in source.GetComponents<BoxCollider>())
            {
                BoxCollider collider = destination.AddComponent<BoxCollider>();
                collider.center = originalCollider.center;
                collider.size = originalCollider.size;
                collider.isTrigger = originalCollider.isTrigger;
            }

            // Copy SphereCollider
            foreach (var originalCollider in source.GetComponents<SphereCollider>())
            {
                SphereCollider collider = destination.AddComponent<SphereCollider>();
                collider.center = originalCollider.center;
                collider.radius = originalCollider.radius;
                collider.isTrigger = originalCollider.isTrigger;
            }

            // Copy CapsuleCollider
            foreach (var originalCollider in source.GetComponents<CapsuleCollider>())
            {
                CapsuleCollider collider = destination.AddComponent<CapsuleCollider>();
                collider.center = originalCollider.center;
                collider.radius = originalCollider.radius;
                collider.height = originalCollider.height;
                collider.direction = originalCollider.direction; // 0 = X, 1 = Y, 2 = Z
                collider.isTrigger = originalCollider.isTrigger;
            }

            // Copy MeshCollider
            foreach (var originalCollider in source.GetComponents<MeshCollider>())
            {
                MeshCollider collider = destination.AddComponent<MeshCollider>();
                collider.sharedMesh = originalCollider.sharedMesh;
                //don't want to do boring repetitive work :^)
                collider.convex = true;
                collider.isTrigger = originalCollider.isTrigger;
            }
        }
        #endregion
    }
}
