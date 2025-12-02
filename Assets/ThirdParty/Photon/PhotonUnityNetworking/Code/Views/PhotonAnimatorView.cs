// ----------------------------------------------------------------------------
// <copyright file="PhotonAnimatorView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Mecanim animations via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

namespace Photon.Pun {
    using System.Collections.Generic;
    using UnityEngine;
    public class PhotonAnimatorView : MonoBehaviour {
        #region Enums

        public enum ParameterType {
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9,
        }


        public enum SynchronizeType {
            Disabled = 0,
            Discrete = 1,
            Continuous = 2,
        }


        [System.Serializable]
        public class SynchronizedParameter {
            public ParameterType Type;
            public SynchronizeType SynchronizeType;
            public string Name;
        }


        [System.Serializable]
        public class SynchronizedLayer {
            public SynchronizeType SynchronizeType;
            public int LayerIndex;
        }

        #endregion


        #region Properties

        #if PHOTON_DEVELOP
        public PhotonAnimatorView ReceivingSender;
        #endif

        #endregion

        [HideInInspector]
        [SerializeField]
        private bool ShowLayerWeightsInspector = true;

        [HideInInspector]
        [SerializeField]
        private bool ShowParameterInspector = true;

        #pragma warning restore 0414

        [HideInInspector]
        [SerializeField]
        private List<SynchronizedParameter> m_SynchronizeParameters = new List<SynchronizedParameter>();

        [HideInInspector]
        [SerializeField]
        private List<SynchronizedLayer> m_SynchronizeLayers = new List<SynchronizedLayer>();
    }
}