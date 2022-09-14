using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectiveUIDisplay : MonoBehaviour {
    [SerializeField] private Image starImage;
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

    private void OnEnable() {
        if (paperRustleSource == null) {
            paperRustleSource = gameObject.AddComponent<AudioSource>();
            paperRustleSource.playOnAwake = false;
            paperRustleSource.spatialBlend = 0f;
            paperRustleSource.loop = false;
            paperRustleSource.enabled = false;
        }
        ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
        ObjectiveManager.AddObjectiveUpdatedListener(OnObjectiveUpdated);
        OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
    }

    private void OnDisable() {
        ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
        ObjectiveManager.RemoveObjectiveUpdatedListener(OnObjectiveUpdated);
    }

    void OnObjectiveUpdated(DragonMailObjective objective) {
        if (objective == null) {
            return;
        }

        title.text = objective.GetTitle();
        description.text = objective.GetTextBody();
    }

    private void OnObjectiveSwapped(DragonMailObjective objective) {
        if (isActiveAndEnabled) {
            StopAllCoroutines();
            StartCoroutine(ObjectiveSwapRoutine(objective));
        } else {
            if (objective == null) {
                scrollAnimator.SetBool(Rollout, false);
            } else {
                title.text = objective.GetTitle();
                description.text = objective.GetTextBody();
                starImage.gameObject.SetActive(!objective.autoAdvance);
                scrollAnimator.SetBool(Rollout, true);
            }
        }
    }

    private IEnumerator ObjectiveSwapRoutine(DragonMailObjective newObjective) {
        if (newObjective == null) {
            scrollAnimator.SetBool(Rollout, false);
            yield break;
        } else {
            if (scrollAnimator.GetBool(Rollout)) {
                scrollAnimator.SetBool(Rollout, false);
                yield return new WaitForSeconds(1.5f);
            }
            scrollAnimator.SetBool(Rollout, true);
        }

        starImage.gameObject.SetActive(!newObjective.autoAdvance);
        title.text = newObjective.GetTitle();
        description.text = newObjective.GetTextBody();
        scrollAnimator.SetBool(Rollout, true);
        paperRustleSource.enabled = true;
        paperRustle.Play(paperRustleSource);
        yield return new WaitForSeconds(paperRustleSource.clip.length + 0.1f);
        paperRustleSource.enabled = false;
    }
}
