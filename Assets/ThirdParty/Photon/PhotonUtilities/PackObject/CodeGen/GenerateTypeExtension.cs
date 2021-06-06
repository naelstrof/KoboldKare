// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Photon.Compression.Internal
{
	public static class GenerateTypeExt
	{

		public const string PACK_PREFIX = "Pack_";
		public const string PACKFRM_PREFIX = "PackFrame_";
		public const string PREV_FRM_NAME = "prevFrame";
		public const string PREV_NAME = "prev";

		public const string SUPRESS_UNUSED = "#pragma warning disable 1635";
		public const string RESTORE_UNUSED = "#pragma warning restore 1635";
		public const string SUPRESS_UNUSED2 = "#pragma warning disable 0219";
		public const string RESTORE_UNUSED2 = "#pragma warning disable 0219";

		private static StringBuilder sb = new StringBuilder();
		private static StringBuilder sbFrame = new StringBuilder();
		private static StringBuilder sbFactory = new StringBuilder();
		private static StringBuilder sbSnapshot = new StringBuilder();
		private static StringBuilder sbCopyToFrame = new StringBuilder();
		
		private static StringBuilder sbCapture = new StringBuilder();
		private static StringBuilder sbApplyToObj = new StringBuilder();
		private static StringBuilder sbCopyFrameToFrame = new StringBuilder();
		private static StringBuilder sbInterpToObj = new StringBuilder();
		private static StringBuilder sbInterpToFrame = new StringBuilder();
		private static StringBuilder sbDelegates = new StringBuilder();

		private static StringBuilder sbInit = new StringBuilder();
		private static StringBuilder sbPack = new StringBuilder();
		private static StringBuilder sbUnpack = new StringBuilder();

		static bool hasSnapCallbacks;

		private static string serializationFlags = typeof(SerializationFlags).Name;
		private static string serializationFlagsLine1 = serializationFlags + " flags = " + serializationFlags + "." + System.Enum.GetName(typeof(SerializationFlags), SerializationFlags.None) + ";\n";

		public static StringBuilder GeneratePackCode(this Type objType, TypeInfo objTypeInfo, PackObjectAttribute pObjAttr)
		{
			bool isStruct = objType.IsValueType;

			string objTypeName = objType.Name;
			string objTypeNamespace = objType.Namespace;
			string ffulltype = (objType.Namespace == null || objType.Namespace == "") ? objType.Name : (objType.Namespace + "." + objType.Name);

			string structCast = isStruct ? "*(" + ffulltype + "*)" : null;

			string packFrameName = "PackFrame_" + objTypeName;

			string fullClass = ReplacePlusWithPeriod(objType.FullName);

			string derefObj = objType.IsValueType ? "(*(" + ffulltype + "*)trg)" : "t";

			GenerateFields(objType);

			string fullObjName = objType.FullName; // (objTypeNamespace == null || objTypeNamespace == "") ? objTypeName : (objTypeNamespace + "." + objTypeName);

			sb.Length = 0;
			sb._("using System;").EOL();
			sb._("using System.Reflection;").EOL();
			sb._("using System.Collections;").EOL();
			sb._("using Photon.Utilities;").EOL();
			sb._("using UnityEngine;").EOL();

			sb._(SUPRESS_UNUSED).EOL();
			sb._(SUPRESS_UNUSED2).EOL();
			/// Namespace
			{
				sb._("namespace Photon.Compression.Internal").EOL();
				sb._("{").EOL();

				/// PackFrame
				{
					sb.__("public class ", packFrameName, " : PackFrame").EOL();
					sb.__("{").EOL();

					/// Frame Fields
					sb._(sbFrame.ToString());
					sb.EOL();

					/// Interpolate to Frame
					sb.___("public static void Interpolate(PackFrame start, PackFrame end, PackFrame trg, float time, ref FastBitMask128 readyMask, ref int maskOffset)").EOL();

					sb.___("{").EOL();

					if (sbInterpToFrame.Length != 0)
					{
						sb.____("var s = start as ", packFrameName, ";").EOL();
						sb.____("var e = end as ", packFrameName, ";").EOL();
						sb.____("var t = trg as ", packFrameName, ";").EOL();
						//sb.____("int maskOffset = 0;").EOL();
						sb.____("var mask = end.mask;").EOL();

						sb.Append(sbInterpToFrame);
					}

					sb.___("}").EOL();

					/// Interpolate to Object/Struct
					if (isStruct)
						sb.___("public static void Interpolate(PackFrame start, PackFrame end, IntPtr trg, float time, ref FastBitMask128 readyMask, ref int maskOffset)").EOL();
					else
						sb.___("public static void Interpolate(PackFrame start, PackFrame end, System.Object trg, float time, ref FastBitMask128 readyMask, ref int maskOffset)").EOL();

					sb.___("{").EOL();

					if (sbInterpToObj.Length != 0)
					{
						sb.____("var s = start as ", packFrameName, ";").EOL();
						sb.____("var e = end as ", packFrameName, ";").EOL();
						if (!isStruct)
							sb.____("var t = trg as ", fullObjName, ";").EOL();
						sb.____("var mask = end.mask;").EOL();

						sb.Append(sbInterpToObj);
					}
					
					sb.___("}").EOL();

					/// SnapshotCallback
					{
						if (isStruct)
							sb.___("public static void SnapshotCallback(PackFrame snapframe, PackFrame targframe, IntPtr trg, ref FastBitMask128 readyMask, ref int maskOffset)").EOL();
						else
							sb.___("public static void SnapshotCallback(PackFrame snapframe, PackFrame targframe, System.Object trg, ref FastBitMask128 readyMask, ref int maskOffset)").EOL();

						sb.___("{").EOL();

						if (hasSnapCallbacks/* sbSnapshot.Length != 0*/)
						{
							sb.____("var snap = snapframe as ", packFrameName, ";").EOL();
							sb.____("var targ = targframe as ", packFrameName, ";").EOL();
							if (!isStruct)
								sb.____("var t = trg as ", fullObjName, ";").EOL();
							sb.____("var snapmask = snapframe.mask;").EOL();
							sb.____("var targmask = targframe.mask;").EOL();

							sb.Append(sbSnapshot);

							if (pObjAttr.postSnapCallback != null)
							{
								sb.____((isStruct ? "unsafe " : ""), "{ " ,derefObj, ".", pObjAttr.postSnapCallback, "(); }").EOL();
							}

						}

						sb.___("}").EOL();
					}

					/// Capture Object to Frame
					{
						if (isStruct)
							sb.___("public static void Capture(IntPtr src, PackFrame trg)").EOL();
						else
							sb.___("public static void Capture(System.Object src, PackFrame trg)").EOL();

						sb.___("{").EOL();

						if (sbCapture.Length != 0)
						{
							if (!isStruct)
								sb.____("var s = src as ", fullObjName, ";").EOL();

							sb.____("var t = trg as ", packFrameName, ";").EOL();
							sb.Append(sbCapture);
						}
						
						sb.___("}").EOL();
					}

					/// Apply Frame to Object
					{
						string postApplyCallback = pObjAttr.postApplyCallback;

						if (isStruct)
							sb.___("public static void Apply(PackFrame src, IntPtr trg, ref FastBitMask128 mask, ref int maskOffset)").EOL();
						else
							sb.___("public static void Apply(PackFrame src, System.Object trg, ref FastBitMask128 mask, ref int maskOffset)").EOL();

						sb.___("{").EOL();

						if (sbApplyToObj.Length != 0)
						{
							sb.____("var s = src as ", packFrameName, ";").EOL();

							if (!isStruct)
								sb.____("var t = trg as ", fullObjName, ";").EOL();

							if (postApplyCallback != null)
								sb.____("bool haschanged = false;").EOL();

							sb.Append(sbApplyToObj);

							if (postApplyCallback != null)
							{
								sb.____((isStruct ? "unsafe " : ""), "{ if (haschanged) ", derefObj, ".", postApplyCallback, "(); }").EOL();
							}
						}

						sb.___("}").EOL();
					}

					/// Copy Frame to Frame
					{
						sb.___("public static void Copy(PackFrame src, PackFrame trg)").EOL();

						sb.___("{").EOL();

						if (sbCopyFrameToFrame.Length != 0)
						{
							sb.____("var s = src as ", packFrameName, ";").EOL();
							sb.____("var t = trg as ", packFrameName, ";").EOL();

							sb.Append(sbCopyFrameToFrame);
						}
						
						sb.___("}").EOL();
					}

					sb.EOL();

					/// Factory
					sb.___("public static PackFrame Factory() { return new ", packFrameName, "(){ mask = new FastBitMask128(", PACK_PREFIX, objTypeName, ".TOTAL_FIELDS), ")
						._("isCompleteMask = new FastBitMask128(", PACK_PREFIX, objTypeName, ".TOTAL_FIELDS) }; }").EOL();

					if (isStruct)
						sb.___("public static PackFrame[] Factory(IntPtr trg, int count){");
					else
						sb.___("public static PackFrame[] Factory(System.Object trg, int count){ var t = trg as ", fullObjName, ";");

					sb._("var frames = new ", packFrameName, "[count]; ");

					sb._("for (int i = 0; i < count; ++i) { ")
						._("var frame = new ", packFrameName, "(){ mask = new FastBitMask128(", PACK_PREFIX, objTypeName, ".TOTAL_FIELDS), ")
						._("isCompleteMask = new FastBitMask128(", PACK_PREFIX, objTypeName, ".TOTAL_FIELDS) }; ");

					sb.Append(sbFactory);
					sb._(" frames[i] = frame; } ");

					sb._("return frames; }").EOL();



					/// End of class
					sb.__("}").EOL();
					sb.EOL();
				}

				/// Start Extension Class
				{
					sb.__("public static class ", PACK_PREFIX, objTypeName).EOL();
					sb.__("{").EOL();

					sb.___("public const int LOCAL_FIELDS = ")._(objTypeInfo.localFieldCount)._(";").EOL().EOL();
					sb.___("public const int TOTAL_FIELDS = ")._(objTypeInfo.totalFieldCount)._(";").EOL().EOL();
					sb.___("public static PackObjectDatabase.PackObjectInfo packObjInfo;").EOL();
					sb._(sbDelegates.ToString());

					sb.___("public static bool initialized;").EOL();
					sb.___("public static bool isInitializing;").EOL();
					sb.EOL();

					/// Initialize Method
					{
						sb.___("[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]").EOL();
						sb.___("public static void Initialize()").EOL();
						sb.___("{").EOL();
						sb.____("if (initialized) return;").EOL();
						sb.____("isInitializing = true;").EOL();
						sb.____("int maxBits = 0;").EOL();
						sb.____("var pObjAttr = (typeof(", ffulltype, ").GetCustomAttributes(typeof(PackObjectAttribute), false)[0] as PackObjectAttribute);").EOL();
						sb.____("var defaultKeyRate = pObjAttr.defaultKeyRate;").EOL();
						sb.____("FastBitMask128 defReadyMask = new FastBitMask128(TOTAL_FIELDS);").EOL();
						sb.____("int fieldindex = 0;").EOL();
						sb.EOL();
						sb._(sbInit.ToString());

						sb.____("packObjInfo = new PackObjectDatabase.PackObjectInfo(" +
							//", typeof(", packFrameName, ")" +
							"defReadyMask, Pack, Pack, Unpack" +
							", maxBits")
							._(", ", packFrameName, ".Factory")
							._(", ", packFrameName, ".Factory")
							._(", ", packFrameName, ".Apply")
							._(", ", packFrameName, ".Capture")
							._(", ", packFrameName, ".Copy")
							._(", ", packFrameName, ".SnapshotCallback")
							._(", ", packFrameName, ".Interpolate")
							._(", ", packFrameName, ".Interpolate")
							._(", TOTAL_FIELDS")
							._(");").EOL();

						sb.____("PackObjectDatabase.packObjInfoLookup.Add(typeof(", fullClass, "), packObjInfo);").EOL();

						sb.____("isInitializing = false;").EOL();
						sb.____("initialized = true;").EOL();

						sb.___("}").EOL();
					}

					/// Pack / Unpack
					{
						string stdArgsPack = "ref FastBitMask128 mask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags";
						string stdArgsUPck = "ref FastBitMask128 mask, ref FastBitMask128 isCompleteMask, ref int maskOffset, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags";
						string prevFrameCast = "var prev = " + PREV_FRM_NAME + " as " + PACKFRM_PREFIX + objTypeName + ";\n";

						if (isStruct)
						{
							/// Pack
							sb.___("public static SerializationFlags Pack(IntPtr obj, PackFrame prevFrame, " + stdArgsPack + ")").EOL();
							sb.___("{").EOL();
							sb.____(ffulltype, " packable; unsafe { packable = ", structCast, "obj; }").EOL();
							sb.____(prevFrameCast);
							sb.____(serializationFlagsLine1);
							sb._(sbPack.ToString());
							sb.____("return flags;").EOL();
							sb.___("}").EOL();

							///// Unpack
							//sb.___("public static SerializationFlags Unpack(IntPtr obj, PackFrame prevFrame, " + standardArgs + ")").EOL();
							//sb.___("{").EOL();
							//sb.___("Debug.LogError(\"What is using this??\"); return 0;").EOL();

							////sb.____(ffulltype, " packable; unsafe { packable = ", structCast, "obj; }").EOL();
							////sb.____(prevFrameCast);
							////sb.____(serializationFlagsLine1);
							////sb._(sbUnpack.ToString());
							////sb.____("return flags;").EOL();
							//sb.___("}").EOL();
						}
						else
						{
							/// Pack
							sb.___("public static SerializationFlags Pack(ref System.Object obj, PackFrame prevFrame, " + stdArgsPack + ")").EOL();
							sb.___("{").EOL();
							sb.____("var packable = obj as ", fullClass, ";").EOL();
							sb.____(prevFrameCast);
							sb.____(serializationFlagsLine1);
							sb._(sbPack.ToString());
							sb.____("return flags;").EOL();
							sb.___("}").EOL();

							///// Unpack
							//sb.___("public static SerializationFlags Unpack(ref System.Object obj, PackFrame prevFrame, " + standardArgs + ")").EOL();
							//sb.___("{").EOL();
							//sb.___("Debug.LogError(\"What is using this??\"); return 0;").EOL();

							////sb.____("var packable = obj as ", fullClass, ";").EOL();
							////sb.____(prevFrameCast);
							////sb.____(serializationFlagsLine1);
							////sb._(sbUnpack.ToString());
							////sb.____("return flags;").EOL();
							//sb.___("}").EOL();
						}

						/// Pack Ext
						sb.___("public static SerializationFlags Pack(ref ", fullClass, " packable, PackFrame prevFrame, " + stdArgsPack + ")").EOL();
						sb.___("{").EOL();
						sb.____(prevFrameCast);
						sb.____(serializationFlagsLine1);
						sb._(sbPack.ToString());
						sb.____("return flags;").EOL();
						sb.___("}").EOL();

						///// Unpack Ext
						//sb.___("public static SerializationFlags Unpack(ref ", fullClass, " packable, PackFrame prevFrame, " + standardArgs + ")").EOL();
						//sb.___("{").EOL();
						//sb.___("Debug.LogError(\"What is using this??\"); return 0;").EOL();

						////sb.____(prevFrameCast);
						////sb.____(serializationFlagsLine1);
						////sb._(sbUnpack.ToString());
						////sb.____("return flags;").EOL();
						//sb.___("}").EOL();

						/// Pack Generic Frame To Buffer
						sb.___("public static SerializationFlags Pack(PackFrame obj, PackFrame prevFrame, " + stdArgsPack + ")").EOL();
						sb.___("{").EOL();
						sb.____("var packable = obj as ", packFrameName, ";").EOL();
						sb.____(prevFrameCast);
						sb.____(serializationFlagsLine1);
						sb._(sbPack.ToString());
						sb.____("return flags;").EOL();
						sb.___("}").EOL();
						/// Unpack Ext Pack Frame
						sb.___("public static SerializationFlags Unpack(PackFrame obj, " + stdArgsUPck + ")").EOL();
						sb.___("{").EOL();
						sb.____("var packable = obj as ", packFrameName, ";").EOL();
						//sb.____(prevFrameCast);
						sb.____(serializationFlagsLine1);
						sb._(sbUnpack.ToString());
						sb.____("return flags;").EOL();
						sb.___("}").EOL();

						/// Pack Master Frame To Buffer
						sb.___("public static SerializationFlags Pack(ref ", packFrameName, " packable, PackFrame prevFrame, " + stdArgsPack + ")").EOL();
						sb.___("{").EOL();
						sb.____(prevFrameCast);
						sb.____(serializationFlagsLine1);
						sb._(sbPack.ToString());
						sb.____("return flags;").EOL();
						sb.___("}").EOL();
						/// Unpack Ext Pack Frame
						sb.___("public static SerializationFlags Unpack(ref ", packFrameName, " packable, " + stdArgsUPck + ")").EOL();
						sb.___("{").EOL();
						//sb.____(prevFrameCast);
						sb.____(serializationFlagsLine1);
						sb._(sbUnpack.ToString());
						sb.____("return flags;").EOL();
						sb.___("}").EOL();
					}

					sb.__("}").EOL();
				}
				sb._("}");

				sb.EOL();
				sb._(RESTORE_UNUSED2).EOL();
				sb._(RESTORE_UNUSED).EOL();

			}
			return sb;
		}

		private static void GenerateFields(Type pObjType)
		{
			sbFrame.Length = 0;
			sbFactory.Length = 0;
			sbSnapshot.Length = 0;
			sbCopyToFrame.Length = 0;
			sbCapture.Length = 0;
			sbApplyToObj.Length = 0;
			sbCopyFrameToFrame.Length = 0;
			sbInterpToObj.Length = 0;
			sbInterpToFrame.Length = 0;
			sbDelegates.Length = 0;
			sbInit.Length = 0;
			sbPack.Length = 0;
			sbUnpack.Length = 0;

			hasSnapCallbacks = false;

			var pObjAttr = (pObjType.GetCustomAttributes(typeof(PackObjectAttribute), false)[0] as PackObjectAttribute);

			var defaultInclusion = pObjAttr.defaultInclusion;

			var fields = pObjType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

			for (int i = 0, cnt = fields.Length; i < cnt; ++i)
			{
				var fieldinfo = fields[i];

				string fname = fieldinfo.Name;
				string objNamespace = pObjType.Namespace;
				string objTypeName = (objNamespace != null && objNamespace != "") ? (pObjType.Namespace + "." + pObjType.Name) : pObjType.Name;

				Type ftype = fieldinfo.FieldType;
				string ftypename = fieldinfo.FieldType.Name;

				string ffulltype;
				string genericarg;
				string primitive;

				if (ftype.IsGenericType)
				{
					primitive = ftype.GetGenericArguments()[0].ToString();
					ffulltype = ftype.ToString().Replace("`1[", "<").Replace("]", ">");
					genericarg = "<" + primitive + ">";
					
					//ftypename = fieldinfo.FieldType.Name.Replace("`1", "<") + genericarg.Namespace +"." + genericarg.Name + ">";
					ftypename = fieldinfo.FieldType.Name.Replace("`1", "");
				}
				else
				{
					ffulltype = (ftype.Namespace == null || ftype.Namespace == "") ? ftype.Name : (ftype.Namespace + "." + ftype.Name);
					ftypename = fieldinfo.FieldType.Name;
					primitive = ffulltype;
					genericarg = "";
				}


				/// We only want to pack fields that have a packing attribute
				var packAttrs = fieldinfo.GetCustomAttributes(typeof(SyncVarBaseAttribute), false);

                bool isExplicitInclusion = defaultInclusion == DefaultPackInclusion.Explicit;

                if (isExplicitInclusion && packAttrs.Length == 0)
					continue;

				SyncVarBaseAttribute packAttr; // = (objType.GetCustomAttributes(typeof(SyncVarBaseAttribute), false)[0] as SyncVarBaseAttribute);

				if (ftype.IsValueType && !ftype.IsUnManaged())
				{
					Debug.LogWarning(ftypename + " is a PackObject, but cannot be packed because it is a managed type. Structs cannot contain references or any managed types to be packable.");
					continue;
				}

                bool isPublic = fieldinfo.IsPublic;

                bool generatePackAttribute;
				/// If we are allowing unattributed fields to be serialized, we need to create an attribute for them.
				if (packAttrs.Length == 0)
				{
                    // Only add attributes automatically if field is public. Otherwise including all public fields
                    // Will find all kinds of MonoB internals.
                    if (!isPublic)
                        continue;

					packAttr = new SyncVarAttribute();
					generatePackAttribute = true;
				}
				else
				{
                    // TODO: make this a loop in case other attributes are also on this field.
					packAttr = packAttrs[0] as SyncVarBaseAttribute;
					generatePackAttribute = false;
				}

                bool typeIsSupported = TypeIsHandled(ftype, packAttr);

                string attrname = packAttr.GetType().Name;

                if (typeIsSupported && !isPublic)
				{
					Debug.LogWarning("<b>[" + attrname.Replace("Attribute", "") + "]</b> on field <b>" + ftypename + " " + fname + "</b> of <b>" + objTypeName + "</b> supports type, but field is not public. Change field to public to include in packing.");
					continue;
				}

				var nestedPackObjAttrs = ftype.GetCustomAttributes(typeof(PackObjectAttribute), false);
				PackObjectAttribute nestedPackObjAttr = nestedPackObjAttrs.Length == 0 ? null : (nestedPackObjAttrs[0] as PackObjectAttribute);
				bool isNestedPackObj = nestedPackObjAttr != null;

#if UNITY_2018_1_OR_NEWER

				if (isNestedPackObj && !UnityEditor.PlayerSettings.allowUnsafeCode && ftype.IsValueType)
				{
					Debug.LogWarning("Cannot use " + typeof(PackObjectAttribute).Name + " on struct/value types unless PlayerSettings.allowUnsafeCode == true.");
					continue;

				}
#endif

				/// Pack Attribute doesn't support this type
				if (!typeIsSupported)
				{

					/// See if maybe we are trying to pack a nested PackObj
					if (!isNestedPackObj)
					{
                        /// If this field doesn't have an PackAttribute, and is only being processed because we are not using DefaultPackInclusion.Explicit
                        if (!generatePackAttribute)
                        {
                            Debug.LogWarning("<b>[" + attrname.Replace("Attribute", "") + "]</b> on field <b>" + ftypename + " " + fname + "</b> of <b>" + objTypeName + fname + "</b> does not support type <b>"
                                + ftypename + "</b>. Will be ignored.");
                        }

						continue;
					}


					/// Check if this nested PackObj creates a nested loop - will break terribly if so.
					if (CheckForNestedLoop(pObjType, ftype))
					{
						Debug.LogWarning("Field <b>" + ftypename + " " + fname + "</b> of <b>" + objTypeName + "</b> contains an infinite nested loop with field <b>" +  "</b>. Will not be included in serialization.");
						continue;
					}

					isNestedPackObj = true;
				}

				bool isEnum = fieldinfo.FieldType.IsEnum;
				string enumUnderType = null;
				string enumType = null; ;
				if (isEnum)
				{
					enumUnderType = "(" + System.Enum.GetUnderlyingType(fieldinfo.FieldType).Name + ")";
					enumType = "(" + ffulltype + ")";
				}

				Generate_FieldFrame(fieldinfo, sbFrame, fname, ftypename, ffulltype, isNestedPackObj, packAttr);
				Generate_Factory(fieldinfo, sbFactory, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_Snapshot(fieldinfo, sbSnapshot, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_Capture(fieldinfo, sbCapture, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, pObjAttr, packAttr);
				Generate_CopyTo(fieldinfo, sbCopyToFrame, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_ApplyToObj(fieldinfo, sbApplyToObj, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, pObjAttr, packAttr);
				Generate_CopyFrameToFrame(fieldinfo, sbCopyFrameToFrame, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_InterpToObj(fieldinfo, sbInterpToObj, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_InterpToFrame(fieldinfo, sbInterpToFrame, objTypeName, fname, ftypename, ffulltype, isNestedPackObj, pObjType, packAttr);
				Generate_FieldDelegates(fieldinfo, sbDelegates, fname, ftypename, genericarg, isNestedPackObj);
				Generate_FieldInit(fieldinfo, sbInit, fname, ftypename, ffulltype, genericarg, primitive, isNestedPackObj, generatePackAttribute, packAttr);
				Generate_FieldPack(fieldinfo, sbPack, fname, ftypename, ffulltype, isNestedPackObj, enumUnderType, enumType);
				Generate_FieldUnpack(fieldinfo, sbUnpack, fname, ftypename, ffulltype, isNestedPackObj, enumUnderType, enumType, packAttr);
			}

			return;
		}

		private static void Generate_Factory(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool isNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			string t = pObjType.IsValueType ? "(*(" + objName + "*)trg)" : "t";

			/// List Factory
			if (fInfo.FieldType.IsGenericType && fInfo.FieldType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
				sb._("frame.", fname, ".AddRange(", t,".", fname, "); ")._("frame.", fname, "_mask = new BitArray(frame.", fname, ".Count); ");
		}

		private static void Generate_FieldFrame(FieldInfo fInfo, StringBuilder sb, string fname, string ftypename, string ffulltype, bool isNest, SyncVarBaseAttribute fAttr)
		{
			if (isNest)
				sb.___("public PackFrame_", ftypename, " ", fname, " = new PackFrame_", ftypename, "();").EOL();
			else
			{
				var fielddeclare = fAttr.GetFieldDeclareCodeGen(fInfo.FieldType, ffulltype, fname);

				sb.___(fielddeclare).EOL();
				//sb.___("public ", ffulltype, " ", fname, ";").EOL();

			}
		}

		private static bool IsSyncAsTrigger(PackObjectAttribute pObjAttr, SyncVarBaseAttribute fAttr)
		{
			var syncAs = fAttr.syncAs;
			return
				syncAs == SyncAs.Auto ? (pObjAttr.syncAs == SyncAs.Auto ? false : pObjAttr.syncAs == SyncAs.Trigger) :
				syncAs == SyncAs.Trigger ? true : false;
		}
		private static void Generate_Capture(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool isNest, Type pObjType, PackObjectAttribute pObjAttr, SyncVarBaseAttribute fAttr)
		{
			string s = pObjType.IsValueType ? "(*(" + objName + "*)src)" : "s";

			bool isTrigger = IsSyncAsTrigger(pObjAttr, fAttr);

			string triggerReset = (isTrigger) ? s + "." + fname + " = new " + ffulltype + "(); " : "" ;


			if (isNest)
			{
				if (fInfo.FieldType.IsValueType)
					sb.____("unsafe { fixed (", ffulltype, "* p = &s.", fname, ") PackFrame_", ftypename, ".Capture((IntPtr)p, t.", fname, "); }").EOL();
				else
					sb.____("PackFrame_", ftypename, ".Capture(", s, ".", fname, ", t.", fname, ");").EOL();
			}
			else
			{
				var captureField = fAttr.GetCaptureCodeGen(fInfo.FieldType, fname, s, "t");
				if (pObjType.IsValueType)
					sb.____("unsafe { ", captureField, triggerReset," } ").EOL();
				else
					sb.____(captureField, " ", triggerReset).EOL();
				//sb.____("t.", fname, " = ", s, ".", fname, ";").EOL();
			}
		}

		private static void Generate_CopyTo(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool isNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			string s = pObjType.IsValueType ? "(*(" + objName + "*)src)" : "s";

			if (isNest)
			{
				if (fInfo.FieldType.IsValueType)
					sb.____("unsafe { fixed (", ffulltype, "* p = &s.", fname, ") PackFrame_", ftypename, ".Copy((IntPtr)p, t.", fname, "); }").EOL();
				else
					sb.____("PackFrame_", ftypename, ".Copy(", s, ".", fname, ", t.", fname, ");").EOL();
			}
			else
			{
				var copyfield = fAttr.GetCopyCodeGen(fInfo.FieldType, fname, s, "t");
				if (pObjType.IsValueType)
					sb.____("unsafe { ",copyfield, " } ").EOL();
				else
					sb.____(copyfield).EOL();
				//sb.____("t.", fname, " = ", s, ".", fname, ";").EOL();
			}

		}

		/// Snapshot
		private static void Generate_Snapshot(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool inNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			string t = pObjType.IsValueType ? "(*(" + objName + "*)trg)" : "t";

			if (inNest)
			{
				if (fInfo.FieldType.IsValueType)
					sb.____("unsafe { fixed (", ffulltype, "* p = &t.", fname, ") PackFrame_", ftypename)._(".SnapshotCallback(snap.", fname, ", targ.", fname, ", (IntPtr)p, ref readyMask, ref maskOffset); }").EOL();
				else
					sb.____("PackFrame_", ftypename)._(".SnapshotCallback(snap.", fname, ", targ.", fname)._(", ", t, ".", fname, ", ref readyMask, ref maskOffset);").EOL();

				hasSnapCallbacks = true;
			}
			else
			{
				string callback = fAttr.snapshotCallback;

				/// Add the hook/callback if one exists
				MethodInfo callbackInfo = fInfo.HashCallback(fAttr.snapshotCallback, null);
				if (callbackInfo != null)  // TryGetCallbackInfo(fInfo, callback, objType, out callbackInfo))
				{
					hasSnapCallbacks = true;

					if (pObjType.IsValueType)
						sb.____("if(readyMask[maskOffset]) unsafe { ");
					else
						sb.____("if(readyMask[maskOffset]) { ");

					var gens = callbackInfo.ContainsGenericParameters;
					sb._("var snapval = snapmask[maskOffset] ? snap.", fname, " : ", t, ".", fname, "; ");
					sb._("var targval = targmask[maskOffset] ? targ.", fname, " : snapval; ");
					sb._(t, ".", callback, "(snapval,  targval); ");
					sb._("} maskOffset++;").EOL();
				}
				else
					sb.____("maskOffset++;").EOL();

			}
		}
		/// Apply Frame to Object/Struct elements
		private static void Generate_ApplyToObj(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool inNest, Type pObjType, PackObjectAttribute pObjAttr, SyncVarBaseAttribute fAttr)
		{
			string t = pObjType.IsValueType ? "(*(" + objName + "*)trg)" : "t";

			if (inNest)
			{
				if (fInfo.FieldType.IsValueType)
					sb.____("unsafe { fixed (", ffulltype, "* p = &t.", fname, ") PackFrame_", ftypename, ".Apply(s.", fname, ", (IntPtr)p, ref mask, ref maskOffset); }").EOL();
				else
					sb.____("PackFrame_", ftypename, ".Apply(s.", fname, ", ", t, ".", fname, ", ref mask, ref maskOffset);").EOL();
			}
			else
			{
				string copyfield = fAttr.GetCopyCodeGen(fInfo.FieldType, fname, "s", t);
				string callback = fAttr.applyCallback;
				SetValueTiming callbackSetValue = fAttr.setValueTiming;

				string hasChangedSegment = pObjAttr.postApplyCallback == null ? "" : " haschanged = true; ";

				if (pObjType.IsValueType)
					sb.____("unsafe { if (mask[maskOffset]) { ", hasChangedSegment);
				else
					sb.____("{ if (mask[maskOffset]){ ", hasChangedSegment);

				/// Add the hook/callback if one exists
				MethodInfo callbackInfo = fInfo.HashCallback(fAttr.applyCallback, null);
				if (callbackInfo != null)  // TryGetCallbackInfo(fInfo, callback, objType, out callbackInfo))
				{
					if (callbackSetValue == SetValueTiming.AfterCallback)
					{
						sb._(t, ".", callback, "(s.", fname, ", ")._(t, ".", fname, "); ");
						sb._(copyfield, " } ");
					}
					else if (callbackSetValue == SetValueTiming.BeforeCallback)
					{
						sb._("var hold = ", t, ".", fname, "; ");
						sb._(copyfield);
						sb._(t, ".", callback, "(s.", fname, ", hold); } ");
					}
					else
					{
						sb._(t, ".", callback, "(s.", fname, ", ")._(t, ".", fname, "); } ");
					}

				}
				/// No callback code needed
				else
					sb._(copyfield, " } ");

				bool isTrigger = IsSyncAsTrigger(pObjAttr, fAttr);

				if (isTrigger)
					sb._("else { ", t, ".", fname, " = new ", ffulltype, "(); } ");

				sb._("} maskOffset++;").EOL();

			}
		}

		/// Lerp Frames to Obj
		private static void Generate_InterpToObj(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool inNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			//if (!fAttr.interpolate)
			//	return;

			string t = pObjType.IsValueType ? "(*(" + objName + "*)trg)" : "t";

			if (inNest)
			{
				if (fInfo.FieldType.IsValueType)
					sb.____("unsafe { fixed (", ffulltype, "* p = &t.", fname, ") PackFrame_", ftypename)._(".Interpolate(s.", fname, ", e.", fname, ", (IntPtr)p, time, ref mask, ref maskOffset); }").EOL();
				else
					sb.____("PackFrame_", ftypename, ".Interpolate(s.", fname, ", e.", fname, ", ")._(t, ".", fname, ", time, ref mask, ref maskOffset);").EOL();
			}
			else
			{
				if (fAttr.interpolate)
				{
					string interpfield = fAttr.GetInterpolateCode(fInfo.FieldType, fname, "s", "e", t);
					if (interpfield != null)
					{
						if (pObjType.IsValueType)
							sb.____("if (mask[maskOffset]) unsafe { ");
						else
							sb.____("if (readyMask[maskOffset] && mask[maskOffset]){ ");


						sb._(interpfield, " } maskOffset++;").EOL();
					}
				}
				else
					sb.____("maskOffset++;").EOL();
			}
		}


		/// Lerp Frames to Frame
		private static void Generate_InterpToFrame(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool inNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			//if (!fAttr.interpolate)
			//	return;

			string t = "t";

			if (inNest)
			{
				//if (fInfo.FieldType.IsValueType)
				//	sb.____("unsafe { fixed (", ffulltype, "* p = &t.", fname, ") PackFrame_", ftypename)._(".Interpolate(s.", fname, ", e.", fname, ", t.", fname, ", time, ref mask, ref maskOffset); }").EOL();
				//else
					sb.____("PackFrame_", ftypename, ".Interpolate(s.", fname, ", e.", fname, ", ")._(t, ".", fname, ", time, ref mask, ref maskOffset);").EOL();
			}
			else
			{
				string interpfield = fAttr.GetInterpolateCode(fInfo.FieldType, fname, "s", "e", t);
				if (interpfield == null)
					return;

				if (pObjType.IsValueType)
					sb.____("if (mask[maskOffset]) unsafe { ");
				else
					sb.____("if (mask[maskOffset]){ ");

				sb._(interpfield);

				sb._("} maskOffset++;").EOL();

			}
		}
		//private static bool TryGetCallbackInfo(FieldInfo finfo, string callback, Type objType, out MethodInfo callbackInfo)
		//{
		//	/// No callback declared. Normal case.
		//	if (callback == null)
		//	{
		//		callbackInfo = null;
		//		return false;
		//	}

		//	Type ftype = finfo.FieldType;

		//	callbackInfo = objType.GetMethod(callback, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

		//	/// Callback is invalid
		//	if (callbackInfo == null)
		//	{
		//		//Debug.LogWarning("Method named <b>'" + callback  +"'</b> referenced by PackObj <b>'" + (objType.IsValueType ? "struct " : "class ") + objType.Name +" ."+ finfo.Name + "'</b> does not exist. Check spelling and use nameof() rather than a string.");
		//		return false;
		//	}

		//	var parms = callbackInfo.GetParameters();
		//	bool isStatic = callbackInfo.IsStatic;
		//	bool isPublic = callbackInfo.IsPublic;

		//	if (callbackInfo == null)
		//		goto BadCallback;

		//	if (!callbackInfo.IsPublic)
		//		goto BadCallback;

		//	if (parms.Length != 2)
		//		goto BadCallback;

		//	if (parms[0].ParameterType == ftype && parms[1].ParameterType == ftype)
		//		return true;

		//	BadCallback:

		//	//string staticString = isStatic ? "static " : "";
		//	//string accessString = isPublic ? "public " : "private ";
		//	//string typename = ftype.Name;
		//	//string parmstring = "";
		//	//for (int i = 0; i < parms.Length; ++i)
		//	//	parmstring += (i == 0 ? "" : ", ") + parms[i].ParameterType.Name;

		//	//Debug.LogWarning("<b>Invalid Callback</b> <i>'" + accessString + staticString + callback + "(" + parmstring + ")'</i> on <b>" + objType.Name + "</b>" +
		//	//	". Needs to have format: <i>'public " + callback + "(" + typename + ", " + typename + ")'</i>");
		//	return false;
		//}

		private static void Generate_CopyFrameToFrame(FieldInfo fInfo, StringBuilder sb, string objName, string fname, string ftypename, string ffulltype, bool isNest, Type pObjType, SyncVarBaseAttribute fAttr)
		{
			string copyfield = fAttr.GetCopyCodeGen(fInfo.FieldType, fname, "s", "t");
			
			sb.____(copyfield).EOL();

			//sb.____("t.", fname, " = s.", fname, ";").EOL();
		}

		private static void Generate_FieldDelegates(FieldInfo fInfo, StringBuilder sb, string fname, string ftype, string genericarg, bool isNest)
		{
			if (isNest)
				return;

			if (fInfo.FieldType.IsEnum)
				ftype = System.Enum.GetUnderlyingType(fInfo.FieldType).Name;

			if (fInfo.FieldType.IsGenericType)
			{
				sb.___("static Pack", ftype, "Delegate", genericarg, " ", fname, "Packer;").EOL();
				sb.___("static Unpack", ftype, "Delegate", genericarg, " ", fname, "Unpacker;").EOL();
			}
			else
			{
				sb.___("static PackDelegate<", ftype, ">", genericarg, " ", fname, "Packer;").EOL();
				sb.___("static UnpackDelegate<", ftype, ">", genericarg, " ", fname, "Unpacker;").EOL();
			}
			
			sb.EOL();
		}


		private static void Generate_FieldInit(FieldInfo fInfo, StringBuilder sb, string fname, string ftypename, string ffulltype, string genericarg, string primitive, bool isNest, bool generateAttr, SyncVarBaseAttribute fAttr)
		{
			Type ftype = fInfo.FieldType;
			Type declrType = fInfo.DeclaringType;

			bool isEnum = fInfo.FieldType.IsEnum;
			if (isEnum)
				ftypename = System.Enum.GetUnderlyingType(fInfo.FieldType).Name;

			string fullDeclaring = (declrType.Namespace == null || declrType.Namespace == "") ? declrType.Name : (declrType.Namespace + "." + declrType.Name);

			/// Initialization Code for fields that are nested PackObjects
			if (isNest)
			{
				sb.____("if (!", PACK_PREFIX, ftypename, ".isInitializing)").EOL();
				sb.____("{").EOL();

				sb._____(PACK_PREFIX, ftypename, ".Initialize();").EOL();
				sb._____("var packObjInfo = ", typeof(PackObjectDatabase).Name, ".GetPackObjectInfo(typeof(", ffulltype, "));").EOL();
				sb._____("defReadyMask.OR(packObjInfo.defaultReadyMask, fieldindex);").EOL();
				sb._____("maxBits += packObjInfo.maxBits; fieldindex++;").EOL();

				sb.____("}").EOL();
			}

			/// Initialization Code for standard PackAttr fields
			else
			{
				if (generateAttr)
					sb.____("PackAttribute ", fname, "PackAttr = new PackAttribute();").EOL();
				else
					sb.____(fAttr.GetType().Name, " ", fname, "PackAttr = (", fAttr.GetType().Name, ")(typeof(", fullDeclaring)._(").GetField(\"", fname, "\").GetCustomAttributes(typeof(SyncVarBaseAttribute), false)[0] as ", fAttr.GetType().Name, ");").EOL();

				sb.____(fname, "Packer = (", fname, "PackAttr as IPack", ftypename, genericarg, ").Pack;").EOL();
				sb.____(fname, "Unpacker = (", fname, "PackAttr as IPack", ftypename, genericarg, ").Unpack;").EOL();
				sb.____(fname, "PackAttr.Initialize(typeof(", primitive, "));").EOL();
				sb.____("if (", fname, "PackAttr.keyRate == KeyRate.UseDefault) ", fname, "PackAttr.keyRate = (KeyRate)defaultKeyRate;").EOL();
				sb.____("if (", fname, "PackAttr.syncAs == SyncAs.Auto) ", fname, "PackAttr.syncAs = pObjAttr.syncAs;").EOL();
				sb.____("if (", fname, "PackAttr.syncAs == SyncAs.Auto) ", fname, "PackAttr.syncAs = SyncAs.State;").EOL();
				sb.____("if (", fname, "PackAttr.syncAs == SyncAs.Trigger) ", "defReadyMask[fieldindex] = true;").EOL();
				sb.____("maxBits += ", fAttr.GetMaxBits(fInfo.FieldType).ToString(), "; fieldindex++;").EOL();
				//sb.____("maxBits += ", fname, "PackAttr.GetMaxBits(typeof(", ffulltype, "));").EOL();
			}
			sb.EOL();
		}

		private static void Generate_FieldPack(FieldInfo fInfo, StringBuilder sb, string fname, string ftypename, string ffulltype, bool isNest, string enumUnderCast, string enumTypeCast)
		{

			if (isNest)
				sb.____("flags |= ")
					._(PACK_PREFIX, ftypename, ".Pack(ref packable.", fname, ", " + PREV_NAME + ".", fname, ", ref mask, ref maskOffset, buffer, ref bitposition, frameId, writeFlags);").EOL();
			else
			{
				sb.____("{").EOL();
				/// Enum handling
				if (enumTypeCast != null)
				{
					sb._____("var temp = ", enumUnderCast, "packable.myTestEnum;").EOL();
					sb._____("var flag = ", fname, "Packer(ref temp, ")._(enumUnderCast, PREV_NAME, ".", fname, ", buffer, ref bitposition, frameId, writeFlags);").EOL();
				}
				/// Standard Handling
				else
					sb._____("var flag = ", fname, "Packer(ref packable.", fname, ", ")._(PREV_NAME, ".", fname, ", buffer, ref bitposition, frameId, writeFlags);").EOL();

				sb._____("mask[maskOffset] = flag != ", typeof(SerializationFlags).Name,".", Enum.GetName(typeof(SerializationFlags), SerializationFlags.None), ";	flags |= flag; maskOffset++;").EOL();
				sb.____("}").EOL();
			}
		}

		private static void Generate_FieldUnpack(FieldInfo fInfo, StringBuilder sb, string fname, string ftypename, string ffulltype, bool isNestedPackObj, string enumUnderCast, string enumTypeCast, SyncVarBaseAttribute fAttr)
		{
			if (isNestedPackObj)
				sb.____("flags |= ")
					._(PACK_PREFIX, ftypename, ".Unpack(ref packable.", fname, ", ref mask, ref isCompleteMask, ref maskOffset, buffer, ref bitposition, frameId, writeFlags);").EOL();
			else
			{
				sb.____("{").EOL();
				sb._____("if (mask[maskOffset]) {").EOL();
				if (enumTypeCast != null)
				{
					sb.______("var temp = ", enumUnderCast, "packable.myTestEnum;").EOL();
					sb.______("var flag = ", fname, "Unpacker(ref temp, buffer, ref bitposition, frameId, writeFlags);").EOL();
					sb.______("packable.", fname, " = ", enumTypeCast, "temp;").EOL();
                }
				else if (fInfo.FieldType.IsGenericType)
					sb.______("var flag = ", fname, "Unpacker(ref packable.", fname, ", packable.",fname,"_mask, buffer, ref bitposition, frameId, writeFlags);").EOL();
				else
					sb.______("var flag = ", fname, "Unpacker(ref packable.", fname, ", buffer, ref bitposition, frameId, writeFlags);").EOL();

				if (fAttr.syncAs == SyncAs.Trigger)
					sb.______("mask[maskOffset] = flag != 0;").EOL();
				else
					sb.______("isCompleteMask[maskOffset] = (flag & SerializationFlags.IsComplete) != 0; mask[maskOffset] = flag != 0; flags |= flag; ").EOL();

				/// Triggers will always set the complete mask as true
				if (fAttr.syncAs == SyncAs.Trigger)
					sb._____(" } isCompleteMask[maskOffset] = true; maskOffset++;").EOL();
				else
					sb._____(" } maskOffset++;").EOL();
				sb.____("}").EOL();
			}

		}

		private static bool TypeIsHandled(Type fieldType, SyncVarBaseAttribute packAttr)
		{
			if (fieldType.IsEnum)
				fieldType = Enum.GetUnderlyingType(fieldType);

			var interfaces = packAttr.GetType().GetInterfaces();
			foreach (var i in interfaces)
			{
				var isupporteds = i.GetCustomAttributes(typeof(PackSupportedTypesAttribute), false);
				
				if (isupporteds.Length == 0)
					continue;

				var supportedTypeAttr = isupporteds[0] as PackSupportedTypesAttribute;

				var supportedType = supportedTypeAttr.supportedType;

				if (supportedType.IsGenericType && supportedType.IsGenericTypeDefinition)
				{
					
					supportedType = supportedType.GetGenericArguments()[0];
					Debug.Log("Found Generic type using " + supportedType.Name + " allowing it until I revisit this code and make it more restrictive.");
					return true;
				}

				//Debug.Log(supportedType.IsGenericParameter + " " + supportedType.IsGenericType + " " + supportedType.IsGenericTypeDefinition + " "+ supportedType.type);

				if (supportedType == fieldType)
					return true;
			}
			return false;
		}

		private static string ReplacePlusWithPeriod(string str)
		{
			return str.Replace("+", ".");
		}

		private static int safetycounter;
		private static List<Type> nestCheck = new List<Type>();

		public static bool CheckForNestedLoop(Type type, Type checkIn)
		{
			nestCheck.Clear();

			/// Only check if this is a PackObj, since we are looking for nested pack objects.
			var attrs = checkIn.GetCustomAttributes(typeof(PackObjectAttribute), false);
			if (attrs.Length == 0)
				return false;

			safetycounter++;

			if (safetycounter > 200)
			{
				Debug.Log("Ouch");
				return true;
			}

			var fields = checkIn.GetFields();

			foreach (var field in fields)
			{
				if (field.FieldType == type)
					return true;

				/// Recurse into nested PackObjects to check for recursion
				if (field.FieldType.GetCustomAttributes(typeof(SyncVarBaseAttribute), false).Length != 0)
					if (CheckForNestedLoop(type, field.FieldType))
						return true;
			}
			return false;
		}

		#region SB extensions

		#region No Indent

		private static StringBuilder _(this StringBuilder sb, string text)
		{
			return sb.Append(text);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2)
		{
			return sb.Append(text).Append(text2);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder _(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}


		#endregion

		#region Indent 1

		private static StringBuilder __(this StringBuilder sb, string text)
		{
			return sb.Append("\t").Append(text);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2)
		{
			return sb.Append("\t").Append(text).Append(text2);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append("\t").Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append("\t").Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append("\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append("\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder __(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append("\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}

		#endregion

		#region Indent 2

		private static StringBuilder ___(this StringBuilder sb, string text)
		{
			return sb.Append("\t\t").Append(text);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2)
		{
			return sb.Append("\t\t").Append(text).Append(text2);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}

		private static StringBuilder ___(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8)
		{
			return sb.Append("\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7).Append(text8);
		}

		#endregion

		#region Indent 3

		private static StringBuilder ____(this StringBuilder sb)
		{
			return sb.Append("\t\t\t");
		}

		private static StringBuilder ____(this StringBuilder sb, string text)
		{
			return sb.Append("\t\t\t").Append(text);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7).Append(text8);
		}

		private static StringBuilder ____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8, string text9)
		{
			return sb.Append("\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7).Append(text8).Append(text9);
		}

		#endregion

		#region Indent 4

		private static StringBuilder _____(this StringBuilder sb, string text)
		{
			return sb.Append("\t\t\t\t").Append(text);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder _____(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}

		#endregion

		#region Indent 5

		private static StringBuilder ______(this StringBuilder sb, string text)
		{
			return sb.Append("\t\t\t\t\t").Append(text);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2)
		{
			return sb.Append("\t\t\t\t").Append(text).Append(text2);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2, string text3)
		{
			return sb.Append("\t\t\t\t\t").Append(text).Append(text2).Append(text3);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2, string text3, string text4)
		{
			return sb.Append("\t\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2, string text3, string text4, string text5)
		{
			return sb.Append("\t\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6)
		{
			return sb.Append("\t\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6);
		}

		private static StringBuilder ______(this StringBuilder sb, string text, string text2, string text3, string text4, string text5, string text6, string text7)
		{
			return sb.Append("\t\t\t\t\t").Append(text).Append(text2).Append(text3).Append(text4).Append(text5).Append(text6).Append(text7);
		}

		#endregion

		private static StringBuilder _(this StringBuilder sb, int text)
		{
			return sb.Append(text.ToString());
		}

		private static StringBuilder EOL(this StringBuilder sb)
		{
			return sb.Append("\n");
		}

		#endregion
	}
}

#endif