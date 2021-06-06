// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple
{
    /// <summary>
    /// Companion component to IContactSystem components. This component indexes all IContactSystem components so their references can be serialized efficiently.
    /// </summary>
    public class ContactManager : MonoBehaviour
    {
        public List<IContactSystem> contactSystems = new List<IContactSystem>(0);
        public List<IContactTrigger> contactTriggers = new List<IContactTrigger>(0);

        // Awake is used rather than IOnAwake, since this component may be added on the fly by IContactSystem components, and it will miss the IOnAwake callback.
        public void Awake()
        {
            transform.GetNestedComponentsInChildren<IContactSystem, NetObject>(this.contactSystems, true);
            transform.GetNestedComponentsInChildren<IContactTrigger, NetObject>(this.contactTriggers, true);

            int cnt = this.contactSystems.Count;

            if (cnt > 255)
                throw new IndexOutOfRangeException("NetObjects may not have more than 255 IContactSystem components on them.");

            for (byte i = 0; i < cnt; ++i)
                this.contactSystems[i].SystemIndex = i;
        }

        public IContactSystem GetContacting(int index)
        {
            return this.contactSystems[index];
        }
    }
}

