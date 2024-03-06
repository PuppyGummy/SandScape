using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace EditorUI
{
    public class ClothesCreator : EditorWindow
    {
        [MenuItem("SandScape Tools/Clothes Creator")]
        public static void ShowExample()
        {
            ClothesCreator wnd = GetWindow<ClothesCreator>();
            wnd.titleContent = new GUIContent("Clothes Creator");
            wnd.maxSize = new Vector2(380.0f, 160.0f);
            wnd.minSize = wnd.maxSize;
        }

        #region Fields

        private ObjectField thinMeshField;
        private ObjectField fitMeshField;
        private ObjectField fatMeshField;
        private Button createButton;
        private DropdownField categoryField;
        private TextField nameField;
        private Button refreshInventoryButton;
        private Button clearInventoryButton;

        #endregion

        #region UI

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            
            // VisualElements objects can contain other VisualElement following a tree hierarchy
            Label mainLabel = new Label("Clothes Creator")
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
            
            //Item name field
            nameField = new TextField
            {
                label = "Item name",
                value = "Enter name here...",
                style =
                {
                    marginLeft = 10,
                    marginRight = 10
                }
            };
            root.Add(nameField);
            
            //Model fields
            thinMeshField = new ObjectField
            {
                label = "Thin Model",
                tooltip = "The prefab model, containing both mesh and materials",
                objectType = typeof(GameObject),
                style = { marginLeft = 10, marginRight = 10 },
                allowSceneObjects = false
            };
            root.Add(thinMeshField);
            
            fitMeshField = new ObjectField
            {
                label = "Fit Model",
                tooltip = "The prefab model, containing both mesh and materials",
                objectType = typeof(GameObject),
                style = { marginLeft = 10, marginRight = 10 }
            };
            root.Add(fitMeshField);
            
            fatMeshField = new ObjectField
            {
                label = "Fat Model",
                tooltip = "The prefab model, containing both mesh and materials",
                objectType = typeof(GameObject),
                style = { marginLeft = 10, marginRight = 10 }
            };
            root.Add(fatMeshField);
            
            //Only allow meshes to be uploaded
            thinMeshField.objectType = typeof(Mesh);
            fitMeshField.objectType = typeof(Mesh);
            fatMeshField.objectType = typeof(Mesh);
            
            //Category field
            categoryField = new DropdownField
            {
                label = "Category",
                choices = new List<string> { "Hair", "Top", "Bottom", "Shoe"}
            };
            categoryField.value = categoryField.choices[0];
            categoryField.style.marginLeft = 10;
            categoryField.style.marginRight = 10;
            root.Add(categoryField);
            
            //Finalize section
            Label finalizeLabel = new Label("Create Item")
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

        #endregion

        #region Methods

        private Action OnCreateClicked()
        {
            return CreateClothPrefab;
        }
        
        private Action OnRefreshClicked()
        {
            return RefreshItems;
        }
        
        private Action OnClearClicked()
        {
            return ClearItems;
        }

        private void CreateClothPrefab()
        {
            CreatePrefab();
            ResetUI();
        }

        private void ClearItems()
        {
            GetItemManager().ClearList();
        }

        private void RefreshItems()
        {
            GetItemManager().RefreshList();
        }

        private static CustomizationItemManager GetItemManager()
        {
            if(CustomizationItemManager.Instance != null)
            {
                return CustomizationItemManager.Instance;
            }

            Debug.LogError("No Customization manager! Are you in the right scene?");
            return null;
        }

        private void CreatePrefab()
        {
            const string rootPath = "Assets/Prefabs/Clothes/";
            string categoryPath = categoryField.value + "/";
            string localPath;
            
            GameObject prefabObject = new GameObject();
            
            if (!Directory.Exists(rootPath))
                Debug.LogError("Failed to create asset! Are we missing a folder?");

            localPath = rootPath + categoryPath + nameField.value + ".prefab";
            
            prefabObject = BuildPrefab(prefabObject);
            
            if (!Directory.Exists(rootPath))
                Debug.LogError("Failed to create asset! Are we missing a folder?");

            localPath = rootPath + categoryPath + nameField.value + ".prefab";
            
            PrefabUtility.SaveAsPrefabAsset(prefabObject, localPath);

            DestroyImmediate(prefabObject);
        }

        private GameObject BuildPrefab(GameObject prefabObject)
        {
            ClothingItem clothingItem = prefabObject.AddComponent<ClothingItem>();
            clothingItem.itemVariants = new List<Mesh>
            {
                (Mesh)thinMeshField.value,
                (Mesh)fitMeshField.value,
                (Mesh)fatMeshField.value
            };

            return prefabObject;
            //TODO: Destroy object
        }

        void ResetUI()
        {
            nameField.value = "None...";
            thinMeshField.value = null;
            fitMeshField.value = null;
            fatMeshField.value = null;
        }

        #endregion
    }
}