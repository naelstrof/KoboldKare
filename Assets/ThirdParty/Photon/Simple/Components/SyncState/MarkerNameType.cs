// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities
{
	public enum MarkerType { None, Custom, Default, Player, NPC }

	[System.Serializable]
	public struct MarkerNameType
	{
		[HideInInspector] public MarkerType type;
		[HideInInspector] public int hash;
		[HideInInspector] public string name;

		public MarkerNameType(MarkerType vitalType)
		{
			this.type = vitalType;
			this.name = System.Enum.GetName(typeof(MarkerType), vitalType);
			this.hash = name.GetHashCode();
		}

		public MarkerNameType(string name)
		{
			this.type = (MarkerType)NameTypeUtils.GetVitalTypeForName(name, enumNames);
			this.name = name;
			this.hash = name.GetHashCode();
		}

		public static string[] enumNames = System.Enum.GetNames(typeof(MarkerType));

		public override string ToString()
		{
			return "NameType: " + type + " " + name + " " + hash;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(MarkerNameType))]
	[CanEditMultipleObjects]
	public class MarkerTypeDrawer : NameTypeDrawer
	{
		protected override string[] EnumNames { get { return MarkerNameType.enumNames; } }
	}

#endif
}

