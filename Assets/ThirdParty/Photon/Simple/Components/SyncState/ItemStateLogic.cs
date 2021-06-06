// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------


namespace Photon.Pun.Simple
{
	[System.Serializable]
	public class ObjStateLogic : MaskLogic
	{
		protected static int[] stateValues = (int[])System.Enum.GetValues(typeof(ObjStateEditor));
		protected static string[] stateNames = System.Enum.GetNames(typeof(ObjStateEditor));

		protected override bool DefinesZero { get { return true; } }
		protected override string[] EnumNames { get { return stateNames; } }
		protected override int[] EnumValues { get { return stateValues; } }
		protected override int DefaultValue { get { return (int)ObjStateEditor.Visible; } }

#if UNITY_EDITOR
		protected override string EnumTypeName { get { return typeof(ObjStateEditor).Name; } }
#endif


	}
}
