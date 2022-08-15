using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ISavable {
    void Save(BinaryWriter writer);
    void Load(BinaryReader reader);
}
