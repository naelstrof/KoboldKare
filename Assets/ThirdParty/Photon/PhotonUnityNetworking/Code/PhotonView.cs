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
        [FormerlySerializedAs("ownershipTransfer")]
        public OwnershipOption OwnershipTransfer = OwnershipOption.Fixed;
        [SerializeField] [FormerlySerializedAs("viewIdField")]
        public int sceneViewId = 0;
        
        private static List<PhotonView> photonViews = new ();

        public enum ObservableSearch {
            Manual,
            AutoFindActive,
            AutoFindAll
        }
        public ObservableSearch observableSearch = ObservableSearch.Manual;
        public List<Component> ObservedComponents;

        public static event System.Action<PhotonView> OnPhotonViewAdd;
        public static event System.Action<PhotonView> OnPhotonViewRemove;

        public static bool TryFind(out PhotonView view, int sceneViewID) {
            foreach (var v in photonViews) {
                if (v.sceneViewId == sceneViewID) {
                    view = v;
                    return true;
                }
            }

            foreach (var v in FindObjectsByType<PhotonView>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                if (v.sceneViewId == sceneViewID) {
                    view = v;
                    return true;
                }
            }

            view = null;
            return false;
        }


        private bool tracked = false;
        void Start() {
            tracked = true;
            OnPhotonViewAdd?.Invoke(this);
            photonViews.Add(this);
        }

        void OnDestroy() {
            if (!tracked) {
                return;
            }
            OnPhotonViewRemove?.Invoke(this);
            photonViews.Remove(this);
        }
    }
}