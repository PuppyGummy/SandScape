using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_6_2
{
	public static class VersionControl
	{
		static Version thisVersion = new Version("1.6.2");

		public struct Version
		{
			public int major;
			public int minor;
			public int patch;

			public Version(int major, int minor, int patch)
			{
				this.major = major;
				this.minor = minor;
				this.patch = patch;
			}

			public Version(string version)
			{
				this = ConvertFromString(version);
			}

			public static Version ConvertFromString(string s)
			{
				Version version = new Version(0, 0, 0);

				string[] split = s.Split(".");
				if (split != null)
				{
					if (split.Length >= 1)
						int.TryParse(split[0], out version.major);

					if (split.Length >= 2)
						int.TryParse(split[1], out version.minor);

					if (split.Length >= 3)
						int.TryParse(split[2], out version.patch);
				}

				return version;
			}

			public static bool operator >(Version v1, Version v2)
			{
				if (v1.major > v2.major)
					return true;  //major is newer
				else if (v1.major < v2.major)
					return false; //major is older

				//major is equal

				if (v1.minor > v2.minor)
					return true;  //minor is newer
				else if (v1.minor < v2.minor)
					return false; //minor is older

				//minor is equal

				if (v1.patch > v2.patch)
					return true;  //patch is newer
				else if (v1.patch < v2.patch)
					return false; //patch is older

				//patch is equal, versions are equal
				return false;
			}

			public static bool operator <(Version v1, Version v2)
			{
				if (v1.major < v2.major)
					return true;  //major is older
				else if (v1.major > v2.major)
					return false; //major is newer

				//major is equal

				if (v1.minor < v2.minor)
					return true;  //minor is older
				else if (v1.minor > v2.minor)
					return false; //minor is newer

				//minor is equal

				if (v1.patch < v2.patch)
					return true;  //patch is older
				else if (v1.patch > v2.patch)
					return false; //patch is newer

				//patch is equal, versions are equal
				return false;
			}
		}

		public static string ConvertToString(this Version version)
		{
			return version.major + "." + version.minor + "." + version.patch;
		}

		public static Version GetStoredVersion()
		{
			string s = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconVersion", thisVersion.ConvertToString());
			return Version.ConvertFromString(s);
		}

		public static void UpdateStoredVersion()
		{
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconVersion", thisVersion.ConvertToString());
		}

		public static bool IsStoredVersionOld()
		{
			Version version = GetStoredVersion();

			if (thisVersion > version)
				return true;

			return false;
		}

		public static void CheckUpdate(List<Icon> icons)
		{
			Version lastVersion = GetStoredVersion();

			//---1.0 Updates---//
			//No updates required (initial release)

			//---1.1 Updates---//
			if (lastVersion < new Version("1.1"))
			{
				foreach (Icon icon in icons)
				{
					icon.exportName = icon.assetName;
					int extensionPos = icon.exportName.LastIndexOf('.');
					icon.exportName = icon.exportName.Substring(0, extensionPos);

				}
			}

			//---1.2 Updates---//
			//No updates required

			//---1.2.1 Updates---//
			if (lastVersion < new Version("1.2.1"))
			{
				foreach (Icon icon in icons)
				{
					if (icon.camerasScaleFactor == 0)
						icon.camerasScaleFactor = 1;
				}
			}

			//---1.3 Updates---//
			if (lastVersion < new Version("1.3"))
			{
				foreach (Icon icon in icons)
				{
					//icon.fixEdges = true; --depreciated (v1.6.1)
					icon.filterMode = FilterMode.Point;
				}
			}

			//---1.4 Updates---//
			//No updates required

			//---1.5 Updates---//
			//No updates required

			//---1.5.1 Updates---//
			//No updates required

			//---1.6 Udpates---//
			if (lastVersion < new Version("1.6"))
			{
				foreach (Icon icon in icons)
				{
					icon.perspLastScale = icon.camerasScaleFactor;

					if (icon.GUIDs != null && icon.GUIDs.Length >= 4)
						icon.assetGUID = icon.GUIDs[3];
				}
			}

			//---1.6.1 Updates---//
			//FixEdgesMode fix moved to 1.6.2, as bug in 1.6.1 implementation

			//---1.6.2 Updates---//
			if (lastVersion < new Version("1.6.2"))
			{
				foreach (Icon icon in icons)
				{
					icon.fixEdgesMode = Icon.FixEdgesModes.Regular;
				}
			}
		}
	}
}