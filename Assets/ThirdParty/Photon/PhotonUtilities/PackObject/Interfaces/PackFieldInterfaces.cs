// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Photon.Compression.Internal
{

	[AttributeUsage(AttributeTargets.Interface)]
	public class PackSupportedTypesAttribute : Attribute
	{
		public Type supportedType;

		public PackSupportedTypesAttribute(Type supportedType)
		{
			this.supportedType = supportedType;
		}
	}

	public delegate SerializationFlags PackDelegate<T>(ref T value, T prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	public delegate SerializationFlags UnpackDelegate<T>(ref T value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	public delegate SerializationFlags PackListDelegate<T>(ref List<T> value, List<T> prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags) where T : struct;
	public delegate SerializationFlags UnpackListDelegate<T>(ref List<T> value, BitArray mask, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags) where T : struct;

	//public delegate SerializationFlags PackEnumDelegate(ref Int32 value, Int32 prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	//[PackSupportedTypes(typeof(Enum))]
	//public interface IPackBEnum
	//{
	//	SerializationFlags Pack(ref Int32 value, Int32 prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	//	SerializationFlags Unpack(ref Int32 value, Int32 prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	//}

	[PackSupportedTypes(typeof(List<>))]
	public interface IPackList<T> where T : struct
	{
		SerializationFlags Pack(ref List<T> value, List<T> prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref List<T> value, BitArray mask, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Boolean))]
	public interface IPackBoolean
	{
		SerializationFlags Pack(ref Boolean value, Boolean prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Boolean value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Char))]
	public interface IPackChar
	{
		SerializationFlags Pack(ref Char value, Char prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Char value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Byte))]
	public interface IPackByte
	{
		SerializationFlags Pack(ref Byte value, Byte prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Byte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(SByte))]
	public interface IPackSByte
	{
		SerializationFlags Pack(ref SByte value, SByte prevvalue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref SByte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(UInt16))]
	public interface IPackUInt16
	{
		SerializationFlags Pack(ref UInt16 value, UInt16 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref UInt16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Int16))]
	public interface IPackInt16
	{
		SerializationFlags Pack(ref Int16 value, Int16 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Int16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(UInt32))]
	public interface IPackUInt32
	{
		SerializationFlags Pack(ref UInt32 value, UInt32 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref UInt32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Int32))]
	public interface IPackInt32
	{
		SerializationFlags Pack(ref Int32 value, Int32 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Int32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(UInt64))]
	public interface IPackUInt64
	{
		SerializationFlags Pack(ref UInt64 value, UInt64 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref UInt64 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Int64))]
	public interface IPackInt64
	{
		SerializationFlags Pack(ref Int64 value, Int64 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Int64 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	public delegate SerializationFlags PackSingleDelegate(ref Single value, Single preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	[PackSupportedTypes(typeof(Single))]
	public interface IPackSingle
	{
		SerializationFlags Pack(ref Single value, Single preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Single value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Double))]
	public interface IPackDouble
	{
		SerializationFlags Pack(ref Double value, Double preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Double value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(String))]
	public interface IPackString
	{
		SerializationFlags Pack(ref String value, String preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref String value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(StringBuilder))]
	public interface IPackStringBuilder
	{
		SerializationFlags Pack(ref StringBuilder value, StringBuilder preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref StringBuilder value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	#region Unity Types

	[PackSupportedTypes(typeof(Vector2))]
	public interface IPackVector2
	{
		SerializationFlags Pack(ref Vector2 value, Vector2 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Vector2 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

	[PackSupportedTypes(typeof(Vector3))]
	public interface IPackVector3
	{
		SerializationFlags Pack(ref Vector3 value, Vector3 preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
		SerializationFlags Unpack(ref Vector3 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
	}

    [PackSupportedTypes(typeof(Vector2Int))]
    public interface IPackVector2Int
    {
        SerializationFlags Pack(ref Vector2Int value, Vector2Int preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
        SerializationFlags Unpack(ref Vector2Int value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
    }

    [PackSupportedTypes(typeof(Vector3Int))]
    public interface IPackVector3Int
    {
        SerializationFlags Pack(ref Vector3Int value, Vector3Int preValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
        SerializationFlags Unpack(ref Vector3Int value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags);
    }

    #endregion

}
