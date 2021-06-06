// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;

namespace Photon.Pun.Simple.Internal
{

	[System.Serializable]
	public class ParameterDefaults
	{
		public ParameterInterpolation interpolateFloats = ParameterInterpolation.Hold;
		public ParameterInterpolation interpolateInts = ParameterInterpolation.Hold;

		public ParameterExtrapolation extrapolateFloats = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateInts = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateBools = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateTriggers = ParameterExtrapolation.Default;

		public bool includeFloats = true;
		public bool includeInts = true;
		public bool includeBools = true;
		public bool includeTriggers = false;

		public SmartVar defaultFloat = (float)0f;
		public SmartVar defaultInt = (int)0;
		public SmartVar defaultBool = (bool)false;
		public SmartVar defaultTrigger = (bool)false;
	}
}
