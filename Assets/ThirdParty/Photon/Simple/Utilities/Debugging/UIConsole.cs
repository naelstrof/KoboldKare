// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace Photon.Pun.Simple.Debugging
{
	public class UIConsole : MonoBehaviour
	{
		public int maxSize = 3000;

		public bool logToDebug = true;

		public readonly static StringBuilder strb = new StringBuilder();

		private static UIConsole single;
		public static UIConsole Single
		{
			get
			{
				if (single == null)
					CreateGUI();

				return single;
			}
		}

		private static Text uitext;


		// Start is called before the first frame update
		void Awake()
		{
			single = this;
			uitext = GetComponent<Text>();
			uitext.text = strb.ToString();
		}

		public static void Log(string str)
		{
			if (!single)
				return;

			if (strb.Length > single.maxSize)
				strb.Length = 0;

			if (uitext != null)
			{
				strb.Append(str).Append("\n");
				uitext.text = strb.ToString();
			}

			if (single.logToDebug)
				Debug.Log(str);
		}

		/// <summary>
		/// Shortcut for Append to the strb StringBuilder. Be sure to Refresh() to apply to UI.
		/// </summary>
		/// <param name="str"></param>
		public UIConsole _(object str) { strb.Append(str.ToString()); return single; }
		public UIConsole _(string str) { strb.Append(str); return single; }
		public UIConsole _(int str) { strb.Append(str); return single; }
		public UIConsole _(uint str) { strb.Append(str); return single; }
		public UIConsole _(byte str) { strb.Append(str); return single; }
		public UIConsole _(sbyte str) { strb.Append(str); return single; }
		public UIConsole _(short str) { strb.Append(str); return single; }
		public UIConsole _(ushort str) { strb.Append(str); return single; }
		public UIConsole _(long str) { strb.Append(str); return single; }
		public UIConsole _(ulong str) { strb.Append(str); return single; }
		public UIConsole _(float str) { strb.Append(str); return single; }
		public UIConsole _(double str) { strb.Append(str); return single; }

		/// <summary>
		/// Shortcut for Append(Space).
		/// </summary>
		public UIConsole __ { get { strb.Append(" "); return single; } }

		/// <summary>
		/// Update the UI element with the current StringBuilder strb value.
		/// </summary>
		public static void Refresh()
		{
			if (!single)
				return;

			if (uitext != null)
			{
				uitext.text = strb.ToString();
			}
		}
		
		public static void Clear()
		{
			strb.Length = 0;

			if (uitext)
				uitext.text = strb.ToString();
		}

		public static UIConsole CreateGUI()
		{
			var go = new GameObject("UI CONSOLE");
			var canvas = go.AddComponent<Canvas>();
			var textgo = new GameObject("CONSOLE TEXT");
			textgo.transform.parent = go.transform;
			uitext = textgo.AddComponent<Text>();

			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			uitext.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			uitext.verticalOverflow = VerticalWrapMode.Overflow;
			uitext.horizontalOverflow = HorizontalWrapMode.Overflow;
			uitext.alignment = TextAnchor.UpperCenter;
			//uitext.rectTransform.anchoredPosition = new Vector2(-Screen.width / 2, Screen.height / 2);
			//uitext.rectTransform.offsetMin = new Vector2(Screen.width / 2, -Screen.height / 2);
			uitext.rectTransform.pivot = new Vector2(0, 0);
			uitext.rectTransform.anchorMin = new Vector2(0, 0);
			uitext.rectTransform.anchorMax = new Vector2(1, 1);
			uitext.rectTransform.offsetMax = new Vector2(0, 0);
			//uitext.rectTransform.anchorMax = new Vector2(1, 1);
			//uitext.rectTransform.anchorMax = new Vector2(Screen.width / 2, Screen.height / 2);


			single = textgo.AddComponent<UIConsole>();
			return single;

		}

	}
}

