using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	[Serializable]
	public class IconEditor
	{
		//---PUBLIC---//
		//Textures
		public Texture2D previewBackgroundImage;
		public Texture2D scrollAreaBackgroundImage;
		public Texture2D scaleLinkOnImage;
		public Texture2D scaleLinkOffImage;
		public Texture2D separatorTex;

		//Icon resolution
		public Vector2Int renderResolution;
		public Vector2Int renderSize;
		public int resMultiplyerIndex;
		public float[] resMultiplyers = new float[] { 0.25f, 0.5f, 1f };

		//Icons
		public Icon currentIcon;
		public bool updateFlag;

		//Tabs
		public bool linkScale;
		public MaterialEditor materialEditor;
		public Material mat;
		public ReorderableList reorderableList;
		public string lastPresetPath;
		public string[] tabNames = new string[] { "Object", "Camera", "Lighting", "Animation", "Post-Processing", "Export" };

		//RapidIcon Window Elements/Settings
		public RapidIconWindow window;
		public AssetGrid assetGrid;
		public ReorderableListCallbacks reorderableListCallbacks;
		public bool fullscreen;
		public float fullWidth;
		public float sepWidth;
		public float oldMinWidth;

		//Other
		public bool replaceAll;

		//---INTERNAL---//
		//Preview zoom/resolution
		int zoomScaleIndex;
		float[] zoomScales = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f, 1f };
		string[] zoomScalesStrings = new string[] { "25%", "50%", "75%", "100%", "125%", "150%", "200%", "300%", "Scale to Fit (num %)" };
		string[] resMultiplyersStrings = new string[] { "Quarter", "Half", "Full" };
		int zoomFitByWidthHeight; //0: height, 1: width

		//Preview GUI
		Vector2 previewScrollPos;
		GUIStyle renderStyle;
		GUIStyle scrollStyle;
		Vector2 previewAreaSize;
		Rect previewRect;
		DraggableSeparator previewDraggableSeparator;

		//Tabs
		Vector2 controlsScrollPos;
		int tab;

		//Icons
		int currentIconIndex;
		bool updateAllFlag;

		//Other
		bool undoHold;
		bool sceneChangeUpdate = false;

		public IconEditor(AssetGrid grid, RapidIconWindow w)
		{
			//---Initialise IconEditor---//
			//Set RapidIcon elements
			CheckAndSetWindow(w);
			assetGrid = grid;

			//Render resolution/style
			renderResolution = new Vector2Int(256, 256);
			renderSize = new Vector2Int(256, 256);
			renderStyle = new GUIStyle();
			renderStyle.stretchWidth = true;
			renderStyle.stretchHeight = true;
			renderStyle.stretchHeight = true;
			renderStyle.stretchWidth = true;

			//Set preview zoom/resolution setting
			zoomScaleIndex = 8;
			resMultiplyerIndex = 2;

			//Create colour textures
			scrollAreaBackgroundImage = Utils.CreateColourTexture(4, 4, new Color32(50, 50, 50, 255));
			if (EditorGUIUtility.isProSkin)
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
			else
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));

			//Load textures
			previewBackgroundImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/previewGrid.png");
			scaleLinkOnImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/linkOn.png");
			scaleLinkOffImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/linkOff.png");

			//Create horizontal separator
			previewDraggableSeparator = new DraggableSeparator(SeparatorTypes.Horizontal);

			//Set bools
			linkScale = true;
			replaceAll = false;

			//Create material editor
			mat = new Material(Shader.Find("RapidIcon/ImgShader"));
			materialEditor = (MaterialEditor)Editor.CreateEditor(mat);

			//Create reorderable list for post-processing shaders
			List<Material> blankList = new List<Material>();
			reorderableList = new ReorderableList(blankList, typeof(Material), true, true, true, true);
			reorderableListCallbacks = new ReorderableListCallbacks(this);
			reorderableList.drawElementCallback = reorderableListCallbacks.DrawListItems;
			reorderableList.drawHeaderCallback = reorderableListCallbacks.DrawHeader;
			reorderableList.onSelectCallback = reorderableListCallbacks.SelectShader;
			reorderableList.onAddCallback = reorderableListCallbacks.AddShader;
			reorderableList.onRemoveCallback = reorderableListCallbacks.RemoveShader;
			reorderableList.onReorderCallback = reorderableListCallbacks.ShadersReorded;

			//Configure undo
			undoHold = false;
			Undo.undoRedoPerformed += OnUndo;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			//---Check variables are set---//
			CheckAndSetWindow(w);

			//---On first update of new scene---//
			if (!sceneChangeUpdate)
			{
				foreach (Icon icon in assetGrid.objectIcons.Values)
				{
					if (icon.postProcessingMaterials.Count > 0 && icon.postProcessingMaterials[0] == null)
					{
						if (icon.saveData)
						{
							//---Load the MatInfo if icon has been saved---//
							icon.LoadMatInfo();
						}
						else
						{
							//---Reset the icon if not saved---//
							ObjectPathPair obj = new ObjectPathPair(icon.assetObject, icon.assetPath);
							Icon newIcon = assetGrid.CreateIcon(obj);
							Utils.CopyIconSettings(newIcon, currentIcon, -1);
							Utils.UpdateIcon(currentIcon, this);
						}

						//---Update all icons---//
						updateAllFlag = true;
						sceneChangeUpdate = true;
					}
				}
			}

			//---If icon(s) selected---//
			if (assetGrid.selectedIcons.Count > 0)
			{
				//---Get the currently selected icon---//
				currentIconIndex = Mathf.Clamp(currentIconIndex, 0, assetGrid.selectedIcons.Count - 1);
				currentIcon = assetGrid.selectedIcons[currentIconIndex];

				//---Check if the object asscoiated with the icon has been deleted---//
				if (currentIcon.assetObject == null)
				{
					//---Flag as deleted and remove from selection---//
					currentIcon.deleted = true;
					assetGrid.selectedIcons.Remove(currentIcon);

					//---If other icon(s) still selected, update currentIcon - otherwise return---//
					if (assetGrid.selectedIcons.Count > 0)
					{
						if (currentIconIndex > assetGrid.selectedIcons.Count - 1)
							currentIconIndex = assetGrid.selectedIcons.Count - 1;

						currentIcon = assetGrid.selectedIcons[currentIconIndex];
					}
					else
						return;
				}

				//---Create a GUI area, prevents buggy behaviour when moving left separator---//
				Rect r = new Rect(window.rightSeparator.rect);
				if (fullscreen)
				{
					r.x = 0;
					r.width = window.position.width;
				}
				else
				{
					r.width = width;
				}
				GUILayout.BeginArea(r);
				GUILayout.BeginVertical();

				//---Update icons if flag set---//
				if (updateFlag)
				{
					Utils.UpdateIcon(currentIcon, this);
					updateFlag = false;
				}
				if (updateAllFlag)
				{
					assetGrid.RefreshAllIcons();
					updateAllFlag = false;
				}

				//---Check current icon has a full render---//
				Utils.CheckCurrentIconRender(this);

				//---Draw the icon preview---//
				DrawPreview();
				GUILayout.Space(2);

				//---Draw the preview zoom/resolution controls---//
				DrawPreviewResAndZoom();

				//---Draw the draggable separator---//
				previewDraggableSeparator.Draw(100, window.position.height - 100, window);
				GUILayout.Space(8);

				//---Draw the icon selector---//
				DrawIconSelecter();

				//---Draw the tab selector---//
				tab = GUILayout.Toolbar(tab, tabNames);
				sepWidth = width - 50;

				//---Draw the tabs---//
				DrawTabs(width);

				//---Check mouse inputs to rotate/zoom the preview---//
				CheckMouseMovement();

				//---End GUI elements---//
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("No Icons Selected");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}
		}

		public void SaveData()
		{
			//---Save the separator---//
			previewDraggableSeparator.SaveData("RapidIconSepPosPreview");

			//---Add the icons to be saved to list---//
			IconData iconData = new IconData();
			foreach (KeyValuePair<UnityEngine.Object, Icon> icon in assetGrid.objectIcons)
			{
				if (icon.Value.saveData)
					iconData.icons.Add(icon.Value);
			}

			//---Save the icon data---//
			Utils.SaveIconData(iconData);

			//---Save the preview zoom/resolution settings and current tab---//
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconPreviewResIdx", resMultiplyerIndex);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconPreviewZoomIdx", zoomScaleIndex);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconEditorTab", tab);

			//---Save the window position and width if fullscreen mode set---//
			if (fullscreen)
			{
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosX", window.position.xMax - (fullWidth + window.position.width));
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosY", window.position.y);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowWidth", fullWidth + window.position.width);
			}
			else
			{
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosX", -1);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosY", -1);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowWidth", -1);
			}
		}

		public void LoadData()
		{
			//---Load icon data---//
			IconData iconData = new IconData();
			iconData = Utils.LoadIconData();

			assetGrid.objectIcons = new Dictionary<UnityEngine.Object, Icon>();
			if (iconData != null)
			{
				foreach (Icon icon in iconData.icons)
				{
					if (icon.assetObject != null)
						assetGrid.objectIcons.Add(icon.assetObject, icon);
				}
			}

			//---Load the draggable separator data---//
			previewDraggableSeparator.LoadData("RapidIconSepPosPreview", 650);

			//---Load the preview zoom/resolution settings---//
			resMultiplyerIndex = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconPreviewResIdx", -1);
			zoomScaleIndex = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconPreviewZoomIdx", -1);

			if (resMultiplyerIndex == -1)
				resMultiplyerIndex = 2;

			if (zoomScaleIndex == -1)
				zoomScaleIndex = 8;

			//---Load the current tab---//
			tab = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconEditorTab", 0);
		}

		void OnUndo()
		{
			//---Need to update icon after undo---//
			updateFlag = true;

			//---Reload the mat info---//
			foreach (Icon icon in assetGrid.objectIcons.Values)
				icon.LoadMatInfo();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		void DrawIconSelecter()
		{
			GUILayout.BeginHorizontal();

			//---Draw previous icon select button---//
			if (GUILayout.Button("<", GUILayout.Width(100)) && currentIconIndex > 0)
				currentIconIndex--;

			//---Draw dropdown list of selected assets---//
			int idx = 0;
			string[] iconNames = new string[assetGrid.selectedIcons.Count];
			foreach (Icon icon in assetGrid.selectedIcons)
				iconNames[idx++] = (icon.assetName);
			currentIconIndex = EditorGUILayout.Popup(currentIconIndex, iconNames);

			//---Draw next icon select button---//
			if (GUILayout.Button(">", GUILayout.Width(100)) && currentIconIndex < assetGrid.selectedIcons.Count - 1)
				currentIconIndex++;

			GUILayout.EndHorizontal();
		}

		void CheckMouseMovement()
		{
			//---Detect right mouse button down---//
			if (Event.current.button == 1 && Event.current.type == EventType.Layout)
			{
				//---Check if the mouse is over the preview render---//
				if (previewRect.Contains(Event.current.mousePosition))
				{
					//---Record the object for undo, but only once until hold released---//
					if (!undoHold)
					{
						Undo.RecordObject(window, "Camera Rotation");
						undoHold = true;
					}

					//---Get rotation from mouse x movement---//
					Quaternion camTurnAngle = Quaternion.AngleAxis(Event.current.delta.x * 0.2f, Vector3.up);

					//---Combine with rotation from mouse y movement---//
					Vector3 axis = Quaternion.LookRotation(currentIcon.cameraTarget - currentIcon.cameraPosition) * Vector3.right;
					camTurnAngle *= Quaternion.AngleAxis(Event.current.delta.y * 0.2f, axis);

					//---Rotate the camera by moving it---//
					currentIcon.cameraPosition = camTurnAngle * currentIcon.cameraPosition;

					//---Flag the icon to be saved and updated---//
					currentIcon.saveData = true;
					updateFlag = true;
					window.Repaint();
				}
			}
			//---Detect scroll wheel movement---//
			else if (Event.current.type == EventType.ScrollWheel)
			{
				//---Check if the mouse is over the preview render---//
				if (previewRect.Contains(Event.current.mousePosition))
				{
					//---Record the object for undo, but only once until hold released---//
					if (!undoHold)
					{
						Undo.RecordObject(window, "Camera Zoom");
						undoHold = true;
					}

					//---Zoom the camra---//
					if (currentIcon.cameraOrtho)
					{
						//---If ortho then change the camra size---//
						if (Mathf.Sign(Event.current.delta.y) == 1)
						{
							currentIcon.camerasScaleFactor /= 1.1f;
						}
						else
						{
							currentIcon.camerasScaleFactor *= 1.1f;
						}
					}
					else
					{
						//---If not ortho than move the camera---//
						//Scale stil updated so ortho view matches
						if (Mathf.Sign(Event.current.delta.y) == 1)
						{
							currentIcon.camerasScaleFactor /= 1.1f;
							currentIcon.cameraPosition *= 1.1f;
						}
						else
						{
							currentIcon.camerasScaleFactor *= 1.1f;
							currentIcon.cameraPosition /= 1.1f;
						}
					}

					//---Flag the icon to be saved and updated---//
					currentIcon.saveData = true;
					updateFlag = true;
					window.Repaint();
				}
			}
			//--If no mouse inputs then release undo hold---//
			else if (undoHold)
				undoHold = false;
		}

		void DrawPreview()
		{
			//---Setup scroll style if null---//
			if (scrollStyle == null)
			{
				scrollStyle = new GUIStyle(GUI.skin.scrollView);
				scrollStyle.margin.left = 1;
				scrollStyle.normal.background = scrollAreaBackgroundImage;
			}

			//---Begin GUI elements---//
			previewScrollPos = GUILayout.BeginScrollView(previewScrollPos, scrollStyle, GUILayout.Height(previewDraggableSeparator.value));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			//---Get the render rect---//
			Rect renderRect = GUILayoutUtility.GetRect(renderSize.x, renderSize.y);
			renderRect.size -= new Vector2(12, 12);
			renderRect.center += new Vector2(6, 6);

			//---Draw the background checkerboard texture---//
			GUI.DrawTextureWithTexCoords(renderRect, previewBackgroundImage, new Rect(0, 0, renderRect.width / 32, renderRect.height / 32));

			//---Draw the icon render---//
			GUI.DrawTexture(renderRect, currentIcon.fullRender);

			//---End GUI elements---//
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndScrollView();


			if (Event.current.type == EventType.Repaint)
			{
				//---Get difference in pixels between export resolution and preview window resolution---//
				previewRect = GUILayoutUtility.GetLastRect();
				previewAreaSize = previewRect.size;
				Vector2 delta = currentIcon.exportResolution - previewRect.size;

				//---Normalise the delta to the export resolution---//
				delta.x /= currentIcon.exportResolution.x;
				delta.y /= currentIcon.exportResolution.y;

				//---If export resolution is bigger than preview window (x and y)---//
				if (delta.x > 0 && delta.y > 0)
				{
					//---If normalised delta is larger in y---//
					if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
						zoomFitByWidthHeight = 0; //fit by height
					else
						zoomFitByWidthHeight = 1; //fit by width
				}
				//---If export resolution is smaller than preview window (x and y)---//
				else if (delta.x <= 0 && delta.y <= 0)
				{
					//---If normalised delta is larger in x---//
					if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
						zoomFitByWidthHeight = 1; //fit by width
					else
						zoomFitByWidthHeight = 0; //fit by height
				}
				//---If export resolution is bigger than preview window (y only)---//
				else if (delta.y > 0)
					zoomFitByWidthHeight = 0; //fit by height
											  //---If export resolution is bigger than preview window (x only)---//
				else if (delta.x > 0)
					zoomFitByWidthHeight = 1; //fit by width
			}
		}

		void DrawPreviewResAndZoom()
		{
			//---Begin GUI elements---//
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			//---Calculate zoom scale for 'Zoom to Fit'---//
			switch (zoomFitByWidthHeight)
			{
				case 0: //fit by height
					zoomScales[8] = previewAreaSize.y / (float)(currentIcon.exportResolution.y);
					break;
				case 1: //fit by width
					zoomScales[8] = previewAreaSize.x / (float)(currentIcon.exportResolution.x);
					break;
			}

			//---Set this zoom scale to 1 if not icons selected---//
			if (assetGrid.selectedIcons.Count == 0)
				zoomScales[8] = 1;

			//---Set the string for the zoom option---//
			string s = "Scale to Fit (" + (100f * (float)zoomScales[8]).ToString("f1") + "%)";
			zoomScalesStrings[8] = s;

			//---Draw preview resolution selection dropdown---//
			GUILayout.Label("Preview Resolution", GUILayout.Width(115));
			resMultiplyerIndex = EditorGUILayout.Popup(resMultiplyerIndex, resMultiplyersStrings, GUILayout.Width(70));
			renderResolution = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, resMultiplyers[resMultiplyerIndex]);

			//---If preview resolution changed then update the icon---//
			if (EditorGUI.EndChangeCheck())
			{
				currentIcon = assetGrid.selectedIcons[currentIconIndex];
				if (currentIcon != null)
				{
					updateFlag = true;
				}
			}

			//---Draw the preview zoom selection dropdown---//
			GUILayout.Space(32);
			GUILayout.Label("Zoom", GUILayout.Width(40));
			zoomScaleIndex = EditorGUILayout.Popup(zoomScaleIndex, zoomScalesStrings, GUILayout.Width(150));
			renderSize = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, zoomScales[zoomScaleIndex]);

			//---End GUI elements---//
			GUILayout.EndHorizontal();
		}

		void DrawTabs(float width)
		{
			//---Begin scroll view---//
			controlsScrollPos = GUILayout.BeginScrollView(controlsScrollPos, GUILayout.Height(window.position.height - previewDraggableSeparator.value - 98));

			//---Draw the controls for the selected tab---//
			switch (tab)
			{
				case 0:
					Tabs.DrawObjectControls(this);
					break;
				case 1:
					Tabs.DrawCameraControls(this);
					break;
				case 2:
					Tabs.DrawLightingControls(this);
					break;
				case 3:
					Tabs.DrawAnimationControls(this);
					break;
				case 4:
					Tabs.DrawPostProcessingControls(this);
					break;
				case 5:
					Tabs.DrawExportControls(this);
					break;
			}

			//---If any tab other than export tab---//
			if (tab < 5)
			{
				//---Draw button to apply settings to all selected icons---//
				GUILayout.Space(8);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply to All Selected Icons", GUILayout.Width(200)))
					Utils.ApplyToAllSelectedIcons(tab, this);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			//---Draw button to reset this icon's settings---//
			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset This Icon", GUILayout.Width(200)))
			{
				//---Display confirmation window---//
				int result = EditorUtility.DisplayDialogComplex("Confirm", "Are you sure you want to reset settings for the current icon? This cannot be undone.", "Reset " + tabNames[tab] + " Settings Only", "Reset All", "Cancel");
				if (result != 2)
				{
					//---Create a reset icon---//
					ObjectPathPair obj = new ObjectPathPair(currentIcon.assetObject, currentIcon.assetPath);
					Icon newIcon = assetGrid.CreateIcon(obj);

					if (result == 1)
						Utils.CopyIconSettings(newIcon, currentIcon, -1); //reset all
					else if (result == 0)
						Utils.CopyIconSettings(newIcon, currentIcon, tab); //reset current tab only

					//---Update the icon---//
					Utils.UpdateIcon(currentIcon, this);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---Draw button to reset all selected icons' settings---//
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset All Selected Icons", GUILayout.Width(200)))
			{
				//---Display confirmation window---//
				int result = EditorUtility.DisplayDialogComplex("Confirm", "Are you sure you want to reset settings for the current icons? This cannot be undone.", "Reset " + tabNames[tab] + " Settings Only", "Reset All", "Cancel");
				if (result != 2)
				{
					int index = 0;
					foreach (Icon ic in assetGrid.selectedIcons)
					{
						//---Display progress bar---//
						EditorUtility.DisplayProgressBar("Resetting Icons (" + index + " / " + (assetGrid.selectedIcons.Count) + ")", ic.assetName, (float)(index++) / (float)assetGrid.selectedIcons.Count);

						//---Create a reset icon---//
						ObjectPathPair obj = new ObjectPathPair(ic.assetObject, ic.assetPath);
						Icon newIcon = assetGrid.CreateIcon(obj);

						if (result == 1)
							Utils.CopyIconSettings(newIcon, ic, -1); //reset all
						else if (result == 0)
							Utils.CopyIconSettings(newIcon, ic, tab); //reset current tab only

						//---Update the icon---//
						Utils.UpdateIcon(ic, this);
					}

					//---Clear progress bar---//
					EditorUtility.ClearProgressBar();
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---End scroll view---//
			GUILayout.EndScrollView();

			//---Draw fullscreen toggle button---//
			if (GUILayout.Button(fullscreen ? "<" : ">", GUILayout.Width(20)))
			{
				//---Toggle fullscreen---//
				fullscreen = !fullscreen;

				//---Set window position and size---//
				if (fullscreen)
				{
					fullWidth = window.position.width - width;
					oldMinWidth = window.minSize.x;
					window.minSize = new Vector2(400, window.minSize.y);
					window.position = new Rect(window.position.xMax - width, window.position.y, width, window.position.height);
				}
				else
				{
					window.position = new Rect(window.position.xMax - (fullWidth + window.position.width), window.position.y, fullWidth + window.position.width, window.position.height);
					window.minSize = new Vector2(oldMinWidth, window.minSize.y);
				}
			}

		}
	}
}