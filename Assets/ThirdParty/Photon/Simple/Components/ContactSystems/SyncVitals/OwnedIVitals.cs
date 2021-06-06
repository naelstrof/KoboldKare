// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Photon.Pun.Simple
{

	public static class OwnedIVitals
	{
		/// <summary>
		/// List of all VitalsComponents that belong to this connection. Used for connecting VitalUI elements to Vitals at runtime.
		/// </summary>
		public static List<IVitalsSystem> ownedVitalComponents = new List<IVitalsSystem>();
		public static List<IOnChangeOwnedVitals> iOnChangeOwnedVitals = new List<IOnChangeOwnedVitals>();

		public static IVitalsSystem LastItem
		{
			get
			{
				int cnt = ownedVitalComponents.Count;
				return (cnt > 0) ? ownedVitalComponents[cnt - 1] : null;
			}
		}

		public static void OnChangeAuthority(IVitalsSystem ivc, bool isMine, bool asServer)
		{

			if (isMine)
			{
				if (!ownedVitalComponents.Contains(ivc))
				{
					ownedVitalComponents.Add(ivc);
					for (int i = 0; i < iOnChangeOwnedVitals.Count; ++i)
						iOnChangeOwnedVitals[i].OnChangeOwnedVitals(ivc, null);
				}
			}
			else
			{
				if (ownedVitalComponents.Contains(ivc))
				{
					ownedVitalComponents.Remove(ivc);
					for (int i = 0; i < iOnChangeOwnedVitals.Count; ++i)
						iOnChangeOwnedVitals[i].OnChangeOwnedVitals(null, ivc);
				}
			}
		}
	}
}
