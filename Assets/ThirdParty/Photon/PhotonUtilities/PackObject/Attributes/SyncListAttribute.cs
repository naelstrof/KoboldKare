// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Compression.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Compression
{
	public class SyncListAttribute : SyncVarBaseAttribute
	, IPackList<Int32>
	{
		#region List<Int32>

		public SerializationFlags Pack(ref List<Int32> value, List<Int32> prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			bool isKeyframe = IsKeyframe(frameId);
			bool forced = (writeFlags & (SerializationFlags.Force | SerializationFlags.ForceReliable | SerializationFlags.NewConnection)) != 0;

			//bool notforced = !IsForced(frameId, writeFlags);

			SerializationFlags flags = SerializationFlags.None;
			int holdpos = bitposition;

			for (int i = 0, cnt = value.Count; i < cnt; ++i)
			{
				var val = value[i];

				if (!isKeyframe)
				{
					if (!forced && val == prevValue[i])
					{
						buffer.WriteBool(false, ref bitposition);
						continue;
					}
					else
						buffer.WriteBool(true, ref bitposition);
				}

				buffer.WriteSignedPackedBytes(val, ref bitposition, bitCount);
				flags |= SerializationFlags.HasContent;
			}

			if (flags == SerializationFlags.None)
				bitposition = holdpos;

			//Debug.LogError(cnt + " SER " + frameId + " " + value[1] + " flgs: " + flags);

			return flags;
		}

		public SerializationFlags Unpack(ref List<Int32> value, BitArray isCompleteMask, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			//bool notforced = !IsForced(frameId, writeFlags);
			bool isKeyframe = IsKeyframe(frameId);

			SerializationFlags flags = SerializationFlags.None;

			var isComplete = SerializationFlags.IsComplete;

			for (int i = 0, cnt = value.Count; i < cnt; ++i)
			{
				if (!isKeyframe)
				{
					if (!buffer.ReadBool(ref bitposition))
					{
						isComplete = SerializationFlags.None;
						isCompleteMask[i] = false;
						//value[i] = prevValue[i];
						continue;
					}
				}

				isCompleteMask[i] = true;

				value[i] = buffer.ReadSignedPackedBytes(ref bitposition, bitCount);

				flags |= SerializationFlags.HasContent;
			}

			//Debug.LogError("Unpack List DES " + frameId + " <b>" + value[0] +":"+ value[1] + ":" + value[2] + "</b> " + " flgs: " + (flags | isComplete));
			//if (isComplete == SerializationFlags.IsComplete)
			//	Debug.LogError("Complete Synclist");

			return flags | isComplete;
		}

		#endregion

		#region List<Int32>

		public SerializationFlags Pack(ref List<UInt32> value, List<UInt32> prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			bool isKeyframe = IsKeyframe(frameId);
			bool forced = (writeFlags & (SerializationFlags.Force | SerializationFlags.ForceReliable | SerializationFlags.NewConnection)) != 0;

			//bool notforced = !IsForced(frameId, writeFlags);

			SerializationFlags flags = SerializationFlags.None;
			int holdpos = bitposition;

			for (int i = 0, cnt = value.Count; i < cnt; ++i)
			{
				var val = value[i];

				if (!isKeyframe)
				{
					if (!forced && val == prevValue[i])
					{
						buffer.WriteBool(false, ref bitposition);
						continue;
					}
					else
						buffer.WriteBool(true, ref bitposition);
				}

				buffer.WritePackedBytes(val, ref bitposition, bitCount);
				flags |= SerializationFlags.HasContent;
			}

			if (flags == SerializationFlags.None)
				bitposition = holdpos;

			//Debug.LogError(cnt + " SER " + frameId + " " + value[1] + " flgs: " + flags);

			return flags;
		}

		public SerializationFlags Unpack(ref List<UInt32> value, BitArray isCompleteMask, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			//bool notforced = !IsForced(frameId, writeFlags);
			bool isKeyframe = IsKeyframe(frameId);

			SerializationFlags flags = SerializationFlags.None;

			var isComplete = SerializationFlags.IsComplete;

			for (int i = 0, cnt = value.Count; i < cnt; ++i)
			{
				if (!isKeyframe)
				{
					if (!buffer.ReadBool(ref bitposition))
					{
						isComplete = SerializationFlags.None;
						isCompleteMask[i] = false;
						//value[i] = prevValue[i];
						continue;
					}
				}

				isCompleteMask[i] = true;

				value[i] = (uint)buffer.ReadPackedBytes(ref bitposition, bitCount);

				flags |= SerializationFlags.HasContent;
			}

			//Debug.LogError("Unpack List DES " + frameId + " <b>" + value[0] +":"+ value[1] + ":" + value[2] + "</b> " + " flgs: " + (flags | isComplete));
			//if (isComplete == SerializationFlags.IsComplete)
			//	Debug.LogError("Complete Synclist");

			return flags | isComplete;
		}

		#endregion

		/// <summary>
		/// Only copies elements when their bit in the associated mask == true.
		/// </summary>
		public static void Copy<T>(List<T> src, List<T> trg, BitArray mask) where T : struct
		{
			for (int i = 0, cnt = src.Count; i < cnt; ++i)
			{
				if (mask.Get(i))
					trg[i] = src[i];
			}
		}

		public static void Capture<T>(List<T> src, List<T> trg) where T : struct
		{
			for (int i = 0, cnt = src.Count; i < cnt; ++i)
			{
				trg[i] = src[i];
			}
		}

#if UNITY_EDITOR
		public override string GetFieldDeclareCodeGen(Type fieldType, string fulltypename, string fname)
		{
			/// Add a BitArray mask for Lists to the PackFrame
			return base.GetFieldDeclareCodeGen(fieldType, fulltypename, fname) + " public BitArray " + fname + "_mask;";

		}

		public override string GetCaptureCodeGen(Type fieldType, string fieldName, string s, string t)
		{
			return "{ for (int i = 0, cnt = " + s + "." + fieldName + ".Count; i < cnt; ++i) { " + t +"." + fieldName + "[i] = " + s + "." + fieldName + "[i]; } }";

		}
		public override string GetCopyCodeGen(Type fieldType, string fieldName, string s, string t)
		{
			return 
				"{ for (int i = 0, cnt = " + s + "." + fieldName + ".Count; i < cnt; ++i) { " +
					"if ("+ s + "." + fieldName + "_mask.Get(i)) " + t + "." + fieldName + "[i] = " + s + "." + fieldName + "[i]; } } ";
		}
#endif

	}
}

