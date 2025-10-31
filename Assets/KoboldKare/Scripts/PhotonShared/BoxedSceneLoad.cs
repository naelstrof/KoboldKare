using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BoxedSceneLoad {
    enum Source { None, Unity, Addressables, Task }

    Source source = Source.None;
    AsyncOperation unityOp;
    AsyncOperationHandle<SceneInstance> addrHandle;
    Task task;
    event Action completedInternal;

    public bool IsDone {
        get {
            return source switch {
                Source.Unity => unityOp?.isDone ?? true,
                Source.Addressables => addrHandle.IsDone,
                Source.Task => (task?.Status ?? TaskStatus.RanToCompletion) == TaskStatus.RanToCompletion,
                _ => true
            };
        }
    }

    public float Progress {
        get {
            return source switch {
                Source.Unity => unityOp?.progress ?? 1f,
                Source.Addressables => addrHandle.PercentComplete,
                Source.Task => IsDone ? 1f : 0f,
                _ => 1f
            };
        }
    }

    public event Action OnCompleted {
        add {
            completedInternal += value;
            if (IsDone) value?.Invoke();
        }
        remove => completedInternal -= value;
    }

    public static BoxedSceneLoad FromUnity(AsyncOperation op) {
        var boxed = new BoxedSceneLoad { source = Source.Unity, unityOp = op };
        if (op != null) {
            op.completed += _ => boxed.completedInternal?.Invoke();
        } else {
            boxed.completedInternal?.Invoke();
        }
        return boxed;
    }

    public static BoxedSceneLoad FromAddressables(AsyncOperationHandle<SceneInstance> handle) {
        var boxed = new BoxedSceneLoad { source = Source.Addressables, addrHandle = handle };
        handle.Completed += _ => boxed.completedInternal?.Invoke();
        return boxed;
    }
    
    public static BoxedSceneLoad FromTask(Task handle) {
        var boxed = new BoxedSceneLoad { source = Source.Task, task = handle };
        handle.ContinueWith(_ => boxed.completedInternal?.Invoke());
        return boxed;
    }
}