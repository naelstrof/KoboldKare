using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIK : MonoBehaviour {
    private Animator animator;
    private List<Hand> hands = new List<Hand>();
    private class Hand {
        public float positionWeight = 0f;
        public float rotationWeight = 0f;
        public Coroutine routine;
        public Vector3 position;
        public Quaternion goalRotation;
        public bool transitioning = false;
    }
    public IEnumerator WeightUp(int hand) {
        while (hands[hand].positionWeight != 1f) {
            hands[hand].positionWeight = Mathf.MoveTowards(hands[hand].positionWeight, 1f, Time.deltaTime);
            hands[hand].rotationWeight = Mathf.MoveTowards(hands[hand].rotationWeight, 1f, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        hands[hand].transitioning = false;
    }
    public IEnumerator WeightDown(int hand) {
        while (hands[hand].positionWeight != 0f) {
            hands[hand].positionWeight = Mathf.MoveTowards(hands[hand].positionWeight, 0f, Time.deltaTime);
            hands[hand].rotationWeight = Mathf.MoveTowards(hands[hand].rotationWeight, 0f, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        hands[hand].transitioning = false;
    }
    private void Awake() {
        animator = GetComponent<Animator>();
        hands.Add(new Hand());
        hands.Add(new Hand());
    }
    public void SetIKTarget(int hand, Vector3 position, Quaternion rotation) {
        hands[hand].position = position;
        hands[hand].goalRotation = rotation;
        if (!hands[hand].transitioning && hands[hand].positionWeight != 1f) {
            hands[hand].routine = StartCoroutine(WeightUp(hand));
            hands[hand].transitioning = true;
        }
    }
    public void UnsetIKTarget(int hand) {
        if (hands[hand].transitioning && hands[hand].routine != null) {
            StopCoroutine(hands[hand].routine);
        }
        StartCoroutine(WeightDown(hand));
    }
    private void OnAnimatorIK(int layerIndex) {
        if (!isActiveAndEnabled) {
            return;
        }

        animator.SetIKPosition(AvatarIKGoal.LeftHand, hands[0].position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, hands[0].goalRotation);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, hands[0].positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, hands[0].rotationWeight);

        animator.SetIKPosition(AvatarIKGoal.RightHand, hands[1].position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, hands[1].goalRotation);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, hands[1].positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, hands[1].rotationWeight);
    }
}
