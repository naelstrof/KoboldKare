// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

using Photon.Compression;
using Photon.Pun.UtilityScripts;
using Photon.Utilities;

namespace Photon.Pun.Simple.Internal
{
	/// <summary>
	/// Settings for a single Animator parameter.
	/// </summary>
	[System.Serializable]
	public class ParameterSettings
	{
		// store paramtype so we can check in editor to see if name got reused for a different type
		public int hash;
		public AnimatorControllerParameterType paramType;
		public bool include;
		public ParameterInterpolation interpolate;
		public ParameterExtrapolation extrapolate;
		public SmartVar defaultValue;

		/// TODO: FieldOffset these
		public LiteFloatCrusher fcrusher;
		public LiteIntCrusher icrusher;

		// Constructor
		public ParameterSettings(int hash, ParameterDefaults defs, ref int paramCount, AnimatorControllerParameterType paramType)
		{
			this.hash = hash;
			this.paramType = paramType;

			switch (paramType)
			{
				case AnimatorControllerParameterType.Float:
					include = defs.includeFloats;
					interpolate = defs.interpolateFloats;
					extrapolate = defs.extrapolateFloats;
					defaultValue = (float)defs.defaultFloat;
					fcrusher = new LiteFloatCrusher(LiteFloatCompressType.Half16, 0, 1, true);
					break;

				case AnimatorControllerParameterType.Int:
					include = defs.includeInts;
					interpolate = defs.interpolateInts;
					extrapolate = defs.extrapolateInts;
					defaultValue = (int)defs.defaultInt;
					icrusher = new LiteIntCrusher();
					break;

				case AnimatorControllerParameterType.Bool:
					include = defs.includeBools;
					interpolate = ParameterInterpolation.Hold;
					extrapolate = ParameterExtrapolation.Hold;
					defaultValue = (bool)defs.defaultBool;
					break;

				case AnimatorControllerParameterType.Trigger:
					include = defs.includeTriggers;
					interpolate = ParameterInterpolation.Default;
					extrapolate = ParameterExtrapolation.Default;
					defaultValue = (bool)defs.defaultTrigger;
					break;

				default:
					break;
			}
		}

		private static readonly List<int> rebuiltHashes = new List<int>();
		private static readonly List<ParameterSettings> rebuiltSettings = new List<ParameterSettings>();
#if UNITY_EDITOR
		private static List<string> reusableNameList = new List<string>();
#endif
		public static List<string> RebuildParamSettings(Animator a, ref ParameterSettings[] paraSettings, ref int paramCount, ParameterDefaults defs)
		{

#if UNITY_EDITOR
			var ac = a.GetController();
			var parms = ac.parameters;
#else
			var parms = a.parameters;
#endif
			rebuiltHashes.Clear();
			rebuiltSettings.Clear();
#if UNITY_EDITOR
			reusableNameList.Clear();
#endif
			bool hasChanged = false;

			paramCount = parms.Length;
			for (int i = 0; i < paramCount; ++i)
			{
				var p = parms[i];
#if UNITY_EDITOR
				reusableNameList.Add(p.name);
#endif
				var hash = p.nameHash;
				int pos = GetHashIndex(paraSettings, hash);
				if (pos != i)
					hasChanged = true;
				/// remap existing switches for this hash, or if not found make a new one
				rebuiltHashes.Add(hash);
				rebuiltSettings.Add((pos == -1) ? new ParameterSettings(hash, defs, ref paramCount, p.type) : paraSettings[pos]);
			}

			if (hasChanged)
				paraSettings = rebuiltSettings.ToArray();

#if UNITY_EDITOR
			return reusableNameList;
#else
			return null;
#endif
		}

		private static int GetHashIndex(ParameterSettings[] ps, int lookfor)
		{
			for (int i = 0, cnt = ps.Length; i < cnt; ++i)
				if (ps[i].hash == lookfor)
					return i;
			return -1;
		}

	}
}
