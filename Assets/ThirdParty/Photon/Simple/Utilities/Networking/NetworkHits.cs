// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Compression;
using System.Collections.Generic;

namespace Photon.Pun.Simple
{
	/// <summary>
	/// A reusable collection of NetworkHit that can be serialized.
	/// </summary>
	public class NetworkHits
	{
		public readonly List<NetworkHit> hits = new List<NetworkHit>();
		public bool nearestOnly;
		public int nearestIndex = -1;
		public int bitsForContactGroupMask;

		public NetworkHits(bool nearestOnly, int bitsForContactGroupMask)
		{
			this.nearestOnly = nearestOnly;
			this.bitsForContactGroupMask = bitsForContactGroupMask;
		}

		public void Reset(bool nearestOnly, int bitsForContactGroupMask)
		{
			this.nearestOnly = nearestOnly;
			this.bitsForContactGroupMask = bitsForContactGroupMask;
			hits.Clear();
			nearestIndex = -1;
		}

		public void Clear()
		{
			hits.Clear();
			nearestIndex = -1;
		}

		public SerializationFlags Serialize(byte[] buffer, ref int bitposition, int bitsForColliderId)
		{
			SerializationFlags flags = SerializationFlags.None;

			if (nearestOnly)
			{
				if (nearestIndex != -1)
				{
					buffer.WriteBool(true, ref bitposition);
					hits[nearestIndex].Serialize(buffer, ref bitposition, bitsForContactGroupMask, bitsForColliderId);
					flags = SerializationFlags.HasContent;
				}
				else
					buffer.WriteBool(false, ref bitposition);
			}
			else
			{
				for (int i = 0, cnt = hits.Count; i < hits.Count; ++i)
				{
					buffer.WriteBool(true, ref bitposition);
					hits[i].Serialize(buffer, ref bitposition, bitsForContactGroupMask, bitsForColliderId);
					flags = SerializationFlags.HasContent;

				}
				buffer.WriteBool(false, ref bitposition);
			}

			return flags;
		}

		public SerializationFlags Deserialize(byte[] buffer, ref int bitposition, int bitsForColliderId)
		{
			hits.Clear();
			SerializationFlags flags = SerializationFlags.None;

			//bool nearestOnly = definition.hitscanType.IsCast() && definition.nearestOnly;
			if (nearestOnly)
			{
				if (buffer.ReadBool(ref bitposition))
				{
					hits.Add(NetworkHit.Deserialize(buffer, ref bitposition, bitsForContactGroupMask, bitsForColliderId));
					flags = SerializationFlags.HasContent;
					nearestIndex = 0;
				}
				else
					nearestIndex = -1;
			}
			else
			{
				while (buffer.ReadBool(ref bitposition))
				{
					hits.Add(NetworkHit.Deserialize(buffer, ref bitposition, bitsForContactGroupMask, bitsForColliderId));
					flags = SerializationFlags.HasContent;
				}
			}

			return flags;
		}

		public override string ToString()
		{
			string str = GetType().Name;
			for (int i = 0; i < hits.Count; ++i)
				str += "\nObj:" + hits[i].netObjId + " Mask:" + hits[i].hitMask;
			
			return str;
		}
	}
}

