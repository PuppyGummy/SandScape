using System;
using System.Collections.Generic;

namespace RapidIcon_1_6_2
{
	[Serializable]
	public class IconData
	{
		public List<Icon> icons;

		public IconData()
		{
			icons = new List<Icon>();
		}
	}
}