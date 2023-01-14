using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableCharacterMultiSelect : MultiSelectPanel {
    protected override void Start() {
        List<MultiSelectOption> options = new List<MultiSelectOption>();
        var playableCharacters = PlayableCharacterDatabase.GetPlayableCharacters();
        foreach (var playableCharacter in playableCharacters) {
            var option = new MultiSelectOption {
                label = playableCharacter.key,
                enabled = playableCharacter.enabled
            };
            option.onValueChanged += (newValue) => {
                playableCharacter.enabled = newValue;
            };
            options.Add(option);
        }
        SetOptions(options);
    }
}
