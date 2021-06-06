// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple.Pooling
{
	/// <summary>
	/// A VERY basic pooling system, because not using pooling is a bad for things like projectiles. 
	/// The statics are the pool manager. The non-static fields and methods are for pool members this is attached to.
	/// Objects are returned to the pool when they are SetActive(false).
	/// </summary>
	public class Pool : MonoBehaviour
	{

#if UNITY_EDITOR
		public const bool tidyUpInEditorMode = true;
		public static Transform tidyUpParent;
#endif
		/// <summary>
		/// Storage struct for the creation values for when we need to grow the pool.
		/// </summary>
		private struct PoolItemDef
		{
			public GameObject prefab;
			public int growBy;
			public Type scriptToAdd;

			public PoolItemDef(GameObject prefab, int growBy, Type scriptToAdd)
			{
				this.prefab = prefab;
				this.growBy = growBy;
				this.scriptToAdd = scriptToAdd;
			}
		}

		#region Static PoolManager items

		/// <summary>
		/// The Dict of pools, indexed by the source prefab.
		/// </summary>
		private static Dictionary<GameObject, Stack<Pool>> pools = new Dictionary<GameObject, Stack<Pool>>();

		/// <summary>
		/// Stores the creation parameters for a pool, for use when expanding it.
		/// </summary>
		private static Dictionary<GameObject, PoolItemDef> poolItemDefs = new Dictionary<GameObject, PoolItemDef>();

		/// <summary>
		/// Add a prefab to the Pool list, and create a pool. Returns the list index.
		/// </summary>
		/// <param name="_prefab"></param>
		/// <param name="_growBy"></param>
		/// <param name="_scriptToAdd">Indicate a typeof(Component) that you want added (if not already there) to the root of all instantiated pool items.</param>
		/// <returns></returns>
		public static void AddPrefabToPool(GameObject _prefab, int startingSize = 8, int _growBy = 8, Type _scriptToAdd = null, bool tidyUp = false)
		{
#if UNITY_EDITOR
			if (!_prefab)
			{
				Debug.LogWarning("Attempt to add null object to the pool.");
			}
#endif

			if (poolItemDefs.ContainsKey(_prefab))
				return;

			pools.Add(_prefab, new Stack<Pool>());

			var poolDef = new PoolItemDef(_prefab, _growBy, _scriptToAdd);
			poolItemDefs.Add(_prefab, poolDef);
			
			GrowPool(_prefab, startingSize);

			if (tidyUp)
			{
#if UNITY_EDITOR
				_prefab.transform.parent = tidyUpParent;
#endif
				_prefab.gameObject.SetActive(false);
			}
		}

		public static Pool Spawn(GameObject origPrefab, Transform t, float duration = 5f)
		{
			return Spawn(origPrefab, t.position, t.rotation, duration);
		}

		public static Pool Spawn(GameObject origPrefab, Vector3 pos, Quaternion rot, Vector3 scl, float duration = 5f)
		{
			var pool = Spawn(origPrefab, pos, rot, duration);
			pool.gameObject.transform.localScale = scl;
			return pool;

		}
		public static Pool Spawn(GameObject origPrefab, Vector3 pos, Quaternion rot, float duration = 5f)
		{
			//if (!pools.ContainsKey(origPrefab))
			//	AddPrefabToPool(origPrefab);

			if (pools[origPrefab].Count == 0)
				GrowPool(origPrefab);

			/// Check to see if the pool has been destroyed somehow, and reuild it if so.
			if (!pools[origPrefab].Peek())
			{
				pools[origPrefab].Clear();
				GrowPool(origPrefab);
			}

			Pool p = pools[origPrefab].Pop();

			p.transform.position = pos;
			p.transform.rotation = rot;
			p.deathClock = duration;
			// Only enable if we are counting down for expiration.
			p.enabled = (duration > 0);

			p.gameObject.SetActive(true);

			return p;
		}

		private static void GrowPool(GameObject _prefab, int growAmt = -1)
		{
#if UNITY_EDITOR
			// put pooled items onto a parent to tidy up in editor mode - completely ignored in release
			if (tidyUpInEditorMode && !tidyUpParent)
				tidyUpParent = new GameObject("Pool Items (Editor Only)").transform;
#endif

			PoolItemDef def = poolItemDefs[_prefab];

			// Grow by the amount originally specified if no growAmt is given.
			int growby = (growAmt < 1) ? def.growBy : growAmt;

			for (int i = 0; i < growby; i++)
			{

#if UNITY_EDITOR
				// put pooled items onto a parent to tidy up in editor mode - completely ignored in release
				GameObject go = Instantiate(def.prefab, (tidyUpInEditorMode) ? tidyUpParent : null);
#else
				GameObject go = Instantiate(def.prefab);
#endif
				AddItemToPool(go, _prefab, def);
			}
		}

		private static void AddItemToPool(GameObject go, GameObject _prefab, PoolItemDef def)
		{
			go.SetActive(false);

			//Precache some stuff so this pool get later doesn't require any GetComponent calls.
			Pool p = go.AddComponent<Pool>();
			p.CacheComponents();

			// Add the scrpitToAdd if it doesn't already exist
			if (def.scriptToAdd != null && go.GetComponent(def.scriptToAdd) == null)
				p.extraScript = go.AddComponent(def.scriptToAdd);

			p.origPrefab = _prefab;

			// Add this new instance to the pool
			pools[_prefab].Push(p);
		}

		private static void ReturnToPool(Pool p, GameObject _prefab)
		{
			pools[_prefab].Push(p);
		}

		#endregion

		#region Fields and Methods used by pool items this is attached to.

		// The original prefab acts as the dictionary key for this pool.
		[HideInInspector] public GameObject origPrefab;
		[HideInInspector] public Rigidbody rb;
		[HideInInspector] public Component extraScript;

		public float deathClock;

		public void CacheComponents()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			deathClock -= Time.deltaTime;
			if (deathClock < 0)
			{
				gameObject.SetActive(false);
			}
		}

		private void OnDisable()
		{
			ReturnToPool(this, origPrefab);
		}

		#endregion
	}
}
