// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple
{
    public class ContactProjectile : MonoBehaviour
        , IProjectile
        , IOnPreSimulate
        , IOnPreUpdate
    {
        protected IProjectileCannon owner;
        public IProjectileCannon Owner { get { return owner; } set { owner = value; } }

        [System.NonSerialized] public Vector3 velocity;

        [System.NonSerialized] public int frameId;
        [System.NonSerialized] public int subFrameId;

        #region Inspector

        [SerializeField] [EnumMask] protected RespondTo terminateOn = RespondTo.IContactTrigger | RespondTo.HitNetObj | RespondTo.HitNonNetObj;
        [SerializeField] [EnumMask] protected RespondTo damageOn = RespondTo.IContactTrigger | RespondTo.HitNetObj | RespondTo.HitNonNetObj;

        #endregion

        // Cache
        protected Rigidbody rb;
        protected Rigidbody2D rb2d;
        protected bool _hasRigidBody;
        public bool HasRigidbody { get { return _hasRigidBody; } }
        protected bool needsSnapshot;
        //protected IContactTrigger ownerContactTrigger;
        protected IContactTrigger localContactTrigger;
        protected bool useRbForces, useRb2dForces, useRBGravity;

        public VitalNameType VitalNameType { get { return new VitalNameType(VitalType.None); } }

        /// Hit callbacks
        public List<IOnNetworkHit> onHit = new List<IOnNetworkHit>();
        public List<IOnTerminate> onTerminate = new List<IOnTerminate>();

        private void Reset()
        {
            /// Projectiles need to detect collisions with everything
            localContactTrigger = GetComponent<IContactTrigger>();
            if (ReferenceEquals(localContactTrigger, null))
                localContactTrigger = gameObject.AddComponent<ContactTrigger>();

        }

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            rb2d = GetComponentInParent<Rigidbody2D>();
            _hasRigidBody = rb || rb2d;
            useRbForces = rb && !rb.isKinematic;
            useRb2dForces = rb2d && !rb2d.isKinematic;
            useRBGravity = (rb && rb.useGravity) || (rb2d && rb.useGravity);
            needsSnapshot = !_hasRigidBody || (rb && rb.isKinematic) || (rb2d && rb2d.isKinematic);
            localContactTrigger = GetComponent<IContactTrigger>();
            /// Register timing callbacks with Master. 
            /// TODO: We likely should slave timings off of the owner
            if (needsSnapshot)
                NetMasterCallbacks.RegisterCallbackInterfaces(this);

            /// No need for the interpolation callback if we are using forces.
            if (_hasRigidBody)
                NetMasterCallbacks.onPreUpdates.Remove(this);

            /// Find interfaces for termination callbacks
            GetComponents(onHit);
            GetComponents(onTerminate);
        }

        private void OnDestroy()
        {
            NetMasterCallbacks.RegisterCallbackInterfaces(this, false, true);
        }

        /// <summary>
        /// Override this method to implement more advanced initial lag compensated snap and velocity values. 
        /// Advance the position/rotation of the projectile here, and set the velocity to a value it should be at AFTER the timeshift is applied, so that it accounts for gravity and drag.
        /// </summary>
        /// <param name="timeshift"></param>
        public virtual void LagCompensate(float timeshift)
        {
            snapPos = transform.position + velocity * timeshift;

            if (rb && rb.useGravity)
                velocity += Physics.gravity * timeshift;
            else if (rb2d)
                velocity += Physics.gravity * timeshift;

        }

        public void Initialize(IProjectileCannon owner, int frameId, int subFrameId, Vector3 localVelocity, RespondTo terminateOn, RespondTo damageOn, float timeshift = 0)
        {
            this.owner = owner;

            // Convert velocity from local to global
            this.velocity = transform.TransformDirection(localVelocity);
            this.terminateOn = terminateOn;
            this.damageOn = damageOn;
            this.frameId = frameId;
            this.subFrameId = subFrameId;

            if (timeshift != 0)
                LagCompensate(timeshift);
            else
                snapPos = transform.position;

            if (useRbForces)
            {
                //rb.position = transform.position;
                rb.MovePosition(snapPos);
                rb.velocity = this.velocity;
            }
            /// TODO: NOT TESTED
            else if (rb2d)
            {
                //rb2d.position = transform.position;
                rb2d.MovePosition(snapPos);
                rb2d.velocity = this.velocity;
            }
            else
            {
                transform.position = snapPos;
                targPos = snapPos + velocity * Time.fixedDeltaTime;
                transform.position = snapPos;
            }

            localContactTrigger.Proxy = owner.ContactTrigger;
        }
       

        Vector3 snapPos, targPos;
        Quaternion snapRot, targRot;

        /// Pre-Fixed
        public void OnPreSimulate(int frameId, int subFrameId)
        {
            if (!useRbForces && !useRb2dForces)
                SimulateTime(Time.fixedDeltaTime);
        }

        public virtual void SimulateTime(float t)
        {
            var oldtarget = targPos;
            targPos = oldtarget + velocity * t;
            snapPos = oldtarget;

            if (useRb2dForces)
                velocity += Physics.gravity * rb2d.gravityScale * t;
            else
                velocity += Physics.gravity * t;

            Interpolate(0);
        }

        /// Interpolation
        public virtual void OnPreUpdate()
        {
            if (!useRbForces && !useRb2dForces)
                Interpolate(NetMaster.NormTimeSinceFixed);
        }

        protected void Interpolate(float t)
        {
            transform.position = Vector3.Lerp(snapPos, targPos, t);
        }

        #region Hit Triggers

        protected static List<NetObject> reusableNetObjects = new List<NetObject>();

        /// <summary>
        /// Override this and extend the base with your own projectile termination code, or place a component on this GameObject with IOnTerminate to define a response.
        /// </summary>
        protected virtual void Terminate()
        {
            Debug.Log("Terminate");
            for (int i = 0, cnt = onTerminate.Count; i < cnt; ++i)
                onTerminate[i].OnTerminate();

            gameObject.SetActive(false);
        }

        #endregion
    }
}
