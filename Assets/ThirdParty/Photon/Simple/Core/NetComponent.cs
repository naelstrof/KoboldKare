// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    /// <summary>
    /// Base class for components that are aware of Networking and the root NetObject, and tie into its startup/shutdown/ownership callbacks.
    /// </summary>
    [HelpURL(Internal.SimpleDocsURLS.OVERVIEW_PATH)]
    public abstract class NetComponent : MonoBehaviour
        , IOnJoinedRoom
        , IOnAwake
        , IOnStart
        , IOnEnable
        , IOnDisable
        , IOnAuthorityChanged
    {

        /// <summary>
        /// Used for shared cached items.
        /// </summary>
        [HideInInspector] [SerializeField] protected int prefabInstanceId;

        protected NetObject netObj;
        public NetObject NetObj { get { return netObj; } }

#if UNITY_EDITOR
        public virtual bool AutoAddNetObj { get { return true; } }
#endif

        //protected SyncState syncState;
        //public SyncState SyncState { get { return syncState; } }

        public RigidbodyType RigidbodyType { get; private set; }


        public int ViewID
        {
            get
            {
                return photonView.ViewID;
            }
        }

        protected PhotonView photonView;
        public PhotonView PhotonView { get { return photonView; } }
        public bool IsMine { get { return photonView.IsMine; } }
        public int ControllerActorNr { get { return photonView.ControllerActorNr; } }

        protected virtual void Reset()
        {

#if UNITY_EDITOR
            /// Only check the instanceId if we are not playing. Once we build out this is set in stone to ensure all instances and prefabs across network agree.
            if (!Application.isPlaying)
                prefabInstanceId = GetInstanceID();
            
            GetOrAddNetObj();
#endif
        }


        protected virtual void OnValidate()
        {
//#if UNITY_EDITOR
//            GetOrAddNetObj();
//#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// Connect the NetObject on this gameobject to the netObj cached variable.
        /// </summary>
        public void GetOrAddNetObj()
        {
            if (netObj)
                return;

            netObj = transform.GetParentComponent<NetObject>();

            if (netObj == null && AutoAddNetObj)
            {
                Debug.Log("No NetObject yet on " + name + ". Adding one to root now.");
                netObj = transform.root.gameObject.AddComponent<NetObject>();
            }
        }

#endif

        public virtual void OnJoinedRoom()
        {

        }

        public void Awake()
        {
            // If this NetComponent doesn't have a NetObject, IOnAwake will not fire.
            if (!transform.GetParentComponent<NetObject>())
                OnAwakeInitialize(false);
        }
        /// <summary>
        /// Be sure to use base.OnAwake() when overriding. 
        /// This is called when the NetObject runs Awake(). All code that depends on the NetObj being initialized should use this
        /// rather than Awake();
        /// </summary>
        public virtual void OnAwake()
        {
            netObj = transform.GetParentComponent<NetObject>();

            //if (netObj)
            //	syncState = netObj.GetComponent<SyncState>();

            EnsureComponentsDependenciesExist();

            OnAwakeInitialize(true);
        }

        /// <summary>
        /// Awake code that will run whether or not a NetObject Exists
        /// </summary>
        /// <returns>Returns true if this is a NetObject</returns>
        public virtual void OnAwakeInitialize(bool isNetObject)
        {

        }

        protected virtual NetObject EnsureComponentsDependenciesExist()
        {
            if (!netObj)
                netObj = transform.GetParentComponent<NetObject>();

            if (netObj)
            {

                photonView = netObj.GetComponent<PhotonView>();
                
                //if (this is IContactSystem)
                //	if (ReferenceEquals(netObj.GetComponent<IContactTrigger>(), null))
                //		netObj.gameObject.AddComponent<ContactTrigger>();

                RigidbodyType = (netObj.Rb) ? RigidbodyType.RB : (netObj.Rb2D) ? RigidbodyType.RB2D : RigidbodyType.None;

                return netObj;
            }
            else
            {
                Debug.LogError("NetComponent derived class cannot find a NetObject on '" + transform.root.name + "'.");
                return null;
            }
        }


        public virtual void Start()
        {
            if (!netObj)
                OnStartInitialize(false);
        }

        public virtual void OnStart()
        {

            OnStartInitialize(true);
        }

        /// <summary>
        /// Awake code that will run whether or not a NetObject Exists
        /// </summary>
        /// <returns>Returns true if this is a NetObject</returns>
        public virtual void OnStartInitialize(bool isNetObject)
        {

        }

        public virtual void OnPostEnable()
        {

        }

        public virtual void OnPostDisable()
        {
            hadFirstAuthorityAssgn = false;
        }

        protected bool hadFirstAuthorityAssgn;

        /// <summary>
        /// Updates authority values on authority changes.
        /// </summary>
        /// <param name="controllerChanged"></param>
        public virtual void OnAuthorityChanged(bool isMine, bool controllerChanged)
        {
            if (!controllerChanged)
                return;

            if (!hadFirstAuthorityAssgn)
            {
                OnFirstAuthorityAssign(isMine, controllerChanged);
                hadFirstAuthorityAssgn = true;
            }
        }

        public virtual void OnFirstAuthorityAssign(bool isMine, bool asServer)
        {

        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(NetComponent), true)]
    [CanEditMultipleObjects]
    public class NetComponentEditor : HeaderEditorBase
    {
        protected override bool UseThinHeader
        {
            get
            {
                return true;
            }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/NetComponentText";
            }
        }

        //protected readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();

        //protected override string BackTexturePath
        //{
        //    get { return "Header/GreenBack"; }
        //}

    }
#endif

}
