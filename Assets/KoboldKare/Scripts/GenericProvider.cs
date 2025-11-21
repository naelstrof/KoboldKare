using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;

public class GenericProvider<T> : ResourceProviderBase where T : UnityEngine.Object {
    private readonly Dictionary<string, List<T>> objects = new();
    public List<object> GetKeys() {
        List<object> keys = new List<object>();
        foreach (var kvp in objects) {
            keys.Add(kvp.Key);
        }
        return keys;
    }

    private string id;

    public GenericProvider(string id) {
        this.id = id;
    }

    public override string ProviderId => id;

    public override bool CanProvide(Type t, IResourceLocation location) {
        if (!typeof(T).IsAssignableFrom(t)) {
            return false;
        }
        return objects.ContainsKey(location.InternalId);
    }
    
    public void Add(string id, T obj) {
        if (!objects.ContainsKey(id)) {
            objects.Add(id, new List<T>());
        }
        objects[id].Add(obj);
    }
    public void Remove(string id, T obj) {
        if (!objects.TryGetValue(id, out var stringTableCollection)) {
            return;
        }
        stringTableCollection.Remove(obj);
    }

    public override void Provide(ProvideHandle provideHandle) {
        var location = provideHandle.Location;
        if (objects.TryGetValue(location.InternalId, out var table)) {
            provideHandle.Complete(table[^1], true, null);
        } else {
            provideHandle.Complete((StringTable)null, false, new ArgumentNullException("Key not found in StringTableProvider: " + location.InternalId));
        }
    }

    public override void Release(IResourceLocation location, object asset) {
    }
}