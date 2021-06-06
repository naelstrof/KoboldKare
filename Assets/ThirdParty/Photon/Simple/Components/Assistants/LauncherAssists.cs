// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

#if PUN_2_OR_NEWER
using Photon.Pun;
using Photon.Pun.UtilityScripts;
#endif

using UnityEditor;
using UnityEngine;

namespace Photon.Pun.Simple.Assists
{
	public static class LauncherAssists
	{

		[MenuItem(AssistHelpers.ADD_TO_SCENE_TXT + "Auto Room Launchers", false, AssistHelpers.PRIORITY)]
        public static void AddLaunchersToScene()
		{
#if PUN_2_OR_NEWER

			var connAndJoin = Object.FindObjectOfType<ConnectAndJoinRandom>();
			var onJoined = Object.FindObjectOfType<OnJoinedInstantiate>();

			/// If one of the components exists, use the gameobj that is on. If neither exists, make a new gameobject.
			GameObject go;
			if (!connAndJoin && !onJoined)
				go = new GameObject("PUN2 Launchers");
			else
				go = connAndJoin ? connAndJoin.gameObject : onJoined.gameObject;

			/// Create the missing components (if any)
			if (connAndJoin == null)
				connAndJoin = go.AddComponent<ConnectAndJoinRandom>();

			if (onJoined == null)
				onJoined = go.AddComponent<OnJoinedInstantiate>();

			/// Get the current selection
			var selection = Selection.activeGameObject;

			/// If a gameobj was selected, see if it can be added to the prefab list
			if (selection)
				if (onJoined.AddPrefabToList(selection))
					Debug.Log("Automatically Adding '" + selection.name + "' as PlayerPrefab for " + connAndJoin.GetType().Name + " because it was selected.");
				else
					Debug.Log("Selection is was not a valid prefab or already exists in the prefab list, and was not added.");
			/// change selection to GameObject of the Launchers
			Selection.activeGameObject = onJoined.gameObject;
#endif
		}

	}
}

#endif
