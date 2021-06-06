// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if GHOST_WORLD

using UnityEngine;
using Photon.Pun.UtilityScripts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple.GhostWorlds
{
	///// <summary>
	///// Extended Monobehaviour with cached values for RBs and CCs
	///// </summary>
	//[AddComponentMenu("")]
	//public class AnimatableMonobehaviour : MonoBehaviour
	//{
	//	[System.NonSerialized] public Rigidbody rb;
	//	[System.NonSerialized] public Rigidbody2D rb2d;
	//}

	[AddComponentMenu("")]
	public class Ghost : MonoBehaviour
		, IHasNetworkID
	{
		public IHaunted haunted;

		[System.NonSerialized] public Rigidbody rb;
		[System.NonSerialized] public Rigidbody2D rb2d;

		private uint viewID;
		public uint ViewID { get { return viewID; } }

		public void Initialize(IHaunted haunted)
		{
			this.haunted = haunted;
			var iViewID = haunted.GameObject.GetComponent<IHasNetworkID>();
			viewID = (iViewID != null) ? iViewID.ViewID : 0;

			/// Clone the RBs
			var hRB = haunted.GameObject.GetComponent<Rigidbody>();
			var hRB2D = haunted.GameObject.GetComponent<Rigidbody2D>();

			if (hRB)
				rb = gameObject.AddComponent<Rigidbody>().GetCopyOf(hRB);
			else if (hRB2D)
				rb2d = gameObject.AddComponent<Rigidbody2D>().GetCopyOf(hRB2D);
		}

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			ShowDebugCross(active);
#endif
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD

		[System.NonSerialized] public Renderer[] debugRenderers;

		public void ShowDebugCross(bool value)
		{
			for (int i = 0; i < debugRenderers.Length; ++i) debugRenderers[i].enabled = value;
		}
#endif
	}

#if UNITY_EDITOR


	[CustomEditor(typeof(Ghost))]
	[CanEditMultipleObjects]
	public class GhostEditor : HeaderEditorBase
	{
		protected override string Instructions
		{
			get
			{
				return "Automatically attached component for resimulaiton/rewind objects.";
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