using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnPress : MonoBehaviour {
    public GameObject what;
    public void Execute() {
        Destroy(what);
    }
}
