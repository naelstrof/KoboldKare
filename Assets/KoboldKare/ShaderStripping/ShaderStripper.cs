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
        m_Keywords.Add(new ShaderKeyword("_ENVIRONMENTREFLECTIONS_OFF"));
        m_Keywords.Add(new ShaderKeyword("_SPECULARHIGHLIGHTS_OFF"));
    }

    // Multiple callback may be implemented.
    // The first one executed is the one where callbackOrder is returning the smallest number.
    public int callbackOrder { get { return 0; } }

    bool ShouldRemove(ShaderKeywordSet set) {
        foreach(var keyword in m_Keywords) {
            if (set.IsEnabled(keyword)) {
                return true;
            }
        }
        return false;
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData) {
        for (int i = 0; i < shaderCompilerData.Count; ++i) {
            if (ShouldRemove(shaderCompilerData[i].shaderKeywordSet)) {
                shaderCompilerData.RemoveAt(i);
                --i;
            }
        }
    }
}
#endif