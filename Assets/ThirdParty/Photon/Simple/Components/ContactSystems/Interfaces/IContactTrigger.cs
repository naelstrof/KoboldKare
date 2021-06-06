// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Photon.Pun.Simple {

	public interface IContactTrigger
	{
        NetObject NetObj { get; }
        byte Index { get; set; }
        Consumption ContactCallbacks(ContactEvent contactEvent);
		void OnContact(IContactTrigger otherCT, ContactType contactType);
		IContactTrigger Proxy { get; set; }
		bool PreventRepeats { get; set; }
        List<IContactSystem> ContactSystems { get; }
        ISyncContact SyncContact { get; }
        IContactGroupsAssign ContactGroupsAssign { get; }
    }

}
