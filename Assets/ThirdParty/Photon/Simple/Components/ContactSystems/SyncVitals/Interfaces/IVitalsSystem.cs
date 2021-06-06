// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------


namespace Photon.Pun.Simple
{
	
	/// <summary>
	/// Object contains a Vitals class reference.
	/// </summary>
	public interface IVitalsSystem : IContactSystem
	{
		Vitals Vitals { get; }
	}

	/// <summary>
	/// Interface indicates interest in knowing when there has been a change in which Vitals are owned by this connection.
	/// Used for things like health bars, that need to determine which netobj's vitals they should be monitoring at runtime.
	/// </summary>
	public interface IOnChangeOwnedVitals
	{
		void OnChangeOwnedVitals(IVitalsSystem added, IVitalsSystem removed);
	}
}
