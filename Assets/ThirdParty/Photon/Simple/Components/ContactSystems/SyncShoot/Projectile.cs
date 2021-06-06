// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Utilities;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif


//namespace Photon.Pun.Simple
//{
//    public class Projectile : MonoBehaviour
//        , IProjectile
//        , IOnPreSimulate
//        //, IOnInterpolate
//        , IOnContactEvent
//        , IOnPreUpdate
//    {
//        protected IProjectileCannon owner;
//        public IProjectileCannon Owner { get { return owner; } set { owner = value; } }

//        [System.NonSerialized] public Vector3 velocity;
//        //[System.NonSerialized] public Vector3 angVelocity;

//        [System.NonSerialized] public int frameId;
//        [System.NonSerialized] public int subFrameId;

//        #region Inspector

//        [SerializeField]
//        [EnumMask(true)]
//        protected ContactType triggerOn = ContactType.Enter;

//        [SerializeField] [EnumMask] protected RespondTo terminateOn = RespondTo.IContactTrigger | RespondTo.HitNetObj | RespondTo.HitNonNetObj;
//        [SerializeField] [EnumMask] protected RespondTo damageOn = RespondTo.IContactTrigger | RespondTo.HitNetObj | RespondTo.HitNonNetObj;
//        [SerializeField] protected bool ignoreOwner;

//        #endregion

//        // Cache
//        protected Rigidbody rb;
//        protected Rigidbody2D rb2d;
//        protected bool _hasRigidBody;
//        public bool HasRigidbody { get { return _hasRigidBody; } }
//        protected bool needsSnapshot;
//        //protected IContactTrigger ownerContactTrigger;
//        protected IContactTrigger localContactTrigger;
//        protected bool useRbForces, useRb2dForces;

//        public VitalNameType VitalNameType { get { return new VitalNameType(VitalType.None); } }

//        /// Hit callbacks
//        public List<IOnNetworkHit> onHit = new List<IOnNetworkHit>();
//        public List<IOnTerminate> onTerminate = new List<IOnTerminate>();

//        private void Reset()
//        {
//            /// Projectiles need to detect collisions with everything
//            localContactTrigger = GetComponent<IContactTrigger>();
//            if (ReferenceEquals(localContactTrigger, null))
//                localContactTrigger = gameObject.AddComponent<ContactTrigger>();

//        }

//        private void Awake()
//        {
//            rb = GetComponentInParent<Rigidbody>();
//            rb2d = GetComponentInParent<Rigidbody2D>();
//            _hasRigidBody = rb || rb2d;
//            useRbForces = rb && !rb.isKinematic;
//            useRb2dForces = rb2d && !rb2d.isKinematic;

//            needsSnapshot = !_hasRigidBody || (rb && rb.isKinematic) || (rb2d && rb2d.isKinematic);
//            localContactTrigger = GetComponent<IContactTrigger>();
//            /// Register timing callbacks with Master. 
//            /// TODO: We likely should slave timings off of the owner
//            if (needsSnapshot)
//                NetMasterCallbacks.RegisterCallbackInterfaces(this);

//            /// No need for the interpolation callback if we are using forces.
//            if (_hasRigidBody)
//                NetMasterCallbacks.onPreUpdates.Remove(this);

//            /// Find interfaces for termination callbacks
//            GetComponents(onHit);
//            GetComponents(onTerminate);
//        }

//        private void OnDestroy()
//        {
//            NetMasterCallbacks.RegisterCallbackInterfaces(this, false, true);
//        }

//        /// <summary>
//        /// Override this method to implement more advanced initial lag compensated snap and velocity values. 
//        /// Advance the position/rotation of the projectile here, and set the velocity to a value it should be at AFTER the timeshift is applied, so that it accounts for gravity and drag.
//        /// </summary>
//        /// <param name="timeshift"></param>
//        public virtual void LagCompensate(float timeshift)
//        {
//            snapPos = transform.position + velocity * timeshift;

//            velocity += Physics.gravity * timeshift;
//        }

//        public void Initialize(IProjectileCannon owner, int frameId, int subFrameId, Vector3 localVelocity, RespondTo terminateOn, RespondTo damageOn, float timeshift = 0)
//        {
//            this.owner = owner;

//            // Convert velocity from local to global
//            this.velocity = transform.TransformDirection(localVelocity);
//            this.terminateOn = terminateOn;
//            this.damageOn = damageOn;
//            this.frameId = frameId;
//            this.subFrameId = subFrameId;

//            if (timeshift != 0)
//                LagCompensate(timeshift);
//            else
//                snapPos = transform.position;

//            if (useRbForces)
//            {
//                //rb.position = transform.position;
//                rb.MovePosition(snapPos);
//                rb.velocity = this.velocity;
//            }
//            /// TODO: NOT TESTED
//            else if (rb2d)
//            {
//                //rb2d.position = transform.position;
//                rb2d.MovePosition(snapPos);
//                rb2d.velocity = this.velocity;
//            }
//            else
//            {
//                transform.position = snapPos;
//                targPos = snapPos + velocity * Time.fixedDeltaTime;
//                transform.position = snapPos;
//            }

//            localContactTrigger.Proxy = owner.ContactTrigger;
//        }

//        public bool OnContactEvent(ref ContactEvent contactEvent)
//        {
//            var contactType = contactEvent.contactType;

//            if (triggerOn != 0 && (triggerOn & contactType) == 0)
//                return false;

//            if (ignoreOwner && !ReferenceEquals(contactEvent.contactSystem, null) && contactEvent.contactSystem.ViewID == owner.ViewID)
//                return false;

//            OnHit(contactEvent);
//            return true;

//        }


//        Vector3 snapPos, targPos;
//        Quaternion snapRot, targRot;

//        /// Pre Fixed
//        public void OnPreSimulate(int frameId, int subFrameId)
//        {
//            if (!useRbForces && !useRb2dForces)
//                SimulateTime(Time.fixedDeltaTime);
//        }

//        //public void InitialSimulate(float t)
//        //{
//        //    if (useRbForces)
//        //    {
//        //        rb.MovePosition(rb.position + rb.rotation * velocity * t);
//        //        if (rb.useGravity)
//        //            velocity += Physics.gravity * t;
//        //    }
//        //    /// UNTESTED
//        //    else if (useRb2dForces)
//        //    {
//        //        rb2d.MovePosition(rb2d.position + (Vector2)transform.TransformVector(velocity));
//        //        if (rb2d.gravityScale != 0)
//        //            velocity += Physics.gravity * rb2d.gravityScale * t;
//        //    }
//        //    else
//        //    {
//        //        targ = snap + transform.rotation * velocity * t;

//        //        velocity += Physics.gravity * t;
//        //        Interpolate(0);
//        //    }
//        //}

//        public virtual void SimulateTime(float t)
//        {
//            var oldtarget = targPos;
//            targPos = oldtarget + velocity * t;
//            snapPos = oldtarget;

//            if (useRb2dForces)
//                velocity += Physics.gravity * rb2d.gravityScale * t;
//            else
//                velocity += Physics.gravity * t;

//            Interpolate(0);
//        }

//        /// Interpolation
//        public virtual void OnPreUpdate()
//        {
//            if (!useRbForces && !useRb2dForces)
//                Interpolate(NetMaster.NormTimeSinceFixed);
//        }

//        protected void Interpolate(float t)
//        {
//            transform.position = Vector3.Lerp(snapPos, targPos, t);
//        }

//        #region Hit Triggers

//        protected static List<NetObject> reusableNetObjects = new List<NetObject>();

//        protected virtual void OnHit(ContactEvent contactEvent)
//        {

//            Debug.Log("OnHit");
//            List<NetObject> netObjects = reusableNetObjects;
//            /// Collect the nested parent NetObjs for correct self-hit testing. The first one returned is actual object that was hit.

//            var other = contactEvent.contacter;

//#if PUN_2_OR_NEWER
//            other.NetObj.transform.GetNestedComponentsInParents(netObjects);
//#endif
//            NetObject netObj = netObjects.Count == 0 ? null : netObjects[0];

//            /// Hit was not NetOBj
//            if (ReferenceEquals(netObj, null))
//            {
//                bool terminateOnNonNetObj = (terminateOn & RespondTo.HitNonNetObj) != 0;

//                if (terminateOnNonNetObj)
//                    Terminate();
//            }
//            /// Hit was NetObj
//            else
//            {
//                int netObjId = netObj.ViewID;

//                /// Test all nested NetObjs for a match with self
//                bool hitWasOwner = false;
//                var ownerNetObj = owner.NetObj;
//                foreach (var netobj in netObjects)
//                    if (ReferenceEquals(netobj, ownerNetObj))
//                    {
//                        hitWasOwner = true;
//                        break;
//                    }

//                /// Hit was self
//                if (hitWasOwner)
//                {
//                    if ((terminateOn & RespondTo.HitSelf) != 0)
//                    {
//                        Debug.LogError("Terminate On Self");
//                        Terminate();
//                    }

//                    if ((damageOn & RespondTo.HitSelf) == 0)
//                        return;
//                }

//                /// TODO: this collider ID code is pretty hacked together. Need to decide if syncing the collider ID
//                /// should even happen, and if so how colliders are determined for some events like triggers.
//                bool isCollider = ((other is Collider) || (other is Collider2D));
//                int colliderId = (isCollider) ? netObj.colliderLookup[other] : 0;

//                /// Hit was NetObj
//                var contactGroup = other.GetComponent<IContactGroupAssign>();

//                int mask = ReferenceEquals(contactGroup, null) ? 0 : contactGroup.Mask;

//                ///// TEST
//                //if (true)
//                //{
//                //    localContactTrigger.Proxy.Trigger(contactEvent);
//                //}
//                //else

//                /// If this connection owns this launcher/projectile, log this hit
//                owner.QueueHit(new NetworkHit(netObjId, mask, colliderId));

//                if ((terminateOn & RespondTo.HitNetObj) != 0)
//                    Terminate();
//            }
//        }

//        /// <summary>
//        /// Override this and extend the base with your own projectile termination code, or place a component on this GameObject with IOnTerminate to define a response.
//        /// </summary>
//        protected virtual void Terminate()
//        {
//            for (int i = 0, cnt = onTerminate.Count; i < cnt; ++i)
//                onTerminate[i].OnTerminate();

//            gameObject.SetActive(false);
//        }

//        #endregion
//    }

//#if UNITY_EDITOR

//    [CustomEditor(typeof(Projectile), true)]
//    [CanEditMultipleObjects]
//    public class ProjectileEditor : ReactorHeaderEditor
//    {
//        protected override string Instructions
//        {
//            get
//            {
//                return "Defines a projectile that can be used by " + typeof(SyncCannon).Name + ".";
//            }
//        }

//        //protected override void OnInspectorGUIInjectMiddle()
//        //{
//        //	base.OnInspectorGUIInjectMiddle();
//        //	EditorGUILayout.LabelField("<b>OnTriggerEvent()</b>\n{\n  Terminate()\n  owner.QueueHit() \n}", richBox);
//        //}
//    }
//#endif
//}
