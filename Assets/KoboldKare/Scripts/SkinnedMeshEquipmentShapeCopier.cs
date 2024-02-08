using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshEquipmentShapeCopier : MonoBehaviour
{
    public SkinnedMeshRenderer source;
    public List<string> targetPaths = new List<string>();
    public List<string> blendShapeNames = new List<string>();
    private List<SkinnedMeshRenderer> targets = new List<SkinnedMeshRenderer>();

    void Update()
    {
        FindTargets();

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] != null)
            {
                CopyBlendShapes(targets[i]);
            }
            else
            {
                targets.RemoveAt(i);
            }
        }
    }

    void FindTargets()
    {
        foreach (string path in targetPaths)
        {
            Transform targetTransform = transform.Find(path);
            if (targetTransform != null)
            {
                SkinnedMeshRenderer target;
                if (targetTransform.TryGetComponent(out target))
                {
                    // Solo agregar el target si no está ya en la lista
                    if (!targets.Contains(target))
                    {
                        targets.Add(target);
                    }
                }
            }
        }
    }

    void CopyBlendShapes(SkinnedMeshRenderer target)
    {
        foreach (string blendShapeName in blendShapeNames)
        {
            int sourceBlendShapeIndex = source.sharedMesh.GetBlendShapeIndex(blendShapeName);
            int targetBlendShapeIndex = target.sharedMesh.GetBlendShapeIndex(blendShapeName);

            if (sourceBlendShapeIndex != -1 && targetBlendShapeIndex != -1)
            {
                float weight = source.GetBlendShapeWeight(sourceBlendShapeIndex);

                // Sumar 5 al peso solo si el valor es diferente de cero
                if (weight != 0f)
                {
                    weight += 13f;
                }

                target.SetBlendShapeWeight(targetBlendShapeIndex, weight);
            }
        }
    }
}




