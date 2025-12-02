// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbody2DView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize 2d rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using UnityEngine;

    public class PhotonRigidbody2DView : MonoBehaviour {
        [HideInInspector]
        public bool m_SynchronizeVelocity = true;
        [HideInInspector]
        public bool m_SynchronizeAngularVelocity = false;

        [HideInInspector]
        public bool m_TeleportEnabled = false;
        [HideInInspector]
        public float m_TeleportIfDistanceGreaterThan = 3.0f;
    }
}