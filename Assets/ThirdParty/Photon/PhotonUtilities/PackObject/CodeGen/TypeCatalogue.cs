// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Photon.Compression.Internal
{
    [CreateAssetMenu()]
    [System.Serializable]
    public class TypeCatalogue : ScriptableObject
    {

        public static char _ = Path.DirectorySeparatorChar;

        public const string MENU_PATH = "Window/Photon Unity Networking/";

        public const string PHOTON_UTILITIES_FOLDER_GUID = "1e336284e5d53884a957a795a62474a4";
        public const string GENERATED_SUBFOLDER = "_GeneratedPackExtensions";

        public static string photonUtilitiesFolderPath;
        public static string packObjectFolderPath; // = "Assets" + _ + "Photon" + _ + "PhotonUtilities" + _ + "PackObject" + _;
        public static string codeGenFolderPath; // = packObjectFolderPath + GENERATED_SUBFOLDER + _;
        public static string codegenEditorResourcePath; // = packObjectFolderPath + "CodeGen" + _ + "Editor" + _ + "Resources" + _;

        private static void FindPaths() {

            photonUtilitiesFolderPath = AssetDatabase.GUIDToAssetPath(PHOTON_UTILITIES_FOLDER_GUID);

            if (photonUtilitiesFolderPath == "")
                Debug.LogWarning("Photon/PhotonUtilities folder has had its .meta file deleted. This can lead to unpredictable errors. Please restore.");
            else
            {
                packObjectFolderPath = photonUtilitiesFolderPath + _ + "PackObject" + _;
                codeGenFolderPath = packObjectFolderPath + GENERATED_SUBFOLDER + _;
                codegenEditorResourcePath = packObjectFolderPath + "CodeGen" + _ + "Editor" + _ + "Resources" + _;
            }
        }

        public static TypeCatalogue single;

        [UnityEditor.InitializeOnLoadMethod]
        public static void Initialize()
        {
            FindPaths();
            EnsureExists();

            CompilationPipeline.assemblyCompilationFinished -= CompileFinished;
            CompilationPipeline.assemblyCompilationFinished += CompileFinished;

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        //private static void HandleOnPlayModeChanged(PlayModeStateChange obj)
        //{
        //	switch (obj)
        //	{
        //		case PlayModeStateChange.EnteredEditMode:
        //			break;

        //		case PlayModeStateChange.ExitingEditMode:
        //			break;

        //		case PlayModeStateChange.EnteredPlayMode:
        //			break;

        //		case PlayModeStateChange.ExitingPlayMode:
        //			break;
        //	}
        //}


        private static bool rebuilding;
        /// Delete any generated extensions that are throwing up errors.
        private static void CompileFinished(string arg1, CompilerMessage[] arg2)
        {

            if (!PackObjectSettings.Single.deleteBadCode)
                return;

            // bool that indicates if we looked for codegen orphaned by user moving the folder
            bool cleanOrphanCodegen = false;

            /// Check if any errors have popped up related to one of the generated files
            foreach (var arg in arg2)
            {
                // Check if this is a codegen file by its path and having Pack_ in the name.
                if (arg.type == CompilerMessageType.Error && arg.file.Contains(GENERATED_SUBFOLDER) && arg.file.Contains("Pack_"))
                {
                    // TEST - added to delete permanent loop due to users moving folders around.
                    File.Delete(arg.file);

                    // Search entire project for codegen that doesn't belong if we detected a name collision (user moved old folder most likely)
                    if (!cleanOrphanCodegen && arg.message.Contains("already contains"))
                    {
                        cleanOrphanCodegen = true;
                    }

                    deleteAllPending = true;
                }

                // If we detected orphaned code, seek and destroy.
                if (cleanOrphanCodegen)
                {
                    var possibleCodegen = Directory.GetFiles("Assets" + _, "Pack_*", SearchOption.AllDirectories);
                    Debug.Log("<b>Found</b> " + possibleCodegen.Length + " possible orphans");
                    foreach (var f in possibleCodegen)
                    {
                        Debug.Log("possible: " + f);
                        if (f.Contains(GENERATED_SUBFOLDER) && !f.Contains(codeGenFolderPath))
                        {
                            Debug.LogWarning("Codegen appears to have been moved. Deleting. " + f);
                            File.Delete(f);
                            //deleteAllPending = true;
                            rebuilding = true;
                            continue;
                        }
                    }
                }

                if (deleteAllPending)
                {
                    // Suppressing errors in the log that we are going to clear up.
                    Debug.ClearDeveloperConsole();
                    Debug.LogWarning("Script errors found in PackObject codegen. Deleting outdated codegen. " +
                        "To see these file errors and manually delete, disable 'Delete Bad Code' in PackObjectSettings.");
                }
            }
        }

        public static bool rescanPending;
        private static bool deleteAllPending;
        private static void EditorUpdate()
        {

            if (deleteAllPending)
            {
                deleteAllPending = false;
                DeleteAllPackCodeGen();
                rebuilding = true;
                EditorUtility.SetDirty(single);
            }
            if (rescanPending)
            {
                rescanPending = false;
                RescanAssembly();
            }
        }


        /// <summary>
        /// Trigger the rebuild at a later timing segment than the end of CompileFinished to avoid relentless looping.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void PostCompile()
        {

            if (!PackObjectSettings.Single.autoGenerate)
            {
                if (TypeCatalogue.single.catalogue.Count > 0)
                    Debug.LogWarning("Codegen disabled in " + typeof(PackObjectSettings).Name + ". Not checking for new or changed PackObjects (SyncVars). Enable Codegen if you are using PackObjects.");
                return;
            }

            if (rebuilding)
            {
                Debug.Log("Skipping a post compile rebuild.");
                rebuilding = false;
                return;
            }

            Debug.Log("Rescanning assembly for [PackObject] and [SyncVar] changes. " +
                "If you are not using Simple SyncVars, this scan can be disabled by un-checking 'Auto Generate' in PackObjectSettings.");
            rescanPending = true;

        }

        /// <summary>
        /// Finds or creates the singleton database.
        /// </summary>
        /// <returns></returns>
        public static TypeCatalogue EnsureExists()
        {
            if (single == null)
            {
                single = Resources.Load<TypeCatalogue>("TypeCatalogue");
            }

            if (single == null)
            {
                single = ScriptableObject.CreateInstance<TypeCatalogue>();

                if (!Directory.Exists(codegenEditorResourcePath))
                {
                    Directory.CreateDirectory(codegenEditorResourcePath);
                    Debug.Log("Expected directory for TypeCatalogue ScriptableObject asset does not exist. Creating :" + codegenEditorResourcePath);
                }

                AssetDatabase.CreateAsset(single, codegenEditorResourcePath + "TypeCatalogue.asset");
                AssetDatabase.Refresh();
            }

            return single;
        }

        public TypeInfoDict catalogue = new TypeInfoDict();

        //[MenuItem(MENU_PATH + "Delete All PackObj Codegen")]
        public static void DeleteAllPackCodeGen()
        {
            /// Get collection of current CodeGen files
            if (!Directory.Exists(codeGenFolderPath))
            {
                Debug.LogWarning("Unable to find target directory for generated code. Creating: " + codeGenFolderPath);
                Directory.CreateDirectory(codeGenFolderPath);
            }

            DirectoryInfo d = new DirectoryInfo(codeGenFolderPath);//Assuming Test is your Folder
            FileInfo[] files = d.GetFiles("*.cs"); //Getting Text files

            if (files.Length == 0)
                return;

            foreach (var f in files)
            {
                File.Delete(f.FullName);
                File.Delete(f.FullName + ".meta");
            }

            single.catalogue.Clear();
            EditorUtility.SetDirty(single);

            AssetDatabase.Refresh();
        }

        //[MenuItem(MENU_PATH + "Rebuild PackObj Codegen")]
        public static void RebuildSNSCodegen()
        {
            Compression.Internal.TypeCatalogue.DeleteAllPackCodeGen();
            Compression.Internal.TypeCatalogue.rescanPending = true;
        }


        private static List<string> reusableFilePaths = new List<string>();
        private static HashSet<Type> tempProcessedTypes = new HashSet<Type>();
        private static List<string> unusedTypes = new List<string>();

        //[MenuItem("Window/Rescan ASM")]
        public static void RescanAssembly()
        {
            EnsureExists();
            var watch0 = System.Diagnostics.Stopwatch.StartNew();

            unusedTypes.Clear();
            unusedTypes.AddRange(single.catalogue.keys);

            AssetDatabase.Refresh();
            bool haschanged = false;

            /// Get collection of current CodeGen files
            DirectoryInfo d = new DirectoryInfo(codeGenFolderPath);//Assuming Test is your Folder

            /// Create directory if its gone missing.
            if (!d.Exists)
                d.Create();

            FileInfo[] files = d.GetFiles("*.cs"); //Getting Text files

            tempProcessedTypes.Clear();

            /// Make record of all codegen files, so we can clean up any unassociated ones.
            reusableFilePaths.Clear();
            foreach (var f in files)
                reusableFilePaths.Add(codeGenFolderPath + f.Name);

            /// Check every type in the ASM for PackObj, and Catalogue them
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var t in a.GetTypes())
                {
                    var typeinfo = single.MakeRecordCurrent(t, ref haschanged);

                    if (unusedTypes.Contains(t.FullName))
                        unusedTypes.Remove(t.FullName);

                    /// Remove from our deletion file paths list
                    if (typeinfo != null)
                    {
                        reusableFilePaths.Remove(typeinfo.filepath);
                    }
                }

            /// Delete any files that don't have an associated TypeInfo
            foreach (var f in reusableFilePaths)
            {
                Debug.Log("<b>Deleting outdated file: </b>" + f);
                File.Delete(f);
                haschanged = true;
            }

            foreach(var unused in unusedTypes)
            {
                Debug.Log("Type " + unused + " is no longer a PackObject, or no longer exists. Removing from the PackObject catalog system.");
                single.catalogue.Remove(unused);
            }

            if (haschanged)
            {
                try
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                catch
                {

                }
            }

            watch0.Stop();
            Debug.Log("Rebuilding PackObject Codegen - took <i>" + watch0.ElapsedMilliseconds + "ms</i>. "
                + (haschanged ? "Changes where found." : "No changes found.")
                + "\nNOTE: Automatic PackObject codegen can be disabled in PackObjectSettings.");
        }


        public TypeInfo MakeRecordCurrent(Type type, ref bool haschanged)
        {
            tempProcessedTypes.Add(type);

            TypeInfo currTypeInfo;
            int index = catalogue.TryGetValue(type.FullName, out currTypeInfo);

            var attrs = type.GetCustomAttributes(typeof(PackObjectAttribute), false);

            /// Has no attributes, not even worth checking.
            if (attrs.Length == 0)
            {
                return null;
            }

#if UNITY_2018_1_OR_NEWER
			if (!PlayerSettings.allowUnsafeCode && type.IsValueType)
			{
				Debug.LogWarning(typeof(PackObjectAttribute).Name + " on structs will be ignored unless PlayerSettings.allowUnsafeCode == true.");
				return null;
			}
#endif

            /// Is a PackObject, create or amend as needed
            var packObjAttr = (attrs[0] as PackObjectAttribute);

            /// Attribute is not PackObject
            if (packObjAttr == null)
            {
                /// not a packObj, but a record exists. Delete it.
                if (currTypeInfo != null)
                {
                    catalogue.Remove(type.FullName);
                    Debug.Log("Deleting record-less codegen " + type.Name);
                    haschanged = true;
                }
                return null;
            }

            /// Is a PackObject, but is a struct that is managed (needs to be unmanaged - no refs in it)
            if (type.IsValueType && !type.IsUnManaged())
            {
                Debug.LogWarning(type.Name + " is a PackObject, but cannot be packed because it is a managed type. Structs cannot contain references or any managed types to be packable. ");

                /// not a packObj, but a record exists. Delete it.
                if (currTypeInfo != null)
                {
                    catalogue.Remove(type.FullName);
                    haschanged = true;
                }
                return null;
            }

            /// This is a new record, so create our type for filling
            if (index == -1)
            {
                currTypeInfo = new TypeInfo(type);
                haschanged = true;
            }

            GetPackableFields(type, currTypeInfo, packObjAttr);

            /// Type is a packObj, but no record exists yet.
            /// Get a list of fields and associated pack attribute for each field
            if (currTypeInfo == null)
            {
                haschanged = true;
                Debug.Log("No record yet for " + type.Name);
                return GenerateAndRecord(type, currTypeInfo, packObjAttr);
            }

            currTypeInfo.filepath = GetExtFilepath(type);

            /// If generated file time has changed - it can't be trusted. Delete and regen.
            if (currTypeInfo.codegenFileWriteTime != File.GetLastWriteTime(currTypeInfo.filepath).Ticks)
            {
                haschanged = true;
                Debug.Log("Codegen file time out of date, regenerating. " + currTypeInfo.filepath);
                return GenerateAndRecord(type, currTypeInfo, packObjAttr);
            }

            long hash = type.TypeToHash64();
            /// If field/attributes don't match, regenerate
            if (currTypeInfo.hashcode != hash)
            {
                haschanged = true;
                //Debug.Log(currTypeInfo.hashcode + " Type Compare mismatch " + hash);
                currTypeInfo.hashcode = hash;
                return GenerateAndRecord(type, currTypeInfo, packObjAttr);
            }

            //Debug.Log(type.Name + " has not changed " + currTypeInfo.hashcode);
            haschanged = false;
            return currTypeInfo;
        }

        public TypeInfo GenerateAndRecord(Type type, TypeInfo typeInfo, PackObjectAttribute packObjAttr)
        {
            string filepath = typeInfo.filepath;

            Debug.Log("Generating codegen for PackObj <b>" + type.FullName + "</b> to file: " + filepath);
            StringBuilder sb = type.GeneratePackCode(typeInfo, packObjAttr);
            File.WriteAllText(filepath, sb.ToString());

            typeInfo.codegenFileWriteTime = File.GetLastWriteTime(filepath).Ticks;
            catalogue.Add(type, typeInfo);

            EditorUtility.SetDirty(this);
            //AssetDatabase.SaveAssets();

            return typeInfo;
        }


        public void GetPackableFields(Type type, TypeInfo currTypeInfo, PackObjectAttribute packObjAttr)
        {
            int nestedFieldCount = 0;
            int localFieldCount = 0;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var defaultInclusion = packObjAttr.defaultInclusion;

            for (int i = 0, cnt = fields.Length; i < cnt; ++i)
            {
                var fieldInfo = fields[i];

                ///// Don't include any nested packObjs that will produce a nested loop
                //if (type.CheckForNestedLoop())
                //{
                //	Debug.LogWarning("<b>" + type.Name + "</b> contains a nested loop with field <b>" + fieldInfo.FieldType + " " + fieldInfo.Name + "</b>. Will not be included in serialization.");
                //	continue;
                //}


                var attrs = fieldInfo.GetCustomAttributes(typeof(SyncVarBaseAttribute), false);

                /// Only pack if marked with a Pack, or if we are set to capture all public
                if (defaultInclusion == DefaultPackInclusion.Explicit && attrs.Length == 0)
                    continue;

                /// Count up fields in nested
                var nestedAttrs = fieldInfo.FieldType.GetCustomAttributes(typeof(PackObjectAttribute), false);
                if (nestedAttrs.Length != 0)
                {
                    bool haschanged = false;
                    bool alreadyCurrent = (tempProcessedTypes.Contains(fieldInfo.FieldType));
                    var nestedTypeInfo = (alreadyCurrent) ? catalogue.GetTypeInfo(fieldInfo.FieldType) : MakeRecordCurrent(fieldInfo.FieldType, ref haschanged);

                    if (nestedTypeInfo != null)
                        nestedFieldCount += nestedTypeInfo.totalFieldCount;
                    else
                        continue;
                }
                else
                    localFieldCount++;
            }

            currTypeInfo.localFieldCount = localFieldCount;
            currTypeInfo.totalFieldCount = localFieldCount + nestedFieldCount;
        }

        public static string GetExtFilepath(Type type)
        {
            string filename = "Pack_" + type.Name + ".cs";
            return codeGenFolderPath + filename;
        }
    }
}

#endif