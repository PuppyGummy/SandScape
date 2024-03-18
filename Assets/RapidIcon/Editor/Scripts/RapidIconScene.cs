using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal; //URP only

namespace RapidIcon_1_6_2
{
	public class RapidIconStage : PreviewSceneStage
	{
		//---INTERNAL---//
		Camera cam;
		Color ambientLightColour;
		AmbientMode ambientMode;
		bool fogEnabled;

		public void SetScene(UnityEngine.SceneManagement.Scene scene_in)
		{
			this.scene = scene_in;
		}

		public void SetupScene(Icon icon)
		{
			//---Create scene objects---//
			GameObject obj = GameObject.Instantiate((GameObject)icon.assetObject);
			GameObject camGO = new GameObject("camera");
			GameObject lightGO = new GameObject("light");

			//---Place the objects in the scene---//
			StageUtility.PlaceGameObjectInCurrentStage(camGO);
			StageUtility.PlaceGameObjectInCurrentStage(lightGO);
			StageUtility.PlaceGameObjectInCurrentStage(obj);

			//---Apply the object settings---//
			obj.transform.position = icon.objectPosition;
			obj.transform.localScale = icon.objectScale;
			obj.transform.eulerAngles = icon.objectRotation;

			//---Add the camera component and apply camera settings---//
			cam = camGO.AddComponent<Camera>();
			cam.scene = this.scene;
			cam.clearFlags = CameraClearFlags.Nothing;
			cam.transform.position = icon.cameraPosition;
			cam.transform.LookAt(icon.cameraTarget);
			cam.orthographic = icon.cameraOrtho;
			cam.orthographicSize = icon.cameraSize;
			cam.orthographicSize /= icon.camerasScaleFactor;
			cam.fieldOfView = icon.cameraFov;
			cam.nearClipPlane = 0.001f;
			cam.farClipPlane = 10000;
			cam.depthTextureMode = DepthTextureMode.Depth;
			cam.clearFlags = CameraClearFlags.Color;
			//cam.GetUniversalAdditionalCameraData().renderPostProcessing = false; //URP only

			//---Add light componenet and apply lighting settings---//
			Light dirLight = lightGO.AddComponent<Light>();
			dirLight.type = LightType.Directional;
			dirLight.color = icon.lightColour;
			dirLight.transform.eulerAngles = icon.lightDir;
			dirLight.intensity = icon.lightIntensity;

			//---Store current environment settings---//
			ambientLightColour = RenderSettings.ambientLight;
			ambientMode = RenderSettings.ambientMode;
			fogEnabled = RenderSettings.fog;

			//---Apply environment settings---//
			RenderSettings.ambientLight = icon.ambientLightColour;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.fog = false;

			//---Apply animation settings---//
			if (icon.animationClip != null)
			{
				float t = icon.animationClip.length * icon.animationOffset;
				icon.animationClip.SampleAnimation(obj, t);
			}
		}

		public Texture2D RenderIcon(int width, int height)
		{
			//---Setup render texture---//
			width = Mathf.Clamp(width, 8, 2048);
			height = Mathf.Clamp(height, 8, 2048);
			var rtd = new RenderTextureDescriptor(width, height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);
			rt.Create();

			//---Render camera to render texture---//
			cam.targetTexture = rt;
			cam.aspect = (float)width / (float)height;
			cam.Render();
			cam.targetTexture = null;
			cam.ResetAspect();

			//---Convert render texture to Texture2D---//
			Texture2D render = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
			var oldActive = RenderTexture.active;
			RenderTexture.active = rt;
			render.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			render.Apply();

			//---Cleanup render texture---//
			RenderTexture.active = oldActive;
			rt.Release();

			//---Restore environment settings---//
			RenderSettings.ambientLight = ambientLightColour;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.fog = fogEnabled;

			return render;
		}

		protected override GUIContent CreateHeaderContent()
		{
			GUIContent headerContent = new GUIContent();
			headerContent.text = "RapidIcon";
			return headerContent;
		}
	}
}