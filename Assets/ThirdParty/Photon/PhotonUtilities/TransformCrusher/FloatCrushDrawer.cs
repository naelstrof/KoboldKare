// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using Photon.Utilities;
using emotitron.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{

#if UNITY_EDITOR

	public abstract class CrusherDrawer : PropertyDrawer
	{
		public static bool FC_ISPRO = FloatCrusher.ISPRO;
		public static bool QC_ISPRO = QuatCompress.ISPRO;

		public const int PADDING = 4;
		public const int LINEHEIGHT = 16;
		public const float HEADR_HGHT = 16f;
		public const float COMPMTHD_HGHT = LINEHEIGHT + 3;
		public const float SETTINGS_HGHT = LINEHEIGHT;
		public const float ACCCNTR_HGHT = LINEHEIGHT /*+ SPACING*/;

		public const int ACTUAL_HGHT = LINEHEIGHT;
		public const float BCL_HEIGHT = SPACING + LINEHEIGHT + LINEHEIGHT + LINEHEIGHT + SPACING; /*+ PADDING + SPACING*/
		public const float DISBL_HGHT = 20f;
		public const float BTTM_MARGIN = 4;
		public const int SPACING = 2;

		protected float line, paddedleft, paddedright, paddedwidth, fieldleft, fieldwidth, labelwidth, stdfieldwidth, rightinputsleft;

		public static GUIStyle miniLabelRight = new GUIStyle((GUIStyle)"MiniLabel") { alignment = TextAnchor.MiddleRight };

		public static GUIStyle miniFadedLabelRight = (EditorGUIUtility.isProSkin) ?
			new GUIStyle((GUIStyle)"MiniLabel") { alignment = TextAnchor.MiddleRight } :
			new GUIStyle((GUIStyle)"MiniLabel") { alignment = TextAnchor.MiddleRight, normal = ((GUIStyle)"PR DisabledLabel").normal };

		public static GUIStyle miniFadedLabel = (EditorGUIUtility.isProSkin) ?
			new GUIStyle((GUIStyle)"MiniLabel") :
			new GUIStyle((GUIStyle)"MiniLabel") { normal = ((GUIStyle)"PR DisabledLabel").normal };

		public static GUIStyle MINI_TXT_FLD_STYLE = new GUIStyle("MiniTextField") { fontSize = 10 };

		public static GUIStyle MINI_TGL_STYLE = "OL Toggle";
		public static GUIStyle MINI_LBL_STYLE = "MiniLabel";
		public static GUIStyle FOLDOUT_STYLE = "Foldout";

		protected Rect ir;
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			ir = EditorGUI.IndentedRect(r);

			paddedleft = r.xMin + PADDING;
			paddedright = r.xMax - PADDING;
			paddedwidth = r.width - PADDING * 2;
			labelwidth = 86;// EditorGUIUtility.labelWidth - 32;
			fieldleft = paddedleft + labelwidth;
			fieldwidth = paddedwidth - labelwidth;
			stdfieldwidth = 50f;
			rightinputsleft = r.xMin + (r.width - stdfieldwidth) - PADDING;
		}

		public void ProFeatureDialog(string extratext)
		{
			if (!EditorUtility.DisplayDialog("Pro Version Feature", "Adjustable bits are only available in \nTransform Crusher Pro.", "OK", "Open in Asset Store"))
				Application.OpenURL("https://assetstore.unity.com/packages/tools/network/transform-crusher-116587");
		}
	}

	[CustomPropertyDrawer(typeof(FloatCrusher))]
	[CanEditMultipleObjects]

	public class FloatCrusherDrawer : CrusherDrawer
	{
		FloatCrusher fc;
		SerializedProperty p;
		private const string HALFX = "180°";
		private const string FULLX = "360°";
		private static GUIContent gc_fc = new GUIContent();

		private Texture2D colortex;

		protected int holdindent;
		protected float colwidths;
		protected float height;

		Rect r;
		bool haschanged;


		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, property);

			haschanged = false;
			p = property;

			base.OnGUI(r, property, label);

			bool isWrappedInElementCrusher = DrawerUtils.GetParent(property) is ElementCrusher;
			bool disableRange = (label.tooltip != null && label.tooltip.Contains("DISABLE_RANGE"));

			gc_fc.text = label.text;
			gc_fc.tooltip = (disableRange) ? null : label.tooltip;

			holdindent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			int hash = fc.GetHashCode();
			height = CalculateHeight(fc, hash);

			this.r = r;


			line = r.yMin;

			colortex =
				(fc.axis == Axis.X) ? SolidTextures.red2D :
				(fc.axis == Axis.Y) ? SolidTextures.green2D :
				(fc.axis == Axis.Z) ? SolidTextures.blue2D :
				SolidTextures.gray2D;

			if (!isWrappedInElementCrusher)
			{
				Rect outline = ir;
				outline.xMin--;
				outline.yMin--;
				outline.xMax++;
				outline.yMax++;
				SolidTextures.DrawTexture(outline, SolidTextures.lowcontrast2D);

			}
			SolidTextures.DrawTexture(ir, colortex);
			//SolidTextures.DrawTexture(new Rect(ir.xMin, line, ir.width, height), colortex);
			//SolidTextures.DrawTexture(new Rect(ir.xMin + 1, ir.yMin + 1, ir.width - 2, ir.height - 2), colortex);
			line++;

			line += SPACING;

			fc.expanded = DrawHeader(r, gc_fc, fc.expanded);

			line += PADDING + HEADR_HGHT;

			if (fc.Enabled && fc.expanded)
			{
				bool noSettings = (fc.BitsDeterminedBy == BitsDeterminedBy.HalfFloat || fc.BitsDeterminedBy == BitsDeterminedBy.Uncompressed || fc.BitsDeterminedBy == BitsDeterminedBy.Disabled);

				DrawCompressionMethod();

				EditorGUI.BeginDisabledGroup(disableRange);
				if (!noSettings && !(fc.TRSType == TRSType.Normal))
					DrawCodecSettings(property);
				EditorGUI.EndDisabledGroup();

				if (!noSettings)
					DrawAccurateCenter();

				if (!noSettings && fc.showBCL)
					DrawBCL();

				if (!noSettings)
					DrawActualValues();

			}

			if (haschanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}

			EditorGUI.indentLevel = holdindent;
			EditorGUI.EndProperty();
		}

		private bool DrawHeader(Rect r, GUIContent label, bool expanded)
		{
			const int FOLDOUT = 12;

			GUIContent headertext =
				new GUIContent((fc.axis == Axis.X) ? (fc.TRSType == TRSType.Euler) ? "X (Pitch)" : "X" :
				(fc.axis == Axis.Y) ? (fc.TRSType == TRSType.Euler) ? "Y (Yaw)" : "Y" :
				(fc.axis == Axis.Z) ? (fc.TRSType == TRSType.Euler) ? "Z (Roll)" : "Z" :
				(fc.axis == Axis.Uniform) ? "Uniform" :
				label.text,

				label.tooltip
				);


			if (fc.showEnableToggle)
			{
				bool _enabled = GUI.Toggle(new Rect(ir.xMin + PADDING + FOLDOUT, line, 32, LINEHEIGHT), fc.Enabled, GUIContent.none);

				if (_enabled)
					expanded = EditorGUI.Toggle(new Rect(ir.xMin + PADDING, line, 12, LINEHEIGHT), expanded, FOLDOUT_STYLE);
				else
					EditorGUI.Toggle(new Rect(ir.xMin + PADDING, line, 12, LINEHEIGHT), false, FOLDOUT_STYLE);

				EditorGUI.LabelField(new Rect(ir.xMin + PADDING + 16 + FOLDOUT, line, 200, LINEHEIGHT), headertext);
				if (fc.Enabled != _enabled)
				{
					haschanged = true;
					Undo.RecordObject(p.serializedObject.targetObject, ("Toggle Axis Enable"));
					fc.Enabled = _enabled;
				}
			}
			else
			{
				expanded = EditorGUI.Toggle(new Rect(ir.xMin + PADDING, line, 12, LINEHEIGHT), expanded, FOLDOUT_STYLE);
				EditorGUI.LabelField(new Rect(ir.xMin + PADDING + FOLDOUT, line, 200, LINEHEIGHT), new GUIContent(headertext));
			}

			//EditorGUI.indentLevel = 0;

			if (!fc.Enabled)
				return expanded;

			if (fc.TRSType == TRSType.Euler && fc.axis == Axis.X)
				if (GUI.Button(new Rect(paddedwidth - 70, line, 42, LINEHEIGHT), fc.UseHalfRangeX ? HALFX : FULLX, (GUIStyle)"minibutton"))
				{
					haschanged = true;
					Undo.RecordObject(p.serializedObject.targetObject, "Toggle X 180/360");
					fc.UseHalfRangeX = !fc.UseHalfRangeX;
				}

			//Debug.Log("BITS?? "+ fc.Bits + " _bits:" + p.FindPropertyRelative("_bits").GetArrayElementAtIndex(0).intValue);
			String bitstr = fc.Bits + " Bits";
			EditorGUI.LabelField(new Rect(paddedleft, line, paddedwidth, LINEHEIGHT), bitstr, miniLabelRight);

			return expanded;
		}

		private void DrawCodecSettings(SerializedProperty p)
		{

			/// Line for the optional ranges

			if (fc.TRSType == TRSType.Euler)
			{
				bool useRange = EditorGUI.Toggle(new Rect(ir.xMin + PADDING, line, 40, LINEHEIGHT), GUIContent.none, fc.LimitRange, MINI_TGL_STYLE);
				// if range just got turned off, reset the min/max to full rotation ranges
				if (fc.LimitRange != useRange)
				{
					haschanged = true;
					Undo.RecordObject(p.serializedObject.targetObject, "Toggle Limit Range");
					fc.LimitRange = useRange;
				}

				if (useRange)
					DrawRotationRanges();
				else
					GUI.Label(new Rect(ir.xMin + PADDING + 16, line, 120, LINEHEIGHT), new GUIContent("Limit Ranges"), MINI_LBL_STYLE);

			}
			else
				DrawBasicRanges();

			line += SETTINGS_HGHT;
		}

		private void DrawAccurateCenter()
		{
			/// Line for the Accuracte Center toggle

			string centervalue = fc.CenterValue.ToString();

			GUIContent zeroCenterContent = new GUIContent("Use Accurate Center (" + centervalue + ")", "Scales the range to not use the highest compressed value, " +
				"resulting in an odd number of increments - which allows for an absolute center value. " +
				"Use this setting if you need to have an absolute and exact center value (such as zero). " +
				"Accurate Center with current range settings is " + centervalue);

			GUI.Label(new Rect(ir.xMin + PADDING + 16, line, paddedwidth, LINEHEIGHT), zeroCenterContent, MINI_LBL_STYLE);
			bool zeroToggle = GUI.Toggle(new Rect(ir.xMin + PADDING, line, 32, LINEHEIGHT), fc.AccurateCenter, GUIContent.none, MINI_TGL_STYLE);
			if (fc.AccurateCenter != zeroToggle)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Toggle AccurateCenter");
				fc.AccurateCenter = zeroToggle;
			}

			line += ACCCNTR_HGHT;
		}

		private void DrawCompressionMethod()
		{

			BitsDeterminedBy btb = (BitsDeterminedBy)
				EditorGUI.EnumPopup(new Rect(ir.xMin + PADDING, line, labelwidth - 8, LINEHEIGHT), GUIContent.none, fc.BitsDeterminedBy);

			// IF we switched to pro - the btb value is actually the bits value, force a change to SetBits
			if (FC_ISPRO && btb >= 0)
			{
				fc.Bits = (int)btb;
			}

			else if (!FC_ISPRO && btb == BitsDeterminedBy.SetBits)//.CustomBits)
			{
				// In case we went from pro to free... quietly set this back to non-custom.
				if (fc.BitsDeterminedBy != BitsDeterminedBy.SetBits)
					ProFeatureDialog("");
				else
					fc.Bits = fc.Bits;

			}
			else if (fc.BitsDeterminedBy != btb)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Changed Crusher Bits Determined By");
				fc.BitsDeterminedBy = btb;
				p.serializedObject.Update();
			}

			float fieldleft = paddedleft + labelwidth;
			float fieldwidth = paddedwidth - labelwidth;

			//const float labelW = 48f;
			const float inputW = 50f;
			float input2Left = paddedright - inputW;

			switch (fc.BitsDeterminedBy)
			{
				case BitsDeterminedBy.HalfFloat:
					break;

				case BitsDeterminedBy.Uncompressed:
					break;

				case BitsDeterminedBy.Disabled:
					break;

				case BitsDeterminedBy.Resolution:

					EditorGUI.LabelField(new Rect(rightinputsleft - 128, line, 128, LINEHEIGHT), new GUIContent(holdindent < 2 ? "Min resolution: 1/" : "Min res 1/"), miniLabelRight);

					uint res = (uint)EditorGUI.DelayedIntField(new Rect(input2Left, line - 1, inputW, LINEHEIGHT), GUIContent.none, (int)fc.Resolution);

					if (fc.Resolution != res)
					{
						haschanged = true;
						Undo.RecordObject(p.serializedObject.targetObject, "Changed Resolution value");
						fc.Resolution = res;
					}

					break;

				case BitsDeterminedBy.Precision:

					EditorGUI.LabelField(new Rect(rightinputsleft - 128, line, 128, LINEHEIGHT), new GUIContent((holdindent < 2) ? "Min precision: " : "Min prec: "), miniLabelRight);
					float precision = EditorGUI.DelayedFloatField(new Rect(input2Left, line - 1, inputW, LINEHEIGHT), GUIContent.none, fc.Precision);

					if (fc.Precision != precision)
					{
						haschanged = true;
						Undo.RecordObject(p.serializedObject.targetObject, "Changed Precision value");
						fc.Precision = (float)Math.Round(precision * 100000) / 100000;
					}

					break;

				default:

					if (FC_ISPRO && fc.BitsDeterminedBy == (BitsDeterminedBy.SetBits))
					{
						EditorGUI.indentLevel = holdindent;
#if UNITY_2019_3_OR_NEWER
						int bits = EditorGUI.IntSlider(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT + 2), GUIContent.none, fc.Bits, 0, 32);
#else
						int bits = EditorGUI.IntSlider(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT), GUIContent.none, fc.Bits, 0, 32);
#endif
						EditorGUI.indentLevel = 0;

						if (fc.Bits != bits)
						{
							haschanged = true;
							Undo.RecordObject(p.serializedObject.targetObject, "Changed Bits value");
							fc.Bits = bits;
						}
						break;
					}

					float sliderleft = ir.xMin + PADDING + labelwidth; // ir.xMin + PADDING + labelwidth;
					float sliderwidth = ir.width - PADDING - labelwidth - PADDING; // - sliderleft - PADDING;

					GUI.enabled = false;
#if UNITY_2019_3_OR_NEWER
					EditorGUI.IntSlider(new Rect(sliderleft, line, sliderwidth, LINEHEIGHT + 2), GUIContent.none, (int)fc.BitsDeterminedBy, 0, 32);
#else
					EditorGUI.IntSlider(new Rect(sliderleft, line, sliderwidth, LINEHEIGHT), GUIContent.none, (int)fc.BitsDeterminedBy, 0, 32);
#endif
					//EditorGUI.IntSlider(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT), GUIContent.none, (int)fc.BitsDeterminedBy, 0, 32);
					GUI.enabled = true;

					break;
			}

			line += COMPMTHD_HGHT;
		}

		private void DrawBasicRanges()
		{
			GUI.Label(new Rect(ir.xMin + PADDING, line /*- 2*/, labelwidth, LINEHEIGHT), new GUIContent(holdindent < 2 ? "Range:" : "Rng:"), MINI_LBL_STYLE);
			//EditorGUI.LabelField(new Rect(ir.xMin + PADDING, line /*- 2*/, labelwidth, LINEHEIGHT), new GUIContent(holdindent < 2 ? "Range:" : "Rng:"), MINI_LBL_STYLE);

			const float labelW = 48f;
			const float inputW = 50f;

			float input1Left = fieldleft;
			float input2Left = paddedright - inputW;
			float label1Left = input1Left - labelW;
			float label2Left = input2Left - labelW;

			GUI.Label(new Rect(label1Left, line /*- 2*/, labelW, LINEHEIGHT), new GUIContent("min: "), miniLabelRight);

			float min = EditorGUI.DelayedFloatField(new Rect(input1Left, line, inputW, LINEHEIGHT), GUIContent.none, fc.Min, MINI_TXT_FLD_STYLE);

			if (fc.Min != min)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Min value Change");
				fc.Min = min;
			}

			GUI.Label(new Rect(label2Left, line /*- 2*/, labelW, LINEHEIGHT), new GUIContent("max: "), miniLabelRight);

			float max = EditorGUI.DelayedFloatField(new Rect(input2Left, line, inputW, LINEHEIGHT), GUIContent.none, fc.Max, MINI_TXT_FLD_STYLE);

			if (fc.Max != max)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Max value change");
				fc.Max = max;
			}

			//EditorGUI.indentLevel = holdindent;
		}

		public struct MinMax { public float min; public float max; }
		private MinMax DrawMinMaxSlider(float minIn, float maxIn, float minLimit, float maxLimit, bool showDegrees = true)
		{
			const float input1offset = 10;
			float inputWidth = (holdindent < 2) ? (showDegrees ? 40f : 50f) : showDegrees ? 30f : 40f;
			float degreeSpace = showDegrees ? 10f : 0;
			GUIContent lbl = showDegrees ? new GUIContent("°") : GUIContent.none;

			float left = r.xMin;

			float input1left = fieldleft - inputWidth - input1offset;
			float input2left = rightinputsleft;
			float sliderleft = fieldleft;
			float sliderwidth = (input2left - fieldleft) - PADDING;

			float minOut = EditorGUI.DelayedFloatField(new Rect(input1left, line, inputWidth, LINEHEIGHT), GUIContent.none, minIn, MINI_TXT_FLD_STYLE);
			float maxOut = EditorGUI.DelayedFloatField(new Rect(input2left, line, inputWidth, LINEHEIGHT), GUIContent.none, maxIn, MINI_TXT_FLD_STYLE);

			EditorGUI.LabelField(new Rect(input1left, line, inputWidth + degreeSpace, LINEHEIGHT), lbl, (GUIStyle)"RightLabel");
			EditorGUI.LabelField(new Rect(input2left, line, inputWidth + degreeSpace, LINEHEIGHT), lbl, (GUIStyle)"RightLabel");

			EditorGUI.MinMaxSlider(new Rect(sliderleft, line - 1, sliderwidth, LINEHEIGHT), ref minOut, ref maxOut, minLimit, maxLimit);

			return new MinMax() { min = minOut, max = maxOut };
		}

		private void DrawRotationRanges()
		{
			float sliderMax = (fc.axis == Axis.X && fc.UseHalfRangeX) ? 90 : 360;
			float sliderMin = (fc.axis == Axis.X && fc.UseHalfRangeX) ? -90 : -360;

			var minmax = DrawMinMaxSlider(fc.Min, fc.Max, sliderMin, sliderMax);

			float usedRange = Math.Min(minmax.max, sliderMax) - Math.Max(minmax.min, sliderMin);

			if (usedRange > 360)
				minmax.max = Mathf.Min(minmax.min + 360, 360);

			if (fc.Min != minmax.min || fc.Min < sliderMin)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Min value change");
				fc.Min = Mathf.Max((int)minmax.min, sliderMin);
			}

			if (fc.Max != minmax.max || fc.Max > sliderMax)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Max value change");
				fc.Max = Mathf.Min((int)minmax.max, sliderMax);
			}
		}

		static GUIContent GC_ACTUAL_LONG = new GUIContent("Actual:");

		private void DrawActualValues()
		{
			GUI.Label(new Rect(ir.xMin + PADDING, line, paddedwidth, ACTUAL_HGHT), ((holdindent < 2) ? GC_ACTUAL_LONG : GUIContent.none), miniFadedLabel);

			float prec = fc.GetPrecAtBits();
			float res = fc.GetResAtBits();
			// restrict prec to 5 characters
			string resstr = res.ToString((res < 0) ? "0.0000" : (res > 9999) ? "E2" : "F4");
			string precstr = prec.ToString("F" + Math.Min(4, Math.Max(0, (4 - (int)Math.Log10(prec)))).ToString());
			string str = "res: 1/" + resstr + (fc.TRSType == TRSType.Euler ? "°" : "") + "   prec: " + precstr + (fc.TRSType == TRSType.Euler ? "°" : "");
			GUI.Label(new Rect(paddedleft, line, paddedwidth, ACTUAL_HGHT), str, miniFadedLabelRight);

			line += ACTUAL_HGHT;
		}

		static GUIContent rstLabel = new GUIContent("R", "Reset to default Bit Culling Level bit values.");
		static GUIContent showBclContent = new GUIContent("BCL", "Show Bit Culling Level details for this crusher. Bit Culling is an experimental advanced compresion method that should be ignored until you read up on it in the docs.");

		private void DrawBCL()
		{

			EditorGUI.LabelField(new Rect(paddedleft + 12, line, 60, LINEHEIGHT), showBclContent, MINI_LBL_STYLE);

			bool bclToggle = EditorGUI.Toggle(new Rect(paddedleft, line, 32, LINEHEIGHT), GUIContent.none, fc.expandBCL, FOLDOUT_STYLE);

			if (fc.expandBCL != bclToggle)
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Toggle Show BCL Details");
				fc.expandBCL = bclToggle;
			}

			float labelw = 48;
			float fieldL = ir.xMin + labelw;
			float fieldw = ir.width - labelw;
			float fldWdth4th = fieldw / 4;
			float fldWdth8th = fieldw / 8;
			float fldWdth16th = fieldw / 16;

			if (fc.expandBCL)
				SolidTextures.DrawTexture(new Rect(fieldL, line, fieldw, BCL_HEIGHT - SPACING), SolidTextures.darken052D);

			DrawBCLField(BitCullingLevel.DropAll, fieldL, fldWdth4th, fldWdth8th, fldWdth16th);
			DrawBCLField(BitCullingLevel.DropHalf, fieldL, fldWdth4th, fldWdth8th, fldWdth16th);
			DrawBCLField(BitCullingLevel.DropThird, fieldL, fldWdth4th, fldWdth8th, fldWdth16th);
			DrawBCLField(BitCullingLevel.NoCulling, fieldL, fldWdth4th, fldWdth8th, fldWdth16th);

			line += LINEHEIGHT;
			if (!fc.expandBCL)
				return;

			EditorGUI.LabelField(new Rect(paddedleft, line, labelw, LINEHEIGHT), "bits", MINI_LBL_STYLE);

			// Reset Button
			if (GUI.Button(new Rect(fieldL - 18 - 2, line, 18, LINEHEIGHT), rstLabel, (GUIStyle)"minibuttonleft"))
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "Reset BCL To Default");
				fc.SetBits(fc.Bits);
			}

			line += LINEHEIGHT;
			EditorGUI.LabelField(new Rect(paddedleft, line, labelw, LINEHEIGHT), "zones", MINI_LBL_STYLE);
			line += LINEHEIGHT;

			line += SPACING;
		}

		private void DrawBCLField(BitCullingLevel bcl, float fieldL, float fldWdth4th, float fldWdth8th, float fldWdth16th)
		{
			int holdindent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			float lines = line;

			float left = fieldL + fldWdth4th * (3 - (int)bcl);

			if (fc.expandBCL)
			{
				if (bcl == BitCullingLevel.DropAll || bcl == BitCullingLevel.DropThird)
					SolidTextures.DrawTexture(new Rect(left, line, fldWdth4th, BCL_HEIGHT), SolidTextures.darken052D);

				string clvl =
				r.width > 300 ?
				(((int)bcl == 0) ? "Cull None" : ((int)bcl == 1) ? "Lvl 1" : ((int)bcl == 2) ? "Lvl 2" : "Cull All") :
				(((int)bcl == 0) ? "None" : ((int)bcl == 1) ? "Lvl 1" : ((int)bcl == 2) ? "Lvl 2" : "All");

				EditorGUI.LabelField(new Rect(left, lines, fldWdth4th, LINEHEIGHT), clvl, (GUIStyle)"ProjectBrowserGridLabel");
				lines += LINEHEIGHT;
			}

			EditorGUI.indentLevel = 0;
			int bits = EditorGUI.DelayedIntField(new Rect(left + fldWdth16th, lines + 1, fldWdth8th, LINEHEIGHT), fc.GetBits(bcl), MINI_TXT_FLD_STYLE);
			EditorGUI.indentLevel = 0;

			if (bits != fc.GetBits(bcl))
			{
				haschanged = true;
				Undo.RecordObject(p.serializedObject.targetObject, "BCL " + bcl + " changed");
				fc.SetBits(bits, bcl);

				Debug.Log("bcl " + bcl + " " + bits + " " + fc.GetBits(bcl));
			}

			if (fc.expandBCL)
			{
				lines += LINEHEIGHT;
				var rng = (double)Math.Abs(fc.Max - fc.Min);
				string str = ((fc.GetBits(bcl) == 0) ? 0 : (rng / Math.Pow(2, fc.Bits - fc.GetBits(bcl)))).ToString("####0.#####");
				EditorGUI.LabelField(new Rect(left, lines, fldWdth4th, LINEHEIGHT), str, (GUIStyle)"ProjectBrowserGridLabel");
			}

			EditorGUI.indentLevel = holdindent;
		}

		private float CalculateHeight(FloatCrusher fc, int hash)
		{
			

			bool noSettings = (fc.BitsDeterminedBy == BitsDeterminedBy.HalfFloat || fc.BitsDeterminedBy == BitsDeterminedBy.Uncompressed || fc.BitsDeterminedBy == BitsDeterminedBy.Disabled);
			bool noRange = fc.TRSType == TRSType.Normal;

			float bclLine = (fc.expandBCL) ? BCL_HEIGHT : (fc.showBCL) ? LINEHEIGHT : 0;

			float settingsLen = (noSettings) ? 0 : (noRange ? 0 : SETTINGS_HGHT) + ACCCNTR_HGHT + ACTUAL_HGHT + bclLine;

			return PADDING + HEADR_HGHT + PADDING +
				(fc.Enabled && fc.expanded ?
				(COMPMTHD_HGHT + ((noSettings) ? 0 : settingsLen)) :
				0);
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			// Hackjob way to get the target - needs to reference a serialized field in order to work.
			fc = (FloatCrusher)DrawerUtils.GetParent(property.FindPropertyRelative("_min"));
			int hash = fc.GetHashCode();
			return CalculateHeight(fc, hash);
		}
	}

#endif

}
