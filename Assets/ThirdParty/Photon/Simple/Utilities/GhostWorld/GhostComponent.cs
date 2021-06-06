// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if GHOST_WORLD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple.GhostWorlds
{
	[AddComponentMenu("")]
	public class GhostComponent : MonoBehaviour
	{
		/// <summary>
		/// Currently all HauntedComponents link to a shared GhostComponent on the Ghost, 
		/// so the HauntedComponent may be tied to more than one Components in the regular game scene.
		/// </summary>
		public List<IHauntedComponent> iHauntedComponents =  new List<IHauntedComponent>();
		public Ghost ghost;

		[System.NonSerialized] public Rigidbody rb;
		[System.NonSerialized] public Rigidbody2D rb2d;

#if !UNITY_2018_OR_NEWER
		[System.NonSerialized] int layer;
		[System.NonSerialized] int layerMask;
#endif

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			rb2d = GetComponent<Rigidbody2D>();
		}

		public void AddHaunted(IHauntedComponent iHauntedComponent, Ghost ghost)
		{
			iHauntedComponents.Add(iHauntedComponent);
			iHauntedComponent.GhostComponent = this;
			this.ghost = ghost;
		}
	}
}



#endif