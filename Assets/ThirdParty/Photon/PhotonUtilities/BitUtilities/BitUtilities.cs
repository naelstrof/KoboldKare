// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Utilities
{
	public static class BitUtilities
	{
		/// <summary>
		/// Returns the min number of bits required to describe any value between 0 and int maxvalue
		/// </summary>
		public static int GetBitsForMaxValue(this int maxvalue)
		{
			for (int i = 0; i < 32; ++i)
				if (maxvalue >> i == 0)
					return i;
			return 32;
		}

		/// <summary>
		/// Returns the min number of bits required to describe any value between 0 and uint maxvalue
		/// </summary>
		public static int GetBitsForMaxValue(this uint maxvalue)
		{
			for (int i = 0; i < 32; ++i)
				if (maxvalue >> i == 0)
					return i;
			return 32;
		}

	}
}

