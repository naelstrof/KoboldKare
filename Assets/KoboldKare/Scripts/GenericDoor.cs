using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(Photon.Pun.PhotonView))]
public class GenericDoor : GenericUsable, IPunObservable, ISavable {
    public AudioClip openSFX, closeSFX;
    public Sprite openSprite, closeSprite;
    public VisualEffect activeWhenOpen;
    public AudioSource soundWhileOpen;
    public Animator animator;
    AudioSource audioSource;
    private int usedCount;
    protected bool opened {
        get { return (usedCount % 2) != 0; }
    }
    public virtual void Start(){
        audioSource = GetComponent<AudioSource>();
        UpdateState();
    }
    public override Sprite GetSprite(Kobold k) {
        return opened ? closeSprite : openSprite;
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        usedCount++;
        UpdateState();
    }
    private void UpdateState() {
        if(opened){ 
            Open();
        } else{
            Close();
        }
    }
    // These should never be called externally, use Use() to open and close.
    protected virtual void Open(){
        animator.SetBool("Open", true);
        audioSource.Stop();
        audioSource.PlayOneShot(openSFX);
        if(activeWhenOpen != null) activeWhenOpen.SendEvent("Fire");
        if(soundWhileOpen != null) soundWhileOpen.Play();
    }
    protected virtual void Close(){
        animator.SetBool("Open", false);
        audioSource.Stop();
        audioSource.PlayOneShot(closeSFX);
        if(activeWhenOpen != null) activeWhenOpen.Stop();
        if(soundWhileOpen != null) soundWhileOpen.Stop();
    }
}
