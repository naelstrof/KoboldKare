// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using emotitron.Utilities;
using Photon.Compression;
using UnityEngine;

namespace Photon.Pun.Simple
{

	public struct ContactEvent
	{
		readonly public IContactSystem contactSystem;
        readonly public IContactTrigger contactTrigger;
        readonly public ContactType contactType;

		public ContactEvent(IContactSystem contactSystem, IContactTrigger contacter, ContactType contactType)
		{
			this.contactSystem = contactSystem;
			this.contactTrigger = contacter;
			this.contactType = contactType;
		}

		public ContactEvent(ContactEvent contactEvent)
		{
			this.contactSystem = contactEvent.contactSystem;
			this.contactTrigger = contactEvent.contactTrigger;
			this.contactType = contactEvent.contactType;
		}
        
        //public void Serialize(byte[] buffer, ref int bitposition)
        //{
        //    buffer.WritePackedBytes((uint)contactSystem.NetObj.photonView.ViewID, ref bitposition, 32);
        //}

        //public static ContactEvent Deserialize(byte[] buffer, ref int bitposition)
        //{
        //    int viewID = (int)buffer.ReadPackedBytes(ref bitposition, 32);
        //    byte 
            
        //}

#if UNITY_EDITOR
		public override string ToString()
		{
			var cs = (this.contactSystem as Component);
			return "ContactSystem: " + (cs ? (cs.name + " : " + cs.GetType().Name) : null) + " <b>" + contactType 
				+ "</b> othrcol: " +  ((contactTrigger as Component) ? (contactTrigger as Component).name : "null")
				;
		}
#endif
	}
}
