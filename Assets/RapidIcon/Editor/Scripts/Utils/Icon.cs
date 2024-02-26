using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	[Serializable]
	public class Icon
	{
		//---MatProperty Definition---//
		[Serializable]
		public struct MatProperty<T>
		{
			public string name;
			public T value;

			public MatProperty(string n, T v)
			{
				name = n;
				value = v;
			}
		}

		//---MaterialInfo Definition---//
		[Serializable]
		public struct MaterialInfo
		{
			public string shaderName;
			public string displayName;
			public bool toggle;
			public List<MatProperty<int>> intProperties;
			public List<MatProperty<float>> floatProperties;
			public List<MatProperty<float>> rangeProperties;
			public List<MatProperty<Color>> colourProperties;
			public List<MatProperty<Vector4>> vectorProperties;
			public List<MatProperty<string>> textureProperties;

			//---Constructor---//
			public MaterialInfo(string n)
			{
				shaderName = n;
				displayName = n;
				toggle = true;
				intProperties = new List<MatProperty<int>>();
				floatProperties = new List<MatProperty<float>>();
				rangeProperties = new List<MatProperty<float>>();
				colourProperties = new List<MatProperty<Color>>();
				vectorProperties = new List<MatProperty<Vector4>>();
				textureProperties = new List<MatProperty<string>>();
			}
		}

		//---Variable Definitions---//
		//Asset
		public string assetPath;
		public string assetName;
		public string assetShortenedName;
		public string folderPath;
		public UnityEngine.Object assetObject;
		public string assetGUID;
		public string[] GUIDs; //No longer used, but kept for backwards compatibility of old save data

		//Object Settings
		public Vector3 objectPosition;
		public Vector3 objectRotation;
		public Vector3 objectScale;

		//Camera Settings
		public Vector3 cameraPosition;
		public Vector3 cameraTarget;
		public bool cameraOrtho;
		public float cameraFov;
		public float cameraSize;
		public float camerasScaleFactor;
		public float perspLastScale;

		//Lighting Settings
		public Color lightColour;
		public Vector3 lightDir;
		public float lightIntensity;
		public Color ambientLightColour;

		//Animation Settings
		public AnimationClip animationClip;
		public float animationOffset;
		public string animationPath;

		//Post-Processing Settings
		public List<Material> postProcessingMaterials;
		public Dictionary<Material, String> materialDisplayNames;
		public Dictionary<Material, bool> materialToggles;
		public List<MaterialInfo> matInfo;
		public enum FixEdgesModes { None, Regular, WithDepthTexture};
		public FixEdgesModes fixEdgesMode;
		public FilterMode filterMode;

		//Export Settings
		public string exportFolderPath;
		public string exportName;
		public string exportPrefix;
		public string exportSuffix;
		public Vector2Int exportResolution;

		//Renders
		public Texture2D previewRender;
		public Texture2D fullRender;

		//Misc
		public Texture2D selectionTexture;
		public bool selected;
		public bool saveData;
		public bool deleted;
		public int assetGridIconIndex;

		public Icon(Shader objRenderShader, string rapidIconRootFolder, string path)
		{
			//---Initialise Icon---//
			//Object Settings
			objectPosition = Vector3.zero;
			objectRotation = Vector3.zero;
			objectScale = Vector3.one;

			//Camera Settings
			cameraPosition = new Vector3(1, Mathf.Sqrt(2), 1);
			perspLastScale = 1;
			cameraOrtho = true;
			cameraFov = 60;
			cameraSize = 5;
			camerasScaleFactor = 1;
			cameraTarget = Vector3.zero;

			//Lighting Settings
			ambientLightColour = Color.gray;
			lightColour = Color.white;
			lightDir = new Vector3(50, -30, 0);
			lightIntensity = 1;

			//Post-Processing Settings
			postProcessingMaterials = new List<Material>();
			matInfo = new List<MaterialInfo>();
			materialDisplayNames = new Dictionary<Material, string>();
			materialToggles = new Dictionary<Material, bool>();
			filterMode = FilterMode.Point;
			fixEdgesMode = FixEdgesModes.Regular;
			Material defaultRender = new Material(objRenderShader);
			postProcessingMaterials.Add(defaultRender);
			materialDisplayNames.Add(defaultRender, "Object Render");
			materialToggles.Add(defaultRender, true);

			//Export Settings
			exportResolution = new Vector2Int(256, 256);
			exportFolderPath = rapidIconRootFolder;
			exportFolderPath += "Exports/";
		}

		public void LoadDefaultAnimationClip()
		{
			//---Load the animation included in the imported assset (if there is one)---//
			AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
			animationClip = clip;

			//---If no clip loaded, then try and get one from the Animator component (if there is one)---//
			if (animationClip == null)
			{
				//---Try to get animator component from prefab---//
				GameObject gameObject = null;
				Animator animator = null;
				try
				{
					gameObject = PrefabUtility.LoadPrefabContents(assetPath);
					animator = gameObject.GetComponent<Animator>();
				}
				catch
				{ /*Not a prefab - don't need to do anything just catch the error*/ }

				//---If prefab has Animator component, then try to get first animation clip---//
				if (animator != null)
				{
					AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
					if (animatorController != null && animatorController.animationClips != null && animatorController.animationClips.Length > 0)
						animationClip = animatorController.animationClips[0];
				}

				//---Unload the prefab---//
				if (gameObject != null)
					PrefabUtility.UnloadPrefabContents(gameObject);
			}
		}

		public void Update(Vector2Int fullRenderSize)
		{
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void Update(Vector2Int fullRenderSize, Vector2Int previewSize)
		{
			previewRender = Utils.RenderIcon(this, previewSize.x, previewSize.y);
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void CompleteLoadData(bool loadFixEdgesMode)
		{
			//---Load asset from GUID---//
			if (assetGUID != string.Empty)
			{
				assetObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGUID));
			}

			//---Load animation from path---//
			if (animationPath != string.Empty)
			{
				animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(animationPath, typeof(AnimationClip));
			}

			//---Load the post-processing material info---//
			LoadMatInfo(loadFixEdgesMode);
		}

		public void PrepareForSaveData()
		{
			//---Get GUID of asset---//
			assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assetObject));

			//---Get path of animation---//
			animationPath = AssetDatabase.GetAssetPath(animationClip);

			//---Clear the renders and assset object---//
			previewRender = fullRender = null;
			assetObject = null;

			//---Save the post-processing material info---//
			SaveMatInfo();

			//---Clear the post-processing list/dictionaries---//
			postProcessingMaterials.Clear();
			materialDisplayNames.Clear();
			materialToggles.Clear();
		}

		public void LoadMatInfo(bool loadFixEdgesMode = true)
		{
			//---Initialise post-processing list/dictionaries---//
			postProcessingMaterials = new List<Material>();
			materialDisplayNames = new Dictionary<Material, string>();
			materialToggles = new Dictionary<Material, bool>();

			if (matInfo != null && matInfo.Count > 0 && saveData)
			{
				//---For each material, load it's properties---//
				foreach (MaterialInfo mi in matInfo)
				{
					//Create new material with the correct shader
					Material m = new Material(Shader.Find(mi.shaderName));

					//Set int properties (2021.1 or newer)
					bool reg = false;
					bool depthTex = false;
					foreach (MatProperty<int> property in mi.intProperties)
					{
						//Custom property for default render shader
						if (mi.shaderName == "RapidIcon/ObjectRender")
						{
							if(property.name == "custom_PreMulAlpha")
							{
								reg = (property.value == 1 ? true : false);
							}


							if (property.name == "custom_UseDepthTexture")
							{
								depthTex = (property.value == 1 ? true : false);
							}
						}

#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						m.SetInt(property.name, property.value);
#endif
					}

					//---Handle default render shader---//
					if (loadFixEdgesMode && mi.shaderName == "RapidIcon/ObjectRender")
					{
						if (reg && depthTex)
							fixEdgesMode = FixEdgesModes.WithDepthTexture;
						else if (reg)
							fixEdgesMode = FixEdgesModes.Regular;
						else
							fixEdgesMode = FixEdgesModes.None;
					}

					//Set float properties
					foreach (MatProperty<float> property in mi.floatProperties)
						m.SetFloat(property.name, property.value);

					//Set range properties
					foreach (MatProperty<float> property in mi.rangeProperties)
						m.SetFloat(property.name, property.value);

					//Set colour properties
					foreach (MatProperty<Color> property in mi.colourProperties)
						m.SetColor(property.name, property.value);

					//Set vector properties
					foreach (MatProperty<Vector4> property in mi.vectorProperties)
						m.SetVector(property.name, property.value);

					//Set texture properties
					foreach (MatProperty<string> property in mi.textureProperties)
					{
						//Load the texture from GUID, if not null
						if (property.name != "null")
						{
							m.SetTexture(property.name, (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(property.value)));
						}
					}

					//Add the material to the post-processing stack
					postProcessingMaterials.Add(m);
					materialDisplayNames.Add(m, mi.displayName);
					materialToggles.Add(m, mi.toggle);
				}
			}
		}

		public void SaveMatInfo()
		{
			//---Clear the matInfo and then store the info for each material in the post-processing stack---//
			matInfo.Clear();
			foreach (Material mat in postProcessingMaterials)
			{
				if (mat == null)
					continue;

				//---Create the new material info---//
				MaterialInfo mi = new MaterialInfo(mat.shader.name);

				//---Store all of the material's properties---//
				int propCount = mat.shader.GetPropertyCount();
				for (int i = 0; i < propCount; i++)
				{
					//---Get properties name and type---//
					string propName = mat.shader.GetPropertyName(i);
					UnityEngine.Rendering.ShaderPropertyType propType = mat.shader.GetPropertyType(i);

					switch (propType)
					{
						//---Store int properties (2021.1 or newer)---///
#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						case UnityEngine.Rendering.ShaderPropertyType.Int:
							mi.intProperties.Add(new MatProperty<int>(propName, mat.GetInt(propName)));
							break;
#endif
						//---Store float properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Float:
							mi.floatProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						//---Store range properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Range:
							mi.rangeProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						//---Store colour properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Color:
							mi.colourProperties.Add(new MatProperty<Color>(propName, mat.GetColor(propName)));
							break;

						//---Store vector properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Vector:
							mi.vectorProperties.Add(new MatProperty<Vector4>(propName, mat.GetVector(propName)));
							break;

						//---Store texture properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Texture:
							Texture t = mat.GetTexture(propName);
							if (t != null)
								//Store GUID of texture
								mi.textureProperties.Add(new MatProperty<string>(propName, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t))));
							else
								mi.textureProperties.Add(new MatProperty<string>(propName, "null"));
							break;
					}

				}

				//---Add custom property for default render shader---//
				if (mat.shader.name == "RapidIcon/ObjectRender")
				{
					bool enable = (fixEdgesMode == FixEdgesModes.Regular) || (fixEdgesMode == FixEdgesModes.WithDepthTexture);
					mi.intProperties.Add(new MatProperty<int>("custom_PreMulAlpha", enable ? 1 : 0));
					mi.intProperties.Add(new MatProperty<int>("custom_UseDepthTexture", (fixEdgesMode == FixEdgesModes.WithDepthTexture) ? 1 : 0));
				}

				//---Store the name and toggle state, then add the matInfo to the list of all matInfos to be save---//
				mi.displayName = materialDisplayNames[mat];
				mi.toggle = materialToggles[mat];
				matInfo.Add(mi);
			}
		}
	}
}