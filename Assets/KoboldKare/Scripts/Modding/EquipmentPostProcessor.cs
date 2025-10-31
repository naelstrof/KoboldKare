using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipmentPostProcessor : ModPostProcessor {
    private struct ModStubEquipmentPair {
        public ModManager.ModStub stub;
        public Equipment equipment;
    }
    
    private List<ModStubEquipmentPair> addedEquipments;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;
    private AsyncOperationHandle inherentAssetsHandle;
    
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(Equipment), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<Equipment>(locations, LoadEquipment);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }

    public override async Task Awake() {
        await base.Awake();
        addedEquipments = new ();
        opHandles = new();
        inherentAssetsHandle = Addressables.LoadAssetsAsync<Equipment>(searchLabel.RuntimeKey, LoadInherentEquipment);
        await inherentAssetsHandle.Task;
    }
    private void LoadInherentEquipment(Equipment equipment) {
        EquipmentDatabase.AddEquipment(equipment);
    }

    private void LoadEquipment(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        addedEquipments.Add(new ModStubEquipmentPair() {
            stub = currentStub,
            equipment = equipment
        });
        EquipmentDatabase.AddEquipment(equipment);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedEquipments.Count;i++) {
            if(addedEquipments[i].stub.GetRepresentedBy(data)) {
                EquipmentDatabase.RemoveEquipment(addedEquipments[i].equipment);
                addedEquipments.RemoveAt(i);
                i--;
            }
        }
        for (int i=0;i<opHandles.Count;i++) {
            if(opHandles[i].stub.GetRepresentedBy(data)) {
                if (opHandles[i].handle.IsValid()) {
                    Addressables.Release(opHandles[i].handle);
                }
                opHandles.RemoveAt(i);
                i--;
            }
        }
        return base.UnloadAssets(data);
    }

}
