using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

public interface ISavable {
    void Save(JSONNode node);
    void Load(JSONNode node);
}
