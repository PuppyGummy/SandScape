using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	static class Utils
	{
		public static Bounds GetObjectBounds(Icon icon)
		{
			//---Get object---//
			GameObject go = (GameObject)icon.assetObject;

			//---Store the prefabs position before zero-ing it---//
			Vector3 prefabPos = go.transform.position;
			go.transform.position = Vector3.zero;

			//---Create a bounds object and encapsulate the bounds of the object mesh---//
			MeshRenderer mr = go.GetComponent<MeshRenderer>();
			Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
			if (mr != null)
				bounds.Encapsulate(mr.bounds);
			else
			{
				SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
				if (smr != null)
					bounds.Encapsulate(smr.bounds);
			}

			//---Encapsulate the bounds of the object's children objects as well---//
			Utils.EncapsulateChildBounds(go.transform, ref bounds);

			//---Reset the prefab postion to the stored value---//
			go.transform.position = prefabPos;

			return bounds;
		}

		public static Vector3 GetObjectAutoOffset(Icon icon, Bounds bounds)
		{
			//Apply the offset to the icon object position
			return -bounds.center;
		}

		public static float GetCameraAuto(Icon icon, Bounds bounds)
		{
			//---Scale camera size and position so that the object fits in the render---//
			Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), 1.05f * Vector3.one);
			Vector3 corner = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			corner = trs * corner;

			Vector2 refB2 = new Vector2(0.74f, 0.53f);
			Vector2 b2 = refB2 * corner.magnitude;

			return b2.magnitude;
		}

		public static void ExportIcon(Icon icon, bool inBatchExport, IconEditor iconEditor)
		{
			//---Create export folder if it doesn't already exist---//
			if (!Directory.Exists(icon.exportFolderPath))
				Directory.CreateDirectory(icon.exportFolderPath);

			//---Get the full export file name---//
			string fileName = icon.exportFolderPath + icon.exportPrefix + icon.exportName + icon.exportSuffix + ".png";

			//---If it exists already, check if user want's to replace it---//
			if (System.IO.File.Exists(fileName) && !iconEditor.replaceAll)
			{
				int result = 1;

				if (inBatchExport)
					result = EditorUtility.DisplayDialogComplex("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Skip", "Replace All");
				else
					result = EditorUtility.DisplayDialog("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Cancel") ? 0 : 1;

				if (result == 1)
					return;
				else if (result == 2)
				{
					iconEditor.replaceAll = true;
				}
			}

			//---Delete any existing file---//
			if (AssetDatabase.IsValidFolder(icon.exportFolderPath))
				AssetDatabase.DeleteAsset(fileName);

			//---Render the icon---//
			Texture2D exportRender = Utils.RenderIcon(icon, icon.exportResolution.x, icon.exportResolution.y);

			//---Encode to png and save file---//
			byte[] bytes = exportRender.EncodeToPNG();
			File.WriteAllBytes(fileName, bytes);
		}

		public static void FinishExportIcon(List<Icon> icons)
		{
			//---Loop through all icons in list and finish export---//
			foreach (Icon icon in icons)
				FinishExportIcon(icon);
		}

		public static void FinishExportIcon(Icon icon)
		{
			//---Refresh asset database and get full filename---//
			AssetDatabase.Refresh();
			string fileName = icon.exportFolderPath + icon.exportPrefix + icon.exportName + icon.exportSuffix + ".png";

			if (AssetDatabase.IsValidFolder(icon.exportFolderPath.Substring(0, icon.exportFolderPath.Length - 1)))
			{
				//---Set the importer settings---//
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
				textureImporter.alphaIsTransparency = true;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
				textureImporter.SaveAndReimport();
				textureImporter.filterMode = icon.filterMode;
			}

			//---Refresh asset database---//
			AssetDatabase.Refresh();
		}

		public static void UpdateIcon(Icon icon, IconEditor iconEditor)
		{
			//---Get the render resolution---//
			iconEditor.renderResolution = Utils.MutiplyVector2IntByFloat(iconEditor.currentIcon.exportResolution, iconEditor.resMultiplyers[iconEditor.resMultiplyerIndex]);

			//---Update the icon render---//
			icon.Update(iconEditor.renderResolution, new Vector2Int(128, (int)(((float)iconEditor.renderResolution.y / (float)iconEditor.renderResolution.x) * 128)));
		}

		public static void ApplyToAllSelectedIcons(int tab, IconEditor iconEditor)
		{
			//---Display confirmation window---//
			int result = EditorUtility.DisplayDialogComplex("Apply to All Selected Icons", "Would you like to apply only " + iconEditor.tabNames[tab] + " settings, or all settings?", iconEditor.tabNames[tab] + " Settings Only", "Cancel", "All Settings");

			if (result == 1) //cancel
				return;
			else
			{
				//---Record object for undo---//
				Undo.RegisterCompleteObjectUndo(iconEditor.window, "Apply to all icons");

				//---Loop through selected icons---//
				int index = 1;
				foreach (Icon icon in iconEditor.assetGrid.selectedIcons)
				{
					//---If not the current icon---//
					if (icon != iconEditor.currentIcon)
					{
						//---Display progress bar---//
						EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (iconEditor.assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (iconEditor.assetGrid.selectedIcons.Count - 1)));

						//---Copy icon settings from current icon to this icon---//
						CopyIconSettings(iconEditor.currentIcon, icon, result == 2 ? -1 : tab);

						//--Save the icon---//
						icon.SaveMatInfo();
						icon.saveData = true;

						//---Update icon if all settings copied, or if any tab other than export copied (or export resolution changed---//
						if ((result == 2) | !(tab == 5 && icon.exportResolution == iconEditor.currentIcon.exportResolution))
							Utils.UpdateIcon(icon, iconEditor);
					}
				}

				//---If the post-processing tab has been copied, then create a new material editor---//
				if (tab == 4 || tab == -1)
				{
					Editor.DestroyImmediate(iconEditor.materialEditor);
					if (iconEditor.reorderableList.list != null && iconEditor.reorderableList.list.Count > 0)
						iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);
				}

				//---Clear the progress bar---//
				EditorUtility.ClearProgressBar();
			}
		}

		public static void CopyIconSettings(Icon from, Icon to, int tab)
		{
			//---Copy object settings---//
			if (tab == 0 || tab == -1)
			{
				to.objectPosition = from.objectPosition;
				to.objectRotation = from.objectRotation;
				to.objectScale = from.objectScale;
			}

			//---Copy camera settings---//
			if (tab == 1 || tab == -1)
			{
				to.cameraPosition = from.cameraPosition;
				to.cameraOrtho = from.cameraOrtho;
				to.cameraFov = from.cameraFov;
				to.cameraSize = from.cameraSize;
				to.camerasScaleFactor = from.camerasScaleFactor;
				to.perspLastScale = from.perspLastScale;
				to.cameraTarget = from.cameraTarget;
			}

			//--Copy lighting settings---//
			if (tab == 2 || tab == -1)
			{
				to.ambientLightColour = from.ambientLightColour;
				to.lightColour = from.lightColour;
				to.lightDir = from.lightDir;
				to.lightIntensity = from.lightIntensity;
			}

			//---Copy animation settings---//
			if (tab == 3 || tab == -1)
			{
				to.animationClip = from.animationClip;
				to.animationOffset = from.animationOffset;
			}

			//---Copy post-processing settings---//
			if (tab == 4 || tab == -1)
			{
				foreach (Material mat in to.postProcessingMaterials)
					Editor.DestroyImmediate(mat);

				to.postProcessingMaterials.Clear();

				foreach (Material mat in from.postProcessingMaterials)
				{
					Material newMat = new Material(mat);
					to.postProcessingMaterials.Add(newMat);
					to.materialDisplayNames.Add(newMat, from.materialDisplayNames[mat]);
					to.materialToggles.Add(newMat, from.materialToggles[mat]);
				}

				to.fixEdgesMode = from.fixEdgesMode;
				to.filterMode = from.filterMode;
			}

			//---Copy export settings---//
			if (tab == 5 || tab == -1)
			{
				to.exportResolution = from.exportResolution;
				to.exportFolderPath = from.exportFolderPath;
				to.exportPrefix = from.exportPrefix;
				to.exportSuffix = from.exportSuffix;
			}

			//---Save the icon---//
			to.saveData = true;
		}

		public static Texture2D CreateColourTexture(int width, int height, Color32 c)
		{
			//---Create new texture---//
			Texture2D tex = new Texture2D(width, height);

			//---Loop through pixels and set colour---//
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					tex.SetPixel(x, y, c);
				}
			}

			//---Apply changes and set filter mode---//
			tex.Apply();
			tex.filterMode = FilterMode.Point;

			return tex;
		}

		public static Texture2D RenderIcon(Icon icon, int width = 128, int height = 128)
		{
			//---Create stage and scene---//
			var scene = EditorSceneManager.NewPreviewScene();
			if (scene == null)
			{
				Debug.LogError("Error creating RapidIcon preview scene");
				return Utils.CreateColourTexture(width, height, Color.clear);
			}

			var stage = ScriptableObject.CreateInstance<RapidIconStage>();
			if (stage == null)
			{
				Debug.LogError("Error creating RapidIcon stage");
				return Utils.CreateColourTexture(width, height, Color.clear);
			}

			stage.SetScene(scene);

			//---Go to stage---//
			StageUtility.GoToStage(stage, true);

			//---Setup scene---//
			stage.SetupScene(icon);

			//and render icon---//
			Texture2D render = stage.RenderIcon(width, height);

			//---Fix alpha edges---//
			if (icon.fixEdgesMode == Icon.FixEdgesModes.Regular || icon.fixEdgesMode == Icon.FixEdgesModes.WithDepthTexture)
				render = Utils.FixAlphaEdges(render, icon.fixEdgesMode == Icon.FixEdgesModes.WithDepthTexture);

			//---Known bug---//
			if (icon.materialToggles == null)
			{
				icon.LoadMatInfo();
				Debug.LogError("[RapidIcon] Undo Error: This is a known bug, if you \"Apply to All Selected Icons\" and then try to undo after changing your icon selection, the tool will not be able to undo the changes.");
			}

			//---Apply post-processing shaders---//
			Texture2D img = new Texture2D(width, height);
			img.Reinitialize(width, height);
			foreach (Material m in icon.postProcessingMaterials)
			{
				if (icon.materialToggles != null)
				{
					if (icon.materialToggles[m])
					{
						var rtd = new RenderTextureDescriptor(img.width, img.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
						var rt = new RenderTexture(rtd);

						if (m == null)
							continue;

						if (m.shader.name == "RapidIcon/ObjectRender")
							m.SetTexture("_Render", render);

						Graphics.Blit(img, rt, m);

						RenderTexture.active = rt;
						img = new Texture2D(img.width, img.height);
						img.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
						img.Apply();
						RenderTexture.active = null;
						rt.Release();
					}
				}
			}

			//---Apply filter mode---//
			img.filterMode = icon.filterMode;

			//---Cleanup stage and scene---//
			StageUtility.GoToMainStage();
			EditorSceneManager.ClosePreviewScene(scene);
			ScriptableObject.DestroyImmediate(stage);

			return img;
		}

		public static void CheckCurrentIconRender(IconEditor iconEditor)
		{
			//---If the current icon doesn't have a full render, then render one---//
			if (!iconEditor.currentIcon.fullRender)
			{
				iconEditor.renderResolution = Utils.MutiplyVector2IntByFloat(iconEditor.currentIcon.exportResolution, iconEditor.resMultiplyers[iconEditor.resMultiplyerIndex]);
				iconEditor.currentIcon.fullRender = Utils.RenderIcon(iconEditor.currentIcon, iconEditor.renderResolution.x, iconEditor.renderResolution.y);
				iconEditor.currentIcon.fullRender.hideFlags = HideFlags.DontSave;
			}
		}

		public static Texture2D FixAlphaEdges(Texture2D tex, bool useDepthTex)
		{
			//---Create render texture---//
			var rtd = new RenderTextureDescriptor(tex.width, tex.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);

			//---Blit texture to render texture using ImgShader---//
			Material mat = new Material(Shader.Find("RapidIcon/ImgShader"));
			mat.SetInt("_UseDepthTexture", useDepthTex ? 1 : 0); 
			Graphics.Blit(tex, rt, mat); 

			//---Copy the render texture to Texture2D---//
			RenderTexture.active = rt;
			Texture2D baked = new Texture2D(tex.width, tex.height);
			baked.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			baked.Apply();

			//---Cleanup---//
			RenderTexture.active = null;
			rt.Release();

			return baked;
		}

		public static void SaveIconData(IconData iconData)
		{
			//---Perpare each icon for saving---//
			foreach (Icon icon in iconData.icons)
			{
				icon.PrepareForSaveData();
			}

			//---Save the data---//
			string data = JsonUtility.ToJson(iconData);
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconData", data);
		}

		public static IconData LoadIconData()
		{
			//---Load the icon data---//
			string data = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconData");
			IconData iconData = JsonUtility.FromJson<IconData>(data);

			//---Complete the load data for each icon---//
			if (iconData != null)
			{
				//---Handle version control---//
				VersionControl.CheckUpdate(iconData.icons);
				bool loadFixEdgesModeFromSave = !(VersionControl.GetStoredVersion() < new VersionControl.Version("1.6.2")); //Keep at 1.6.2, do not update to latest version

				foreach (Icon icon in iconData.icons)
					icon.CompleteLoadData(loadFixEdgesModeFromSave);
			}

			//---Update current stored version---//
			VersionControl.UpdateStoredVersion();

			return iconData;
		}

		//---Extension method to convert Vector2 to Vector2Int---//
		public static Vector2Int ToVector2Int(this Vector2 v)
		{
			return new Vector2Int((int)v.x, (int)v.y);
		}

		public static void EncapsulateChildBounds(Transform t, ref Bounds bounds)
		{
			//---For each child object, encapsulate its bounds---//
			MeshRenderer mr;
			for (int i = 0; i < t.childCount; i++)
			{
				mr = t.GetChild(i).GetComponent<MeshRenderer>();
				if (mr != null)
					bounds.Encapsulate(mr.bounds);
				else
				{
					SkinnedMeshRenderer smr = t.GetChild(i).GetComponent<SkinnedMeshRenderer>();
					if (smr != null)
						bounds.Encapsulate(smr.bounds);
				}

				EncapsulateChildBounds(t.GetChild(i), ref bounds);
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Delete All Saved Data")]
		static void DeleteEditorPrefs()
		{
			if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete all RapidIcon data? This will delete all of your icon settings and cannot be undone", "Delete", "Cancel"))
			{
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconOpenedFolders");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedFolders");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosLeft");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedAssets");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconAssetGridScroll");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosRight");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosPreview");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewResIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewZoomIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconEditorTab");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconData");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconIconsRefreshed");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconFilterIdx");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconVersion");
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Don't Save On Close")]
		static void DontSaveOnExit()
		{
			RapidIconWindow.dontSaveOnExit = true;
		}

		public static Vector2Int MutiplyVector2IntByFloat(Vector2Int vec, float f)
		{
			Vector2Int res = vec;
			res.x = (int)(res.x * f);
			res.y = (int)(res.y * f);

			return res;
		}
	}
}