// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{

	public enum PassThruType { SetTrigger, ResetTrigger, Play, PlayFixed, CrossFade, CrossFadeFixed }
	/// <summary>
	/// A generic passthrough used to store Play/Crossfade/SetTrigger calls for deferment
	/// </summary>
	public struct AnimPassThru
	{
		public PassThruType passThruType;
		public int hash;
		public float normlTime;
		public float fixedTime;
		public float duration;
		public int layer;
		public LocalApplyTiming localApplyTiming;

		public AnimPassThru(PassThruType triggerType, int hash, int layer, float normTime, float otherTime, float duration, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
#if UNITY_EDITOR
			if (normTime > 1)
				UnityEngine.Debug.LogWarning(typeof(SyncAnimator).Name + " requires normalizedTime values to be between 0 and 1. Be sure to clamp or modulus 1 your values.");
#endif
			this.passThruType = triggerType;
			this.hash = hash;
			this.normlTime = normTime;
			this.fixedTime = otherTime;
			this.duration = duration;
			this.layer = layer;
			this.localApplyTiming = localApplyTiming;
		}
	}
}
