// Copyright 2019 Vilar24

// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished 
// to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS 
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BlendshapeSplitter : AssetPostprocessor {

    private void OnPostprocessModel(GameObject g) {
        Apply(g.transform);
    }

    private void Apply(Transform t) {
        CreateAsymmetricalBlendShapes(t.gameObject, 0.04f);
        foreach (Transform child in t)
            Apply(child);
    }

    private void CreateAsymmetricalBlendShapes(GameObject gameObject, float blendDistance) {
        if (gameObject.GetComponent<SkinnedMeshRenderer>() == null) return;
        Mesh mesh = gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        int cachedBlendShapeCount = mesh.blendShapeCount;
        float tempBlendDistance = blendDistance;
        List<int> reorderIndices = new List<int>();
        for (int i=0;i<cachedBlendShapeCount;i++) {
            reorderIndices.Add(i);
        }
        int indexOffset = 0;
        for (int i = 0; i < cachedBlendShapeCount; i++) {
            if (mesh.GetBlendShapeName(i).Contains("LeftRight")) {
                tempBlendDistance = blendDistance;
                MatchCollection matches = Regex.Matches(mesh.GetBlendShapeName(i), ".*LeftRight(?<foo>[0-9]+)", RegexOptions.ExplicitCapture);
                foreach (Match match in matches) {
                    tempBlendDistance = float.Parse(match.Groups["foo"].Value) * 0.01f;
                }
                reorderIndices[i] = -1; // Delete original blend shape
                CreateAsymmetricalBlendShape(mesh, i, false, tempBlendDistance);
                reorderIndices.Add(i+indexOffset);
                CreateAsymmetricalBlendShape(mesh, i, true, tempBlendDistance);
                reorderIndices.Add(i+indexOffset+1);
                // Shift everything to the right to fit the extra blendshape, except for indicies we've already generated for new blendshapes.
                for(int o=i;o<reorderIndices.Count-2-indexOffset*2;o++) {
                    if (reorderIndices[o] >= 0) {
                        reorderIndices[o]++;
                    }
                }
                indexOffset += 1;
            }
        }
        ReorderBlendShapes(mesh, reorderIndices);
    }

    // Takes a mesh and a set of indices and reorders them.
    // Negative indices delete the target blendshape. Any other index is the target where the blendshape will be placed.
    // For example if mesh had blendshapes { a, b, c, d },
    // and endingIndices had indices       {-1, 1, 0, 2 }
    // Then blendshape a would be deleted and the resulting blendshapes would look like { c, b, d }
    // The endingIndices has to be ~PERFECT~ so don't submit indices with repeating positive terms or things will mess up.
    private void ReorderBlendShapes(Mesh mesh, List<int> endingIndices) {
        List<string> names = new List<string>();
        List<Vector3[]> dverts = new List<Vector3[]>();
        List<Vector3[]> dnorms = new List<Vector3[]>();
        List<Vector3[]> dtangs = new List<Vector3[]>();
        for (int i = 0;i<mesh.blendShapeCount;i++ ) {
            Vector3[] vertices = mesh.vertices;
            Vector3[] deltaVertices = new Vector3[mesh.vertices.Length];
            Vector3[] deltaNormals = new Vector3[mesh.vertices.Length];
            Vector3[] deltaTangents = new Vector3[mesh.vertices.Length];
            mesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);
            names.Add(mesh.GetBlendShapeName(i));
            dverts.Add(deltaVertices);
            dnorms.Add(deltaNormals);
            dtangs.Add(deltaTangents);
        }
        int blendShapeCount = 0;
        foreach( int i in endingIndices ) {
            if ( i >= 0 ) {
                blendShapeCount++;
            }
        }
        mesh.ClearBlendShapes();
        while (mesh.blendShapeCount != blendShapeCount) {
            bool found = false;
            for (int i = 0; i<blendShapeCount; i++) {
                int loc = endingIndices.IndexOf(i);
                if ( i == mesh.blendShapeCount && loc >= 0 && loc < endingIndices.Count) {
                    found = true;
                    mesh.AddBlendShapeFrame(names[loc], 100, dverts[loc], dnorms[loc], dtangs[loc]);
                }
            }
            if (!found ) {
                throw new UnityException("Inproperly constructed reorder index...");
            }
        }
    }

    private void CreateAsymmetricalBlendShape(Mesh mesh, int shapeIndex, bool right, float blendDistance) {
        float modelWidth = 0f;
        Vector3[] vertices = mesh.vertices;
        Vector3[] deltaVertices = new Vector3[mesh.vertices.Length];
        Vector3[] deltaNormals = new Vector3[mesh.vertices.Length];
        Vector3[] deltaTangents = new Vector3[mesh.vertices.Length];
        mesh.GetBlendShapeFrameVertices(shapeIndex, 0, deltaVertices, deltaNormals, deltaTangents);
        float rightFactor = 0;
        for (int i = 0; i < vertices.Length; i++) {
            if (modelWidth < vertices[i].x) modelWidth = vertices[i].x;
        }
        blendDistance = blendDistance * modelWidth;
        for (int i = 0; i < deltaVertices.Length; i++) {
            rightFactor = (vertices[i].x + blendDistance) / (blendDistance * 2f);
            rightFactor = Mathf.Clamp01(rightFactor);
            if (!right) rightFactor = 1f - rightFactor;
            deltaVertices[i] = deltaVertices[i] * rightFactor;
            deltaNormals[i] = deltaNormals[i] * rightFactor;
            deltaTangents[i] = deltaTangents[i] * rightFactor;
        }
        string[] separatingStrings = { "LeftRight" };
        mesh.AddBlendShapeFrame(mesh.GetBlendShapeName(shapeIndex).Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries)[0] + (right ? "Right" : "Left"), 100, deltaVertices, deltaNormals, deltaTangents);
    }

}
#endif