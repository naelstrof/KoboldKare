using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

public class GenericResourceLocator<T> : IResourceLocator where T : UnityEngine.Object {
    private GenericProvider<T> provider;
    private string id;
    public GenericResourceLocator(string id, GenericProvider<T> provider) {
        this.provider = provider;
        this.id = id;
    }

    public string LocatorId => id;
    public IEnumerable<object> Keys => provider.GetKeys();
    public bool Locate(object key, Type type, out IList<IResourceLocation> locations) {
        if (!typeof(T).IsAssignableFrom(type)) {
            locations = null;
            return false;
        }

        if (provider.GetKeys().Contains(key)) {
            locations = new List<IResourceLocation>();
            locations.Add(new ResourceLocationBase(key as string, key as string, provider.ProviderId, type));
            return true;
        } else {
            locations = null;
            return false;
        }
    }
}
