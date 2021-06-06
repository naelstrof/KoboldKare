// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using emotitron.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

	public enum VitalType { None, Custom, Health, Armor, Shield, Energy, Mana, Rage }

	
	/// TODO: Make this a generic when working on .net versions that supports enum generic constraint.

	/// <summary>
	/// Enum type and name pair. Name will be the same text as the enum, unless the enum is Custom. Name value is converted to hash in editor and can be used as dict key.
	/// </summary>
	[System.Serializable]
	public struct VitalNameType
	{
		[HideInInspector] public VitalType type;
		[HideInInspector] public int hash;
		[HideInInspector] public string name;

		public VitalNameType(VitalType vitalType)
		{
			this.type = vitalType;
			this.name = System.Enum.GetName(typeof(VitalType), vitalType);
			this.hash = name.GetHashCode();
		}

		public VitalNameType(string name)
		{
			this.type = (VitalType)NameTypeUtils.GetVitalTypeForName(name, enumNames);
			this.name = name;
			this.hash = name.GetHashCode();
		}

		public static string[] enumNames = System.Enum.GetNames(typeof(VitalType));
		public string[] EnumNames { get { return enumNames; } }

		public override string ToString()
		{
			return "VitalNameType: " + type + " " + name + " " + hash;
		}
	}


#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(VitalNameType))]
	[CanEditMultipleObjects]
	public class VitalNameTypeDrawer : NameTypeDrawer
	{
		//protected override string NameTypeFieldname {  get { return "vitalType"; } }
		//protected override string NameFieldname { get { return "vitalName"; } }
		protected override string[] EnumNames { get { return VitalNameType.enumNames; } }
	}

#endif
}
