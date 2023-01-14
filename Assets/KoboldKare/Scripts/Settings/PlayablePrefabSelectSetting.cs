using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "PlayablePrefabSelect", menuName = "Unity Scriptable Setting/KoboldKare/PlayablePrefab", order = 1)]
public class PlayablePrefabSelectSetting : SettingDropdown {
    private void Awake() {
        PlayableCharacterDatabase.AddPlayersChangedListener(LoadDatabase);
    }

    private void OnDestroy() {
        PlayableCharacterDatabase.RemovePlayersChangedListener(LoadDatabase);
    }

    private void LoadDatabase(ReadOnlyCollection<PlayableCharacterDatabase.PlayableCharcterInfo> playableCharacters) {
        List<string> newOptions = new List<string>();
        foreach (var playableCharacter in playableCharacters) {
            newOptions.Add(playableCharacter.key);
        }
        dropdownOptions = newOptions.ToArray();
        selectedValue = Mathf.Clamp(selectedValue,0,dropdownOptions.Length);
    }
}
