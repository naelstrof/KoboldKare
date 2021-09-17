using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAudio : MonoBehaviour {
    public float maxVolume = 1f;
    private AudioSource scrapeSoundOutput;
    private AudioSource impactSoundOutput;
    private float soundDelay = 0.2f;
    private float lastSoundTime = 0f;
    private Rigidbody body;
    public IEnumerator WaitOneFrameThenPause() {
        yield return null;
        scrapeSoundOutput.volume = 0f;
        scrapeSoundOutput.Pause();
    }
    private void Start() {
        body = GetComponent<Rigidbody>();
        var steamAudioSetting = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SteamAudio");
        scrapeSoundOutput = gameObject.AddComponent<AudioSource>();
        scrapeSoundOutput.spatialBlend = 1f;
        scrapeSoundOutput.rolloffMode = AudioRolloffMode.Custom;
        scrapeSoundOutput.minDistance = 0f;
        scrapeSoundOutput.maxDistance = 25f;
        scrapeSoundOutput.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        scrapeSoundOutput.spatialize = steamAudioSetting.value > 0f;
        scrapeSoundOutput.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;

        impactSoundOutput = gameObject.AddComponent<AudioSource>();
        impactSoundOutput.spatialBlend = 1f;
        impactSoundOutput.rolloffMode = AudioRolloffMode.Logarithmic;
        impactSoundOutput.spatialize = steamAudioSetting.value > 0f;
        impactSoundOutput.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        scrapeSoundOutput.minDistance = 0f;
        scrapeSoundOutput.maxDistance = 25f;
        impactSoundOutput.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        SteamAudio.SteamAudioSource steamaudio = gameObject.AddComponent<SteamAudio.SteamAudioSource>();
        steamaudio.enabled = steamAudioSetting.value > 0f;
        //steamaudio.physicsBasedAttenuation = true;
        //steamaudio.occlusionMode = SteamAudio.OcclusionMode.OcclusionWithFrequencyIndependentTransmission;
        //steamaudio.occlusionMode = SteamAudio.OcclusionMode.OcclusionWithNoTransmission;
    }
    void PlaySoundForCollider(Rigidbody thisBody, Rigidbody otherBody, Collider thisCollider, Collider otherCollider, Vector3 contact, Vector3 relativeVelocity, Vector3 impulse, bool self) {
        PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(thisCollider.sharedMaterial);
        if (group == null) {
            return;
        }
        float mag = relativeVelocity.magnitude;

        // kinematic bodies are always considered to not be moving, which can make really loud sound effects on players who are just carrying objects (and their kinematic limbs are smacking them)
        // The impulse doesn't suffer from being incorrect in these situations, so we just use it instead.
        if ((thisBody && thisBody.isKinematic) || (otherBody && otherBody.isKinematic)) {
            mag = impulse.magnitude;
        }

        AudioClip clip = group.GetImpactClip(otherCollider, mag);
        if (clip != null) {
            //if (self) {
                //impactSoundOutput.pitch = Random.Range(0.7f,1.3f);
                //impactSoundOutput.PlayOneShot(clip, Mathf.Min(maxVolume, group.GetImpactVolume(c, mag)));
            //} else {
                GameManager.instance.SpawnAudioClipInWorld(clip, contact, Mathf.Min(maxVolume, group.GetImpactVolume(otherCollider, mag)));
            //}
        }
    }
    void OnCollisionEnter(Collision collision) {
        if (body && body.isKinematic) {
            return;
        }
        ContactPoint cp = collision.GetContact(0);
        PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(cp.thisCollider.sharedMaterial);
        if (group == null) {
            return;
        }
        AudioClip scrapeclip = group.GetScrapeClip(cp.otherCollider);
        if (scrapeclip != null && scrapeSoundOutput != null) {
            scrapeSoundOutput.clip = scrapeclip;
            scrapeSoundOutput.loop = true;
        }

        // Don't play too many sounds or it sounds silly
        if (lastSoundTime != 0f && (Time.timeSinceLevelLoad - lastSoundTime) < soundDelay) {
            return;
        }
        lastSoundTime = Time.timeSinceLevelLoad;
        PlaySoundForCollider(body, collision.rigidbody, cp.thisCollider, cp.otherCollider, cp.point, collision.relativeVelocity, collision.impulse, true);
        if (cp.otherCollider.GetComponentInParent<PhysicsAudio>() == null) {
            PlaySoundForCollider(body, collision.rigidbody, cp.otherCollider, cp.thisCollider, cp.point, collision.relativeVelocity, collision.impulse, false);
        }
    }
    void OnCollisionStay(Collision collision) {
        if (body && body.isKinematic) {
            scrapeSoundOutput.volume = 0f;
            return;
        }
        // Don't scrape on kinematic bodies, usually this is just player hands bumping into carried objects.
        if (!scrapeSoundOutput.isPlaying && scrapeSoundOutput.isActiveAndEnabled && (!collision.rigidbody || !collision.rigidbody.isKinematic)) {
            scrapeSoundOutput.Play();
        }
        ContactPoint cp = collision.GetContact(0);
        PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(cp.thisCollider.sharedMaterial);
        if (group == null) {
            return;
        }
        scrapeSoundOutput.volume = Easing.Cubic.In(Mathf.Clamp01((collision.relativeVelocity.magnitude-group.minScrapeSpeed)));
    }
    void OnCollisionExit(Collision collision) {
        if (isActiveAndEnabled) {
            StopCoroutine("WaitOneFrameThenPause");
            StartCoroutine("WaitOneFrameThenPause");
        } else if (scrapeSoundOutput != null) {
            scrapeSoundOutput.volume = 0f;
            scrapeSoundOutput.Pause();
        }
    }
}
