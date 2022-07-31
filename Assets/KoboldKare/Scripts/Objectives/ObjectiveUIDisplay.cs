using System.Collections;
using UnityEngine;
using TMPro;

public class ObjectiveUIDisplay : MonoBehaviour {
    [SerializeField]
    private Animator scrollAnimator;
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;
    [SerializeField]
    private AudioPack paperRustle;

    private AudioSource paperRustleSource;

    private static readonly int Rollout = Animator.StringToHash("Rollout");

    void Start() {
        if (paperRustleSource == null) {
            paperRustleSource = gameObject.AddComponent<AudioSource>();
            paperRustleSource.playOnAwake = false;
            paperRustleSource.spatialBlend = 0f;
            paperRustleSource.loop = false;
            paperRustleSource.enabled = false;
        }
    }

    private void OnEnable() {
        ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
        ObjectiveManager.AddObjectiveUpdatedListener(OnObjectiveUpdated);
        OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
    }

    private void OnDisable() {
        ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
        ObjectiveManager.RemoveObjectiveUpdatedListener(OnObjectiveUpdated);
    }

    void OnObjectiveUpdated(DragonMailObjective objective) {
        title.text = objective.GetTitle();
        description.text = objective.GetTextBody();
    }

    private void OnObjectiveSwapped(DragonMailObjective objective) {
        if (isActiveAndEnabled) {
            StopAllCoroutines();
            StartCoroutine(ObjectiveSwapRoutine(objective));
        } else {
            title.text = objective.GetTitle();
            description.text = objective.GetTextBody();
            scrollAnimator.SetBool(Rollout, true);
        }
    }

    private IEnumerator ObjectiveSwapRoutine(DragonMailObjective newObjective) {
        if (newObjective == null) {
            scrollAnimator.SetBool(Rollout, false);
            yield break;
        } else {
            scrollAnimator.SetBool(Rollout, true);
        }
        title.text = newObjective.GetTitle();
        description.text = newObjective.GetTextBody();
        scrollAnimator.SetBool(Rollout, true);
        paperRustleSource.enabled = true;
        paperRustle.Play(paperRustleSource);
        yield return new WaitForSeconds(paperRustleSource.clip.length + 0.1f);
        paperRustleSource.enabled = false;
    }
}
