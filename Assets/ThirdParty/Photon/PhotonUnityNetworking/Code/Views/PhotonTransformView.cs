// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using System.IO;
    using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Transform View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTransformView : MonoBehaviourPun, IPunObservable, ISavable {
        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;

        [Tooltip("Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
        public bool m_UseLocal;

        bool m_firstTake = false;

        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        private void Reset()
        {
            // Only default to true with new instances. useLocal will remain false for old projects that are updating PUN.
            m_UseLocal = true;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            var tr = transform;

            if (!this.photonView.IsMine)
            {
                if (m_UseLocal)

                {
                    tr.localPosition = Vector3.MoveTowards(tr.localPosition, this.m_NetworkPosition, this.m_Distance  * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                }
                else
                {
                    tr.position = Vector3.MoveTowards(tr.position, this.m_NetworkPosition, this.m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.rotation = Quaternion.RotateTowards(tr.rotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime *  PhotonNetwork.SerializationRate);
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            var tr = transform;

            // Write
            if (stream.IsWriting)
            {
                if (this.m_SynchronizePosition)
                {
                    if (m_UseLocal)
                    {
                        this.m_Direction = tr.localPosition - this.m_StoredPosition;
                        this.m_StoredPosition = tr.localPosition;
                        stream.SendNext(tr.localPosition);
                        stream.SendNext(this.m_Direction);
                    }
                    else
                    {
                        this.m_Direction = tr.position - this.m_StoredPosition;
                        this.m_StoredPosition = tr.position;
                        stream.SendNext(tr.position);
                        stream.SendNext(this.m_Direction);
                    }
                }

                if (this.m_SynchronizeRotation)
                {
                    if (m_UseLocal)
                    {
                        stream.SendNext(tr.localRotation);
                    }
                    else
                    {
                        stream.SendNext(tr.rotation);
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    stream.SendNext(tr.localScale);
                }
            }
            // Read
            else
            {
                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        if (m_UseLocal)
                            tr.localPosition = this.m_NetworkPosition;
                        else
                            tr.position = this.m_NetworkPosition;

                        this.m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        if (m_UseLocal)
                        {
                            this.m_Distance = Vector3.Distance(tr.localPosition, this.m_NetworkPosition);
                        }
                        else
                        {
                            this.m_Distance = Vector3.Distance(tr.position, this.m_NetworkPosition);
                        }
                    }

                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;

                        if (m_UseLocal)
                        {
                            tr.localRotation = this.m_NetworkRotation;
                        }
                        else
                        {
                            tr.rotation = this.m_NetworkRotation;
                        }
                    }
                    else
                    {
                        if (m_UseLocal)
                        {
                            this.m_Angle = Quaternion.Angle(tr.localRotation, this.m_NetworkRotation);
                        }
                        else
                        {
                            this.m_Angle = Quaternion.Angle(tr.rotation, this.m_NetworkRotation);
                        }
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    tr.localScale = (Vector3)stream.ReceiveNext();
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }

        public void Save(BinaryWriter writer, string version) {
            var tr = transform;
            if (this.m_SynchronizePosition) {
                if (m_UseLocal) {
                    writer.Write(tr.localPosition.x);
                    writer.Write(tr.localPosition.y);
                    writer.Write(tr.localPosition.z);
                } else {
                    writer.Write(tr.position.x);
                    writer.Write(tr.position.y);
                    writer.Write(tr.position.z);
                }
            }

            if (this.m_SynchronizeRotation) {
                if (m_UseLocal) {
                    writer.Write(tr.localRotation.x);
                    writer.Write(tr.localRotation.y);
                    writer.Write(tr.localRotation.z);
                    writer.Write(tr.localRotation.w);
                } else {
                    writer.Write(tr.rotation.x);
                    writer.Write(tr.rotation.y);
                    writer.Write(tr.rotation.z);
                    writer.Write(tr.rotation.w);
                }
            }

            if (this.m_SynchronizeScale) {
                writer.Write(tr.localScale.x);
                writer.Write(tr.localScale.y);
                writer.Write(tr.localScale.z);
            }
        }

        public void Load(BinaryReader reader, string version) {
            var tr = transform;
            if (this.m_SynchronizePosition) {
                float posx = reader.ReadSingle();
                float posy = reader.ReadSingle();
                float posz = reader.ReadSingle();
                if (m_UseLocal) {
                    transform.localPosition = new Vector3(posx, posy, posz);
                } else {
                    transform.position = new Vector3(posx, posy, posz);
                }
            }

            if (this.m_SynchronizeRotation) {
                float rotx = reader.ReadSingle();
                float roty = reader.ReadSingle();
                float rotz = reader.ReadSingle();
                float rotw = reader.ReadSingle();
                if (m_UseLocal) {
                    transform.localRotation = new Quaternion(rotx,roty,rotz,rotw);
                } else {
                    transform.rotation = new Quaternion(rotx,roty,rotz,rotw);
                }
            }

            if (this.m_SynchronizeScale) {
                float scalex = reader.ReadSingle();
                float scaley = reader.ReadSingle();
                float scalez = reader.ReadSingle();
                transform.localScale = new Vector3(scalex, scaley, scalez);
            }
        }
    }
}