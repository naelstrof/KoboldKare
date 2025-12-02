// ----------------------------------------------------------------------------
// <copyright file="PhotonView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Contains the PhotonView class.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun {
    using UnityEngine;
    using UnityEngine.Serialization;
    using System.Collections.Generic;
    public class PhotonView : MonoBehaviour {
        public byte Group = 0;
        public int prefixField = -1;

        [FormerlySerializedAs("synchronization")]
        public ViewSynchronization Synchronization = ViewSynchronization.UnreliableOnChange;

        protected internal bool mixedModeIsReliable = false;

        /// <summary>Defines if ownership of this PhotonView is fixed, can be requested or simply taken.</summary>
        /// <remarks>
        /// Note that you can't edit this value at runtime.
        /// The options are described in enum OwnershipOption.
        /// The current owner has to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.
        /// </remarks>
        [FormerlySerializedAs("ownershipTransfer")]
        public OwnershipOption OwnershipTransfer = OwnershipOption.Fixed;


        public enum ObservableSearch {
            Manual,
            AutoFindActive,
            AutoFindAll
        }

        /// Default to manual so existing PVs in projects default to same as before. Reset() changes this to AutoAll for new implementations.
        public ObservableSearch observableSearch = ObservableSearch.Manual;

        public List<Component> ObservedComponents;
    }
}