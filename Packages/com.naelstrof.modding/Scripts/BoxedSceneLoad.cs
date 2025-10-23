using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BoxedSceneLoad {
    enum Source { None, Unity, Addressables }

    Source source = Source.None;
    AsyncOperation unityOp;
    AsyncOperationHandle<SceneInstance> addrHandle;
    event Action completedInternal;

    public bool IsDone {
        get {
            return source switch {
                Source.Unity => unityOp?.isDone ?? true,
                Source.Addressables => addrHandle.IsDone,
                _ => true
            };
        }
    }

    public float Progress {
        get {
            return source switch {
                Source.Unity => unityOp?.progress ?? 1f,
                Source.Addressables => addrHandle.PercentComplete,
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
}