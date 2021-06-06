// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Utilities;
#endif

namespace Photon.Pun.Simple
{

	[System.Serializable]
	public struct MountMaskSelector
	{
		/// <summary>
		/// Default is an index of 0 and a layermask of 0
		/// </summary>
		public int mask;

		public MountMaskSelector(int mountTypeMask)
		{
			this.mask = mountTypeMask;
		}

        public MountMaskSelector(bool allTrue)
        {
            this.mask = allTrue ? MountSettings.AllTrueMask : 0;
        }

        public static implicit operator int(MountMaskSelector selector)
		{
			return selector.mask;
		}

		public static implicit operator MountMaskSelector(int mask)
		{
			return new MountMaskSelector(mask);
		}
	}

#if UNITY_EDITOR
	
	[CustomPropertyDrawer(typeof(MountMaskSelector))]
	[CanEditMultipleObjects]
	public class MountMaskSelectorDrawer : VersaMaskDrawer
	{
		protected override bool FirstIsZero { get { return false; } }
		protected override bool ShowMaskBits {  get { return false; } }

		protected override string[] GetStringNames(SerializedProperty property)
		{
		    return MountSettings.ToArray();
		}

		private static GUIContent bitsGC = new GUIContent("xxx", "Number of bits that will be used to serialize the id of the mount this is attached to.");
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(r, property, label);
			

			if (property.isExpanded)
			{
				int bits = (maskValue).CountTrueBits(MountSettings.mountTypeCount);
				int serbits = bits.GetBitsForMaxValue();
				bitsGC.text = serbits.ToString() + " bits";
				reuseGC.text = " ";
				reuseGC.tooltip = label.tooltip;
				EditorGUI.LabelField(new Rect(r) { height = LINE_SPACING }, reuseGC, bitsGC, (GUIStyle)"RightLabel");
			}
		}
	}

#endif
}
