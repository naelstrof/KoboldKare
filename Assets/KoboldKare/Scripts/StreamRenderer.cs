using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class StreamRenderer : FluidOutput {
    private Material material;
    private Quaternion additiveRotation = Quaternion.identity;
    private Quaternion lastRotation;
    private Vector3 lastPosition;
    private Vector3 additivePosition;
    private float cullEnd = 0f;
    private float cullStart = 0f;
    public bool fluidsOnly = false;
    public float spring = 0.5f;
    public float damping = 25f;
    public float maxDelta = 15f;
    public float translationSpring = 0.5f;
    public float translationDamping = 25f;
    [Range(1,32)]
    public int rayCasts = 10;
    public float velocity = 10f;
    public float gravity = 10f;
    public bool isOn = false;
    public Material splashMaterial;
    private bool readyToFire = false;
    private bool fireAsap = false;
    private ReagentContents fireAsapSource;
    public VisualEffect spray;
    public VisualEffect splash;
    //public VisualEffect splashBits;
    public AudioSource spraySound;
    private AudioSource splashSound;
    public AudioClip goodSplashClip;
    public AudioClip badSplashClip;
    public float radius = 0.1f;
    private float realRadius {
        get {
            if (bucketSource != null && bucketSource.volume < 1) {
                return Mathf.Min(radius, bucketSource.volume * radius);
            }
            return Mathf.Max(radius,0.01f);
        }
    }
    public float penetrationDistance = 0.1f;
    public float streamForce = 10f;
    private int decalTick = 0;
    private int decalRate = 2;
    //private RaycastHit[] cachedHits = new RaycastHit[5];
    private float originalSoundVolume = 1f;
    IEnumerator WaitAndPause() {
        yield return new WaitForSeconds(0.5f);
        spray.SendEvent("OnStop");
    }
    IEnumerator WaitAndDisable() {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
    public void Start() {
        additiveRotation = Quaternion.identity;
        lastRotation = transform.rotation;
        additivePosition = Vector3.zero;
        lastPosition = transform.position;
    }
    public Color color {
        set {
            Gradient colorGrad = new Gradient();
            splash.SetVector4("Color", Color.Lerp(value, Color.white, 0.3f));
            spray.SetVector4("Color", Color.Lerp(value, Color.white, 0.3f));
            if (material == null) {
                MeshRenderer r = GetComponent<MeshRenderer>();
                material = r.material;
            }
            material.color = value;
        }
    }
    private ReagentContents bucketSource;
    private ReagentContents midAirStuff = new ReagentContents();
    public float volumePerSecond = 1f;
    public void Fire(GameObject b) {
        if (b.GetComponentInParent<GenericReagentContainer>() != null) {
            Fire(b.GetComponentInParent<GenericReagentContainer>());
        }
    }
    public void Fire(GenericReagentContainer b) {
        Fire(b.contents, volumePerSecond);
    }
    public override void Fire(ReagentContents b, float vps) {
        if (isOn || b.volume <= 0f) {
            return;
        }
        if (!gameObject.activeInHierarchy) {
            additiveRotation = Quaternion.identity;
            lastRotation = transform.rotation;
            additivePosition = Vector3.zero;
            lastPosition = transform.position;
            if (material == null) {
                MeshRenderer r = GetComponent<MeshRenderer>();
                material = r.material;
            }
            material.SetFloat("_DeltaRotationAmount", 0);
            material.SetVector("_DeltaRotationAxis", Vector3.up);
            material.SetVector("_DeltaTranslation", Vector3.zero);
            material.SetFloat("_CullStart", 0);
            material.SetFloat("_CullEnd", 0);
            gameObject.SetActive(true);
        }
        StopAllCoroutines();
        if ( !readyToFire ) {
            fireAsapSource = b;
            fireAsap = true;
            return;
        }
        volumePerSecond = vps;
        bucketSource = b;
        //color = b.contents.GetColor(GameManager.instance.reagentDatabase);
        spray.Play();
        spray.SendEvent("OnStart");
        spraySound.Play();
        cullStart = 0f;
        cullEnd = 0f;
        isOn = true;
    }
    public override void StopFiring() {
        if ( !isOn ) {
            return;
        }
        spray.SendEvent("OnStop");
        spraySound.Pause();
        isOn = false;
        fireAsap = false;
    }
    void Awake() {
        transform.localScale = new Vector3(1/transform.lossyScale.x, 1/transform.lossyScale.y, 1/transform.lossyScale.z);
        GetComponent<MeshFilter>().sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        MeshRenderer r = GetComponent<MeshRenderer>();
        material = r.material;
        lastRotation = transform.rotation;
        originalSoundVolume = spraySound.volume;
        additiveRotation = Quaternion.identity;
        StopFiring();
        StartCoroutine(WaitAndPause());
        GameObject splashSoundObject = new GameObject("SplashSound", new System.Type[] { typeof(AudioSource) });
        splashSoundObject.transform.SetParent(transform);
        splashSound = splashSoundObject.GetComponent<AudioSource>();
        splashSound.rolloffMode = AudioRolloffMode.Logarithmic;
        splashSound.spatialBlend = 1f;
        splashSound.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        splashSound.loop = false;
    }
    bool RayCast(Vector3 start, Vector3 end, out float hitPoint, out Vector3 worldPoint, out Vector3 normal, out GameObject obj ) {
        RaycastHit info;
        Vector3 dir = Vector3.Normalize(end - start);
        float dist = Vector3.Distance(start, end);
        //int len = Physics.SphereCastNonAlloc(transform.position + start - dir*realRadius*4f, realRadius*4f, dir, cachedHits, dist + realRadius*4f, hitMask, QueryTriggerInteraction.Ignore);
        Debug.DrawLine(transform.position + start - dir *realRadius*4f, transform.position + end + dir *realRadius*4f, Color.red, Time.fixedDeltaTime, false);
        //float closestHit = float.MaxValue;
        //int closestIndex = -1;
        //for (int i=0;i<len;i++) {
        //info = cachedHits[i];
        //if (info.transform.root == transform.root) {
        //continue;
        //}
        //if (info.distance < closestHit) {
        //closestHit = info.distance;
        //closestIndex = i;
        //}
        //}
        //if (closestIndex != -1) {
        if (Physics.SphereCast(transform.position + start - dir * realRadius * 4f, realRadius * 4f, dir, out info, dist + realRadius * 4f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore)) {
            //info = cachedHits[closestIndex];
            hitPoint = (info.distance - realRadius * 4f) / dist + penetrationDistance;
            worldPoint = info.point + dir * penetrationDistance;
            normal = info.normal;
            obj = info.collider.gameObject;
            return true;
        }
        //}
        hitPoint = 0;
        worldPoint = Vector3.zero;
        normal = Vector3.up;
        obj = null;
        return false;
    }
    void StreamCast(Vector3 DeltaRotationAxis, float DeltaRotationAmount, Vector3 DeltaTranslation) {
        Vector3 lastPos = Vector3.zero;
        float lastSample = 0f;
        for (int i = 1; i <= rayCasts; i++) {
            float sample = (float)i * (float)(1f / (float)rayCasts);
            Vector3 pos = (transform.up * velocity * sample) - (sample * sample) * Vector3.up * gravity;
            if (sample < cullStart) {
                lastPos = pos;
                lastSample = sample;
                continue;
            }
            if (sample > cullEnd + (float)(1f / (float)rayCasts)) {
                return;
            }
            Quaternion rotate = Quaternion.Lerp(Quaternion.identity, Quaternion.AngleAxis(DeltaRotationAmount, DeltaRotationAxis), sample);
            pos = rotate * pos;
            pos += DeltaTranslation * sample;
            float hitPoint = 0;
            Vector3 worldPoint;
            Vector3 normal;
            GameObject obj;
            if ( RayCast(lastPos, pos, out hitPoint, out worldPoint, out normal, out obj) ) {
                //if ( obj.transform.root == transform.root ) {
                    //continue;
                //}
                cullEnd = Mathf.Min(cullEnd, lastSample + hitPoint * (float)(1f / (float)rayCasts));
                splash.transform.position = worldPoint;
                //splashBits.transform.position = worldPoint;
                //splash.transform.rotation = Quaternion.FromToRotation(splash.transform.up, normal) * splash.transform.rotation;
                //splashBits.transform.rotation = splash.transform.rotation;
                splash.transform.rotation = Quaternion.FromToRotation(splash.transform.up, normal) * splash.transform.rotation;
                ReagentContents r = midAirStuff.Spill(volumePerSecond * Time.fixedDeltaTime * 10f);
                if (obj) {
                    GenericReagentContainer[] hits = obj.GetComponentsInParent<GenericReagentContainer>();
                    bool filled = false;
                    foreach( GenericReagentContainer b in hits) {
                        if (b.contents != bucketSource) {
                            b.contents.Mix(r / hits.Length, ReagentContents.ReagentInjectType.Spray);
                            if (b.contents.volume >= b.contents.maxVolume) {
                                filled = true;
                            }
                        }
                    }
                    if (!splashSound.isPlaying) {
                        splashSound.transform.position = worldPoint;
                        splashSound.clip = filled ? badSplashClip : goodSplashClip;
                        splashSound.Play();
                    }
                    Rigidbody body = obj.gameObject.GetComponentInParent<Rigidbody>();
                    if (body != null) {
                        body.AddForceAtPosition(Vector3.Normalize(pos-lastPos)*streamForce, worldPoint);
                    }
                    if ( decalTick++ % decalRate == 0 ) {
                        Color c = r.GetColor(ReagentDatabase.instance);
                        if (r.volume <= 0) {
                            c = bucketSource.GetColor(ReagentDatabase.instance);
                        }
                        if (r.volume > 0f) {
                            if (r.ContainsKey(ReagentData.ID.Water) && r[ReagentData.ID.Water].volume > r.volume*0.9f) {
                                GameManager.instance.SpawnDecalInWorld(splashMaterial, worldPoint+normal*0.25f, -normal, Vector2.one*Mathf.Max(realRadius * 45f, 0.25f), c, obj, 0.5f, true, true, true);
                            } else {
                                GameManager.instance.SpawnDecalInWorld(splashMaterial, worldPoint+normal*0.25f, -normal, Vector2.one*Mathf.Max(realRadius * 45f, 0.25f), c, obj, 0.5f, true, true, false);
                            }
                        }
                    }
                }
                if (cullStart > cullEnd) {
                    cullStart = cullEnd;
                }
                //if (absorbed == 0f) {
                    splash.SendEvent("TriggerSplash");
                //}
                //if ( absorbed == 0f ) {
                    //splash.Emit(2);
                    //splashBits.Emit(2);
                //} else {
                    //splashBits.Emit(1);
                //}
                return;
            }
            lastPos = pos;
            lastSample = sample;
        }
    }
    void Update() {
        Quaternion deltaQuaternion = transform.rotation * Quaternion.Inverse(lastRotation);
        //float clampAngle;
        //Vector3 clampAxis;
        //deltaQuaternion.ToAngleAxis(out clampAngle, out clampAxis);
        //deltaQuaternion = Quaternion.AngleAxis(Mathf.Min(maxDelta, clampAngle*spring), clampAxis);
        lastRotation = transform.rotation;
        additiveRotation = Quaternion.Lerp(additiveRotation, Quaternion.identity, Time.deltaTime * damping);
        additiveRotation *= deltaQuaternion;
        float angle;
        Vector3 axis;
        additiveRotation.ToAngleAxis(out angle, out axis);
        if (angle >= 180) {
            angle = 360 - angle;
            axis = -axis;
        }
        if (float.IsNaN(axis.x+axis.y+axis.z) || float.IsInfinity(axis.x + axis.y + axis.z) || float.IsNaN(additivePosition.x) || float.IsNaN(additivePosition.y) || float.IsNaN(additivePosition.z)) {
            axis = Vector3.up;
            additiveRotation = Quaternion.identity;
            additivePosition = Vector3.zero;
        }
        material.SetFloat("_DeltaRotationAmount", -angle * (Mathf.PI/180f) * spring);
        material.SetVector("_DeltaRotationAxis", axis);

        //material.SetVector("_WorldUpDir", transform.InverseTransformDirection(Vector3.up));

        additivePosition += (transform.position - lastPosition) * translationSpring;
        additivePosition = Vector3.Lerp(additivePosition, Vector3.zero, Time.deltaTime * translationDamping);
        lastPosition = transform.position;
        material.SetVector("_DeltaTranslation", -additivePosition);

        if ( isOn ) {
            ReagentContents r;
            if (fluidsOnly) {
                r = bucketSource.FilterFluids(volumePerSecond * Time.fixedDeltaTime, ReagentDatabase.instance);
            } else {
                r = bucketSource.Spill(volumePerSecond * Time.fixedDeltaTime);
            }
            if (r.volume > 0f) {
                color = r.GetColor(ReagentDatabase.instance);
            }
            material.SetFloat("_Radius", realRadius);
            spray.SetFloat("NozzleRadius", realRadius*3f);
            splash.SetFloat("Radius", realRadius);
            midAirStuff.Mix(r);
            cullStart = 0f;
            cullEnd = Mathf.MoveTowards(cullEnd, 1f, Time.fixedDeltaTime);
            spraySound.pitch = Mathf.Max(1f, 1f/Mathf.Max(bucketSource.volume,0.01f));
            spraySound.volume = Mathf.Min(originalSoundVolume,Mathf.Max(originalSoundVolume, originalSoundVolume*(bucketSource.volume/bucketSource.maxVolume))*transform.lossyScale.x);
            if ( midAirStuff.volume <= Mathf.Epsilon ) {
                StopFiring();
            }
        } else {
            cullEnd = Mathf.MoveTowards(cullEnd, 1f, Time.fixedDeltaTime);
            cullStart = Mathf.MoveTowards(cullStart, cullEnd, Time.fixedDeltaTime);
            if ( cullEnd >= 1f && cullStart == cullEnd) {
                //gameObject.SetActive(false);
                StartCoroutine(WaitAndDisable());
            }
        }
        readyToFire = (cullStart == cullEnd);
        if (!readyToFire) {
            StreamCast(axis, -angle * spring, -additivePosition);
        }
        if (readyToFire && fireAsap) {
            midAirStuff.Clear();
            Fire(fireAsapSource, volumePerSecond);
            fireAsap = false;
            fireAsapSource = null;
        }

        material.SetFloat("_CullEnd", cullEnd);
        material.SetFloat("_CullStart", cullStart);
    }
}
