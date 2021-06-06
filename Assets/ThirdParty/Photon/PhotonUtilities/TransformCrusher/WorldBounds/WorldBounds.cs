// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{

	public enum FactorBoundsOn { EnableDisable, AwakeDestroy }
	/// <summary>
	/// Put this object on the root of a game map. It needs to encompass all of the areas the player is capable of moving to.
	/// The object must contain a MeshRenderer in order to get the bounds.
	/// </summary>
	//[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.4n2gizaw79m0")]
	//[AddComponentMenu("Transform Crusher/World Bounds")]
	[ExecuteInEditMode]
	public class WorldBounds : MonoBehaviour
	{

		[SerializeField][HideInInspector]
		private Bounds manualBounds = new Bounds(new Vector3(0,0,0), new Vector3(600, 40, 600));
		public Bounds ManualBounds
		{
			get { return manualBounds; }
			set
			{
				manualBounds = value;
				CollectMyBounds();
			}
		}

		[Tooltip("Selects which WorldBounds group this object should be factored into.")]
		[WorldBoundsSelectAttribute]
		[SerializeField][HideInInspector]
		public int worldBoundsGrp;

		[SerializeField]
		[HideInInspector]
		private bool includeChildren = true;

		[Tooltip("Awake/Destroy will consider element into the world size as long as it exists in the scene (You may need to wake it though). Enable/Disable only factors it in if it is active.")]
		[SerializeField]
		[HideInInspector]
		private BoundsTools.BoundsType factorIn = BoundsTools.BoundsType.Both;
		public BoundsTools.BoundsType FactorIn
		{
			get { return factorIn; }
			set
			{
				factorIn = value;
				CollectMyBounds();
			}
		}
		// sum of all bounds (children included)
		[HideInInspector] public Bounds myBounds;
		[HideInInspector] public int myBoundsCount;



		public System.Action OnWorldBoundsChange;

		void Awake()
		{
			// When mapobjects are waking up, this likely means we are seeing a map change. Silence messages until Start().
			//muteMessages = true;
			CollectMyBounds();
		}

		public void CollectMyBounds()
		{
			var wbso = WorldBoundsSettings.Single;
			if (!wbso)
				return;

			if (WorldBoundsSettings.Single.worldBoundsGroups.Count == 0)
				WorldBoundsSettings.Single.worldBoundsGroups.Add(new WorldBoundsGroup());

			// If this is no longer a existing worldbounds layer, reset to default
			if (worldBoundsGrp >= WorldBoundsSettings.Single.worldBoundsGroups.Count)
				worldBoundsGrp = 0;

			var grp = wbso.worldBoundsGroups[worldBoundsGrp];

			if (factorIn == BoundsTools.BoundsType.Manual)
			{
				myBounds = manualBounds;
				myBoundsCount = 1;
			}
			else
			{
				myBounds = BoundsTools.CollectMyBounds(gameObject, factorIn, out myBoundsCount, includeChildren, false);
			}

			// Remove this from all Groups then readd to the one it currently belongs to.
			WorldBoundsSettings.RemoveWorldBoundsFromAll(this);

			if (myBoundsCount > 0 && enabled)
			{
				if (!grp.activeWorldBounds.Contains(this))
				{
					grp.activeWorldBounds.Add(this);
					grp.RecalculateWorldCombinedBounds();
				}
			}

			if (OnWorldBoundsChange != null)
				OnWorldBoundsChange();
		}

		private void Start()
		{
			//muteMessages = false;
		}

		private void OnEnable()
		{
			FactorInBounds(true);
		}

		private void OnDisable()
		{
			FactorInBounds(false);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(
				WorldBoundsSettings.Single.worldBoundsGroups[worldBoundsGrp]._combinedWorldBounds.center,
				WorldBoundsSettings.Single.worldBoundsGroups[worldBoundsGrp]._combinedWorldBounds.size);
		}

		public void FactorInBounds(bool b)
		{
			if (this == null)
				return;

			// if worldboundsgrp is no longer a layer, reset to default
			if (worldBoundsGrp >= WorldBoundsSettings.Single.worldBoundsGroups.Count)
				worldBoundsGrp = 0;

			var grp = WorldBoundsSettings.Single.worldBoundsGroups[worldBoundsGrp];

			if (b)
			{
				if (!grp.activeWorldBounds.Contains(this))
					grp.activeWorldBounds.Add(this);
			}
			else
			{
				grp.activeWorldBounds.Remove(this);
			}

			grp.RecalculateWorldCombinedBounds();

		}

#if UNITY_EDITOR
		float recheckTimer;
		private void Update()
		{
			if (Application.isPlaying)
				return;

			recheckTimer += Time.deltaTime;
			if (recheckTimer > 1f)
			{
				CollectMyBounds();
			}
		}
#endif

#if UNITY_EDITOR

		public string BoundsReport()
		{
			return WorldBoundsSettings.single.worldBoundsGroups[worldBoundsGrp].BoundsReport();
			//return ("Contains " + myBoundsCount + " bound(s) objects:\n" +
			//	"Center: " + myBounds.center + "\n" +
			//	"Size: " + myBounds.size);
		}
#endif

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(WorldBounds))]
	[CanEditMultipleObjects]
	public class WorldBoundsEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var _target = target as WorldBounds;

			//EditorGUILayout.Space();
			//var mb = serializedObject.FindProperty("manualBounds");
			//EditorGUILayout.PropertyField(mb);

			var factorin = (BoundsTools.BoundsType)EditorGUILayout.EnumPopup("Factor In", _target.FactorIn);
			if (_target.FactorIn != factorin)
			{
				Undo.RecordObject(_target, "Change bounds Factor In");
				_target.FactorIn = factorin;
				_target.CollectMyBounds();
				EditorUtility.SetDirty(target);
				serializedObject.Update();
			}
			if (_target.FactorIn == BoundsTools.BoundsType.Manual)
			{
				EditorGUI.BeginChangeCheck();
				var manualBounds = serializedObject.FindProperty("manualBounds");
				EditorGUILayout.PropertyField(manualBounds);
				if (EditorGUI.EndChangeCheck())
					_target.CollectMyBounds();
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				var includeChildren = serializedObject.FindProperty("includeChildren");
				EditorGUILayout.PropertyField(includeChildren);
				if (EditorGUI.EndChangeCheck())
					_target.CollectMyBounds();
			}

			EditorGUILayout.Space();

			var wrlBndsGrp = serializedObject.FindProperty("worldBoundsGrp");

			int holdval = wrlBndsGrp.intValue;
			EditorGUILayout.PropertyField(wrlBndsGrp);
			serializedObject.ApplyModifiedProperties();
			// If the bounds have changed, we need to recalculate the WorldBoundsGroups to make sure this is factored into the right one.
			if (wrlBndsGrp.intValue != holdval)
			{
				_target.CollectMyBounds();
			}


			EditorGUILayout.Space();
			
		}

	}

#endif
}

