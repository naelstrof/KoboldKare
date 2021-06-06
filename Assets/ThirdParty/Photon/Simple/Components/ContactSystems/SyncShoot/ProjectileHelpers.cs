// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Simple
{
	public static class ProjectileHelpers
	{
		public static GameObject prefab;

		public static GameObject GetPlaceholderProj()
		{
			if (prefab != null)
				return prefab;

            var go = new GameObject("Projectile Placeholder Prefab");

            
			go.gameObject.SetActive(false);
			var rb = go.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			go.AddComponent<ContactProjectile>();
			go.AddComponent<ContactTrigger>();

            var childGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childGO.transform.parent = go.transform;
			childGO.GetComponent<Collider>().isTrigger = true;
            childGO.GetComponent<Renderer>().material.color = Color.yellow;
            childGO.transform.localScale = new Vector3(.1f, .1f, .1f);

            prefab = go;
			return go;
		}
	}
}
