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

}
