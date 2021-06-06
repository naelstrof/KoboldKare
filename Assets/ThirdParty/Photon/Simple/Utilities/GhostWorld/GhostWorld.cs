// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if GHOST_WORLD

using Photon.Pun.UtilityScripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Photon.Pun.Simple.GhostWorlds
{
	/// <summary>
	/// Automatically creates the GhostWorld physics scene at startup, and has methods for populating the world with gameobject clones.
	/// </summary>
	public class GhostWorld
	{

#if UNITY_2019_1_OR_NEWER

		public static PhysicsScene ghostPhysics;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void CreateGhostWorld()
		{
			var sceneparams = new CreateSceneParameters(LocalPhysicsMode.Physics3D) { };

			ghostScene = SceneManager.CreateScene("_GhostWorld", sceneparams);
			ghostPhysics = PhysicsSceneExtensions.GetPhysicsScene(ghostScene);
		}

#else
		public const int GHOST_WORLD_LAYER = 31;
		public const int GHOST_WORLD_LAYERMASK = 1 << GHOST_WORLD_LAYER;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void CreateGhostWorld()
		{
			ghostScene = SceneManager.GetActiveScene();

			/// Untested. Make ghost world non-interactive with all other physics layers
			for (int i = 0; i < GHOST_WORLD_LAYER; ++i)
			{
				Physics.IgnoreLayerCollision(GHOST_WORLD_LAYER, i);
			}

		}
#endif
		public static Scene ghostScene;

		private static readonly List<Collider> reusableColliderList = new List<Collider>(4);
		private static readonly List<INeedsGhostGameObject> reusableNeedsGameObjectClone = new List<INeedsGhostGameObject>();

		static int colliderCount = 0;

		/// <summary>
		/// Replicate entire gameobject as only empty gameobjects and colliders.
		/// </summary>
		internal static Ghost CreateRewindGhost(IHaunted haunted)
		{
			Scene holdscene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(ghostScene);

			/// Create the root of the Ghost
			GameObject ghostGO = new GameObject("ghst." + haunted.GameObject.name);

#if !UNITY_2019_1_OR_NEWER
			/// For old physx these ghosts are in the main scene, and need to be protected from scene changes
			Object.DontDestroyOnLoad(ghostGO);
#endif

			SceneManager.SetActiveScene(holdscene);

			/// Add the Ghost component
			Ghost ghost = ghostGO.AddComponent<Ghost>();

			/// TEST widget
#if UNITY_EDITOR || DEVELOPMENT_BUILD

			var widgetYGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			Object.DestroyImmediate(widgetYGO.GetComponent<Collider>());
			widgetYGO.transform.localScale = new Vector3(.1f, 3f, .1f);
			widgetYGO.transform.parent = ghostGO.transform;

			var widgetXGO = Object.Instantiate(widgetYGO, ghost.transform);
			widgetYGO.transform.Rotate(0f, 0f, 90f);

			var debugCross = new GameObject("Debug Cross");
			debugCross.hideFlags = HideFlags.HideInHierarchy;
			debugCross.transform.parent = ghostGO.transform;
			widgetXGO.transform.parent = debugCross.transform;
			widgetYGO.transform.parent = debugCross.transform;

			ghost.debugRenderers = ghost.GetComponentsInChildren<Renderer>();
			ghost.ShowDebugCross(false);

#endif

			/// Initialize the Ghost component
			ghost.Initialize(haunted);

			colliderCount = 0;
			/// Start the recursion process
			CloneChildrenAndColliders(haunted.GameObject, ghostGO, ghost);

			return ghost;
		}

		/// <summary>
		/// Make a barebones copy of an object including only empty gameobjects and colliders.
		/// </summary>
		private static void CloneChildrenAndColliders(GameObject srcGO, GameObject copyGO, Ghost rootGhost)
		{
			copyGO.transform.localPosition = srcGO.transform.localPosition;
			copyGO.transform.localScale = srcGO.transform.localScale;
			copyGO.transform.localRotation = srcGO.transform.localRotation;

#if !UNITY_2019_1_OR_NEWER
			copyGO.layer = GHOST_WORLD_LAYER;
#else
			copyGO.layer = srcGO.layer;
#endif

			/// Clone the colliders on this node
			CloneColliders(rootGhost, srcGO, copyGO, ref colliderCount);

			/// Create GhostComponent links if any 
			srcGO.GetComponents(reusableNeedsGameObjectClone);

			GhostComponent ghostComponent = null;

			foreach (var iNeedsClone in reusableNeedsGameObjectClone)
			{

				IHauntedComponent ihc = iNeedsClone as IHauntedComponent;
				if (ihc != null)
				{
					if (ghostComponent == null)
						ghostComponent = copyGO.AddComponent<GhostComponent>();

					ghostComponent.AddHaunted(ihc, rootGhost);
				}

				/// Copy any components that are flagged by interface to be copied to ghost
				ICopyToGhost icopy = iNeedsClone as ICopyToGhost;
				if (icopy != null)
					(icopy as Component).ComponentCopy(copyGO);

			}

			///// Copy any components that are flagged by interface to be copied to ghost
			//srcGO.GetComponents(reusableCopyToGhostFind);
			//foreach (var icopy in reusableCopyToGhostFind)
			//{
			//	(icopy as Component).ComponentCopy(copyGO);
			//}

			/// Find all children and repeat this cloning process for each child
			for (int i = 0; i < srcGO.transform.childCount; i++)
			{
				Transform orig = srcGO.transform.GetChild(i);

				/// Test to see if there is any reason to clone children (colliders or components flagged as needing their child cloned)
				if (orig.GetComponentInChildren<Collider>() == null &&
					orig.GetComponentInChildren<INeedsGhostGameObject>() == null)
					continue;

				Transform copy = new GameObject(orig.name).transform;

				copy.parent = copyGO.transform;

				CloneChildrenAndColliders(srcGO.transform.GetChild(i).gameObject, copy.gameObject, rootGhost);
			}
		}

		private static int CloneColliders(Ghost ghost, GameObject sourceObj, GameObject copyObj, ref int colliderCount)
		{
			sourceObj.GetComponents(reusableColliderList);

			for (int i = 0; i < reusableColliderList.Count; ++i)
			{
				if (reusableColliderList[i] != null)
				{
					copyObj.AddColliderCopy(reusableColliderList[i]);
#if !UNITY_2019_1_OR_NEWER
					var ghostcol = copyObj.AddComponent<GhostCollider>();
					ghostcol.SetLayer(sourceObj.layer);
#endif
					colliderCount++;
				}
			}
			return reusableColliderList.Count;
		}

	}
}

#endif