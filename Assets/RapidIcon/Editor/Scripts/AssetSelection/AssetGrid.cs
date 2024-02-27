using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	//---ObjectPathPair Definition---//
	public struct ObjectPathPair
	{
		public ObjectPathPair(UnityEngine.Object obj, string pth)
		{
			UnityEngine_object = obj;
			path = pth;
		}

		public UnityEngine.Object UnityEngine_object;
		public string path;
	};

	[Serializable]
	public class AssetGrid
	{
		//---PUBLIC---//
		//Icons
		public Dictionary<UnityEngine.Object, Icon> objectIcons;
		public Dictionary<string, Icon> sortedIconsByPath;
		public List<Icon> visibleIcons;
		public List<Icon> selectedIcons;

		//Other
		public string rapidIconRootFolder;
		public bool assetGridFocused;

		//---INTERNAL---//
		//GUI
		Vector2 scrollPosition;
		GUIStyle gridStyle, gridLabelStyle;
		int previewSize;

		//Textures
		Texture2D[] assetSelectionTextures;

		//Selection
		int lastSelectedIconIndex;
		int selectionMinIndex, selectionMaxIndex;
		string lastSelectedIndividualFolder;
		List<ObjectPathPair> objectsLoadedFromSelectedFolders;

		//RapidIcon Window Elements
		RapidIconWindow window;
		AssetList assetList;

		//Filter
		int filterIdx;
		string[] filters = new string[] { "t:model t:prefab", "t:prefab", "t:model" };
		string[] filterNames = new string[] { "Prefabs & Models", "Prefabs Only", "Models Only" };

		//Other
		Rect rect;
		bool iconsRefreshed;

		public AssetGrid(AssetList assets)
		{
			//---Initialise AssetGrid---//
			//Asset List
			assetList = assets;

			//Selection
			objectsLoadedFromSelectedFolders = new List<ObjectPathPair>();
			lastSelectedIconIndex = -1;
			selectionMinIndex = int.MaxValue;
			selectionMaxIndex = -1;

			//Icons
			objectIcons = new Dictionary<UnityEngine.Object, Icon>();
			sortedIconsByPath = new Dictionary<string, Icon>();
			selectedIcons = new List<Icon>();

			//Styles
			previewSize = 128;
			gridStyle = new GUIStyle();
			gridStyle.fixedHeight = previewSize;
			gridStyle.fixedWidth = previewSize;
			gridStyle.margin.bottom = 16 + (int)EditorGUIUtility.singleLineHeight + 2;
			gridStyle.margin.left = 16;
			gridStyle.alignment = TextAnchor.MiddleCenter;
			gridLabelStyle = new GUIStyle(gridStyle);
			gridLabelStyle.margin.bottom = 16 + previewSize + 2;
			gridLabelStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
			gridLabelStyle.alignment = TextAnchor.MiddleCenter;
			if (EditorGUIUtility.isProSkin)
				gridLabelStyle.normal.textColor = new Color32(192, 192, 192, 255);
			else
				gridLabelStyle.normal.textColor = Color.black;

			//Filter
			filterIdx = 0;

			//Textures
			assetSelectionTextures = new Texture2D[5];

			string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
			rapidIconRootFolder = "";
			for (int i = 0; i < split.Length - 3; i++)
				rapidIconRootFolder += split[i] + "/";

			assetSelectionTextures[0] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/deselectedAsset.png");
			assetSelectionTextures[1] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveDark.png");
			assetSelectionTextures[2] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveDark.png");
			assetSelectionTextures[3] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveLight.png");
			assetSelectionTextures[4] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveLight.png");
			assetSelectionTextures[0].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[1].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[2].hideFlags = HideFlags.DontSave;
			assetList.lastNumberOfSelected = -1;

			//Other
			iconsRefreshed = true;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			//---Check variables are set---//
			CheckAndSetWindow(w);

			//---Refresh icons after startup---//
			if (!iconsRefreshed && EditorApplication.timeSinceStartup > 15)
			{
				RefreshAllIcons();
				iconsRefreshed = true;
			}

			GUILayout.BeginVertical(GUILayout.Width(width));
			GUILayout.Space(4);

			//---Filter---//
			GUILayout.BeginHorizontal();
			GUILayout.Space(8);
			if (GUILayout.Button("Refresh"))
				ReloadObjects();
			if (GUILayout.Button("Filter: " + filterNames[filterIdx]))
			{
				filterIdx++;
				if (filterIdx == 3)
					filterIdx = 0;

				ReloadObjects();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---Scroll view----//
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

			//---Draw icons---//
			DrawIcons(width);

			//---End GUI elements---//
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			//---Get last rect and check focus---//
			if (Event.current.type == EventType.Repaint)
				rect = new Rect(GUILayoutUtility.GetLastRect());
			CheckFocus(rect);
		}

		public void SaveData()
		{
			//---Save selected assets---//
			string selectedAssetsString = "";
			foreach (KeyValuePair<UnityEngine.Object, Icon> icon in objectIcons)
			{
				selectedAssetsString += "|-A-|" + icon.Value.assetPath + "|-S-|" + icon.Value.selected;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconSelectedAssets", selectedAssetsString);

			//---Save other variables---//
			EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll", scrollPosition.y);
			EditorPrefs.SetBool(PlayerSettings.productName + "RapidIconIconsRefreshed", iconsRefreshed);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconFilterIdx", filterIdx);
		}

		public void LoadData()
		{
			//---Close RapidIcon window if left open when Unity starts---//
			if(!SessionState.GetBool("rapidicon_loaded", false))
			{
				SessionState.SetBool("rapidicon_forceclose", true);
				SessionState.SetBool("rapidicon_loaded", true);
				return;
			}

			//---Load objects in selected folders---//
			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIcons();

			//---Load selected assets---//
			string selectedAssetsString = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconSelectedAssets");
			string[] splitAssets = selectedAssetsString.Split(new string[] { "|-A-|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in splitAssets)
			{
				string[] splitS = s.Split(new string[] { "|-S-|" }, StringSplitOptions.RemoveEmptyEntries);
				string assetPath = splitS[0];
				if (splitS[1] == "True")
				{
					Icon icon = GetIconFromPath(assetPath);
					if (icon != null)
					{
						icon.selected = true;
						selectedIcons.Add(GetIconFromPath(assetPath));
					}
				}
			}

			//---Load other variables---//
			iconsRefreshed = EditorPrefs.GetBool(PlayerSettings.productName + "RapidIconIconsRefreshed");
			scrollPosition = new Vector2(0, EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll"));
			filterIdx = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconFilterIdx", 0);
		}

		void ReloadObjects()
		{
			//---Unload objects
			EditorUtility.UnloadUnusedAssetsImmediate();

			//---Reload the objects from selected folders---//
			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIcons();

			//---Add loaded icons to list---///
			sortedIconsByPath.Clear();
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				Icon icon = objectIcons[loadedObject.UnityEngine_object];
				sortedIconsByPath.Add(icon.assetPath, icon);
			}

			//---Sort the list by path---//
			sortedIconsByPath = SortIconsByFolder(sortedIconsByPath);

			//---Update variables---//
			assetList.lastNumberOfSelected = assetList.selectedFolders.Count;
			lastSelectedIndividualFolder = assetList.selectedFolders[0];
		}

		public void RefreshAllIcons()
		{
			//---Loop through all icons---//
			int index = 1;
			foreach (Icon icon in objectIcons.Values)
			{
				//---Show a progress bar---//
				EditorUtility.DisplayProgressBar("Updating Icons (" + index++ + "/" + objectIcons.Count + ")", icon.assetPath, (float)(index) / (float)(objectIcons.Count));

				//---Update the icon renders---//
				Vector2Int renderResolution = Utils.MutiplyVector2IntByFloat(icon.exportResolution, window.iconEditor.resMultiplyers[window.iconEditor.resMultiplyerIndex]);
				icon.Update(renderResolution, new Vector2Int(128, (int)(((float)renderResolution.y / (float)renderResolution.x) * 128)));
			}

			//---Clear the progress bar when done---//
			EditorUtility.ClearProgressBar();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		List<ObjectPathPair> LoadObjectsInSelectedFolders()
		{
			//---Get asset paths of all assets in selected folders---//
			string[] assetGUIDs = AssetDatabase.FindAssets(filters[filterIdx], assetList.selectedFolders.ToArray());
			string[] assetPaths = new string[assetGUIDs.Length];
			for (int i = 0; i < assetGUIDs.Length; i++)
				assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

			List<ObjectPathPair> loadedObjectPathPairs = new List<ObjectPathPair>();
			foreach (string assetPath in assetPaths)
			{
				//---Get folder path from each of the asset paths---//
				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

				//---Load the asset if the path is in the selected folders list---//
				if (assetList.selectedFolders.Contains(folderPath))
				{
					ObjectPathPair objectPathPair = new ObjectPathPair();

					UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(assetPath);
					objectPathPair.UnityEngine_object = o;
					objectPathPair.path = assetPath;
					loadedObjectPathPairs.Add(objectPathPair);
				}
			}
			return loadedObjectPathPairs;
		}

		void CreateIcons()
		{
			int index = 1;
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				if (!objectIcons.ContainsKey(loadedObject.UnityEngine_object))
				{
					//---Create icon if doesn't already exist---//
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + " / " + (objectsLoadedFromSelectedFolders.Count) + ")", loadedObject.path, (float)(index++) / (float)objectsLoadedFromSelectedFolders.Count);
					objectIcons.Add(loadedObject.UnityEngine_object, CreateIcon(loadedObject));
				}
				else if (objectIcons[loadedObject.UnityEngine_object].deleted)
				{
					objectIcons[loadedObject.UnityEngine_object].deleted = false;
				}
				else
				{
					//---Update asset path if changed---//
					Icon icon = objectIcons[loadedObject.UnityEngine_object];

					string currentPath = AssetDatabase.GetAssetPath(loadedObject.UnityEngine_object);
					string savedPath = icon.assetPath;
					if (savedPath != currentPath)
					{
						Debug.LogWarning("Path updated for " + icon.assetName + " from " + savedPath + " to " + currentPath);
						icon.assetPath = currentPath;

						string[] split;
						split = icon.assetPath.Split('/');
						icon.assetName = split[split.Length - 1];
						if (icon.assetName.Length > 19)
							icon.assetShortenedName = icon.assetName.Substring(0, 16) + "...";
						else
							icon.assetShortenedName = icon.assetName;

						split = icon.assetPath.Split('/');
						icon.folderPath = "";
						for (int i = 0; i < split.Length - 1; i++)
							icon.folderPath += split[i] + (i < split.Length - 2 ? "/" : "");
					}
				}
			}
			EditorUtility.ClearProgressBar();
		}

		public Icon CreateIcon(ObjectPathPair objectPathPair)
		{
			//---Create a new icon object---//
			Icon icon = new Icon(Shader.Find("RapidIcon/ObjectRender"), rapidIconRootFolder, objectPathPair.path);

			//---Set the asset object and path of the icon---//
			icon.assetObject = objectPathPair.UnityEngine_object;
			icon.assetPath = objectPathPair.path;

			//---Get the asset as a GameObject, zero the postion if it's very small in magnitude---//
			GameObject go = (GameObject)icon.assetObject;
			if (icon.objectPosition.magnitude < 0.0001f)
				icon.objectPosition = Vector3.zero;

			//---Zero the GameObject euler angles if they're very small in magnitude---//
			icon.objectRotation = go.transform.eulerAngles;
			if (icon.objectRotation.magnitude < 0.0001f)
				icon.objectRotation = Vector3.zero;

			//---Set the object scale variable---//
			icon.objectScale = go.transform.localScale;

			//---Get default object position to centre it in the icon---//
			Bounds bounds = Utils.GetObjectBounds(icon);
			icon.objectPosition = Utils.GetObjectAutoOffset(icon, bounds);

			//---Get default camera settings to fit the object in the icon render---//
			float camAuto = Utils.GetCameraAuto(icon, bounds);
			icon.cameraSize = camAuto;
			icon.cameraPosition = Vector3.one * camAuto;

			//---Render the preview---//
			icon.previewRender = Utils.RenderIcon(icon, previewSize, (int)(((float)icon.exportResolution.y / (float)icon.exportResolution.x) * previewSize));
			icon.previewRender.hideFlags = HideFlags.DontSave;

			//---Set asset name and shortened asset name---//
			string[] split;
			split = icon.assetPath.Split('/');
			icon.assetName = split[split.Length - 1];
			if (icon.assetName.Length > 19)
				icon.assetShortenedName = icon.assetName.Substring(0, 16) + "...";
			else
				icon.assetShortenedName = icon.assetName;

			//---Set folder path---//
			split = icon.assetPath.Split('/');
			icon.folderPath = "";
			for (int i = 0; i < split.Length - 1; i++)
				icon.folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

			//---Set export name---//
			icon.exportName = icon.assetName;
			int extensionPos = icon.exportName.LastIndexOf('.');
			icon.exportName = icon.exportName.Substring(0, extensionPos);

			//---Set selection texture---//
			icon.selectionTexture = assetSelectionTextures[1];

			//---Load animations---//
			icon.LoadDefaultAnimationClip();

			return icon;
		}

		void DrawIcons(float gridWidth)
		{
			//---Draw margin---//
			GUILayout.Space(14);
			GUILayout.BeginHorizontal();
			GUILayout.Space(16);

			//---Create lists---//
			List<Texture2D> visibleIconRenders = new List<Texture2D>();
			List<Texture2D> visibleIconSelectionTextures = new List<Texture2D>();
			List<string> visibleIconLabels = new List<string>();
			visibleIcons = new List<Icon>();

			//---Reload objects if needed---//
			if (sortedIconsByPath.Count != objectsLoadedFromSelectedFolders.Count || assetList.selectedFolders.Count != assetList.lastNumberOfSelected || assetList.selectedFolders[0] != lastSelectedIndividualFolder)
			{
				ReloadObjects();
			}

			//---Deselect icons if no longer in selected folders / search (i.e. if not visible in the grid)---//
			foreach (Icon icon in objectIcons.Values)
			{
				if (!assetList.selectedFolders.Contains(icon.folderPath) || (assetList.doSearch && !assetList.searchFolders.Contains(icon.folderPath + "/" + icon.assetName)))
				{
					icon.selected = false;
					if (selectedIcons.Contains(icon))
						selectedIcons.Remove(icon);
				}
			}

			int index = 0;
			foreach (KeyValuePair<string, Icon> icon in sortedIconsByPath)
			{
				//---Skip this icon if it's flagged as deleted---//
				if (icon.Value.deleted)
					continue;

				//---Flag  the icon as deleted if asset object is null---//
				else if (icon.Value.assetObject == null)
				{
					icon.Value.deleted = true;
					icon.Value.assetObject = null;
					selectedIcons.Remove(icon.Value);
					continue;
				}

				//---Render the icon preview if it is missing---//
				if (icon.Value.previewRender == null)
				{
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + "/" + (sortedIconsByPath.Count) + ")", icon.Value.assetPath, ((float)index++ / sortedIconsByPath.Count));
					icon.Value.previewRender = Utils.RenderIcon(icon.Value, previewSize, (int)(((float)icon.Value.exportResolution.y / (float)icon.Value.exportResolution.x) * previewSize));
				}

				//---Set the selection texture---//
				if (EditorGUIUtility.isProSkin)
					icon.Value.selectionTexture = icon.Value.selected ? (assetGridFocused ? assetSelectionTextures[1] : assetSelectionTextures[2]) : assetSelectionTextures[0];
				else
					icon.Value.selectionTexture = icon.Value.selected ? (assetGridFocused ? assetSelectionTextures[3] : assetSelectionTextures[4]) : assetSelectionTextures[0];

				//---Add the icon to visibleIcons if it's withing the selected folders, or the search---//
				if (assetList.selectedFolders.Contains(icon.Value.folderPath) && (!assetList.doSearch || assetList.searchFolders.Contains(icon.Value.folderPath + "/" + icon.Value.assetName)))
				{
					visibleIcons.Add(icon.Value);

					//Use warning image if animations enabled
					visibleIconRenders.Add(icon.Value.previewRender);

					visibleIconSelectionTextures.Add(icon.Value.selectionTexture);
					visibleIconLabels.Add(icon.Value.assetShortenedName);
				}
			}
			EditorUtility.ClearProgressBar();

			//---Draw the grid of icons---///
			int count = Mathf.FloorToInt((gridWidth - 16) / (previewSize + 16));
			count = Mathf.Min(count, visibleIcons.Count);
			int clicked = GUILayout.SelectionGrid(-1, visibleIconRenders.ToArray(), count, gridStyle, GUILayout.Width(32 + count * (previewSize + 16)));
			Rect r = GUILayoutUtility.GetLastRect();
			r.y += previewSize + 2;

			//---Draw the label background textures on the grid---//
			int labelClicked = GUI.SelectionGrid(r, -1, visibleIconSelectionTextures.ToArray(), count, gridLabelStyle);

			//---Draw the label texts on the grid---//
			clicked = GUI.SelectionGrid(r, clicked, visibleIconLabels.ToArray(), count, gridLabelStyle);
			if (clicked == -1 && labelClicked != -1)
				clicked = labelClicked;

			//---Draw margin and end GUI elements---//
			GUILayout.Space(16);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---Check mouse clicks and arrow key presses for grid selection---//
			CheckMouseClicks(clicked, visibleIcons);
			CheckArrowKeys(visibleIcons, count);
		}

		void CheckMouseClicks(int clicked, List<Icon> visibleIcons)
		{
			if (clicked >= 0)
			{
				if (!Event.current.control && !Event.current.shift)
				{
					//---Regular click, no ctrl/shift - select the icon---//
					foreach (KeyValuePair<UnityEngine.Object, Icon> icon in objectIcons)
						icon.Value.selected = false;

					visibleIcons[clicked].selected = true;
					visibleIcons[clicked].assetGridIconIndex = clicked;
					selectedIcons.Clear();
					selectionMinIndex = clicked;
					selectionMaxIndex = clicked;
				}
				else if (Event.current.control)
				{
					//---Ctrl click - add icon to existing selection---//
					visibleIcons[clicked].selected = !visibleIcons[clicked].selected;
					visibleIcons[clicked].assetGridIconIndex = clicked;
				}
				else if (Event.current.shift)
				{
					//---Shift click - add all icons between clicks---//
					if (selectionMinIndex != -1 && selectionMaxIndex != -1 && clicked >= selectionMinIndex && clicked <= selectionMaxIndex)
					{
						for (int i = selectionMinIndex; i <= selectionMaxIndex; i++)
						{
							visibleIcons[i].selected = false;
							if (selectedIcons.Contains(visibleIcons[i]))
								selectedIcons.Remove(visibleIcons[i]);
						}

						selectionMinIndex = Mathf.Min(lastSelectedIconIndex, clicked);
						selectionMaxIndex = Math.Max(lastSelectedIconIndex, clicked);
					}
					int minI = Mathf.Min(lastSelectedIconIndex, clicked);
					int maxI = Math.Max(lastSelectedIconIndex, clicked);
					if (minI < 0) minI = 0;
					if (maxI < 0) maxI = 0;

					for (int i = minI; i <= maxI; i++)
					{
						visibleIcons[i].selected = true;
						visibleIcons[i].assetGridIconIndex = i;
						if (!selectedIcons.Contains(visibleIcons[i]))
							selectedIcons.Add(visibleIcons[i]);
					}

				}

				//---If not shift click then toggle the icon from the selection---//
				if (!Event.current.shift)
				{
					if (visibleIcons[clicked].selected && !selectedIcons.Contains(visibleIcons[clicked]))
						selectedIcons.Add(visibleIcons[clicked]);
					else if (selectedIcons.Contains(visibleIcons[clicked]))
						selectedIcons.Remove(visibleIcons[clicked]);
				}

				//---Sort the selected icons by grid index---//
				if (selectedIcons.Count > 1)
					selectedIcons = selectedIcons.OrderBy(a => a.assetGridIconIndex).ToList();

				//---Update variables---//
				selectionMinIndex = Mathf.Min(selectionMinIndex, clicked);
				selectionMaxIndex = Mathf.Max(selectionMaxIndex, clicked);
				lastSelectedIconIndex = clicked;
				assetGridFocused = true;
				window.Repaint();
			}
			else if (Event.current.rawType == EventType.MouseDown && !window.leftSeparator.mouseOver && !window.rightSeparator.mouseOver)
			{
				//---Clear selection if mouse clicked in empty space in asset grid region---//
				Vector2 correctMousePos = Event.current.mousePosition + rect.position;
				if (rect.Contains(correctMousePos))
				{
					selectedIcons.Clear();
					foreach (Icon icon in visibleIcons)
						icon.selected = false;
				}
			}
		}

		void CheckArrowKeys(List<Icon> icons, int gridXIcons)
		{
			//---Check if a key is pressed---//
			if (assetGridFocused && Event.current.isKey && Event.current.type != EventType.KeyUp)
			{
				//---Right arrow key pressed---//
				if (Event.current.keyCode == KeyCode.RightArrow && lastSelectedIconIndex < icons.Count - 1)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex + 1].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex + 1]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!icons[lastSelectedIconIndex + 1].selected)
						{
							icons[lastSelectedIconIndex + 1].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex + 1]);
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
						}

					}
					lastSelectedIconIndex++;
				}
				//---Left arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.LeftArrow && lastSelectedIconIndex > 0)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex - 1].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex - 1]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!icons[lastSelectedIconIndex - 1].selected)
						{
							icons[lastSelectedIconIndex - 1].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex - 1]);
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
						}
					}
					lastSelectedIconIndex--;
				}
				//---Down arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.DownArrow)
				{
					if (lastSelectedIconIndex < icons.Count - gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							//---Select only this icon if no shift/ctrl pressed---//
							foreach (Icon icon in icons)
								icon.selected = false;
							selectedIcons.Clear();

							icons[lastSelectedIconIndex + gridXIcons].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex + gridXIcons]);
						}
						else
						{
							//---Add to current selection if shift/ctrl pressed---///
							if (!icons[lastSelectedIconIndex + gridXIcons].selected)
							{
								icons[lastSelectedIconIndex + gridXIcons].selected = true;
								selectedIcons.Add(icons[lastSelectedIconIndex + gridXIcons]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									icons[i].selected = true;
									selectedIcons.Add(icons[i]);
								}
							}
							else
							{
								icons[lastSelectedIconIndex].selected = false;
								selectedIcons.Remove(icons[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									icons[i].selected = false;
									selectedIcons.Remove(icons[i]);
								}
							}

						}
						lastSelectedIconIndex += gridXIcons;
					}
					else if (lastSelectedIconIndex < Mathf.Floor((float)icons.Count / gridXIcons) * gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							//---Select only this icon if no shift/ctrl pressed---//
							foreach (Icon icon in icons)
								icon.selected = false;
							selectedIcons.Clear();

							icons[icons.Count - 1].selected = true;
							selectedIcons.Add(icons[icons.Count - 1]);
						}
						else
						{
							//---Add to current selection if shift/ctrl pressed---///
							if (!icons[icons.Count - 1].selected)
							{
								icons[icons.Count - 1].selected = true;
								selectedIcons.Add(icons[icons.Count - 1]);
								for (int i = lastSelectedIconIndex; i < icons.Count; i++)
								{
									icons[i].selected = true;
									selectedIcons.Add(icons[i]);
								}
							}
							else
							{
								icons[lastSelectedIconIndex].selected = false;
								selectedIcons.Remove(icons[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < icons.Count - 1; i++)
								{
									icons[i].selected = false;
									selectedIcons.Remove(icons[i]);
								}
							}

						}
						lastSelectedIconIndex = icons.Count - 1;
					}
				}
				//---Up arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.UpArrow && lastSelectedIconIndex >= gridXIcons)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex - gridXIcons].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex - gridXIcons]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!icons[lastSelectedIconIndex - gridXIcons].selected)
						{
							icons[lastSelectedIconIndex - gridXIcons].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex - gridXIcons]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								icons[i].selected = true;
								selectedIcons.Add(icons[i]);
							}
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								icons[i].selected = false;
								selectedIcons.Remove(icons[i]);
							}
						}

					}
					lastSelectedIconIndex -= gridXIcons;
				}
				else if (Event.current.keyCode == KeyCode.A && Event.current.modifiers == EventModifiers.Control)
				{
					//---Select all if ctrl-A pressed---//
					selectedIcons.Clear();
					foreach (KeyValuePair<string, Icon> icon in sortedIconsByPath)
					{
						if (assetList.selectedFolders.Contains(icon.Value.folderPath))
						{
							icon.Value.selected = true;
							selectedIcons.Add(icon.Value);
						}
					}
				}
			}
		}

		Dictionary<string, Icon> SortIconsByFolder(Dictionary<string, Icon> data)
		{
			//---Get a string array of asset paths---//
			string[] assetPaths = new string[data.Keys.Count];
			data.Keys.CopyTo(assetPaths, 0);

			//---Create a dictionary that will hold folder paths as keys, and a list of asset paths in the values (assets within the folder)---//
			Dictionary<string, List<string>> folders = new Dictionary<string, List<string>>();

			//---Create a list for just the folder names---//
			List<string> folderNames = new List<string>();

			foreach (string assetPath in assetPaths)
			{
				//---Get folder path from asset path---//
				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

				//---Add folder to folders dictionary if not already in there---//
				if (!folders.ContainsKey(folderPath))
				{
					folders.Add(folderPath, new List<string>());
					folderNames.Add(folderPath);
				}

				//---Add asset path in the list at the [folderPath] index of the folders dictionary---//
				folders[folderPath].Add(assetPath);
			}

			//---Sort the folder names---//
			folderNames.Sort();

			//---For each of the sorted folders, sort the assets within that folder---//
			string[] sortedAssetPaths = new string[assetPaths.Length];
			int index = 0;
			foreach (string folder in folderNames)
			{
				folders[folder].Sort();
				foreach (string assetPath in folders[folder])
				{
					//---Add the asset paths to the new list in sorted order---//
					sortedAssetPaths[index++] = assetPath;
				}
			}

			//---Use the sorted list of asset paths to create a dictionary of sorted icons---//
			Dictionary<string, Icon> sortedData = new Dictionary<string, Icon>();
			foreach (string assetPath in sortedAssetPaths)
			{
				sortedData.Add(assetPath, data[assetPath]);
			}

			return sortedData;
		}

		void CheckFocus(Rect checkRect)
		{
			//---Check if last mouse click was in the asset grid rect---//
			if (Event.current.rawType == EventType.MouseDown)
			{
				assetGridFocused = checkRect.Contains(Event.current.mousePosition);
			}

			//---Check the RapidIcon window is in focus---//
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(RapidIconWindow))
				assetGridFocused = false;
		}

		Icon GetIconFromPath(string path)
		{
			//---Loop through icons and check if the path matches---//
			foreach (Icon icon in objectIcons.Values)
			{
				if (icon.assetPath == path)
					return icon;
			}

			return null;
		}
	}
}