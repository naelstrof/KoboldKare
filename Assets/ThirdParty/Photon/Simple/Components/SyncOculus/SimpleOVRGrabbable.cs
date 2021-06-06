// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if OCULUS

using UnityEngine;

namespace Photon.Pun.Simple
{
    public class SimpleOVRGrabbable : OVRGrabbable
    {
        public SyncState syncState;

        protected override void Start()
        {
            base.Start();
            syncState = transform.GetComponentInParent<SyncState>();
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);

            if (syncState)
            {
                var mount = hand.GetComponent<Mount>();
                if (mount)
                {
                    syncState.HardMount(mount);
                }
            }
        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);
            if (syncState)
                syncState.HardMount(null);
        }
    }
}

#endif