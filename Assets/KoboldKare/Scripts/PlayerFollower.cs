using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    private GameObject _player = null;

    // Update is called once per frame
    void Update()
    {
        if (_player == null ) {
            _player = GameObject.FindGameObjectWithTag("Player");
            return;
        }
        transform.position = _player.transform.position;
    }
}
