// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Text;
using Photon.Compression.Internal;
using UnityEngine;
using System.Collections.Generic;

namespace Photon.Compression
{

	/// <summary>
	/// The default packing handler. Use specific PackXXX attributes for better control.
	/// </summary>
	public class SyncVarAttribute : SyncVarBaseAttribute,
		IPackByte, IPackSByte,
		IPackUInt16, IPackInt16,
		IPackUInt32, IPackInt32,
		IPackUInt64, IPackInt64,
		IPackSingle, IPackDouble,
		IPackString, IPackStringBuilder,
		IPackVector2, IPackVector3,
		IPackVector2Int, IPackVector3Int,
        IPackBoolean, IPackChar
		//IPackList<Int32>
	{
		public const int MAX_STR_LEN = 63;
		public const int STR_LEN_BITS = 6;

        /// <summary>
        /// Will round float and double types to integer values, allowing for PackedByte compression.
        /// </summary>
        public bool WholeNumbers;

        public SyncVarAttribute(KeyRate keyRate = KeyRate.UseDefault)
		{
			this.keyRate = keyRate;
		}
		
		public override int GetMaxBits(Type fieldType)
		{
			if (fieldType == typeof(String) || fieldType == typeof(StringBuilder))
				return (MAX_STR_LEN + 1) * 16 + STR_LEN_BITS;

			if (fieldType == typeof(Vector2))
				return 64;

			if (fieldType == typeof(Vector2))
				return 96;

			return base.GetMaxBits(fieldType);
		}

		#region Test List

		public bool Compare<T>(List<T> a, List<T> b) where T : struct
		{
			int acnt = a.Count;
			if (acnt != b.Count)
				return false;

			for (int i = 0; i < acnt; ++i)
				if (a[i].Equals(b[i]) == false)
					return false;

			return false;
		}

		

		//public void Copy(List<Int32> to, List<Int32> from)
		//{
		//	to.Clear();
		//	to.AddRange(from);
		//}


		#endregion

		#region Bool

		public SerializationFlags Pack(ref Boolean value, Boolean prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			buffer.Write((value ? (ulong)1 : 0), ref bitposition, 1);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Boolean value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = buffer.Read(ref bitposition, 1) == 0 ? false : true;
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region 8 bits

		public SerializationFlags Pack(ref Byte value, Byte prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.Write(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Byte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (byte)buffer.Read(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref SByte value, SByte prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WriteSigned(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref SByte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (sbyte)buffer.ReadSigned(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region 16 bits

		public SerializationFlags Pack(ref UInt16 value, UInt16 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.Write(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref UInt16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (ushort)buffer.Read(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int16 value, Int16 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			Debug.Log("Pack "+ bitCount);
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WriteSigned(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Int16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (short)buffer.ReadSigned(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region 16 Char

		public SerializationFlags Pack(ref Char value, Char prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.Write(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Char value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Char)buffer.Read(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}


		#endregion


		#region 32 bits

		public SerializationFlags Pack(ref UInt32 value, UInt32 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WritePackedBytes(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref UInt32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (UInt32)buffer.ReadPackedBytes(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int32 value, Int32 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WriteSignedPackedBytes(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Int32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Int32)buffer.ReadSignedPackedBytes(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region 64 bits

		public SerializationFlags Pack(ref UInt64 value, UInt64 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WritePackedBytes(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref UInt64 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (UInt64)buffer.ReadPackedBytes(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int64 value, Int64 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue,  writeFlags))
				return SerializationFlags.None;

			buffer.WriteSignedPackedBytes64(value, ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref Int64 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = buffer.ReadSignedPackedBytes64(ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region Floating Point Singe/Double

		public SerializationFlags Pack(ref Single value, Single prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                int nval = (int)Math.Round(value);
                int pval = (int)Math.Round(prevValue);

                if (!IsForced(frameId, nval, pval, writeFlags))
                    return SerializationFlags.None;

                buffer.WriteSignedPackedBytes(nval, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            else
            {
                if (!IsForced(frameId, value, prevValue, writeFlags))
                    return SerializationFlags.None;

                buffer.Write((uint)(ByteConverter)value, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            
		}

		public SerializationFlags Unpack(ref Single value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                value = buffer.ReadSignedPackedBytes(ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            else
            {
                value = (ByteConverter)buffer.Read(ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
			
		}

		public SerializationFlags Pack(ref Double value, Double prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                long nval = (int)Math.Round(value);
                long pval = (int)Math.Round(prevValue);

                if (!IsForced(frameId, nval, pval, writeFlags))
                    return SerializationFlags.None;

                buffer.WriteSignedPackedBytes64(nval, ref bitposition, 64);
                return SerializationFlags.IsComplete;
            }
            else
            {
                if (!IsForced(frameId, value, prevValue, writeFlags))
                    return SerializationFlags.None;

                buffer.Write((ByteConverter)value, ref bitposition, 64);
                return SerializationFlags.IsComplete;
            }
            
		}

		public SerializationFlags Unpack(ref Double value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                value = buffer.ReadSignedPackedBytes64(ref bitposition, 64);
                return SerializationFlags.IsComplete;
            }
            else
            {
                value = (ByteConverter)buffer.Read(ref bitposition, 64);
                return SerializationFlags.IsComplete;
            }
			
		}

		#endregion

		#region String

		public SerializationFlags Pack(ref String value, String prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			if (!IsForcedClass(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			if (value == null)
			{
				buffer.Write(0, ref bitposition, 6);
				return SerializationFlags.IsComplete;
			}

			/// Clamping
			int cnt = value.Length;
			if (cnt > MAX_STR_LEN)
			{
				cnt = MAX_STR_LEN;
#if UNITY_EDITOR
				Debug.LogWarning("Default Pack attribute has a max string length of 63 characters. Use PackString for more options.");
#endif
			}

			/// Write string len
			buffer.Write((uint)cnt, ref bitposition, 6);

			/// Write string
			for (int i = 0; i < cnt; ++i)
				buffer.Write((ushort)value[i], ref bitposition, bitCount);
			return SerializationFlags.IsComplete;
		}

		private static StringBuilder sb = new StringBuilder(0);

		public SerializationFlags Unpack(ref String value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			sb.Length = 0;
			int cnt = (int)buffer.Read(ref bitposition, 6);

			for (int i = 0; i < cnt; ++i)
				sb.Append((Char)buffer.Read(ref bitposition, bitCount));

			value = sb.ToString();
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region StringBuilder

		public SerializationFlags Pack(ref StringBuilder value, StringBuilder prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			if (!IsForcedClass(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			if (value == null)
			{
				buffer.Write(0, ref bitposition, STR_LEN_BITS);
				return SerializationFlags.IsComplete;
			}

			/// Clamping
			int cnt = value.Length;
			if (cnt > MAX_STR_LEN)
			{
				cnt = MAX_STR_LEN;
#if UNITY_EDITOR
				Debug.LogWarning("Default Pack attribute has a max string length of " + MAX_STR_LEN + " characters. Use PackString for more options. Clamping string to \"" +
					value.ToString(0, MAX_STR_LEN) + "\"");
#endif
			}

			/// Write string len
			buffer.Write((uint)cnt, ref bitposition, STR_LEN_BITS);

			/// Write string
			for (int i = 0; i < cnt; ++i)
				buffer.Write((ushort)value[i], ref bitposition, bitCount);

			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Unpack(ref StringBuilder value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (value == null)
				value = new StringBuilder(64);
			else
				value.Length = 0;

			int cnt = (int)buffer.Read(ref bitposition, STR_LEN_BITS);

			for (int i = 0; i < cnt; ++i)
				value.Append((Char)buffer.Read(ref bitposition, bitCount));
			return SerializationFlags.IsComplete;
		}

		#endregion

		#region Vector2

		public SerializationFlags Pack(ref Vector2 value, Vector2 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                Vector2Int newval = new Vector2Int((int)value.x, (int)value.y);
                Vector2Int oldval = new Vector2Int((int)prevValue.x, (int)prevValue.y);

                if (!IsForced(frameId, newval, oldval, writeFlags))
                    return SerializationFlags.None;

                buffer.WriteSignedPackedBytes(newval.x, ref bitposition, 32);
                buffer.WriteSignedPackedBytes(newval.y, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            else
            {
                if (!IsForced(frameId, value, prevValue, writeFlags))
                    return SerializationFlags.None;

                buffer.Write((uint)(ByteConverter)value.x, ref bitposition, 32);
                buffer.Write((uint)(ByteConverter)value.y, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
			
		}

		public SerializationFlags Unpack(ref Vector2 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                value = new Vector2(
                   buffer.ReadSignedPackedBytes(ref bitposition, 32),
                   buffer.ReadSignedPackedBytes(ref bitposition, 32)
                   );
                return SerializationFlags.IsComplete;
            }
            else
            {
                value = new Vector2(
                   (ByteConverter)buffer.Read(ref bitposition, 32),
                   (ByteConverter)buffer.Read(ref bitposition, 32)
                   );
                return SerializationFlags.IsComplete;
            }
           
		}

		#endregion

		#region Vector3

		public SerializationFlags Pack(ref Vector3 value, Vector3 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                Vector3Int newval = new Vector3Int((int)value.x, (int)value.y, (int)value.z);
                Vector3Int oldval = new Vector3Int((int)prevValue.x, (int)prevValue.y, (int)prevValue.z);

                if (!IsForced(frameId, newval, oldval, writeFlags))
                    return SerializationFlags.None;

                buffer.WriteSignedPackedBytes(newval.x, ref bitposition, 32);
                buffer.WriteSignedPackedBytes(newval.y, ref bitposition, 32);
                buffer.WriteSignedPackedBytes(newval.z, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            else
            {
                if (!IsForced(frameId, value, prevValue, writeFlags))
                    return SerializationFlags.None;

                buffer.Write((uint)(ByteConverter)value.x, ref bitposition, 32);
                buffer.Write((uint)(ByteConverter)value.y, ref bitposition, 32);
                buffer.Write((uint)(ByteConverter)value.z, ref bitposition, 32);
                return SerializationFlags.IsComplete;
            }
            
		}

		public SerializationFlags Unpack(ref Vector3 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
            if (WholeNumbers)
            {
                value = new Vector3(
                    buffer.ReadSignedPackedBytes(ref bitposition, 32),
                    buffer.ReadSignedPackedBytes(ref bitposition, 32),
                    buffer.ReadSignedPackedBytes(ref bitposition, 32));
                return SerializationFlags.IsComplete;
            }
            else
            {
                value = new Vector3(
                   (ByteConverter)buffer.Read(ref bitposition, 32),
                   (ByteConverter)buffer.Read(ref bitposition, 32),
                   (ByteConverter)buffer.Read(ref bitposition, 32));
                return SerializationFlags.IsComplete;
            }

           
		}

        #region Vector2Int

        public SerializationFlags Pack(ref Vector2Int value, Vector2Int prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        {

            if (!IsForced(frameId, value, prevValue, writeFlags))
                return SerializationFlags.None;

            buffer.WriteSignedPackedBytes(value.x, ref bitposition, 32);
            buffer.WriteSignedPackedBytes(value.y, ref bitposition, 32);
            return SerializationFlags.IsComplete;
        }

        public SerializationFlags Unpack(ref Vector2Int value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        {
            value = new Vector2Int(
                buffer.ReadSignedPackedBytes(ref bitposition, 32),
                buffer.ReadSignedPackedBytes(ref bitposition, 32)
                );
            return SerializationFlags.IsComplete;
        }

        #endregion

        #region Vector3Int

        public SerializationFlags Pack(ref Vector3Int value, Vector3Int prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        {

            if (!IsForced(frameId, value, prevValue, writeFlags))
                return SerializationFlags.None;

            buffer.WriteSignedPackedBytes(value.x, ref bitposition, 32);
            buffer.WriteSignedPackedBytes(value.y, ref bitposition, 32);
            buffer.WriteSignedPackedBytes(value.z, ref bitposition, 32);
            return SerializationFlags.IsComplete;
        }

        public SerializationFlags Unpack(ref Vector3Int value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        {
            value = new Vector3Int(
                buffer.ReadSignedPackedBytes(ref bitposition, 32),
                buffer.ReadSignedPackedBytes(ref bitposition, 32),
                buffer.ReadSignedPackedBytes(ref bitposition, 32)
                );
            return SerializationFlags.IsComplete;
        }

        #endregion

        #endregion

        #region Vitals

        //public SerializationFlags Pack(ref VitalsData value, VitalsData prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        //{

        //	if (!IsForcedClass(frameId, value, prevValue,  writeFlags))
        //		return SerializationFlags.None;

        //	bool isKeyFrame = IsKeyframe(frameId);

        //	return value.vitals.Serialize(value, prevValue, buffer, ref bitposition, isKeyFrame);
        //}

        //public SerializationFlags Unpack(ref VitalsData value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
        //{
        //	return value.vitals.Deserialize(value, buffer, ref bitposition, IsKeyframe(frameId));

        //}

        #endregion
    }
}
