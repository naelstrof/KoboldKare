using System;
using System.Threading.Tasks;
using UnityEngine;

public static class AssetBundleRequestExtension {
    public static Task AsTask(this AsyncOperation req) {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (req.isDone) return Task.CompletedTask;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Completed(AsyncOperation op) {
            try {
                tcs.TrySetResult(true);
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            } finally {
                req.completed -= Completed;
            }
        }
        req.completed += Completed;
        return tcs.Task;
    }
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
    public static Task<T> AsSingleAssetTask<T>(this AssetBundleRequest req) where T : UnityEngine.Object {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (req.isDone) return Task.FromResult((T)req.asset);
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Completed(AsyncOperation asyncOperation) {
            try {
                tcs.TrySetResult((T)((AssetBundleRequest)asyncOperation).asset);
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            } finally {
                ((AssetBundleRequest)asyncOperation).completed -= Completed;
            }
        }
        req.completed += Completed;
        return tcs.Task;
    }
}
