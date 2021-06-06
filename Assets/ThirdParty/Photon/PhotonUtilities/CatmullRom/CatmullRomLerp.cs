

using UnityEngine;

namespace Photon.Utilities
{
	public class CatmulRom
	{
		// Float
		public static float CatmullRomLerp(float pre, float start, float end, float post, float t)
		{
#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			float a = 2f * start;
			float b = end - pre;
			float c = 2f * pre - 5f * start + 4f * end - post;
			float d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

		public static float CatmullRomLerp(float pre, float start, float end, float t)
		{
			// extrapolate the 4th position linearly
			float post = end + (end - start);

#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			float a = 2f * start;
			float b = end - pre;
			float c = 2f * pre - 5f * start + 4f * end - post;
			float d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

		// Vector 2
		public static Vector3 CatmullRomLerp(Vector2 pre, Vector2 start, Vector2 end, Vector2 post, float t)
		{
#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			Vector2 a = 2f * start;
			Vector2 b = end - pre;
			Vector2 c = 2f * pre - 5f * start + 4f * end - post;
			Vector2 d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

		public static Vector3 CatmullRomLerp(Vector2 pre, Vector2 start, Vector2 end, float t)
		{
			// extrapolate the 4th position linearly
			Vector2 post = end + (end - start);

#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			Vector2 a = 2f * start;
			Vector2 b = end - pre;
			Vector2 c = 2f * pre - 5f * start + 4f * end - post;
			Vector2 d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

		// Vector 3
		public static Vector3 CatmullRomLerp(Vector3 pre, Vector3 start, Vector3 end, Vector3 post, float t)
		{
#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			Vector3 a = 2f * start;
			Vector3 b = end - pre;
			Vector3 c = 2f * pre - 5f * start + 4f * end - post;
			Vector3 d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

		public static Vector3 CatmullRomLerp(Vector3 pre, Vector3 start, Vector3 end, float t)
		{
			// extrapolate the 4th position linearly
			Vector3 post = end + (end - start);

#if UNITY_EDITOR
			int safety = 0;
#endif
			// Extrapolation
			while (t > 1)
			{

#if UNITY_EDITOR
				if (safety > 10)
				{
					Debug.LogError("Stuck in while");
					break;
				}
				safety++;
#endif
				pre = start;
				start = end;
				end = post;
				post = end + (end - start);
				t -= 1;
			}

			Vector3 a = 2f * start;
			Vector3 b = end - pre;
			Vector3 c = 2f * pre - 5f * start + 4f * end - post;
			Vector3 d = -pre + 3f * (start - end) + post;
			float tsqr = t * t;
			return (a + (b * t) + (c * tsqr) + (d * tsqr * t)) * .5f;
		}

	}
	
}

