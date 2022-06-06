#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
namespace Naelstrof.BodyProportion {
    public class BodyProportionPostProcessor : AssetPostprocessor {
        private void OnPostprocessModel(GameObject obj) {
            Animator animator = obj.GetComponentInChildren<Animator>();
            if (animator == null || !animator.isHuman) {
                return;
            }
            SkinnedMeshRenderer[] skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers) {
                ProcessSkinnedMeshRenderer(animator,skinnedMeshRenderer);
            }
        }

        private void ProcessSkinnedMeshRenderer(Animator animator, SkinnedMeshRenderer renderer) {
            Mesh mesh = renderer.sharedMesh;
            if (mesh == null) {
                return;
            }
            NativeArray<byte> bonesPerVertex = mesh.GetBonesPerVertex();
            NativeArray<BoneWeight1> boneWeights = mesh.GetAllBoneWeights();
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
                if (!BodyProportionStaticSettings.HasFlag((HumanBodyBones)i, BodyProportionStaticSettings.BoneFlags.Blendshape)) {
                    continue;
                }

                CreateBlendshapeFor(animator, renderer, mesh, bonesPerVertex, boneWeights, vertices, (HumanBodyBones)i, 0f);
                CreateBlendshapeFor(animator, renderer, mesh, bonesPerVertex, boneWeights, vertices, (HumanBodyBones)i, 2f);
            }
        }
        private HashSet<int> GetMask(HumanBodyBones bone, Animator animator, SkinnedMeshRenderer targetRenderer) {
            HashSet<int> hashSet = new HashSet<int>();
            Transform targetBone = animator.GetBoneTransform(bone);
            Transform endBone = animator.GetChildBone(bone);
            Transform currentBone = endBone;
            while (currentBone != targetBone) {
                currentBone = currentBone.parent;
                hashSet.Add(TransformToBoneID(currentBone, targetRenderer));
            }
            hashSet.Add(TransformToBoneID(targetBone, targetRenderer));
            return hashSet;
        }

        private int TransformToBoneID(Transform transform, SkinnedMeshRenderer targetRenderer) {
            var bones = targetRenderer.bones;
            for (int i = 0; i < bones.Length; i++) {
                if (transform == bones[i]) {
                    return i;
                }
            }
            return -1;
        }
        private Matrix4x4 TransformToBindPose(Transform transform, SkinnedMeshRenderer targetRenderer, Mesh targetMesh) {
            var bones = targetRenderer.bones;
            return targetMesh.bindposes[TransformToBoneID(transform, targetRenderer)];
        }

        private void CreateBlendshapeFor(Animator animator, SkinnedMeshRenderer targetRenderer, Mesh targetMesh, NativeArray<byte> bonesPerVertex, NativeArray<BoneWeight1> allBoneWeights, List<Vector3> vertexPositions, HumanBodyBones targetBone, float scale) {
            Transform bone = animator.GetBoneTransform(targetBone);
            if (bone == null) {
                return;
            }

            int boneID = TransformToBoneID(bone, targetRenderer);
            Matrix4x4 targetBoneBindpose = TransformToBindPose(animator.GetBoneTransform(targetBone), targetRenderer, targetMesh);
            Matrix4x4 endBoneBindpose = TransformToBindPose(animator.GetChildBone(targetBone), targetRenderer, targetMesh);
            Vector3 boneLengthMask = targetBoneBindpose.inverse.MultiplyPoint(endBoneBindpose.MultiplyPoint(Vector3.zero));
            
            Vector3 boneScaleMask = new Vector3(1f - boneLengthMask.x, 1f - boneLengthMask.y, 1f - boneLengthMask.z);
            HashSet<int> boneIDMask = GetMask(targetBone, animator, targetRenderer);
            List<Vector3> framePositionDelta = new List<Vector3>();
            List<Vector3> frameNormalDelta = new List<Vector3>();
            List<Vector3> frameTangentDelta = new List<Vector3>();
            int wt = 0;
            int vt = 0;
            for (int o = 0; o < bonesPerVertex.Length; o++) {
                // Find the weight of the bone on this vertex.
                float tempWeight = 0f;
                for (int p = 0; p < bonesPerVertex[o]; p++) {
                    if (boneIDMask.Contains(allBoneWeights[wt].boneIndex)) {
                        tempWeight += allBoneWeights[wt].weight;
                    }
                    wt++;
                }

                if (tempWeight == 0f) {
                    vt++;
                    framePositionDelta.Add(Vector3.zero);
                    frameNormalDelta.Add(Vector3.zero);
                    frameTangentDelta.Add(Vector3.zero);
                    continue;
                }
                // Scale it
                Vector3 tempVert = targetBoneBindpose.MultiplyPoint(vertexPositions[vt]);
                Vector3 scaler = Vector3.one;
                scaler.x = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.x);
                scaler.y = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.y);
                scaler.z = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.z);
                tempVert = Vector3.Scale(tempVert, scaler);
                framePositionDelta.Add(targetBoneBindpose.inverse.MultiplyPoint(tempVert) - vertexPositions[vt]);
                frameNormalDelta.Add(Vector3.zero);
                frameTangentDelta.Add(Vector3.zero);
                vt++;
            }

            string targetName = $"{targetBone.ToString()}_{scale:N0}";
            if (targetMesh.GetBlendShapeIndex(targetName) != -1) {
                throw new UnityException(
                    $"Failed to import {targetRenderer.name}, it contains a blendshape name that conflicts with the proportion tool! {targetBone.ToString()}");
            }

            targetMesh.AddBlendShapeFrame(targetName, 100f, framePositionDelta.ToArray(), 
            frameNormalDelta.ToArray(),
            frameTangentDelta.ToArray());
        }
        
    }
}
#endif