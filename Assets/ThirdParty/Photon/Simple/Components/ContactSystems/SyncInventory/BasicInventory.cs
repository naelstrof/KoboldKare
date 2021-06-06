// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	/// <summary>
	/// A simple usable example of an IInventory system. Either create your own by deriving from the Inventory base class or by creating your own class using IInventory.
	/// You can also derive from this class to extend BasicInventory if you only want to make a minor adjustment to it for your needs.
	/// </summary>
	public class BasicInventory : Inventory<Vector3Int>
	{

		[SerializeField]
		public Vector3Int capacity = new Vector3Int(16, 1, 1);
		public int Volume { get { return capacity.x * capacity.y * capacity.z; } }

		public int Used
		{
			get
			{
				int used = 0;

				var mountedObjs = DefaultMount.mountedObjs;
				for (int i = 0, cnt = mountedObjs.Count; i < cnt; ++i)
				{
					var inventoryable = mountedObjs[i] as IInventoryable<Vector3Int>;
					if (inventoryable != null)
					{
						var size = inventoryable.Size;
						int volume = size.x * size.y * size.z;
						used += volume;
					}
				}
				return used;
			}
		}

		public int Remaining { get { return Volume - Used; } }

		/// <summary>
		/// Return if the object being picked up exceeds remaining inventory.
		/// </summary>
		/// <param name="inventoryable"></param>
		/// <returns></returns>
		public override bool TestCapacity(IInventoryable<Vector3Int> inventoryable)
		{
			var size = inventoryable.Size;
			int volume = size.x * size.y * size.z;

            return volume <= Remaining;
		}
	}

}

