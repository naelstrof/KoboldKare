using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class RaymarchScene {
    public static int maxShapes = 128;
    public struct ShapeData {
        public ShapeData(RaymarchShape shape) {
            position = shape.Position;
            scale = shape.Scale;
            colour = new Vector3(shape.colour.r, shape.colour.g, shape.colour.b);
            radius = shape.radius;
            shapeType = (int)shape.shapeType;
            operation = (int)shape.operation;
            blendStrength = shape.blendStrength;
            numChildren = shape.transform.childCount;
        }
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public float radius;
        public int shapeType;
        public int operation;
        public float blendStrength;
        public int numChildren;

        public static int GetSize () {
            return sizeof (float) * 11 + sizeof (int) * 3;
        }
        public void UpdateVariables(RaymarchShape shape) {
            position = shape.Position;
            scale = shape.Scale;
            colour = new Vector3(shape.colour.r, shape.colour.g, shape.colour.b);
            radius = shape.radius;
            shapeType = (int)shape.shapeType;
            operation = (int)shape.operation;
            blendStrength = shape.blendStrength;
            numChildren = shape.transform.childCount;
        }
    }
    public static int GetShapeCount() {
        return allShapes.Count;
    }
    private static bool dirty = true;
    private static List<RaymarchShape> allShapes = new List<RaymarchShape>();
    private static ShapeData[] orderedScene;
    public static void AddShape(RaymarchShape shape) {
        allShapes.Add(shape);
        dirty = true;
    }
    public static void RemoveShape(RaymarchShape shape) {
        allShapes.Remove(shape);
        dirty = true;
    }
    private static void CalculateScene() {
        if (dirty) {
            orderedScene = new ShapeData[Mathf.Min(allShapes.Count, maxShapes)];
            allShapes.Sort ((a, b) => a.operation.CompareTo (b.operation));
        }

        int sceneIndex = 0;
        for (int i=0;i<allShapes.Count&&sceneIndex<maxShapes;++i) {
            // Add top-level shapes (those without a parent)
            if (allShapes[i].transform.parent == null) {
                Transform parentShape = allShapes[i].transform;
                if (dirty) {
                    orderedScene[sceneIndex++] = new ShapeData(allShapes[i]);
                } else {
                    orderedScene[sceneIndex++].UpdateVariables(allShapes[i]);
                }
                // Add all children of the shape (nested children not supported currently)
                for (int j = 0; j < parentShape.childCount && sceneIndex < maxShapes; j++) {
                    RaymarchShape child = parentShape.GetChild (j).GetComponent<RaymarchShape> ();
                    // Child objects of a RaymarchShape MUST also be a RaymarchShape.
                    Assert.IsTrue(child != null);
                    if (child != null && child.isActiveAndEnabled) {
                        // Can't have nested children.
                        Assert.IsTrue(child.transform.childCount == 0);
                        if (dirty) {
                            ShapeData childShape = new ShapeData(child);
                            childShape.numChildren = 0;
                            orderedScene[sceneIndex++] = childShape;
                        } else {
                            orderedScene[sceneIndex++].UpdateVariables(child);
                        }
                    }
                }
            }
        }
        dirty = false;
    }
    public static ShapeData[] GetScene() {
        CalculateScene();
        return orderedScene;
    }
}
