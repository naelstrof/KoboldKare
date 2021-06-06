// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;
using Photon.Utilities;
using emotitron.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{
	public enum CompressLevel { SetBits = -1, Disabled = 0, uint16Low = 16, uint32Med = 32, uint64Hi = 64 }

	/// <summary>
	/// Wrapper for the QuatCompress codec, giving it editor capabilities and compatibility/interoperability with the entire TransformCrusher library.
	/// </summary>
	[System.Serializable]
	public class QuatCrusher : Crusher<QuatCrusher>, IEquatable<QuatCrusher>, ICrusherCopy<QuatCrusher>
	{
		public static bool QC_ISPRO = QuatCompress.ISPRO;

		[Range(16, 64)]
		[SerializeField]
		private int bits;
		public int Bits
		{
			get { return (enabled) ? QC_ISPRO ? bits : RoundBitsToBestPreset(bits) : 0; }
			set
			{
				if (QC_ISPRO)
				{
					bits = value;
					CompressLevel = CompressLevel.SetBits;
				}
				else
				{
					bits = RoundBitsToBestPreset(value);
					CompressLevel = (CompressLevel)bits;
				}

				if (OnRecalculated != null)
					OnRecalculated(this);
			}
		}

		[SerializeField] public CompressLevel _compressLevel;
		public CompressLevel CompressLevel
		{
			get { return _compressLevel; }
			set
			{
				if (QC_ISPRO)
				{
					_compressLevel = value;
					bits = (_compressLevel == CompressLevel.SetBits) ? bits : (int)_compressLevel;
				}
				else
				{
					// If we were using custom bits (moved from Pro to free?), we need to get those bits, and use the closest value we can find
					if (_compressLevel == CompressLevel.SetBits)
						_compressLevel = (CompressLevel)bits;

					_compressLevel = (CompressLevel)RoundBitsToBestPreset((int)value);
					bits = (int)_compressLevel;
				}

				if (OnRecalculated != null)
					OnRecalculated(this);
			}
		}

		[SerializeField] public Transform transform;
		[SerializeField] public bool local;

		[HideInInspector] public bool isStandalone;
		[SerializeField] public bool showEnableToggle;
		[SerializeField] private bool enabled = true;
		[SerializeField] public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;

				if (OnRecalculated != null)
					OnRecalculated(this);
			}
		}


		private QuatCompress.Cache cache;

		[NonSerialized]
		private bool initialized;

		// Constructor
		public QuatCrusher()
		{
			this._compressLevel = CompressLevel.uint64Hi;
			this.showEnableToggle = false;
			this.isStandalone = true;
		}

		public QuatCrusher(int bits, bool showEnableToggle = false, bool isStandalone = true)
		{
			this.bits = (QC_ISPRO) ? bits : RoundBitsToBestPreset(bits);
			this._compressLevel = CompressLevel.SetBits;

			this.showEnableToggle = showEnableToggle;
			this.isStandalone = isStandalone;
		}

		// Constructor
		public QuatCrusher(bool showEnableToggle = false, bool isStandalone = true)
		{
			this.bits = 32;
			this._compressLevel = (QC_ISPRO) ? CompressLevel.SetBits : CompressLevel.uint32Med;
			this.showEnableToggle = showEnableToggle;
			this.isStandalone = isStandalone;
		}

		// Constructor
		public QuatCrusher(CompressLevel compressLevel, bool showEnableToggle = false, bool isStandalone = true)
		{
			this._compressLevel = compressLevel;
			this.bits = (int)compressLevel;
			this.showEnableToggle = showEnableToggle;
			this.isStandalone = isStandalone;
		}

		public void Initialize()
		{
			cache = QuatCompress.caches[/*_compressLevel != 0 ? (int)_compressLevel : */Bits];
			initialized = true;
		}

		public override void OnBeforeSerialize()
		{
		}
		public override void OnAfterDeserialize()
		{
			if (OnRecalculated != null)
				OnRecalculated(this);
		}

		public static int RoundBitsToBestPreset(int bits)
		{
			if (bits > 32)
				return 64;
			if (bits > 16)
				return 32;
			if (bits > 8)
				return 16;
			return 0;
		}

		public ulong Compress()
		{
			if (!initialized)
				Initialize();

			if (local)
				return transform.localRotation.Compress(cache);
			else
				return transform.rotation.Compress(cache);
		}

		public ulong Compress(Quaternion quat)
		{
			if (!initialized)
				Initialize();

			return quat.Compress(cache);
		}

		public Quaternion Decompress(ulong compressed)
		{
			if (!initialized)
				Initialize();

			return compressed.Decompress(cache);
		}
		

		#region Array Buffer Writers

		public ulong Write(Quaternion quat, byte[] buffer, ref int bitposition)
		{
			ulong compressed = Compress(quat);
			buffer.Write(compressed, ref bitposition, bits);
			return compressed;
		}

		public ulong Write(Quaternion quat, uint[] buffer, ref int bitposition)
		{
			ulong compressed = Compress(quat);
			buffer.Write(compressed, ref bitposition, bits);
			return compressed;
		}

		public ulong Write(Quaternion quat, ulong[] buffer, ref int bitposition)
		{
			ulong compressed = Compress(quat);
			buffer.Write(compressed, ref bitposition, bits);
			return compressed;
		}

		public ulong Write(ulong c, byte[] buffer, ref int bitposition)
		{
			buffer.Write(c, ref bitposition, bits);
			return c;
		}

		public ulong Write(ulong c, uint[] buffer, ref int bitposition)
		{
			buffer.Write(c, ref bitposition, bits);
			return c;
		}

		public ulong Write(ulong c, ulong[] buffer, ref int bitposition)
		{
			buffer.Write(c, ref bitposition, bits);
			return c;
		}

		#endregion

		#region Array Buffer Readers

		public Quaternion Read(byte[] buffer, ref int bitposition)
		{
			ulong compressed = buffer.Read(ref bitposition, bits);
			return Decompress(compressed);
		}

		public Quaternion Read(uint[] buffer, ref int bitposition)
		{
			ulong compressed = buffer.Read(ref bitposition, bits);
			return Decompress(compressed);
		}

		public Quaternion Read(ulong[] buffer, ref int bitposition)
		{
			ulong compressed = buffer.Read(ref bitposition, bits);
			return Decompress(compressed);
		}

		#endregion

		public ulong Write(Quaternion quat, ref ulong buffer, ref int bitposition)
		{
			ulong compressed = Compress(quat);
			compressed.Inject(ref buffer, ref bitposition, bits);
			return compressed;
		}

		public Quaternion Read(ref ulong buffer, ref int bitposition)
		{
			ulong compressed = buffer.Read(ref bitposition, bits);
			return Decompress(compressed);
		}

		public void CopyFrom(QuatCrusher source)
		{
			bits = source.bits;
			_compressLevel = source._compressLevel;
			local = source.local;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as QuatCrusher);
		}

		public bool Equals(QuatCrusher other)
		{
			return other != null &&
				   bits == other.bits &&
				   _compressLevel == other._compressLevel &&
				   //EqualityComparer<Transform>.Default.Equals(transform, other.transform) &&
				   local == other.local;
		}

		public override int GetHashCode()
		{
			var hashCode = -282774512;
			hashCode = hashCode * -1521134295 + bits.GetHashCode();
			hashCode = hashCode * -1521134295 + _compressLevel.GetHashCode();
			//hashCode = hashCode * -1521134295 + EqualityComparer<Transform>.Default.GetHashCode(transform);
			hashCode = hashCode * -1521134295 + local.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(QuatCrusher crusher1, QuatCrusher crusher2)
		{
			return EqualityComparer<QuatCrusher>.Default.Equals(crusher1, crusher2);
		}

		public static bool operator !=(QuatCrusher crusher1, QuatCrusher crusher2)
		{
			return !(crusher1 == crusher2);
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(QuatCrusher))]
	[CanEditMultipleObjects]

	public class QuatCrusherDrawer : CrusherDrawer
	{
		public const float TOP_PAD = 4f;
		public const float BTM_PAD = 6f;
		private const float TITL_HGHT = 18f;
		QuatCrusher target;
		bool haschanged;

		int holdindent;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, property);

			haschanged = false;

			base.OnGUI(r, property, label);

			holdindent = EditorGUI.indentLevel;

			property.serializedObject.ApplyModifiedProperties();
			property.serializedObject.Update();

			target = (QuatCrusher)DrawerUtils.GetParent(property.FindPropertyRelative("bits"));


			line = r.yMin;

			//float standalonesheight = target.isStandalone ? (SPACING + LINEHEIGHT) * 2 : 0;
			//float boxheight = SPACING + HEADR_HGHT + SPACING + LINEHEIGHT + standalonesheight + SPACING;

			SolidTextures.DrawTexture(ir, SolidTextures.gray2D);

			//SolidTextures.DrawTexture(new Rect(ir.xMin - 1, line - 1, r.width + 2, boxheight + 2), SolidTextures.lowcontrast2D);
			//SolidTextures.DrawTexture(new Rect(ir.xMin, line, r.width, boxheight), SolidTextures.gray2D);

			line += SPACING;
			DrawHeader(new Rect(r));
			line += HEADR_HGHT + SPACING + SPACING;

			EditorGUI.indentLevel = 0;
			CompressLevel clvl = (CompressLevel)EditorGUI.EnumPopup(new Rect(ir.xMin + PADDING, line, labelwidth - PADDING, LINEHEIGHT), GUIContent.none, target.CompressLevel);
			EditorGUI.indentLevel = holdindent;

			if (!QC_ISPRO)
			{
				// In case we went from pro to free... quietly set this back to non-custom.
				if (target.CompressLevel == CompressLevel.SetBits)
				{
					if (target.Bits != (int)target.CompressLevel)
					{
						haschanged = true;
						target.Enabled = true;
						target.Bits = (int)target.CompressLevel; // CompressLevel =  CompressLevel.uint32Med;
					}

				}

				else if (clvl == CompressLevel.SetBits)
				{
					ProFeatureDialog("");
					if (target.CompressLevel != (CompressLevel)target.Bits)
					{
						haschanged = true;
						target.CompressLevel = (CompressLevel)target.Bits;
					}
				}

				else
				{
					if (target.CompressLevel != clvl)
					{
						target.Enabled = true;
						haschanged = true;
						target.CompressLevel = clvl;
					}
				}
			}

			else if (clvl != target.CompressLevel)
			{
				haschanged = true;
				target.CompressLevel = clvl;
			}

			//var bitssp = property.FindPropertyRelative("bits");

			GUI.enabled = (QC_ISPRO);
#if UNITY_2019_3_OR_NEWER
			int newbits = EditorGUI.IntSlider(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT+ 2),  GUIContent.none, target.Bits, 16, 64);
#else
			int newbits = EditorGUI.IntSlider(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT),  GUIContent.none, target.Bits, 16, 64);
#endif

			//bool bitschanged = EditorGUI.PropertyField(new Rect(fieldleft, line, fieldwidth, LINEHEIGHT), bitssp, GUIContent.none);
			GUI.enabled = true;

			if (QC_ISPRO && newbits != target.Bits)
			{
				//if (target.CompressLevel != CompressLevel.SetBits)
				//{
				haschanged = true;
				target.Bits = newbits;
				//target.CompressLevel = CompressLevel.SetBits;
				//}
				property.serializedObject.Update();
			}

			if (target.isStandalone)
			{
				line += LINEHEIGHT + SPACING;
				EditorGUI.PropertyField(new Rect(paddedleft, line, paddedwidth, LINEHEIGHT), property.FindPropertyRelative("transform"));
				line += LINEHEIGHT + SPACING;
				EditorGUI.PropertyField(new Rect(paddedleft, line, paddedwidth, LINEHEIGHT), property.FindPropertyRelative("local"));
			}

			property.serializedObject.ApplyModifiedProperties();

			if (haschanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
				//AssetDatabase.SaveAssets();
			}

			EditorGUI.EndProperty();
		}


		private void DrawHeader(Rect r)
		{
			String headertext = "Quat Compress";

			EditorGUI.indentLevel = 0;

			if (target.showEnableToggle) //  target.axis != Axis.AlwaysOn)
			{
				bool enabled = EditorGUI.Toggle(new Rect(ir.xMin + PADDING, line, 32, LINEHEIGHT), GUIContent.none, target.Enabled);
				if (target.Enabled != enabled)
				{
					haschanged = true;
					target.Enabled = enabled;
				}
				EditorGUI.LabelField(new Rect(ir.xMin + PADDING + 16, line, ir.width - 18, LINEHEIGHT), new GUIContent(headertext));
			}
			else
			{
				EditorGUI.LabelField(new Rect(ir.xMin + PADDING, line, ir.width, LINEHEIGHT), new GUIContent(headertext));
			}

			EditorGUI.LabelField(new Rect(ir.xMin + PADDING, line, ir.width - PADDING* 2, 16), target.Bits + " Bits", FloatCrusherDrawer.miniLabelRight);

			EditorGUI.indentLevel = holdindent;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float standalones = property.FindPropertyRelative("isStandalone").boolValue ? (SPACING + LINEHEIGHT) * 2 : 0;
			return SPACING + HEADR_HGHT + (SPACING + SPACING + LINEHEIGHT) + standalones + PADDING; // + BTTM_MARGIN;
		}
	}
#endif
		}