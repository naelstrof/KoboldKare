using System;
using System.Threading.Tasks;
using Steamworks;

public static class SteamAPIExtensions {
    public static Task<T> AsTask<T>(this SteamAPICall_t t) {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callResult = CallResult<T>.Create((param, failure) => {
            if (failure) {
                tcs.TrySetException(new Exception("Steam API call failed"));
            } else {
                try {
                    tcs.TrySetResult(param);
                } catch (Exception ex) {
                    tcs.TrySetException(ex);
                }
            }
        });
        callResult.Set(t);
        return tcs.Task;
    }
}
