using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityScriptableSettings;

public class SettingPostProcessor : ModPostProcessor {
    private List<Setting> addedSettings;

    public override void Awake() {
        base.Awake();
        addedSettings = new List<Setting>();
    }

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        addedSettings.Clear();
        var opHandle = Addressables.LoadAssetsAsync<Setting>(locations, LoadSetting);
        await opHandle.Task;
    }

    private void LoadSetting(Setting setting) {
        if (setting == null) {
            return;
        }

        for (int i = 0; i < addedSettings.Count; i++) {
            if (addedSettings[i].name != setting.name) continue;
            SettingsManager.RemoveSetting(addedSettings[i]);
            addedSettings.RemoveAt(i);
            break;
        }
        SettingsManager.AddSetting(setting);
        addedSettings.Add(setting);
    }

    public override void UnloadAllAssets() {
        foreach (var setting in addedSettings) {
            SettingsManager.RemoveSetting(setting);
        }
    }
}
