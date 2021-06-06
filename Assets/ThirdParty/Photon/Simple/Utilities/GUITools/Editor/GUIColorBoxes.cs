// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Utilities
{
	public static class GUIColorBoxes
	{
		private static GUIStyle grayboxstyle;
		public static GUIStyle GrayBoxStyle { get { if (grayboxstyle == null) grayboxstyle = MakeStyle("sv_label_0"); return grayboxstyle; } }

		private static GUIStyle blueboxstyle;
		public static GUIStyle BlueBoxStyle { get { if (blueboxstyle == null) blueboxstyle = MakeStyle("sv_label_1"); return blueboxstyle; } }

		private static GUIStyle cyanboxstyle;
		public static GUIStyle CyanBoxStyle { get { if (cyanboxstyle == null) cyanboxstyle = MakeStyle("sv_label_2"); return cyanboxstyle; } }

		private static GUIStyle greenboxstyle;
		public static GUIStyle GreenBoxStyle { get { if (greenboxstyle == null) greenboxstyle = MakeStyle("sv_label_3");  return greenboxstyle; } }


		private static GUIStyle yelloboxstyle;
		public static GUIStyle YellowBoxStyle { get { if (yelloboxstyle == null) yelloboxstyle = MakeStyle("sv_label_4"); return yelloboxstyle; } }

		private static GUIStyle orngboxstyle;
		public static GUIStyle OrangeBoxStyle { get { if (orngboxstyle == null) orngboxstyle = MakeStyle("sv_label_5"); return orngboxstyle; } }

		private static GUIStyle redboxstyle;
		public static GUIStyle RedBoxStyle { get { if (redboxstyle == null) redboxstyle = MakeStyle("sv_label_6"); return redboxstyle; } }

		private static GUIStyle purpboxstyle;
		public static GUIStyle PurpleBoxStyle { get { if (purpboxstyle == null) purpboxstyle = MakeStyle("sv_label_7"); return purpboxstyle; } }






		private static GUIStyle MakeStyle(string basestyle)
		{
			var newStyle = new GUIStyle(GUI.skin.GetStyle(basestyle)) { fixedHeight = 20, alignment = TextAnchor.UpperCenter, padding = new RectOffset(4, 4, 4, 4) };
			//newStyle.normal.textColor = Color.white;
			return newStyle;
		}
	}
}

