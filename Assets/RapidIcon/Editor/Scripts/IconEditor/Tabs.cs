using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	public static class Tabs
	{
		public static void DrawObjectControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw position field---//
			Vector3 tmpObjPos = EditorGUILayout.Vector3Field("Position", iconEditor.currentIcon.objectPosition);
			Rect posR = GUILayoutUtility.GetLastRect();
			posR.x += 50;
			posR.height = 18;
			posR.width = 50;

			//---Draw auto button for position---//
			if (GUI.Button(posR, "Auto"))
			{
				tmpObjPos = Utils.GetObjectAutoOffset(iconEditor.currentIcon, Utils.GetObjectBounds(iconEditor.currentIcon));
			}

			//---Draw auto all selected button for position---//
			posR.x += 50;
			posR.width = 150;
			if (GUI.Button(posR, "Auto All Selected Icons"))
			{
				//---Loop through all selected icons---//
				int index = 1;
				foreach (Icon icon in iconEditor.assetGrid.selectedIcons)
				{
					//---Draw progress bar---//
					EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (iconEditor.assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (iconEditor.assetGrid.selectedIcons.Count - 1)));

					//---Get auto offset position---//
					if (icon != iconEditor.currentIcon)
					{
						icon.objectPosition = Utils.GetObjectAutoOffset(icon, Utils.GetObjectBounds(icon));
						Utils.UpdateIcon(icon, iconEditor);
					}
					else
					{
						tmpObjPos = Utils.GetObjectAutoOffset(icon, Utils.GetObjectBounds(icon));
					}
				}

				//---Clear progresss bar---//
				EditorUtility.ClearProgressBar();
			}

			//---Draw object rotation and scale fields---//
			Vector3 tmpObjRot = EditorGUILayout.Vector3Field("Rotation", iconEditor.currentIcon.objectRotation);
			Vector3 tmpObjScale = EditorGUILayout.Vector3Field("Scale", iconEditor.currentIcon.objectScale);

			//---Draw link scale toggle---//
			Rect r = GUILayoutUtility.GetLastRect();
			r.position += new Vector2(40, 0);
			if (GUI.Button(r, iconEditor.linkScale ? iconEditor.scaleLinkOnImage : iconEditor.scaleLinkOffImage, GUIStyle.none))
			{
				iconEditor.linkScale = !iconEditor.linkScale;
			}

			//---If any fields changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.window, "Edit Icon Object");

				//---Apply position and rotation---//
				iconEditor.currentIcon.objectPosition = tmpObjPos;
				iconEditor.currentIcon.objectRotation = tmpObjRot;

				//---If link scale disabled, apply scale---//
				if (!iconEditor.linkScale)
				{
					iconEditor.currentIcon.objectScale = tmpObjScale;
				}
				else
				{
					//---If link scale enabled, detect which axis changed and apply to all axes---//
					if (tmpObjScale.x != iconEditor.currentIcon.objectScale.x)
					{
						iconEditor.currentIcon.objectScale = tmpObjScale.x * Vector3.one;
					}
					else if (tmpObjScale.y != iconEditor.currentIcon.objectScale.y)
					{
						iconEditor.currentIcon.objectScale = tmpObjScale.y * Vector3.one;
					}
					else if (tmpObjScale.z != iconEditor.currentIcon.objectScale.z)
					{
						iconEditor.currentIcon.objectScale = tmpObjScale.z * Vector3.one;
					}
				}

				//---Save and update icon---//
				iconEditor.currentIcon.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawCameraControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw camera position field---//
			Vector3 tmpCamPos = EditorGUILayout.Vector3Field("Position", iconEditor.currentIcon.cameraPosition);

			//---Draw camera point of focus field---//
			GUILayout.Space(8);
			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			Vector3 tmpCamTgt = iconEditor.currentIcon.cameraTarget;
			tmpCamTgt = EditorGUILayout.Vector3Field("Point of Focus", iconEditor.currentIcon.cameraTarget);

			//---Draw camera projection mode field---//
			GUILayout.Space(8);
			string[] listOptions = { "Perspective", "Orthographic" };
			EditorGUIUtility.labelWidth = 80;
			int tmpOrtho = EditorGUILayout.Popup("Projection", iconEditor.currentIcon.cameraOrtho ? 1 : 0, listOptions);

			//---Temporary variables for undo---//
			float tmpCamSize = iconEditor.currentIcon.cameraSize;
			float tmpScaleFactor = iconEditor.currentIcon.camerasScaleFactor;
			float tmpCamFov = iconEditor.currentIcon.cameraFov;

			//---If projection mode is ortho---//
			if (tmpOrtho == 1)
			{
				GUILayout.BeginHorizontal();
				//---Draw camera size field---//
				tmpCamSize = EditorGUILayout.FloatField("Size", iconEditor.currentIcon.cameraSize);

				//---Draw auto button---//
				if (GUILayout.Button("Auto", GUILayout.Width(50)))
				{
					tmpCamSize = Utils.GetCameraAuto(iconEditor.currentIcon, Utils.GetObjectBounds(iconEditor.currentIcon));
				}

				//---Draw auto all selected button---//
				if (GUILayout.Button("Auto All Selected Icons", GUILayout.Width(150)))
				{
					//---Loop through all selected icons---//
					int index = 1;
					foreach (Icon icon in iconEditor.assetGrid.selectedIcons)
					{
						//---Draw progress bar---//
						EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (iconEditor.assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (iconEditor.assetGrid.selectedIcons.Count - 1)));

						//---Get auto camera size---//
						if (icon != iconEditor.currentIcon)
						{
							icon.cameraSize = Utils.GetCameraAuto(icon, Utils.GetObjectBounds(icon));
							Utils.UpdateIcon(icon, iconEditor);
						}
						else
						{
							tmpCamSize = Utils.GetCameraAuto(iconEditor.currentIcon, Utils.GetObjectBounds(iconEditor.currentIcon));
						}
					}

					//---Clear progress bar---//
					EditorUtility.ClearProgressBar();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				//---Draw scale factor field---//
				tmpScaleFactor = EditorGUILayout.FloatField("Scale Factor", iconEditor.currentIcon.camerasScaleFactor);

				//---Draw apply scale factor to all selected button---//
				if (GUILayout.Button("Apply to All Selected Icons", GUILayout.Width(170)))
				{
					//---Register object for under---//
					Undo.RegisterCompleteObjectUndo(iconEditor.window, "Apply scale factor to all icons");

					//---Loop through all selected icons---//
					int index = 1;
					foreach (Icon icon in iconEditor.assetGrid.selectedIcons)
					{
						//---If not current icon then apply the scale factor---//
						if (icon != iconEditor.currentIcon)
						{
							EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (iconEditor.assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (iconEditor.assetGrid.selectedIcons.Count - 1)));
							icon.camerasScaleFactor = iconEditor.currentIcon.camerasScaleFactor;
							icon.perspLastScale = iconEditor.currentIcon.perspLastScale;
							icon.saveData = true;
							Utils.UpdateIcon(icon, iconEditor);
						}
					}
					EditorUtility.ClearProgressBar();
				}
				GUILayout.EndHorizontal();
			}
			//---If perspective projection mode---//
			else
			{
				//---Draw FOV field---//
				tmpCamFov = EditorGUILayout.FloatField("Field of View", iconEditor.currentIcon.cameraFov);
			}

			EditorGUIUtility.labelWidth = tmpWdith;
			//---If any field changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.window, "Edit Icon Camera");

				//---Apply camera position, point of focus, and projection mode---//
				iconEditor.currentIcon.cameraPosition = tmpCamPos;
				iconEditor.currentIcon.cameraTarget = tmpCamTgt;
				iconEditor.currentIcon.cameraOrtho = tmpOrtho == 1 ? true : false;

				//---If projection mode is orthographic---//
				if (iconEditor.currentIcon.cameraOrtho)
				{
					//---Apply size and scale factor, and perspLastScale---//
					iconEditor.currentIcon.perspLastScale = iconEditor.currentIcon.camerasScaleFactor;
					iconEditor.currentIcon.cameraSize = tmpCamSize;
					iconEditor.currentIcon.camerasScaleFactor = tmpScaleFactor;
				}
				//---If projection mode is perspective---//
				else
				{
					//---Apply FOV and scale camera position according to perspLastScale---//
					iconEditor.currentIcon.cameraFov = tmpCamFov;
					iconEditor.currentIcon.cameraPosition *= iconEditor.currentIcon.perspLastScale / iconEditor.currentIcon.camerasScaleFactor;
				}

				//---Save and update icon---//
				iconEditor.currentIcon.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawLightingControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw ambient light field---///
			Color tmpAmbLight = EditorGUILayout.ColorField("Ambient Light Colour", iconEditor.currentIcon.ambientLightColour);

			//---Draw diretional light fields---//
			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Directional Light");
			Color tmpLightColour = EditorGUILayout.ColorField("Colour", iconEditor.currentIcon.lightColour);
			Vector3 tmpLightDir = EditorGUILayout.Vector3Field("Rotation", iconEditor.currentIcon.lightDir);
			float tmpLightIntensity = EditorGUILayout.FloatField("Intensity", iconEditor.currentIcon.lightIntensity);

			//---If any fields have changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.window, "Edit Icon Lighting");

				//---Apply settings---//
				iconEditor.currentIcon.ambientLightColour = tmpAmbLight;
				iconEditor.currentIcon.lightColour = tmpLightColour;
				iconEditor.currentIcon.lightDir = tmpLightDir;
				iconEditor.currentIcon.lightIntensity = tmpLightIntensity;

				//---Update and save icon---//
				iconEditor.currentIcon.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawAnimationControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw animation clip field---//
			AnimationClip c = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", iconEditor.currentIcon.animationClip, typeof(AnimationClip), false);

			//---Draw animation offset field---//
			Rect r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
			float offset = EditorGUI.Slider(r, "Animation Offset", iconEditor.currentIcon.animationOffset, 0, 1);

			//---If any fields have changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.window, "Edit Icon Animations");

				//---Apply settings---//
				iconEditor.currentIcon.animationClip = c;
				iconEditor.currentIcon.animationOffset = offset;

				//---Update and save icon---//
				iconEditor.updateFlag = true;
				iconEditor.currentIcon.saveData = true;
			}
		}

		public static void DrawPostProcessingControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw filter mode field---//
			FilterMode tmpFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", iconEditor.currentIcon.filterMode);

			//---If filter mode changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//

				//---Apply sttings, update icon---//
				Undo.RecordObject(iconEditor.window, "Change Filter Mode");
				iconEditor.currentIcon.filterMode = tmpFilterMode;
				iconEditor.updateFlag = true;
			}

			//---Draw reorderable list---//
			iconEditor.reorderableList.list = iconEditor.currentIcon.postProcessingMaterials;
			iconEditor.reorderableList.index = (int)Mathf.Clamp(iconEditor.reorderableList.index, 0, iconEditor.reorderableList.list.Count - 1);
			GUILayout.Space(2);
			iconEditor.reorderableList.DoLayoutList();

			//---Begin change check---//
			EditorGUI.BeginChangeCheck();
			if (iconEditor.reorderableList.list.Count > 0)
			{
				//---Draw shader settings label---//
				string shaderName = iconEditor.currentIcon.postProcessingMaterials[iconEditor.reorderableList.index].shader.name;
				GUILayout.Label("Shader Settings (" + shaderName + ")");
				GUILayout.BeginVertical(GUI.skin.box);

				//---If not the default render shader---//
				if (shaderName != "RapidIcon/ObjectRender")
				{
					//---Get the material properties---//
					UnityEngine.Object[] obj = new UnityEngine.Object[1];
					obj[0] = (UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index];
					MaterialProperty[] props = MaterialEditor.GetMaterialProperties(obj);

					//---If the shader doesn't have a custom GUI---//
					if (iconEditor.materialEditor.customShaderGUI == null)
					{
						//---Draw the properties fields---//
						foreach (MaterialProperty prop in props)
						{
							if (prop.name != "_MainTex" && prop.flags != MaterialProperty.PropFlags.HideInInspector)
							{
								iconEditor.materialEditor.ShaderProperty(prop, prop.displayName);
							}
						}
					}
					else
					//---If the shader does have a custom GUI---//
					{

						//---Get list of all properties---//
						List<MaterialProperty> list = new List<MaterialProperty>(props);

						//---Remove the property from list if it is _MainTex---//
						foreach (MaterialProperty prop in list)
						{
							if (prop.name == "_MainTex")
							{
								list.Remove(prop);
								break;
							}
						}

						//---Draw the custom GUI---//
						props = list.ToArray();
						iconEditor.materialEditor.customShaderGUI.OnGUI(iconEditor.materialEditor, props);
					}

				}
				//---If the default render shader---//
				else
				{
					//---Begine change check---//
					EditorGUI.BeginChangeCheck();

					//---Draw fix alpha edges toggle---//
					Icon.FixEdgesModes tmpFixEdges = (Icon.FixEdgesModes)EditorGUILayout.EnumPopup("Edge Fix Mode", iconEditor.currentIcon.fixEdgesMode);

					//---If toggle changed---//
					if (EditorGUI.EndChangeCheck())
					{
						//---Record object for undo and apply---//
						Undo.RecordObject(iconEditor.window, "Toggle Icon Premultiplied Alpha");
						iconEditor.currentIcon.fixEdgesMode = tmpFixEdges;

						//---Update and save icon---//
						iconEditor.currentIcon.saveData = true;
						iconEditor.updateFlag = true;
					}
				}
				GUILayout.EndVertical();
			}

			//---If any fields changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Save material info and update icon---//
				iconEditor.currentIcon.SaveMatInfo();
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawExportControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw export resolution field, limit to 8x8 - 2048x2048---//
			Vector2Int tmpExportRes = EditorGUILayout.Vector2IntField("Export Resolution", iconEditor.currentIcon.exportResolution);
			tmpExportRes.x = (int)Mathf.Clamp(tmpExportRes.x, 8, 2048);
			tmpExportRes.y = (int)Mathf.Clamp(tmpExportRes.y, 8, 2048);

			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
			//---Add forward slash to end of export path if missing---//
			if (iconEditor.currentIcon.exportFolderPath[iconEditor.currentIcon.exportFolderPath.Length - 1] != '/')
				iconEditor.currentIcon.exportFolderPath += "/";

			//---Draw export folder field---//
			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			iconEditor.currentIcon.exportFolderPath = EditorGUILayout.TextField("Export Folder", iconEditor.currentIcon.exportFolderPath);

			//---If export path is blank, change it to default Export folder---//
			string tmpFolder = iconEditor.currentIcon.exportFolderPath;
			if (tmpFolder == "")
			{
				string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
				string rapidIconRootFolder = "";
				for (int i = 0; i < split.Length - 3; i++)
					rapidIconRootFolder += split[i] + "/";

				iconEditor.currentIcon.exportFolderPath = rapidIconRootFolder + "Exports/";
			}

			//---Draw button to open file explorer to select export folder---//
			EditorGUIUtility.labelWidth = tmpWdith;
			if (GUILayout.Button("Browse", GUILayout.Width(100)))
			{
				string folder = EditorUtility.OpenFolderPanel("Export Folder", iconEditor.currentIcon.exportFolderPath, "");
				if (folder != "")
				{
					string dataPath = Application.dataPath;
					dataPath = dataPath.Substring(0, dataPath.LastIndexOf('/') + 1);
					folder = folder.Replace(dataPath, "");
					iconEditor.currentIcon.exportFolderPath = folder;
				}
			}
			GUILayout.EndHorizontal();

			//---Draw export path prefix/suffix fields---//
			tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			string exportPrefix = EditorGUILayout.TextField("Export Prefix", iconEditor.currentIcon.exportPrefix);
			string exportSuffix = EditorGUILayout.TextField("Export Suffix", iconEditor.currentIcon.exportSuffix);

			//---If any fields changes---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.window, "Edit Icon Export Resolution");

				//---Update icon only if resolution has changed and is valid---//
				bool doUpdate = tmpExportRes != iconEditor.currentIcon.exportResolution;
				if (doUpdate && iconEditor.currentIcon.exportResolution.x > 0 && iconEditor.currentIcon.exportResolution.y > 0)
				{
					iconEditor.updateFlag = true;
				}

				//---Save icon---//
				iconEditor.currentIcon.saveData = true;

				//---Apply settings---//
				iconEditor.currentIcon.exportResolution = tmpExportRes;
				iconEditor.currentIcon.exportPrefix = exportPrefix;
				iconEditor.currentIcon.exportSuffix = exportSuffix;
				iconEditor.currentIcon.saveData = true;
			}

			//---Add button apply to all selected icons---//
			if (GUILayout.Button("Apply to All Selected Icons"))
				Utils.ApplyToAllSelectedIcons(5, iconEditor);

			//---Draw a separator---//
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect r = GUILayoutUtility.GetRect(iconEditor.sepWidth, 1);
			if (iconEditor.separatorTex == null)
			{
				if (EditorGUIUtility.isProSkin)
					iconEditor.separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
				else
					iconEditor.separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));
			}
			GUI.DrawTexture(r, iconEditor.separatorTex);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(12);

			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw export name field---//
			string exportName = EditorGUILayout.TextField("Export Name", iconEditor.currentIcon.exportName);

			//---Draw label showing full name with prefix/suffix---//
			EditorGUIUtility.labelWidth = tmpWdith;
			GUILayout.Label("Full Export Name: " + iconEditor.currentIcon.exportPrefix + iconEditor.currentIcon.exportName + iconEditor.currentIcon.exportSuffix + ".png");

			//---If fields changesd, and export name is valid---//
			if (EditorGUI.EndChangeCheck())
			{
				if (exportName.Length > 0)
				{
					//---Apply and save---//
					iconEditor.currentIcon.exportName = exportName;
					iconEditor.currentIcon.saveData = true;
				}
			}

			//---Draw button to export icon---//
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Export Icon", GUILayout.Width(160)))
			{
				//---Export icon---//
				Utils.ExportIcon(iconEditor.currentIcon, false, iconEditor);
				Utils.FinishExportIcon(iconEditor.currentIcon);

				//---Reset replaceAll---//
				iconEditor.replaceAll = false;
			}

			//---Draw button to export all selected icons---//
			if (GUILayout.Button("Export Selected Icons", GUILayout.Width(160)))
			{
				//---Start asset editing---//
				AssetDatabase.StartAssetEditing();

				//---Loop through all selected icons---//
				int index = 1;
				List<string> exportPaths = new List<string>();
				foreach (Icon icon in iconEditor.assetGrid.selectedIcons)
				{
					//--Show progress bar---//
					EditorUtility.DisplayProgressBar("Exporting Icons (" + index + "/" + iconEditor.assetGrid.selectedIcons.Count + ")", icon.assetPath, ((float)index++ / iconEditor.assetGrid.selectedIcons.Count));

					//---Export icon---//
					Utils.ExportIcon(icon, iconEditor.assetGrid.selectedIcons.Count > 1 ? true : false, iconEditor);
				}

				//---Clear progress bar---//
				EditorUtility.ClearProgressBar();

				//---Stop asset editing and finish export---//
				AssetDatabase.StopAssetEditing();
				Utils.FinishExportIcon(iconEditor.assetGrid.selectedIcons);

				//---Reset replaceAll---//
				iconEditor.replaceAll = false;
			}
			GUILayout.EndHorizontal();
		}
	}
}