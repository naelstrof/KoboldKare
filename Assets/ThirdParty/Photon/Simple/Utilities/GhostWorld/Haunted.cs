// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if GHOST_WORLD

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Pun.Simple.GhostWorlds
{
	public enum ResimInclusion { Never = 0, SelfDesync = 1, PhysicsLayerDesync = 2, Always = 3 }

	/// <summary>
	/// Attach to NetObject or root of any scene object that should be included in the GhostWorld for
	/// rewind and resimulation.
	/// </summary>
	public class Haunted : MonoBehaviour
		, IHaunted
	{

		//[EnumMask]
		public ResimInclusion resimInclusion = ResimInclusion.PhysicsLayerDesync;

		public static List<Haunted> haunteds = new List<Haunted>();

		// cached shortcuts
		[System.NonSerialized] public Ghost ghost;
		//[System.NonSerialized] public INetObject netObj;

		public Ghost Ghost { get { return ghost; } set { ghost = value; } }
		public GameObject GameObject { get { return gameObject; } }

		[System.NonSerialized] public Rigidbody rb;
		[System.NonSerialized] public Rigidbody2D rb2d;

		//[System.NonSerialized] public Rigidbody ghostRB;
		//[System.NonSerialized] public Rigidbody2D ghostRB2D;
		//[System.NonSerialized] public Transform ghostTrans;

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			rb2d = GetComponent<Rigidbody2D>();

			//netObj = GetComponent<INetObject>();
		}

		private void OnEnable()
		{
			haunteds.Add(this);
		}

		private void OnDisable()
		{
			haunteds.Remove(this);
		}

		private void Start()
		{
			ghost = GhostWorld.CreateRewindGhost(this);
			ghost.gameObject.SetActive(true);
		}

		private void OnDestroy()
		{
			if (ghost != null)
				Destroy((ghost as Component).gameObject);
		}
	}

#if UNITY_EDITOR


	[CustomEditor(typeof(Haunted))]
	[CanEditMultipleObjects]
	public class HauntedEditor : HeaderEditorBase
	{
		protected override string Instructions
		{
			get
			{
				return "Attach this component to root of networked GameObject you want available for resimulation.";
			}
		}

		protected override string HelpURL
		{
			get
			{
				return "";
			}
		}

		protected override string TextTexturePath
		{
			get
			{
				return "Header/GhostWorldText";
			}
		}
		
		protected override string BackTexturePath
		{
			get
			{
				return "Header/CyanBack";
			}
		}
	}

#endif
}


#endif