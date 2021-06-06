// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;

namespace Photon.Compression
{
	public interface IPackObjOnReadyChange
	{
		void OnPackObjReadyChange(FastBitMask128 readyMask, bool AllAreReady);
	}
}
