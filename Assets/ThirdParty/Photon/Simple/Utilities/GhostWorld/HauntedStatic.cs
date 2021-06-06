// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if GHOST_WORLD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// For use on Scene gameobjects that do not move, but may want/need to be included in GhostWorld resimulations.
/// </summary>
namespace Photon.Pun.Simple.GhostWorlds
{
	public class HauntedStatic : MonoBehaviour, IHaunted
	{
		[System.NonSerialized] public Ghost ghost;

		public static List<HauntedStatic> haunteds = new List<HauntedStatic>();

		public Ghost Ghost { get { return ghost; } }
		public GameObject GameObject { get { return gameObject; } }

		private void Start()
		{
			ghost = GhostWorld.CreateRewindGhost(this);
			ghost.gameObject.SetActive(true);
		}

		private void OnEnable()
		{
			haunteds.Add(this);
		}

		private void OnDisable()
		{
			haunteds.Remove(this);
		}

		private void OnDestroy()
		{
			if (ghost != null)
				Destroy(ghost.gameObject);
		}
	}

#if UNITY_EDITOR


	[CustomEditor(typeof(HauntedStatic))]
	[CanEditMultipleObjects]
	public class HauntedStaticEditor : HeaderEditorBase
	{
		protected override string Instructions
		{
			get
			{
				return "Attach this component to root of static non-networked GameObjects (walls/terrain) you want available for inclusion in rewind/resimulation.";
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
		
	}

#endif
}

#endif