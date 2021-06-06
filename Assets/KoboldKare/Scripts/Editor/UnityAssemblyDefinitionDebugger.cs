#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public class AsmdefDebug
{
    const string AssemblyReloadEventsEditorPref = "AssemblyReloadEventsTime";
    const string AssemblyCompilationEventsEditorPref = "AssemblyCompilationEvents";
    static readonly int ScriptAssembliesPathLen = "Library/ScriptAssemblies/".Length;

    static Dictionary<string, DateTime> s_StartTimes = new Dictionary<string, DateTime>();

    static StringBuilder s_BuildEvents = new StringBuilder();
    static double s_CompilationTotalTime;

    static AsmdefDebug()
    {
        CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
        CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
        AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
    }

    static void CompilationPipelineOnAssemblyCompilationStarted(string assembly)
    {
        s_StartTimes[assembly] = DateTime.UtcNow;
    }

    static void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2)
    {
        var time = s_StartTimes[assembly];
        var timeSpan = DateTime.UtcNow - s_StartTimes[assembly];
        s_CompilationTotalTime += timeSpan.TotalMilliseconds;
        s_BuildEvents.AppendFormat("{0:0.00}s {1}\n", timeSpan.TotalMilliseconds / 1000f, assembly.Substring(ScriptAssembliesPathLen, assembly.Length - ScriptAssembliesPathLen));
    }

    static void AssemblyReloadEventsOnBeforeAssemblyReload()
    {
        s_BuildEvents.AppendFormat("compilation total: {0:0.00}s\n", s_CompilationTotalTime / 1000f);
        EditorPrefs.SetString(AssemblyReloadEventsEditorPref, DateTime.UtcNow.ToBinary().ToString());
        EditorPrefs.SetString(AssemblyCompilationEventsEditorPref, s_BuildEvents.ToString());
    }

    static void AssemblyReloadEventsOnAfterAssemblyReload()
    {
        var binString = EditorPrefs.GetString(AssemblyReloadEventsEditorPref);

        long bin = 0;
        if (long.TryParse(binString, out bin))
        {
            var date = DateTime.FromBinary(bin);
            var time = DateTime.UtcNow - date;
            var compilationTimes = EditorPrefs.GetString(AssemblyCompilationEventsEditorPref);
            if (!string.IsNullOrEmpty(compilationTimes))
            {
                Debug.Log("Compilation Report\n" + compilationTimes + "Assembly Reload Time: " + time.TotalSeconds + "s\n");
            }
        }
    }
}
#endif
