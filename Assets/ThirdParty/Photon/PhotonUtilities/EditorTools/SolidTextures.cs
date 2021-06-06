// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;

using UnityEditor;

namespace Photon.Utilities
{
	/// <summary>
	/// Create solid textures. Used to avoid buggy flickering of DrawRect. Thanks Unity.
	/// </summary>
	public static class SolidTextures
	{
		public static Texture2D black2D;
		public static Texture2D white2D;
		public static Texture2D lowcontrast2D;
		public static Texture2D highcontrast2D;
		public static Texture2D gray2D;
		public static Texture2D contrastgray2D;
		public static Texture2D red2D;
		public static Texture2D green2D;
		public static Texture2D blue2D;
		public static Texture2D darken502D;
		public static Texture2D darken202D;
		public static Texture2D darken152D;
		public static Texture2D darken102D;
		public static Texture2D darken052D;
		public static Texture2D darken022D;
		public static Texture2D darkenRed10_2D;
		public static Texture2D darkenBlu10_2D;
		public static Texture2D darkenGrn10_2D;

		public static float lite = EditorGUIUtility.isProSkin ? .4f : .8f; // .78f;
		public static float medi = EditorGUIUtility.isProSkin ? .3f : .75f; // .74f;
		public static float dark = EditorGUIUtility.isProSkin ? .2f : .7f; //  .7f;

		//public static float lite = EditorGUIUtility.isProSkin ? .5f : .4f; // .78f;
		//public static float medi = EditorGUIUtility.isProSkin ? .4f : .3f; // .74f;
		//public static float dark = EditorGUIUtility.isProSkin ? .3f : .2f; //  .7f;

		public static float lowcontrast = EditorGUIUtility.isProSkin ? .1f : .5f;
		public static float highcontrast = EditorGUIUtility.isProSkin ? .8f : .4f;

		//public static float lowcontrast = .1f ;
		//public static float highcontrast = .8f ;

#if COLOR_BLIND
		public static Color lowcontrastgray = new Color(lowcontrast, lowcontrast, lowcontrast);
		public static Color highcontrastgray = new Color(highcontrast, highcontrast, highcontrast);
		public static Color gray = EditorGUIUtility.isProSkin ? new Color(dark, dark, dark) : new Color(medi, medi, medi);
		public static Color contrastgray = EditorGUIUtility.isProSkin ? new Color(dark, dark, dark) : new Color(lite, lite, lite);
		public static Color red = EditorGUIUtility.isProSkin ? new Color(lite, dark, medi) : new Color(lite, dark, medi);
		public static Color green = EditorGUIUtility.isProSkin ? new Color(dark, lite, dark) : new Color(dark, lite, dark);
		public static Color blue = EditorGUIUtility.isProSkin ? new Color(dark, dark, lite) : new Color(dark, dark, lite);
#else
		public static Color lowcontrastgray = new Color(lowcontrast, lowcontrast, lowcontrast);
		public static Color highcontrastgray = new Color(highcontrast, highcontrast, highcontrast);
		public static Color gray = EditorGUIUtility.isProSkin ? new Color(dark, dark, dark) : new Color(medi, medi, medi);
		public static Color contrastgray = EditorGUIUtility.isProSkin ? new Color(dark, dark, dark) : new Color(lite, lite, lite);
		public static Color red = EditorGUIUtility.isProSkin ? new Color(medi, dark, dark) : new Color(lite, dark, medi);
		public static Color green = EditorGUIUtility.isProSkin ? new Color(dark, medi, dark) : new Color(dark, lite, dark);
		public static Color blue = EditorGUIUtility.isProSkin ? new Color(dark, dark, medi) : new Color(dark, dark, lite);
#endif

		static SolidTextures()
		{
			CreateDefaultSolids();
		}

		private static void CreateDefaultSolids()
		{
			white2D = CreateSolid(Color.white);
			black2D = CreateSolid(Color.black);
			lowcontrast2D = CreateSolid(lowcontrastgray);
			highcontrast2D = CreateSolid(highcontrastgray);
			gray2D = CreateSolid(gray);
			contrastgray2D = CreateSolid(contrastgray);
			red2D = CreateSolid(red);
			green2D = CreateSolid(green);
			blue2D = CreateSolid(blue);
			darken502D = CreateSolid(new Color(0, 0, 0, .5f));
			darken202D = CreateSolid(new Color(0, 0, 0, .2f));
			darken152D = CreateSolid(new Color(0, 0, 0, .15f));
			darken102D = CreateSolid(new Color(0, 0, 0, .1f));
			darken052D = CreateSolid(new Color(0, 0, 0, .05f));
			darken022D = CreateSolid(new Color(0, 0, 0, .02f));
			darkenRed10_2D = CreateSolid(new Color(1, 0, 0, .1f));
			darkenGrn10_2D = CreateSolid(new Color(0, 1, 0, .1f));
			darkenBlu10_2D = CreateSolid(new Color(0, 0, 1, .1f));
		}

		public static Texture2D CreateSolid(Color color)
		{
			Texture2D tex = new Texture2D(1, 1);
			tex.wrapMode = TextureWrapMode.Repeat;
			tex.SetPixel(0, 0, color);
			tex.Apply();
			return tex;
		}

		private static GUIStyle s_TempStyle = new GUIStyle();

		/// <summary>
		/// Replacement method for EditorGUI.DrawTexture and DrawRect... since they are buggy in drawers and will flicker.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="texture"></param>
		public static void DrawTexture(Rect position, Texture2D texture)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			// Need to constantly check for null, since Unity looses these textures after the editor exist play.
			if (texture == null)
				CreateDefaultSolids();

			s_TempStyle.normal.background = texture;

			s_TempStyle.Draw(position, GUIContent.none, false, false, false, false);
		}

	}
}

#endif
