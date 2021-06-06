using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAnimationDestroy : MonoBehaviour {
    public float delay = 0f;
    public GameObject destroy;
    void Start() {
        Destroy(destroy, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }
}
