// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
    public interface IConsumable
    {
        double Charges { get; set; }
        Consumption Consumption { get; }
        ConsumedDespawn ConsumedDespawn { get; }
    }
}
