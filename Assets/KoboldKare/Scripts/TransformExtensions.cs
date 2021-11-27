using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions {
	public static Vector3 DirectionTo(this Transform source, Transform destination) {
		return source.position.DirectionTo(destination.position);
	}

	public static float DistanceTo(this Transform source, Transform destination) {
		return source.position.DistanceTo(destination.position);
	}

	public static Vector3 VectorTo(this Transform source, Transform destination) {
		return destination.position-source.position;
	}
	public static bool IsChildOf(this Transform child, Transform parent) {
		Transform currentTransform = child.parent;
		while(currentTransform != null && currentTransform != parent) {
			currentTransform = currentTransform.parent;
        }
		if (currentTransform == parent) {
			return true;
        }
		return false;
	}
	public static void GetComponentsInChildrenNoAlloc<T>(this Transform t, List<T> temp, List<T> result) {
        t.GetComponents<T>(temp);
        result.AddRange(temp);
        for(int i=0;i<t.childCount;i++) {
            GetComponentsInChildrenNoAlloc<T>(t.GetChild(i), temp, result);
		}
	}
}
