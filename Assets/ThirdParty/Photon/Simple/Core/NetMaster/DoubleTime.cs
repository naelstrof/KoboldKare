// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------


using UnityEngine;

namespace Photon.Pun.Simple
{
	public static class DoubleTime
	{
		public static double time;
		public static double prevUpdateTime;
		public static double prevFixedTime;
		public static double deltaTime;
		public static double timeSinceFixed;
		public static double fixedTime;
		public static double fixedDeltaTime;
		public static float normTimeSinceFixed;

		public static double updateTime;

		//public static double mixedTime;
		/// <summary>
		/// Delta Time that includes Fixed. Useful for operations that apply in both fixed and update.
		/// </summary>
		public static float mixedDeltaTime;

		public static bool isInFixed;

		public static void SnapFixed()
		{
			prevFixedTime = fixedTime;

			fixedDeltaTime = Time.fixedDeltaTime;

			// Advance FixedTime
			if (fixedTime == 0)
			{
				fixedTime = Time.fixedTime;
				time = Time.time;
			}
			else
			{
				fixedTime += fixedDeltaTime;
				mixedDeltaTime = (float)(fixedTime - time);
				time = fixedTime;
			}

			isInFixed = true;
			isFirstUpdatePostFixed = true;

			//Debug.Log("<b>FIXED CLOCK</b> " + time + "   mixedDelta: " + mixedDeltaTime );
		}

		static bool isFirstUpdatePostFixed;

		public static void SnapUpdate()
		{
			prevUpdateTime = updateTime;

			// Advance updateTime
			if (updateTime == 0)
			{
				updateTime = Time.time;

				/// TEST - Recreate Fixed() that may not have happened
				fixedTime = Time.fixedTime;
				time = fixedTime;
			}
			else
			{
				updateTime += Time.deltaTime;
			}

			timeSinceFixed = updateTime - fixedTime;
			deltaTime = updateTime - prevUpdateTime;
			normTimeSinceFixed = (float)(timeSinceFixed / Time.fixedDeltaTime);

			mixedDeltaTime = isFirstUpdatePostFixed ? (float)(updateTime - time) : Time.deltaTime;

			//Debug.Log("<color=green>UPDATE CLOCK </color> " + time + "    mixedDelta: " +  mixedDeltaTime + " : " + Time.deltaTime);

			time = updateTime;

			isFirstUpdatePostFixed = false;
			isInFixed = false;
		}
	}
}
