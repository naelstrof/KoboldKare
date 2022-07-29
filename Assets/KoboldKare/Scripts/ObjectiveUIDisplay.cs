using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveUIDisplay : MonoBehaviour {
    [SerializeField]
    private Animator scrollAnimator;
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;

    private static readonly int Rollout = Animator.StringToHash("Rollout");

    private void OnEnable() {
        ObjectiveManager.AddObjectiveChangeListener(OnObjectiveChanged);
        OnObjectiveChanged(ObjectiveManager.GetCurrentObjective());
    }

    private void OnDisable() {
        ObjectiveManager.RemoveObjectiveChangeListener(OnObjectiveChanged);
    }

    private void OnObjectiveChanged(DragonMailObjective objective) {
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
        scrollAnimator.SetBool(Rollout, false);
        yield return new WaitForSeconds(2f);
        title.text = newObjective.GetTitle();
        description.text = newObjective.GetTextBody();
        scrollAnimator.SetBool(Rollout, true);
    }
}
