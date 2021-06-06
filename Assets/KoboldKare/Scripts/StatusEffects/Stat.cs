using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Stat", menuName = "Data/Stat", order = 1)]
public class Stat : ScriptableObject {
    public new LocalizedString name;
    public Sprite sprite;
}
