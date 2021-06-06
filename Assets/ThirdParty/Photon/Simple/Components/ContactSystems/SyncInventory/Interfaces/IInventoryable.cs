// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	/// <summary>
	/// Indicates a class that can be added to an IInventory.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IInventoryable<T> : IContactable
	{
		T Size { get; }
	}
}
