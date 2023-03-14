using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModPathHaver : MonoBehaviour {
    public string Path => $"{Application.persistentDataPath}/mods/";
}
