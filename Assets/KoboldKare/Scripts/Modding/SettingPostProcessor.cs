using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityScriptableSettings;

public class SettingPostProcessor : ModPostProcessor {
    private List<Setting> addedSettings;
    AsyncOperationHandle opHandle;

    public override void Awake() {
        base.Awake();
        addedSettings = new List<Setting>();
    }

    public override async Task LoadAllAssets() {
        addedSettings.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<Setting>(assetsHandle.Result, LoadSetting);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
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

    public override Task UnloadAllAssets() {
        foreach (var setting in addedSettings) {
            SettingsManager.RemoveSetting(setting);
        }
        addedSettings.Clear();

        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }

        return Task.CompletedTask;
    }
}
