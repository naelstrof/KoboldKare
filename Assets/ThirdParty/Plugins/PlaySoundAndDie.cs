using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAndDie : MonoBehaviour {
    public void PlayClip(AudioClip c, float volume = 1f) {
        AudioSource a = GetComponent<AudioSource>();
        //a.volume = volume;
        //a.clip = c;
        a.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        a.PlayOneShot(c, volume);
        StartCoroutine(WaitAndDie(c.length + 0.1f));
    }
    private IEnumerator WaitAndDie(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        this.gameObject.SetActive(false);
    }
}
