// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Compression.Internal
{
	public abstract class PackFrame
	{
		public FastBitMask128 mask;
		public FastBitMask128 isCompleteMask;
	}
}

namespace Photon.Compression.Internal
{

	/// <summary>
	/// Runtime lookup for packing delegates all get associated with Types at runtime and are stored here.
	/// Generated extensions register themselves with this on startup.
	/// </summary>
	public static class PackObjectDatabase
	{
		public delegate SerializationFlags PackStructDelegate(IntPtr obj, PackFrame prevFrame, ref FastBitMask128 mask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags forceKeyframe);
		public delegate SerializationFlags PackObjDelegate(ref System.Object obj, PackFrame prevFrame, ref FastBitMask128 mask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags forceKeyframe);
		public delegate SerializationFlags PackFrameDelegate(PackFrame obj, PackFrame prevFrame, ref FastBitMask128 mask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags forceKeyframe);
		public delegate SerializationFlags UnpackFrameDelegate(PackFrame obj, ref FastBitMask128 hasContentMask, ref FastBitMask128 isCompleteMask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags forceKeyframe);

		public delegate void PackCopyFrameToObjectDelegate(PackFrame src, System.Object trg, ref FastBitMask128 mask, ref int maskOffset);
		public delegate void PackCopyFrameToStructDelegate(PackFrame src, IntPtr trg, ref FastBitMask128 mask, ref int maskOffset);

		public delegate void PackSnapshotObjectDelegate(PackFrame snap, PackFrame targ, System.Object trg, ref FastBitMask128 readyMask, ref int maskOffset);
		public delegate void PackSnapshotStructDelegate(PackFrame snap, PackFrame targ, IntPtr trg, ref FastBitMask128 readyMask, ref int maskOffset);

		public delegate void PackInterpFrameToFrameDelegate(PackFrame start, PackFrame end, PackFrame trg, float ntime, ref FastBitMask128 readyMask, ref int maskOffset);
		public delegate void PackInterpFrameToObjectDelegate(PackFrame start, PackFrame end, System.Object trg, float ntime, ref FastBitMask128 readyMask, ref int maskOffset);
		public delegate void PackInterpFrameToStructDelegate(PackFrame start, PackFrame end, IntPtr trg, float ntime, ref FastBitMask128 readyMask, ref int maskOffset);

		public static Dictionary<Type, PackObjectInfo> packObjInfoLookup = new Dictionary<Type, PackObjectInfo>();

		public static PackObjectInfo GetPackObjectInfo(Type type)
		{
			PackObjectInfo info;
			if (packObjInfoLookup.TryGetValue(type, out info))
				return info;

			/// May have not be initialized in time, brute force reflection initialize.
			var packType = Type.GetType("Pack_" + type.Name);
			if (packType != null)
			{
				Debug.LogError("BRUTE FORCE Pack_" + type.Name + ". This shouldn't happen.");
				packType.GetMethod("Initialize").Invoke(null, null);
			}

			if (packObjInfoLookup.TryGetValue(type, out info))
				return info;

#if UNITY_EDITOR
            Debug.LogWarning("PackObject code has not been generated for type '" + type.Name + "'. Is Code Generation disabled in " + typeof(PackObjectSettings).Name + "?");
#endif
            return null;
		}

		public class PackObjectInfo
		{
			public readonly Type packFrameType;
			public readonly int maxBits;
			public readonly int maxBytes;
			public readonly FastBitMask128 defaultReadyMask;

			public readonly PackObjDelegate PackObjToBuffer;
			public readonly PackStructDelegate PackStructToBuffer;
			public readonly PackFrameDelegate PackFrameToBuffer;
			public readonly UnpackFrameDelegate UnpackFrameFromBuffer;
			public Func<PackFrame> FactoryFrame;
			public Func<System.Object, int, PackFrame[]> FactoryFramesObj;
			public Func<IntPtr, int, PackFrame[]> FactoryFramesStruct;
			public PackCopyFrameToObjectDelegate CopyFrameToObj;
			public PackCopyFrameToStructDelegate CopyFrameToStruct;
			public PackSnapshotObjectDelegate SnapObject;
			public PackSnapshotStructDelegate SnapStruct;
			public PackInterpFrameToFrameDelegate InterpFrameToFrame;
			public PackInterpFrameToObjectDelegate InterpFrameToObj;
			public PackInterpFrameToStructDelegate InterpFrameToStruct;
			public Action<System.Object, PackFrame> CaptureObj;
			public Action<IntPtr, PackFrame> CaptureStruct;
			public Action<PackFrame, PackFrame> CopyFrameToFrame;

			public readonly int fieldCount;

			/// Class Type Constructor
			public PackObjectInfo(
				FastBitMask128 defaultReadyMask,
				PackObjDelegate packObjToBuffer, 
				//PackObjDelegate unpackObjFromBuffer,
				PackFrameDelegate packFrameToBuffer,
				UnpackFrameDelegate unpackFrameFromBuffer,
				int maxBits,
				Func<PackFrame> factoryFrame,
				Func<System.Object, int, PackFrame[]> factoryFramesObj,
				PackCopyFrameToObjectDelegate copyFrameToObj,
				Action<System.Object, PackFrame> captureObj,
				Action<PackFrame, PackFrame> copyFrameToFrame,
				PackSnapshotObjectDelegate snapObject,
				PackInterpFrameToFrameDelegate interpFrameToFrame,
				PackInterpFrameToObjectDelegate interpFrameToObj,
				int fieldCount
				)
			{
				this.PackObjToBuffer = packObjToBuffer;
				this.defaultReadyMask = defaultReadyMask;
				//this.unpackObjFromBuffer = unpackObjFromBuffer;
				this.PackFrameToBuffer = packFrameToBuffer;
				this.UnpackFrameFromBuffer = unpackFrameFromBuffer;
				this.maxBits = maxBits;
				this.maxBytes = (maxBits + 7) >> 3;
				this.FactoryFrame = factoryFrame;
				this.FactoryFramesObj = factoryFramesObj;
				this.CopyFrameToObj = copyFrameToObj;
				this.CaptureObj = captureObj;
				this.CopyFrameToFrame = copyFrameToFrame;
				this.SnapObject = snapObject;
				this.InterpFrameToFrame = interpFrameToFrame;
				this.InterpFrameToObj = interpFrameToObj;
				this.fieldCount = fieldCount;
			}

			/// Struct Type Constructor
			public PackObjectInfo(
				FastBitMask128 defaultReadyMask,
				PackStructDelegate packStructToBuffer,
				//PackStructDelegate unpackStructFromBuffer,
				PackFrameDelegate packFrameToBuffer,
				UnpackFrameDelegate unpackFrameFromBuffer,
				int maxBits,
				Func<PackFrame> factoryFrame,
				Func<IntPtr, int, PackFrame[]> factoryFramesStruct,
				PackCopyFrameToStructDelegate copyFrameToStruct,
				Action<IntPtr, PackFrame> captureStruct,
				Action<PackFrame, PackFrame> copyFrameToFrame,
				PackSnapshotStructDelegate snapStruct,
				PackInterpFrameToFrameDelegate interpFrameToFrame,
				PackInterpFrameToStructDelegate interpFrameToStruct,
				int fieldCount
				)
			{
				this.defaultReadyMask = defaultReadyMask;
				this.PackStructToBuffer = packStructToBuffer;
				//this.unpackStructFromBuffer = unpackStructFromBuffer;
				this.PackFrameToBuffer = packFrameToBuffer;
				this.UnpackFrameFromBuffer = unpackFrameFromBuffer;
				this.maxBits = maxBits;
				this.maxBytes = (maxBits + 7) >> 3;
				this.FactoryFrame = factoryFrame;
				this.FactoryFramesStruct = factoryFramesStruct;
				this.CopyFrameToStruct = copyFrameToStruct;
				this.CaptureStruct = captureStruct;
				this.CopyFrameToFrame = copyFrameToFrame;
				this.SnapStruct = snapStruct;
				this.InterpFrameToFrame = interpFrameToFrame;
				this.InterpFrameToStruct = interpFrameToStruct;
				this.fieldCount = fieldCount;

			}
		}
	}
}

