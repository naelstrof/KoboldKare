#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

// Simple example of stripping of a debug build configuration
class ShaderStripper : IPreprocessShaders
{
    List<ShaderKeyword> m_Keywords;

    public ShaderStripper() {
        m_Keywords = new List<ShaderKeyword>();
        m_Keywords.Add(new ShaderKeyword("DEBUG"));
        m_Keywords.Add(new ShaderKeyword("LOD_FADE_CROSSFADE"));
        m_Keywords.Add(new ShaderKeyword("_ADDITIONAL_LIGHTS_VERTEX"));
    }

    // Multiple callback may be implemented.
    // The first one executed is the one where callbackOrder is returning the smallest number.
    public int callbackOrder { get { return 0; } }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData) {
        foreach(var keyword in m_Keywords) {
            for (int i = 0; i < shaderCompilerData.Count; ++i) {
                if (shaderCompilerData[i].shaderKeywordSet.IsEnabled(keyword)) {
                    shaderCompilerData.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}
#endif