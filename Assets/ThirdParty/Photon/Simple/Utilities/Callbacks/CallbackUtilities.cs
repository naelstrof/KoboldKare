// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Photon.Pun.Simple.Internal
{
	public static class CallbackUtilities
	{
		public static int RegisterInterface<T>(List<T> callbackList, object c, bool register) where T : class
		{
			if (ReferenceEquals(callbackList, null))
				callbackList = new List<T>();

			var iface = (c as T);
			if (ReferenceEquals(iface, null))
				return callbackList.Count;

			if (register)
			{
				if (!callbackList.Contains(iface))
					callbackList.Add(iface);
			}
			else
			{
				if (callbackList.Contains(iface))
					callbackList.Remove(iface);
			}

			return callbackList.Count;
		}
	}
}


