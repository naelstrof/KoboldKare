using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using SimpleJSON;

public interface ISavable {
    void Save(JSONNode node);
    Task Load(JSONNode node);
}
