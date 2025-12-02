// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformViewClassic.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun {
    using UnityEngine;
    using System.Collections.Generic;
    public class PhotonTransformViewClassic : MonoBehaviour {
        [HideInInspector]
        public PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();

        [HideInInspector]
        public PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();

        [HideInInspector]
        public PhotonTransformViewScaleModel m_ScaleModel = new PhotonTransformViewScaleModel();
    }


    [System.Serializable]
    public class PhotonTransformViewPositionModel {
        public enum InterpolateOptions
        {
            Disabled,
            FixedSpeed,
            EstimatedSpeed,
            SynchronizeValues,
            Lerp
        }


        public enum ExtrapolateOptions
        {
            Disabled,
            SynchronizeValues,
            EstimateSpeedAndTurn,
            FixedSpeed,
        }


        public bool SynchronizeEnabled;

        public bool TeleportEnabled = true;
        public float TeleportIfDistanceGreaterThan = 3f;

        public InterpolateOptions InterpolateOption = InterpolateOptions.EstimatedSpeed;
        public float InterpolateMoveTowardsSpeed = 1f;

        public float InterpolateLerpSpeed = 1f;

        public ExtrapolateOptions ExtrapolateOption = ExtrapolateOptions.Disabled;
        public float ExtrapolateSpeed = 1f;
        public bool ExtrapolateIncludingRoundTripTime = true;
        public int ExtrapolateNumberOfStoredPositions = 1;
    }

    public class PhotonTransformViewPositionControl {
        PhotonTransformViewPositionModel m_Model;
        float m_CurrentSpeed;
        double m_LastSerializeTime;
        Vector3 m_SynchronizedSpeed = Vector3.zero;
        float m_SynchronizedTurnSpeed = 0;

        Vector3 m_NetworkPosition;
        Queue<Vector3> m_OldNetworkPositions = new Queue<Vector3>();

        bool m_UpdatedPositionAfterOnSerialize = true;
    }


    [System.Serializable]
    public class PhotonTransformViewRotationModel
    {
        public enum InterpolateOptions
        {
            Disabled,
            RotateTowards,
            Lerp,
        }


        public bool SynchronizeEnabled;

        public InterpolateOptions InterpolateOption = InterpolateOptions.RotateTowards;
        public float InterpolateRotateTowardsSpeed = 180;
        public float InterpolateLerpSpeed = 5;
    }

    [System.Serializable]
    public class PhotonTransformViewScaleModel
    {
        public enum InterpolateOptions
        {
            Disabled,
            MoveTowards,
            Lerp,
        }


        public bool SynchronizeEnabled;

        public InterpolateOptions InterpolateOption = InterpolateOptions.Disabled;
        public float InterpolateMoveTowardsSpeed = 1f;
        public float InterpolateLerpSpeed;
    }
}