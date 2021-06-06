using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class DieScript : MonoBehaviour
{
    public GameEvent PlayerRespawn;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Grab")) {
            PlayerRespawn.Raise();
            Destroy(gameObject);
        }
    }
}
