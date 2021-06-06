// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Photon.Pun.UtilityScripts
{


#if UNITY_EDITOR

    /// <summary>
    /// Storage type for AnimatorController cached transition data, which is a bit different than basic state hashes
    /// </summary>
    [System.Serializable]
    public class TransitionInfo
    {
        public int index;
        public int hash;
        public int state;
        public int destination;
        public float duration;
        public float offset;
        public bool durationIsFixed;

        public TransitionInfo(int index, int hash, int state, int destination, float duration, float offset, bool durationIsFixed)
        {
            this.index = index;
            this.hash = hash;
            this.state = state;
            this.destination = destination;
            this.duration = duration;
            this.offset = offset;
            this.durationIsFixed = durationIsFixed;
        }
    }

    public static class AnimatorControllerTools
	{
		public static AnimatorController GetController(this Animator a)
		{
			RuntimeAnimatorController rac = a.runtimeAnimatorController;
			AnimatorOverrideController overrideController = rac as AnimatorOverrideController;

			/// recurse until no override controller is found
			while (overrideController != null)
			{
				rac = overrideController.runtimeAnimatorController;
				overrideController = rac as AnimatorOverrideController;
			}

			return rac as AnimatorController;
		}

		public static void GetTriggerNames(this AnimatorController ctr, List<string> namelist)
		{
			namelist.Clear();

			foreach (var p in ctr.parameters)
				if (p.type == AnimatorControllerParameterType.Trigger)
				{
					if (namelist.Contains(p.name))
					{
						Debug.LogWarning("Idential Trigger Name Found.  Check animator on '" + ctr.name + "' for repeated trigger names.");
					}
					else
						namelist.Add(p.name);
				}
		}

		public static void GetTriggerNames(this AnimatorController ctr, List<int> hashlist)
		{
			hashlist.Clear();

			foreach (var p in ctr.parameters)
				if (p.type == AnimatorControllerParameterType.Trigger)
				{
					hashlist.Add(Animator.StringToHash(p.name));
				}
		}

		/// ------------------------------ STATES --------------------------------------

		public static void GetStatesNames(this AnimatorController ctr, List<string> namelist)
		{
			namelist.Clear();

			foreach (var l in ctr.layers)
			{
				var states = l.stateMachine.states;
				ExtractNames(ctr, l.name, states, namelist);

				var substates = l.stateMachine.stateMachines;
				ExtractSubNames(ctr, l.name, substates, namelist);
			}
		}

		public static void ExtractSubNames(AnimatorController ctr, string path, ChildAnimatorStateMachine[] substates, List<string> namelist)
		{
			foreach (var s in substates)
			{
				var sm = s.stateMachine;
				var subpath = path + "." + sm.name;

				ExtractNames(ctr, subpath, s.stateMachine.states, namelist);
				ExtractSubNames(ctr, subpath, s.stateMachine.stateMachines, namelist);
			}
		}

		public static void ExtractNames(AnimatorController ctr, string path, ChildAnimatorState[] states, List<string> namelist)
		{
			foreach (var st in states)
			{
				string name = (st.state.name);
				string layerName = (path + "." + st.state.name);
				if (!namelist.Contains(name))
				{
					namelist.Add(name);
				}
				if (namelist.Contains(layerName))
				{
					Debug.LogWarning("Idential State Name <i>'" + st.state.name + "'</i> Found.  Check animator on '" + ctr.name + "' for repeated State names as they cannot be used nor networked.");
				}
				else
					namelist.Add((path + "." + st.state.name));
			}

		}

		public static void GetStatesNames(this AnimatorController ctr, List<int> hashlist)
		{
			hashlist.Clear();

			foreach (var l in ctr.layers)
			{
				var states = l.stateMachine.states;
				ExtractHashes(ctr, l.name, states, hashlist);

				var substates = l.stateMachine.stateMachines;
				ExtractSubtHashes(ctr, l.name, substates, hashlist);
			}

		}

		public static void ExtractSubtHashes(AnimatorController ctr, string path, ChildAnimatorStateMachine[] substates, List<int> hashlist)
		{
			foreach (var s in substates)
			{
				var sm = s.stateMachine;
				var subpath = path + "." + sm.name;

				ExtractHashes(ctr, subpath, sm.states, hashlist);
				ExtractSubtHashes(ctr, subpath, sm.stateMachines, hashlist);
			}
		}

		public static void ExtractHashes(AnimatorController ctr, string path, ChildAnimatorState[] states, List<int> hashlist)
		{
			foreach (var st in states)
			{
				int hash = Animator.StringToHash(st.state.name);
				int layrhash = Animator.StringToHash(path + "." + st.state.name);
				if (!hashlist.Contains(hash))
				{
					hashlist.Add(hash);
				}
				if (hashlist.Contains(layrhash))
				{
					Debug.LogWarning("Idential State Name <i>'" + st.state.name + "'</i> Found.  Check animator on '" + ctr.name + "' for repeated State names as they cannot be used nor networked.");
				}
				else
					hashlist.Add(Animator.StringToHash(path + "." + st.state.name));
			}
		}

		//public static void GetTransitionNames(this AnimatorController ctr, List<string> transInfo)
		//{
		//	transInfo.Clear();

		//	transInfo.Add("0");

		//	foreach (var l in ctr.layers)
		//	{
		//		foreach (var st in l.stateMachine.states)
		//		{
		//			string sname = l.name + "." + st.state.name;

		//			foreach (var t in st.state.transitions)
		//			{
		//				string dname = l.name + "." + t.destinationState.name;
		//				string name = (sname + " -> " + dname);
		//				transInfo.Add(name);
		//				//Debug.Log(sname + " -> " + dname + "   " + Animator.StringToHash(sname + " -> " + dname));
		//			}
		//		}
		//	}

		//}


		//public static void GetTransitions(this AnimatorController ctr, List<TransitionInfo> transInfo)
		//{
		//	transInfo.Clear();

		//	transInfo.Add(new TransitionInfo(0, 0, 0, 0, 0, 0, false));

		//	int index = 1;

		//	foreach (var l in ctr.layers)
		//	{
		//		foreach (var st in l.stateMachine.states)
		//		{
		//			string sname = l.name + "." + st.state.name;
		//			int shash = Animator.StringToHash(sname);

		//			foreach (var t in st.state.transitions)
		//			{
		//				string dname = l.name + "." + t.destinationState.name;
		//				int dhash = Animator.StringToHash(dname);
		//				int hash = Animator.StringToHash(sname + " -> " + dname);
		//				TransitionInfo ti = new TransitionInfo(index, hash, shash, dhash, t.duration, t.offset, t.hasFixedDuration);
		//				transInfo.Add(ti);
		//				//Debug.Log(index + " " + sname + " -> " + dname + "   " + Animator.StringToHash(sname + " -> " + dname));
		//				index++;
		//			}
		//		}
		//	}
		//}

	}
#endif

}

