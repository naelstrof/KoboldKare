// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using emotitron.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple
{
	public class TeleportMarker : MonoBehaviour
	{

		#region Inspector

		public MarkerNameType markerType;

		#endregion

		private void OnEnable()
		{
			int hash = markerType.hash;

			if (!lookup.ContainsKey(hash))
				lookup.Add(hash, new List<TeleportMarker>());

			lookup[hash].Add(this);
		}

		private void OnDisable()
		{
			int hash = markerType.hash;

			if (!lookup.ContainsKey(hash))
				return;

			if (!lookup[hash].Contains(this))
				return;

			lookup[hash].Remove(this);
		}

		#region Statics

		public static Dictionary<int, List<TeleportMarker>> lookup = new Dictionary<int, List<TeleportMarker>>();
		public static Dictionary<int, int> nexts = new Dictionary<int, int>();

		public static TeleportMarker GetRandomMarker(int hash, float seed)
		{
			if (!lookup.ContainsKey(hash))
				return null;

			var list = lookup[hash];
			int rand = Random.Range(0, list.Count);
			return list[rand];
		}

		public static TeleportMarker GetNextMarker(int hash)
		{
			if (!lookup.ContainsKey(hash))
				return null;

			var list = lookup[hash];
			var next = nexts[hash];

			next++;

			if (next >= nexts.Count)
				next = 0;

			nexts[hash] = next;

			return list[next];
		}

		#endregion Statics
	}
}

