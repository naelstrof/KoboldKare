// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Pun.Simple.Internal;
using System.Collections.Generic;

namespace Photon.Pun.Simple
{
    public static class NetMasterCallbacks
    {

        #region Outgoing Callbacks

        public static List<IOnTickPreSerialization> onTickPreSerializations = new List<IOnTickPreSerialization>();

        public static List<IOnPreUpdate> onPreUpdates = new List<IOnPreUpdate>();
        public static List<IOnPostUpdate> onPostUpdates = new List<IOnPostUpdate>();

        public static List<IOnPreLateUpdate> onPreLateUpdates = new List<IOnPreLateUpdate>();
        public static List<IOnPostLateUpdate> onPostLateUpdates = new List<IOnPostLateUpdate>();

        public static List<IOnIncrementFrame> onIncrementFrames = new List<IOnIncrementFrame>();
        //public static List<IOnCaptureInputs> onCaptureInputs = new List<IOnCaptureInputs>();

        public static List<IOnPreSimulate> onPreSimulates = new List<IOnPreSimulate>();
        public static List<IOnPostSimulate> onPostSimulates = new List<IOnPostSimulate>();

        public static List<IOnTickSnapshot> onSnapshots = new List<IOnTickSnapshot>();
        public static List<IOnInterpolate> onInterpolates = new List<IOnInterpolate>();

        public static List<IOnPreQuit> onPreQuits = new List<IOnPreQuit>();

        /// <summary>
        /// Add delegates here that should be executed after the current callback timing segment has run. This is for deferring actions that would change the callback list.
        /// </summary>
        public static Queue<System.Action> postCallbackActions = new Queue<System.Action>();
        /// <summary>
        /// Add delegates here that should be executed after the PostSimulation timing has run.
        /// </summary>
        public static Queue<System.Action> postSimulateActions = new Queue<System.Action>();

        public static Queue<System.Action> postSerializationActions = new Queue<System.Action>();

        public struct DelayedRegistrationItem
        {
            public object comp;
            public bool register;
            public DelayedRegistrationItem(object comp, bool register) { this.comp = comp; this.register = register; }
        }
        public static Queue<DelayedRegistrationItem> pendingRegistrations = new Queue<DelayedRegistrationItem>();
        /// <summary>
        /// Find callback interfaces used by NetMaster in this class, and add/remove them from the callback lists.
        /// </summary>
        public static void RegisterCallbackInterfaces(object comp, bool register = true, bool delay = false)
        {

            if (delay || callbacksLocked)
            {
                pendingRegistrations.Enqueue(new DelayedRegistrationItem(comp, register));
                return;
            }
            //if ((comp as Object) && register == false)
            //	Debug.LogError((comp as Object).name + "  UNREGISTER");

            CallbackUtilities.RegisterInterface(onPreUpdates, comp, register);
            CallbackUtilities.RegisterInterface(onPostUpdates, comp, register);

            CallbackUtilities.RegisterInterface(onPreLateUpdates, comp, register);
            CallbackUtilities.RegisterInterface(onPostLateUpdates, comp, register);

            CallbackUtilities.RegisterInterface(onIncrementFrames, comp, register);

            CallbackUtilities.RegisterInterface(onPreSimulates, comp, register);
            CallbackUtilities.RegisterInterface(onPostSimulates, comp, register);

            CallbackUtilities.RegisterInterface(onSnapshots, comp, register);
            CallbackUtilities.RegisterInterface(onInterpolates, comp, register);
            CallbackUtilities.RegisterInterface(onPreQuits, comp, register);
        }

        #endregion

        private static bool callbacksLocked;

        public static bool CallbacksLocked
        {
            set
            {
                callbacksLocked = value;

                if (!value)
                {
                    /// Run any delayed registration tasks.
                    while (pendingRegistrations.Count > 0)
                    {
                        var que = pendingRegistrations.Dequeue();
                        RegisterCallbackInterfaces(que.comp, que.register, false);
                    }

                    while (postCallbackActions.Count > 0)
                    {
                        postCallbackActions.Dequeue().Invoke();
                    }
                }
            }
        }

        public static void OnPreQuitCallbacks()
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPreQuits.Count; i < cnt; i++)
                onPreQuits[i].OnPreQuit();
            CallbacksLocked = false;
        }

        public static void OnPreUpdateCallbacks()
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPreUpdates.Count; i < cnt; ++i)
                onPreUpdates[i].OnPreUpdate();
            CallbacksLocked = false;
        }

        public static void OnInterpolateCallbacks(int _prevFrameId, int _currFrameId, float t)
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onInterpolates.Count; i < cnt; ++i)
                onInterpolates[i].OnInterpolate(_prevFrameId, _currFrameId, t);
            CallbacksLocked = false;
        }

        public static void OnPreLateUpdateCallbacks()
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPreLateUpdates.Count; i < cnt; ++i)
                onPreLateUpdates[i].OnPreLateUpdate();
            CallbacksLocked = false;

        }

        public static void OnPostSimulateCallbacks(int _currFrameId, int _currSubFrameId, bool isNetTick)
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPostSimulates.Count; i < cnt; ++i)
                onPostSimulates[i].OnPostSimulate(_currFrameId, _currSubFrameId, isNetTick);
            CallbacksLocked = false;
        }

        public static void OnIncrementFrameCallbacks(int _currFrameId, int _currSubFrameId, int _prevFrameId, int _prevSubFrameId)
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onIncrementFrames.Count; i < cnt; ++i)
                onIncrementFrames[i].OnIncrementFrame(_currFrameId, _currSubFrameId, _prevFrameId, _prevSubFrameId);
            CallbacksLocked = false;
        }

        public static void OnSnapshotCallbacks(int _currFrameId)
        {
            // Snapshot Others
            CallbacksLocked = true;
            for (int i = 0, cnt = onSnapshots.Count; i < cnt; ++i)
                onSnapshots[i].OnSnapshot(_currFrameId);
            CallbacksLocked = false;

            while (postSimulateActions.Count > 0)
            {
                var action = postSimulateActions.Dequeue();
                action.Invoke();
            }
        }

        public static void OnPreSerializeTickCallbacks(int _currFrameId, byte[] buffer, ref int bitposition)
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onTickPreSerializations.Count; i < cnt; ++i)
                onTickPreSerializations[i].OnPreSerializeTick(_currFrameId, buffer, ref bitposition);
            CallbacksLocked = false;
        }

        //public static void OnPreDeserializeTickCallbacks(int _currFrameId, byte[] buffer, ref int bitposition)
        //{
        //    CallbacksLocked = true;
        //    for (int i = 0, cnt = onTickPreSerializations.Count; i < cnt; ++i)
        //        onTickPreSerializations[i].OnPreDeserializeTick(_currFrameId, buffer, ref bitposition);
        //    CallbacksLocked = false;
        //}

        // LATE

        public static void OnPreSimulateCallbacks(int currentFrameId, int currentSubFrameId)
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPreSimulates.Count; i < cnt; ++i)
            {
                var cb = onPreSimulates[i];
                var b = cb as UnityEngine.Behaviour;
                if (!b || (b.isActiveAndEnabled && b.gameObject.activeInHierarchy))
                    cb.OnPreSimulate(currentFrameId, currentSubFrameId);
            }
            CallbacksLocked = false;
        }
        
        public static void OnPostUpdateCallbacks()
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPostUpdates.Count; i < cnt; ++i)
            {
                var cb = onPostUpdates[i];
                var b = cb as UnityEngine.Behaviour;
                if (!b || (b.isActiveAndEnabled && b.gameObject.activeInHierarchy))
                   cb.OnPostUpdate();
            }
            CallbacksLocked = false;
        }

        public static void OnPostLateUpdateCallbacks()
        {
            CallbacksLocked = true;
            for (int i = 0, cnt = onPostLateUpdates.Count; i < cnt; ++i)
            {
                var cb = onPostLateUpdates[i];
                var b = cb as UnityEngine.Behaviour;
                if (!b || (b.isActiveAndEnabled && b.gameObject.activeInHierarchy))
                    cb.OnPostLateUpdate();
            }
            CallbacksLocked = false;
        }
    }

}
