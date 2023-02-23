using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions {

	public static Vector3 With(this Vector3 original, float? x = null, float? y = null, float? z = null) {
		return new Vector3(x ?? original.x, y ?? original.y, z ?? original.z);
	}

	public static Vector3 GroundVector(this Vector3 original) {
		return original.With(y:0f);
	}

	public static Vector3 KillDepth(this Vector3 original) {
		return original.With(z:0f);
	}

    public static Vector4 AppendW(this Vector3 original, float w) {
        return new Vector4(original.x, original.y, original.z, w);
    }

	public static Vector3 MagnitudeClamped(this Vector3 original, float? min = null, float? max = null) {
		float magnitude = original.magnitude;
		magnitude = Mathf.Max(magnitude, min ?? magnitude);
		magnitude = Mathf.Min(magnitude, max ?? magnitude);
		return original.normalized * magnitude;
	}

	public static Vector3 DirectionTo(this Vector3 source, Vector3 destination) {
		return Vector3.Normalize(destination - source);
	}

	public static float DistanceTo(this Vector3 source, Vector3 destination) {
		return Vector3.Magnitude(destination - source);
	}

	public static Vector3 VectorTo(this Vector3 source, Vector3 destination) {
		return destination - source;
	}
    public static Mesh Copy(this Mesh mesh) {
        var copy = new Mesh();
		copy.uv = new List<Vector2>(mesh.uv).ToArray();
		copy.uv2 = new List<Vector2>(mesh.uv2).ToArray();
		copy.uv3 = new List<Vector2>(mesh.uv3).ToArray();
		copy.uv4 = new List<Vector2>(mesh.uv4).ToArray();
		copy.uv5 = new List<Vector2>(mesh.uv5).ToArray();
		copy.uv6 = new List<Vector2>(mesh.uv6).ToArray();
		copy.uv7 = new List<Vector2>(mesh.uv7).ToArray();
		copy.uv8 = new List<Vector2>(mesh.uv8).ToArray();
		copy.bindposes = new List<Matrix4x4>(mesh.bindposes).ToArray();
		copy.indexFormat = mesh.indexFormat;
		copy.bounds = new Bounds(mesh.bounds.center, mesh.bounds.size);
		copy.vertices = new List<Vector3>(mesh.vertices).ToArray();
		copy.normals = new List<Vector3>(mesh.normals).ToArray();
		copy.tangents = new List<Vector4>(mesh.tangents).ToArray();
		copy.colors = new List<Color>(mesh.colors).ToArray();
		copy.colors32 = new List<Color32>(mesh.colors32).ToArray();
		copy.triangles = new List<int>(mesh.triangles).ToArray();
		copy.boneWeights = new List<BoneWeight>(mesh.boneWeights).ToArray();
		Unity.Collections.NativeArray<BoneWeight1> weights = new Unity.Collections.NativeArray<BoneWeight1>(mesh.GetAllBoneWeights(), Unity.Collections.Allocator.Temp);
		Unity.Collections.NativeArray<byte> bonesPerVertex = new Unity.Collections.NativeArray<byte>(mesh.GetBonesPerVertex(), Unity.Collections.Allocator.Temp);
		copy.SetBoneWeights(bonesPerVertex, weights);
		copy.name = new string(mesh.name.ToCharArray());
		copy.hideFlags = mesh.hideFlags;
        return copy;
    }
}
