// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;

namespace Photon.Compression.Internal
{
	
	public class SyncRangedAttribute : SyncVarBaseAttribute
		, IPackSingle
	{
		LiteFloatCrusher crusher = new LiteFloatCrusher();

		public SyncRangedAttribute(LiteFloatCompressType compression, Single min, Single max, bool accurateCenter)
		{
			LiteFloatCrusher.Recalculate(compression, min, max, accurateCenter, crusher);
		}
		public SerializationFlags Pack(ref Single value, Single preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			uint cval = (uint)crusher.Encode(value);

			if (!IsForced(frameId, writeFlags) && cval == (uint)crusher.Encode(preValue))
				return SerializationFlags.None;

			crusher.WriteCValue(cval, buffer, ref bitposition);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Single value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = crusher.ReadValue(buffer, ref bitposition);
			return SerializationFlags.IsComplete;
		}

	}

}
