// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if SNS_REPORTS && (UNITY_EDITOR || DEVELOPMENT_BUILD)

namespace Photon.Pun.Simple
{
	public static class SimpleDataMonitor
	{
		/// <summary>
		/// Simplistic int wrapper, just to avoid Dictionary desync headaches when modifying values while iterating the dicts.
		/// </summary>
		private class CountValue { public int value; public CountValue(int val) { value = val; } }

		private static Dictionary<NetObject, CountValue> byteTotalsPerNetObj = new Dictionary<NetObject, CountValue>();
		private static Dictionary<NetObject, Dictionary<SyncObject, CountValue>> bitsPerSyncObj = new Dictionary<NetObject, Dictionary<SyncObject, CountValue>>();
		private static Dictionary<NetObject, Dictionary<SyncObject, CountValue>> byteTotalsPerSyncObj = new Dictionary<NetObject, Dictionary<SyncObject, CountValue>>();
		private static int byteTotalLastSecond;

		public static int lastTotalsTime;

		public static void AddData(SyncObject so, int bits)
		{
			var no = so.NetObj;

			EnsureKeysExists(so);

			int t = (int)Time.time;
			if (t != lastTotalsTime)
				NextSecond();

			var soDict = bitsPerSyncObj[no];

			soDict[so].value += bits;
		}

		private static void EnsureKeysExists(SyncObject so)
		{
			var no = so.NetObj;

			if (!bitsPerSyncObj.ContainsKey(no))
			{
				bitsPerSyncObj.Add(no, new Dictionary<SyncObject, CountValue>());
				bitsPerSyncObj[no][so] = new CountValue(0);
				byteTotalsPerSyncObj.Add(no, new Dictionary<SyncObject, CountValue>());
				byteTotalsPerSyncObj[no][so] = new CountValue(0);
				byteTotalsPerNetObj.Add(no, new CountValue(0));
			}

			var soDict = bitsPerSyncObj[no];

			if (!soDict.ContainsKey(so))
			{
				soDict.Add(so, new CountValue(0));
				byteTotalsPerSyncObj[no].Add(so, new CountValue(0));
			}
		}

		private static void NextSecond()
		{
			byteTotalLastSecond = 0;

			foreach (var noKvp in bitsPerSyncObj)
			{
				var no = noKvp.Key;
				byteTotalsPerNetObj[no].value = 0;

				var byteDict = byteTotalsPerSyncObj[no];

				foreach (var soKvp in noKvp.Value)
				{
					var so = soKvp.Key;

					int bytes = (soKvp.Value.value + 7) >> 3;

					byteDict[so].value = bytes;
					byteTotalLastSecond += bytes;
					byteTotalsPerNetObj[no].value += bytes;

					// reset the syncObj bit counter
					bitsPerSyncObj[no][so].value = 0;
				}

				lastTotalsTime = (int)Time.time;
			}
			strb.Length = 0;
			Debug.Log(Report(strb, ReportType.PerSyncObject).ToString());
		}

		public enum ReportType { Total, PerNetObject, PerSyncObject }

		public static StringBuilder strb = new StringBuilder();

		public static StringBuilder Report(StringBuilder strb, ReportType reportType)
		{

			strb.Append(byteTotalLastSecond).Append(" Total Sent Bytes/Sec\n");

			if (reportType != ReportType.Total)
			{
				foreach (var noKvp in byteTotalsPerNetObj)
				{
					var no = noKvp.Key;
					var noTotal = noKvp.Value.value;

					if (!no)
						continue;

					strb.Append("- ").Append(noTotal).Append(" Bytes from ").Append(no.name).Append(no.NetObjId).Append("\n");

					if (reportType == ReportType.PerSyncObject)
						foreach (var soKvp in byteTotalsPerSyncObj[no])
						{
							var so = soKvp.Key;
							var soTotal = soKvp.Value.value;
							strb.Append("-- ").Append(soTotal.ToString("0000")).Append(" Bytes from ").Append(so.GetType().Name);
							if (so.transform.parent != null)
								strb.Append(" (").Append(so.name).Append(")");
							strb.Append("\n");
						}
				}
			}

			return strb;
		}
	}
}

#endif
