// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Simple
{

	public class AutoLayerByAuthority : MonoBehaviour
	{
		//[EnumMask]
		public int authorityLayer = 8;

		//[EnumMask]
		public int nonAuthorityLayer = 9;

		public int projectileLayer = 10;

		public bool applyToChildren = true;

		// Use this for initialization

		private void Awake()
		{
			Physics.IgnoreLayerCollision(authorityLayer, projectileLayer);

			/// Set to projectile layer. Will be overwritten if authority callback happens.
			if (applyToChildren)
				SetChildrenLayer(transform, projectileLayer);
			else
				gameObject.layer = projectileLayer;
		}

		public void OnChangeAuthority(bool IsMine, bool serverIsActive)
		{
			Debug.Log("Auth change " + name);
			int layer = IsMine ? authorityLayer : nonAuthorityLayer;

			if (applyToChildren)
				SetChildrenLayer(transform, layer);
			else
				gameObject.layer = layer;
		}

		public void SetChildrenLayer(Transform t, int layer)
		{
			t.gameObject.layer = layer;

			for (int i = 0; i < t.childCount; ++i)
			{
				Transform child = t.GetChild(i);
				if (child.GetComponent<AutoLayerByAuthority>())
					continue;

				SetChildrenLayer(t.GetChild(i), layer);
			}
		}

	}

}
