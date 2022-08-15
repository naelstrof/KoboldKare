// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbodyView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using System.IO;
    using UnityEngine;


    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Photon Networking/Photon Rigidbody View")]
    public class PhotonRigidbodyView : MonoBehaviourPun, IPunObservable, ISavable
    {
        private float m_Distance;
        private float m_Angle;

        private Rigidbody m_Body;

        private Vector3 m_NetworkPosition;

        private Quaternion m_NetworkRotation;

        [HideInInspector]
        public bool m_SynchronizeVelocity = true;
        [HideInInspector]
        public bool m_SynchronizeAngularVelocity = false;

        [HideInInspector]
        public bool m_TeleportEnabled = false;
        [HideInInspector]
        public float m_TeleportIfDistanceGreaterThan = 3.0f;

        public void Awake()
        {
            this.m_Body = GetComponent<Rigidbody>();

            this.m_NetworkPosition = new Vector3();
            this.m_NetworkRotation = new Quaternion();
        }

        public void FixedUpdate()
        {
            if (!this.photonView.IsMine) {
                this.m_Body.position = Vector3.MoveTowards(this.m_Body.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                this.m_Body.rotation = Quaternion.RotateTowards(this.m_Body.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.m_Body.position);
                stream.SendNext(this.m_Body.rotation);

                if (this.m_SynchronizeVelocity)
                {
                    stream.SendNext(this.m_Body.velocity);
                }

                if (this.m_SynchronizeAngularVelocity)
                {
                    stream.SendNext(this.m_Body.angularVelocity);
                }
            }
            else
            {
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                if (this.m_TeleportEnabled)
                {
                    if (Vector3.Distance(this.m_Body.position, this.m_NetworkPosition) > this.m_TeleportIfDistanceGreaterThan)
                    {
                        this.m_Body.position = this.m_NetworkPosition;
                    }
                }
                
                if (this.m_SynchronizeVelocity || this.m_SynchronizeAngularVelocity)
                {
                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

                    if (this.m_SynchronizeVelocity)
                    {
                        this.m_Body.velocity = (Vector3)stream.ReceiveNext();

                        this.m_NetworkPosition += this.m_Body.velocity * lag;

                        this.m_Distance = Vector3.Distance(this.m_Body.position, this.m_NetworkPosition);
                    }

                    if (this.m_SynchronizeAngularVelocity)
                    {
                        this.m_Body.angularVelocity = (Vector3)stream.ReceiveNext();

                        this.m_NetworkRotation = Quaternion.Euler(this.m_Body.angularVelocity * lag) * this.m_NetworkRotation;

                        this.m_Angle = Quaternion.Angle(this.m_Body.rotation, this.m_NetworkRotation);
                    }
                }
            }
        }

        public void Save(BinaryWriter writer) {
            writer.Write(this.m_Body.position.x);
            writer.Write(this.m_Body.position.y);
            writer.Write(this.m_Body.position.z);

            writer.Write(this.m_Body.rotation.x);
            writer.Write(this.m_Body.rotation.y);
            writer.Write(this.m_Body.rotation.z);
            writer.Write(this.m_Body.rotation.w);

            if (this.m_SynchronizeVelocity) {
                writer.Write(this.m_Body.velocity.x);
                writer.Write(this.m_Body.velocity.y);
                writer.Write(this.m_Body.velocity.z);
            }

            if (this.m_SynchronizeAngularVelocity) {
                writer.Write(this.m_Body.angularVelocity.x);
                writer.Write(this.m_Body.angularVelocity.y);
                writer.Write(this.m_Body.angularVelocity.z);
            }
        }

        public void Load(BinaryReader reader) {
            
            float posx = reader.ReadSingle();
            float posy = reader.ReadSingle();
            float posz = reader.ReadSingle();
            this.m_NetworkPosition = new Vector3(posx, posy, posz);
            this.m_Body.position = this.m_NetworkPosition;
            this.m_Body.transform.position = this.m_NetworkPosition; //Force update?
            float rotx = reader.ReadSingle();
            float roty = reader.ReadSingle();
            float rotz = reader.ReadSingle();
            float rotw = reader.ReadSingle();
            this.m_NetworkRotation = new Quaternion(rotx, roty, rotz, rotw);
            this.m_Body.rotation = this.m_NetworkRotation;
            if (this.m_SynchronizeVelocity) {
                float velx = reader.ReadSingle();
                float vely = reader.ReadSingle();
                float velz = reader.ReadSingle();
                this.m_Body.velocity = new Vector3(velx, vely, velz);
            }
            if (this.m_SynchronizeAngularVelocity) {
                float vangx = reader.ReadSingle();
                float vangy = reader.ReadSingle();
                float vangz = reader.ReadSingle();
                this.m_Body.angularVelocity = new Vector3(vangx, vangy, vangz);
            }
        }
    }
}