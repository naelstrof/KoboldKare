// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if OCULUS


using UnityEngine;

using System.Text;
using UnityEngine.UI;

public class SimpleOVRDebug : MonoBehaviour
{
    public static SimpleOVRDebug instance;
	public static Text text;
	public static StringBuilder sb = new StringBuilder();

    // Start is called before the first frame update
    void Awake()
    {
		instance = this;
		text = GetComponent<Text>();
		text.text = "Console Initialized\n";
    }

    public static void Log(object obj)
	{
		sb.Append(obj.ToString()).Append("\n");
		text.text = sb.ToString();
	}

	public static void Clear()
	{
		sb.Length = 0;
		text.text = sb.ToString();
}


}
#endif

