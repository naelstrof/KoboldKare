// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbodyView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using SimpleJSON;

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

        public void Save(JSONNode node) {
            node["position.x"] = m_Body.position.x;
            node["position.y"] = m_Body.position.y;
            node["position.z"] = m_Body.position.z;
            
            node["rotation.x"] = m_Body.rotation.x;
            node["rotation.y"] = m_Body.rotation.y;
            node["rotation.z"] = m_Body.rotation.z;
            node["rotation.w"] = m_Body.rotation.w;

            if (this.m_SynchronizeVelocity) {
                node["velocity.x"] = m_Body.velocity.x;
                node["velocity.y"] = m_Body.velocity.y;
                node["velocity.z"] = m_Body.velocity.z;
            }

            if (this.m_SynchronizeAngularVelocity) {
                node["angularVelocity.x"] = m_Body.angularVelocity.x;
                node["angularVelocity.y"] = m_Body.angularVelocity.y;
                node["angularVelocity.z"] = m_Body.angularVelocity.z;
            }
        }

        public void Load(JSONNode node) {
            float posx = node["position.x"];
            float posy = node["position.y"];
            float posz = node["position.z"];
            this.m_NetworkPosition = new Vector3(posx, posy, posz);
            this.m_Body.position = this.m_NetworkPosition;
            this.m_Body.transform.position = this.m_NetworkPosition; //Force update?
            float rotx = node["rotation.x"];
            float roty = node["rotation.y"];
            float rotz = node["rotation.z"];
            float rotw = node["rotation.w"];
            this.m_NetworkRotation = new Quaternion(rotx, roty, rotz, rotw);
            this.m_Body.rotation = this.m_NetworkRotation;
            if (this.m_SynchronizeVelocity) {
                float velx = node["velocity.x"];
                float vely = node["velocity.y"];
                float velz = node["velocity.z"];
                this.m_Body.velocity = new Vector3(velx, vely, velz);
            }
            if (this.m_SynchronizeAngularVelocity) {
                float vangx = node["angularVelocity.x"];
                float vangy = node["angularVelocity.y"];
                float vangz = node["angularVelocity.z"];
                this.m_Body.angularVelocity = new Vector3(vangx, vangy, vangz);
            }
        }
    }
}