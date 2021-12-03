using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(Photon.Pun.PhotonView))]
public class GenericDoor : GenericUsable{
    public AudioClip openSFX, closeSFX;
    public Sprite openSprite, closeSprite;
    public VisualEffect activeWhenOpen;
    public AudioSource soundWhileOpen;
    public Animator animator;
    AudioSource audioSource;
    bool openState;

    void Start(){
        displaySprite = openState? closeSprite : openSprite;
        audioSource = GetComponent<AudioSource>();
    }

    public void OpenClose(){ //TODO: Check conditions before allowing interaction
        if(openState){ 
            Close();
        }
        else{
            Open();
        }
        openState = !openState;
    }

    void Open(){
        animator.SetTrigger("Open");
        audioSource.Stop();
        audioSource.PlayOneShot(openSFX);
        displaySprite = closeSprite;
        if(activeWhenOpen != null) activeWhenOpen.SendEvent("Fire");
        if(soundWhileOpen != null) soundWhileOpen.Play();
    }
    void Close(){
        animator.SetTrigger("Close");
        audioSource.Stop();
        audioSource.PlayOneShot(closeSFX);
        displaySprite = openSprite;
        if(activeWhenOpen != null) activeWhenOpen.Stop();
        if(soundWhileOpen != null) soundWhileOpen.Stop();
    }
}
