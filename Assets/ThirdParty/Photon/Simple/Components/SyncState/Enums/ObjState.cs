// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{

	public enum ObjState
	{
		Despawned = 0,
		Visible = 1,
       
        /// <summary>
        /// Indicates that this object is a child of another PhotonView's Mount.
        /// </summary>
		Mounted = 2,
        AnchoredPosition = 4,
        AnchoredRotation = 8,
        /// <summary>
        /// Indicates that this object is mounted with a fixed local position/local rotation.
        /// </summary>
        Anchored = 12,
        Dropped = 16,
		Transit = 32
	}

    public enum ObjStateEditor
    {
        Despawned = 0,
        Visible = 1,
        Mounted = 2,
        AnchoredPosition = 4,
        AnchoredRotation = 8,
        Dropped = 16,
        Transit = 32
    }
}
