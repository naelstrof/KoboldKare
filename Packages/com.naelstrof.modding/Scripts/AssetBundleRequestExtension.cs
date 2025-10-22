using System;
using System.Threading.Tasks;
using UnityEngine;

public static class AssetBundleRequestExtension {
    public static Task<AssetBundle> AsTask(this AssetBundleCreateRequest req) {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (req.isDone) return Task.FromResult(req.assetBundle);
        var tcs = new TaskCompletionSource<AssetBundle>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Completed(AsyncOperation op) {
            try {
                tcs.TrySetResult(((AssetBundleCreateRequest)op).assetBundle);
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            } finally {
                req.completed -= Completed;
            }
        }
        req.completed += Completed;
        return tcs.Task;
    }
    public static Task<UnityEngine.Object> AsSingleAssetTask(this AssetBundleRequest req) {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (req.isDone) return Task.FromResult(req.asset);
        var tcs = new TaskCompletionSource<UnityEngine.Object>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Completed(AsyncOperation op) {
            try {
                tcs.TrySetResult(((AssetBundleRequest)op).asset);
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            } finally {
                req.completed -= Completed;
            }
        }
        req.completed += Completed;
        return tcs.Task;
    }
}
