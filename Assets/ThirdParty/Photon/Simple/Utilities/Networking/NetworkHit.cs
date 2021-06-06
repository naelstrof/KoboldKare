// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Compression;

namespace Photon.Pun.Simple
{

	/// <summary>
	/// A networkable representation of an object hit, from a collision, trigger or hitscan.
	/// </summary>
	public struct NetworkHit
	{
		public readonly int netObjId;
		public readonly int hitMask;
		public readonly int colliderId;

		public NetworkHit(int objectID, int hitMask, int colliderId)
		{
			this.netObjId = objectID;
			this.hitMask = hitMask;
			this.colliderId = colliderId;
		}

		public void Serialize(byte[] buffer, ref int bitposition, int bitsForHitmask, int bitsForColliderId)
		{
			buffer.WritePackedBytes((ulong)netObjId, ref bitposition, 32);
			buffer.Write((ulong)hitMask, ref bitposition, bitsForHitmask);
			buffer.Write((ulong)colliderId, ref bitposition, bitsForColliderId);
		}

		public static NetworkHit Deserialize(byte[] buffer, ref int bitposition, int bitsForHitmask, int bitsForColliderId)
		{
			int objectId = (int)buffer.ReadPackedBytes(ref bitposition, 32);
			int hitmask = (int)buffer.Read(ref bitposition, bitsForHitmask);
			int colliderId = (int)buffer.Read(ref bitposition, bitsForColliderId);

			return new NetworkHit(objectId, hitmask, colliderId);
		}
	}

}

