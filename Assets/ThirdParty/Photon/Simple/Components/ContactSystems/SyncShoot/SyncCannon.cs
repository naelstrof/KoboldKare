// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Simple.Pooling;
using Photon.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    public class SyncCannon : SyncShootBase
        , IProjectileCannon
        //, IOnContactEvent
    {
        public override int ApplyOrder { get { return ApplyOrderConstants.WEAPONS; } }

        #region Inspector Items

        [SerializeField] public GameObject projPrefab;
        [SerializeField] public Vector3 velocity = new Vector3(0, 0, 10f);

        [SerializeField] [EnumMask] public RespondTo terminateOn = RespondTo.HitNetObj | RespondTo.HitNonNetObj;
        [SerializeField] [EnumMask] public RespondTo damageOn = RespondTo.HitNetObj | RespondTo.HitNonNetObj;

        [Tooltip("Projectiles are advanced (lagCompensate * RTT) ms into the future on non-owner clients. This will better time align projectiles to the local players time frame " +
            "(For example dodging a projectile locally is more likely to be how the shooter saw events as well). 0 = Fully in shooters time frame and 1 = Fully in the local players time frame.")]
        [Range(0f, 1f)]
        [SerializeField] public float lagCompensate = 1f;

        #endregion


        //protected override void PopulateFrames()
        //{
        //    int frameCount = TickEngineSettings.frameCount;

        //    frames = new NetHitFrame[frameCount + 1];
        //    for (int i = 0; i <= frameCount; ++i)
        //    {
        //        frames[i] = new NetHitFrame(this, false, i);

        //    }
        //}

        #region Initialization

        protected override void Reset()
        {
            base.Reset();
        }

        public override void OnAwake()
        {
            base.OnAwake();

            /// If no prefab was designated, we need a dummy
            if (projPrefab == null)
            {
                projPrefab = ProjectileHelpers.GetPlaceholderProj();
                Pool.AddPrefabToPool(projPrefab, 8, 8, null, true);
            }
            else
                Pool.AddPrefabToPool(projPrefab);
        }

        #endregion

        protected static List<NetObject> reusableNetObjects = new List<NetObject>();

        //public bool OnContactEvent(ref ContactEvent contactEvent)
        //{
        //    //List<NetObject> netObjects = reusableNetObjects;
        //    ///// Collect the nested parent NetObjs for correct self-hit testing. The first one returned is actual object that was hit.

        //    //var other = contactEvent.contacter;

        //    //other.NetObj.transform.GetNestedComponentsInParents(netObjects);

        //    //NetObject netObj = netObjects.Count == 0 ? null : netObjects[0];

        //    ///// Hit was not NetOBj
        //    //if (ReferenceEquals(netObj, null))
        //    //{
        //    //    return false;
        //    //}
        //    ///// Hit was NetObj
        //    //else
        //    //{
        //    //    int netObjId = netObj.ViewID;

        //    //    /// Test all nested NetObjs for a match with self
        //    //    bool hitWasOwner = false;
        //    //    var ownerNetObj = NetObj;
        //    //    foreach (var netobj in netObjects)
        //    //        if (ReferenceEquals(netobj, ownerNetObj))
        //    //        {
        //    //            hitWasOwner = true;
        //    //            break;
        //    //        }

        //    //    /// Hit was self
        //    //    if (hitWasOwner)
        //    //    {
        //    //        return false;
        //    //    }

        //    //    ///// TODO: this collider ID code is pretty hacked together. Need to decide if syncing the collider ID
        //    //    ///// should even happen, and if so how colliders are determined for some events like triggers.
        //    //    //bool isCollider = ((other is Collider) || (other is Collider2D));
        //    //    //int colliderId = (isCollider) ? netObj.colliderLookup[other] : 0;

        //    //    /// Hit was NetObj
        //    //    var contactGroup = (other as Component).GetComponent<IContactGroupAssign>();

        //    //    int mask = ReferenceEquals(contactGroup, null) ? 0 : contactGroup.Mask;

        //    //    Debug.Log("OnHit " + contactEvent.contactType);

        //    //    /// If this connection owns this launcher/projectile, log this hit
        //    //    QueueHit(new NetworkHit(netObjId, mask, colliderId));

        //    //    return true;
        //    //}
        //    return false;
        //}

        ///// <summary>
        ///// Projectiles report hits back to the launcher, and they are queued for networking and local simulation.
        ///// </summary>
        ///// <param name="hit"></param>
        //public void QueueHit(NetworkHit hit)
        //{

        //    if (IsMine)
        //    {
        //        projectileHitQueue.Enqueue(hit);
        //    }
        //}

        /// <summary>
        /// Create the projectile instance
        /// </summary>
        protected override bool Trigger(Frame frame, int subFrameId, float timeshift = 0)
        {
            Pool p = Pool.Spawn(projPrefab, origin);

            var proxy = p.GetComponent<IContactTrigger>();
            proxy.Proxy = contactTrigger;

            var ssproj = p.GetComponent<IProjectile>();
            ssproj.Initialize(this, frame.frameId, subFrameId, velocity, terminateOn, damageOn, lagCompensate * timeshift);
            ssproj.Owner = this;

            return true;
        }

        #region Timings

        //public override void OnPostSimulate(int frameId, int subFrameId, bool isNetTick)
        //{
        //    /// Base handles the fire triggers. We have special handling for hits, since they come in from the projectiles and are not instant
        //    base.OnPostSimulate(frameId, subFrameId, isNetTick);

        //    NetHitFrame frame = frames[frameId];

        //    /// Owner log hits to frame/subframe
        //    if (projectileHitQueue.Count > 0)
        //    {
        //        while (projectileHitQueue.Count > 0)
        //            frame.netHits[subFrameId].hits.Add(projectileHitQueue.Dequeue());

        //        frame.content = FrameContents.Complete;
        //        frame.hitmask |= ((uint)1 << subFrameId);

        //        HitsCallbacks(frame.netHits[subFrameId]);
        //    }
        //}

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncCannon), true)]
    [CanEditMultipleObjects]
    public class SyncCannonEditor : SyncShootBaseEditor
    {
        protected override string Instructions
        {
            get
            {
                return "Attach this component to any root or child GameObject to define a networked projectile launcher. " +
                    "A NetObject is required on this object or a parent.\n\n" +
                    "Initiate a projectile by calling:\n" +
                    "this" + typeof(SyncCannon).Name + ".QueueTrigger()";
            }
        }

        protected override string HelpURL
        {
            get { return Internal.SimpleDocsURLS.SUBSYS_PATH + "#synccannon_component"; }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/SyncCannonText";
            }
        }
    }
#endif
}

