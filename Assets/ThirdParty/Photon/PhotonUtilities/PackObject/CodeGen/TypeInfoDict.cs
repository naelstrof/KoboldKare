// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Photon.Compression.Internal
{
	[System.Serializable]
	public class TypeInfoDict
	{
		[SerializeField] public List<string> keys = new List<string>();
		[SerializeField] public List<TypeInfo> vals = new List<TypeInfo>();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        public override string ToString()
        {
            int cnt = keys.Count;
            if (cnt == 0)
                return "No PackObjects found.";

            sb.Length = 0;

            sb.Append(keys.Count).Append(" PackObject(s) found:\n\n");

            for (int i = 0; i < cnt; ++i)
            {
                var key = keys[i];
                var val = vals[i];
                sb.Append(keys[i]).Append(" : ").Append(val.totalFieldCount).Append(" fields");
                if (i + 1 != cnt)
                    sb.Append("\n");
            }

            return sb.ToString();
        }


        public bool Add(System.Type type, TypeInfo val)
		{
			return Add(type.FullName, val);
		}

		public bool Add(string key, TypeInfo val)
		{
			int index = keys.IndexOf(key);
			if (index != -1)
				return false;

			keys.Add(key);
			vals.Add(val);

			return true;
		}

		public bool Remove(string key)
		{
			int index = keys.IndexOf(key);
			if (index == -1)
				return false;

			keys.RemoveAt(index);
			vals.RemoveAt(index);

			return true;
		}

		public void RemoveAt(int index)
		{
			keys.RemoveAt(index);
			vals.RemoveAt(index);
		}

        public int Count { get { return keys.Count; } }

		public TypeInfo GetTypeInfo(System.Type type)
		{
			int index = keys.IndexOf(type.FullName);

			if (index == -1)
				return null;
			else
				return vals[index];
		}

		public int TryGetValue(string key, out TypeInfo val)
		{
			int index = keys.IndexOf(key);
			if (index == -1)
			{
				val = null;
				return index;
			}

			val = vals[index];
			return index;
		}

		public void Clear()
		{
			keys.Clear();
			vals.Clear();
		}
	}
}

#endif