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
    private string sceneName;
    private Action activateAction;

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

    public void ActivateScene() {
        switch (source) {
            case Source.Addressables:
                addrHandle.Result.ActivateAsync();
                break;
            case Source.Unity:
                unityOp.allowSceneActivation = true;
                break;
            case Source.Task:
                activateAction?.Invoke();
                break;
        }
    }

    public event Action OnCompleted {
        add {
            completedInternal += value;
            if (IsDone) value?.Invoke();
        }
        remove => completedInternal -= value;
    }

    public static BoxedSceneLoad FromUnity(string sceneName) {
        var op = SceneManager.LoadSceneAsync(sceneName);
        var boxed = new BoxedSceneLoad { source = Source.Unity, unityOp = op, sceneName = sceneName};
        if (op != null) {
            op.allowSceneActivation = false;
            op.completed += _ => boxed.completedInternal?.Invoke();
        } else {
            boxed.completedInternal?.Invoke();
        }
        return boxed;
    }

    public static BoxedSceneLoad FromAddressables(string sceneName) {
        var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);
        var boxed = new BoxedSceneLoad { source = Source.Addressables, addrHandle = handle, sceneName =  sceneName };
        handle.Completed += _ => boxed.completedInternal?.Invoke();
        return boxed;
    }
    
    public static BoxedSceneLoad FromTask(Task handle, string sceneName, Action activateScene) {
        var boxed = new BoxedSceneLoad { source = Source.Task, task = handle, sceneName = sceneName };
        boxed.activateAction = activateScene;
        handle.ContinueWith(_ => boxed.completedInternal?.Invoke());
        return boxed;
    }
}