using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;
using System.Collections;
using System;
using System.Threading.Tasks;
using SimpleJSON;
using Steamworks;

namespace FishNet.Managing.Scened {
    public class KoboldKareSceneProcessor : SceneProcessorBase {
        #region Private.
        /// <summary>
        /// Currently active loading AsyncOperations.
        /// </summary>
        protected List<Task> LoadingAsyncOperations = new();
        protected List<BoxedSceneLoad> AwaitingActivation = new();
        /// <summary>
        /// A collection of scenes used both for loading and unloading.
        /// </summary>
        protected List<UnityScene> Scenes = new();
        /// <summary>
        /// Current AsyncOperation being processed.
        /// </summary>
        protected Task CurrentTask;

        /// <summary>
        /// Last scene to load or begin loading.
        /// </summary>
        private UnityScene _lastLoadedScene;
        #endregion

        /// <summary>
        /// Called when scene loading has begun.
        /// </summary>
        public override void LoadStart(LoadQueueData queueData) {
            base.LoadStart(queueData);
            var bytes = queueData.SceneLoadData.Params;
            var mapLoadInfo = JSONNode.Parse(bytes.ToString());
            if (!mapLoadInfo.HasKey("mods")) {
                throw new Exception($"No mods key found in map load info: {mapLoadInfo.ToString()}");
            }

            List<ModManager.ModStub> modList = new List<ModManager.ModStub>();
            foreach (var node in mapLoadInfo["mods"]) {
                if (ulong.TryParse(node.Value["id"], out var id)) {
                    modList.Add(new ModManager.ModStub(node.Value["title"], (PublishedFileId_t)id, ModManager.ModSource.Any, node.Value["id"]));
                }
            }
            _ = ModManager.SetLoadedMods(modList);
            ResetValues();
        }

        public override void LoadEnd(LoadQueueData queueData) {
            base.LoadEnd(queueData);
            ResetValues();
        }

        /// <summary>
        /// Resets values for a fresh load or unload.
        /// </summary>
        private void ResetValues() {
            CurrentTask = null;
            LoadingAsyncOperations.Clear();
            AwaitingActivation.Clear();
        }

        /// <summary>
        /// Called when scene unloading has begun within an unload operation.
        /// </summary>
        /// <param name = "queueData"></param>
        public override void UnloadStart(UnloadQueueData queueData) {
            base.UnloadStart(queueData);
            Scenes.Clear();
        }

        private async Task LoadAsyncTask(string sceneName, UnityEngine.SceneManagement.LoadSceneParameters parameters) {
            while (!ModManager.GetFinishedLoading()) {
                await Task.Delay(1000);
            }

            if (ModManager.GetFailedToLoadMods()) {
                InstanceFinder.ClientManager.StopConnection();
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
                PopupHandler.instance.SpawnPopup("ModLoadFailed");
                return;
            }
            if (!PlayableMapDatabase.TryGetPlayableMap(sceneName, out var map)) {
                InstanceFinder.ClientManager.StopConnection();
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
                PopupHandler.instance.SpawnPopup("FailedLoad");
                return;
            }
            var handle = map.LoadAsync();
            var task = new TaskCompletionSource<bool>();
            handle.OnCompleted += () => {
                task.SetResult(true);
                AwaitingActivation.Add(handle);
            };
            await task.Task;
        }

        /// <summary>
        /// Begin loading a scene using an async method.
        /// </summary>
        /// <param name = "sceneName">Scene name to load.</param>
        public override void BeginLoadAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneParameters parameters) {
            var task = LoadAsyncTask(sceneName, parameters);
            LoadingAsyncOperations.Add(task);
            _lastLoadedScene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
            CurrentTask = task;
        }

        /// <summary>
        /// Begin unloading a scene using an async method.
        /// </summary>
        /// <param name = "sceneName">Scene name to unload.</param>
        public override void BeginUnloadAsync(UnityScene scene) {
            CurrentTask = UnitySceneManager.UnloadSceneAsync(scene).AsTask();
        }

        /// <summary>
        /// Returns if a scene load or unload percent is done.
        /// </summary>
        /// <returns></returns>
        public override bool IsPercentComplete() {
            return GetPercentComplete() >= 0.9f;
        }

        /// <summary>
        /// Returns the progress on the current scene load or unload.
        /// </summary>
        /// <returns></returns>
        public override float GetPercentComplete() {
            if (CurrentTask != null) return CurrentTask.IsCompleted ? 1f : 0f;
            return 1f;
        }

        /// <summary>
        /// Gets the scene last loaded by the processor.
        /// </summary>
        /// <remarks>This is called after IsPercentComplete returns true.</remarks>
        public override UnityScene GetLastLoadedScene() => _lastLoadedScene;

        /// <summary>
        /// Adds a loaded scene.
        /// </summary>
        /// <param name = "scene">Scene loaded.</param>
        public override void AddLoadedScene(UnityScene scene)
        {
            base.AddLoadedScene(scene);
            Scenes.Add(scene);
        }

        /// <summary>
        /// Returns scenes which were loaded during a load operation.
        /// </summary>
        public override List<UnityScene> GetLoadedScenes()
        {
            return Scenes;
        }

        /// <summary>
        /// Activates scenes which were loaded.
        /// </summary>
        public override void ActivateLoadedScenes()
        {
            for (int i = 0; i < AwaitingActivation.Count; i++) {
                try {
                    AwaitingActivation[i].ActivateScene();
                } catch (Exception e) {
                    SceneManager.NetworkManager.LogError($"An error occured while activating scenes. {e.Message}");
                }
            }
            AwaitingActivation.Clear();
        }

        /// <summary>
        /// Returns if all asynchronized tasks are considered IsDone.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator AsyncsIsDone() {
            bool notDone;
            do {
                notDone = false;
                foreach (var ao in LoadingAsyncOperations) {
                    if (!ao.IsCompleted) {
                        notDone = true;
                        break;
                    }
                }
                yield return null;
            } while (notDone);
        }
    }
}