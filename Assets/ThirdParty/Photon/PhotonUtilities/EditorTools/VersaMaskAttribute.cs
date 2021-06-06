// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Utilities
{
	public class VersaMaskAttribute : PropertyAttribute
	{
		public bool definesZero;
		public Type castTo;

		public VersaMaskAttribute(bool definesZero = false, Type castTo = null)
		{
			this.definesZero = definesZero;
			this.castTo = castTo;
		}
        
    }

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(VersaMaskAttribute))]
	public class VersaMaskAttributeDrawer : VersaMaskDrawer
	{
		protected override bool FirstIsZero
		{
			get
			{
				var attr = attribute as VersaMaskAttribute;
				return attr.definesZero;
			}
		}
		
	}
#endif
}

