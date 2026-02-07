using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(Photon.Pun.PhotonView))]
public class GenericDoor : MonoBehaviour {
    public AudioClip openSFX, closeSFX;
    public Sprite openSprite, closeSprite;
    public VisualEffect activeWhenOpen;
    public AudioSource soundWhileOpen;
    public Animator animator;
    AudioSource audioSource;
    private int usedCount;
    
    private NetworkedEntity networkedEntity;
    protected bool opened {
        get { return (usedCount % 2) != 0; }
    }
    public virtual void Start(){
        audioSource = GetComponent<AudioSource>();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(opened ? closeSprite : openSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
            UpdateState();
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }

    // FIXME FISHNET
    // [PunRPC]
    private void OnUse(NetworkedKobold by) {
        usedCount++;
        UpdateState();
    }
    private void UpdateState() {
        if(opened){ 
            Open();
        } else{
            Close();
        }

        if (networkedEntity) {
            networkedEntity.SetSprite(opened ? closeSprite : openSprite);
        }
    }
    // These should never be called externally, use Use() to open and close.
    protected virtual void Open(){
        animator.SetBool("Open", true);
        audioSource.Stop();
        if (openSFX) {
            audioSource.PlayOneShot(openSFX);
        }
        if(activeWhenOpen != null) activeWhenOpen.SendEvent("Fire");
        if(soundWhileOpen != null) soundWhileOpen.Play();
    }
    protected virtual void Close(){
        animator.SetBool("Open", false);
        audioSource.Stop();
        if (closeSFX) {
            audioSource.PlayOneShot(closeSFX);
        }
        if(activeWhenOpen != null) activeWhenOpen.Stop();
        if(soundWhileOpen != null) soundWhileOpen.Stop();
    }
}
