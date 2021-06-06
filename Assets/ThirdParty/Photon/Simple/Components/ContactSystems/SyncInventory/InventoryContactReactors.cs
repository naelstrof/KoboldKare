// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Simple
{
	/// <summary>
	/// An implementation of InventoryContactReactors<> that can be used as an inv
	/// </summary>
	public class InventoryContactReactors : InventoryContactReactors<Vector3Int>
	{

		[SerializeField]
		protected Vector3Int size = new Vector3Int(1, 1, 1);
		public override Vector3Int Size { get { return size; } }

        public override void OnAwakeInitialize(bool isNetObject)
        {
            base.OnAwakeInitialize(isNetObject);

            volume = size.x * size.y * size.z;
        }
    }
}
