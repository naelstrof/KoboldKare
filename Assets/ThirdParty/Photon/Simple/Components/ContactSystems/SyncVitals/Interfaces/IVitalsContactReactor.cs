// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public interface IVitalsContactReactor : IContactReactor
	{
        VitalNameType VitalNameType { get; }
        double DischargeValue(ContactType contactType = ContactType.Undefined);
        bool AllowOverload { get; }
        bool Propagate { get; }

    }
    public interface IVitalsConsumable : IVitalsContactReactor, IConsumable
    {
        
    }
}