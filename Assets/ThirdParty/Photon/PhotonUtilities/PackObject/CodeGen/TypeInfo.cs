// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

namespace Photon.Compression.Internal
{
	[System.Serializable]
	public class TypeInfo
	{
		public long hashcode;
		public string filepath;
		public long codegenFileWriteTime;
		public int localFieldCount;
		public int totalFieldCount;

		public TypeInfo(System.Type type)
		{
			hashcode = type.TypeToHash64();
		}
	}
}

#endif