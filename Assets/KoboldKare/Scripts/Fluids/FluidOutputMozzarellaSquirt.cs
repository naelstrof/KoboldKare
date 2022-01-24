using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Mozzarella))]
public class FluidOutputMozzarellaSquirt : BaseStreamer, IFluidOutput {
    public enum OutputType {
        Squirt,
        Hose,
        Spray,
    }
    private bool firing = false;
    public bool isFiring { get { return firing; } }

    [SerializeField]
    private OutputType type;
    private MozzarellaRenderer mozzarellaRenderer;
    private FluidHitListener fluidHitListener;
    [Range(0.1f,10f)]
    [SerializeField]
    private float squirtDuration = 0.5f;
    [SerializeField]
    private AnimationCurve volumeCurve;
    [SerializeField]
    private AnimationCurve velocityCurve;
    [SerializeField][Range(0f,1f)]
    private float velocityMultiplier = 0.1f;
    [SerializeField][Range(0f,1f)]
    private float velocityVariance = 0f;
    private WaitForSeconds waitForSeconds;
    [SerializeField]
    private VisualEffect effect;
    [SerializeField][Range(0f,1f)]
    private float particleMultiplier = 1f;
    [SerializeField]
    private float volumeSprayedPerFire = float.MaxValue;
    [SerializeField]
    [FormerlySerializedAs("vps")]
    private float volumePerSecond = 2f;
    private float volumeSprayedThisFire = 0f;
    private Coroutine fireRoutine;
    protected class SquirtStream : BaseStreamer.Stream {
        private float startTime;
        private float duration;
        private float offset;
        public SquirtStream(float startTime, float duration, float offset) {
            this.startTime = startTime;
            this.duration = duration;
            this.offset = offset;
        }
        public bool IsFinished() {
            return Time.time > startTime + duration;
        }
        public override Mozzarella.Point CreatePoint(BaseStreamer streamer, float time) {
            if (!(streamer is FluidOutputMozzarellaSquirt)) {
                throw new UnityException("Tried to use a squirter stream on a non-squirter behavior.");
            }
            float t = Mathf.Clamp01((time-startTime)/duration);
            float perlin = Mathf.PerlinNoise(offset+time,offset+time);
            FluidOutputMozzarellaSquirt squirter = streamer as FluidOutputMozzarellaSquirt;
            Vector3 velocity = Vector3.up*0.025f+squirter.transform.forward*squirter.velocityCurve.Evaluate(t)*(0.75f+perlin*0.25f)*squirter.velocityMultiplier+UnityEngine.Random.insideUnitSphere*squirter.velocityVariance;
            float volume = squirter.volumeCurve.Evaluate(t)*(0.75f+perlin*0.25f);
            return new Mozzarella.Point() {
                position = squirter.transform.position,
                prevPosition = squirter.transform.position - velocity,
                volume = volume,
            };
        }
    }
    protected class HoseStream : BaseStreamer.Stream {
        public float offset;
        public HoseStream(float offset) {
            this.offset = offset;
        }
        public override Mozzarella.Point CreatePoint(BaseStreamer streamer, float time) {
            if (!(streamer is FluidOutputMozzarellaSquirt)) {
                throw new UnityException("Tried to use a hose stream on a non-hose behavior.");
            }
            FluidOutputMozzarellaSquirt hose = streamer as FluidOutputMozzarellaSquirt;
            Vector3 velocity = Vector3.up*0.025f+hose.transform.forward*hose.velocityMultiplier+UnityEngine.Random.insideUnitSphere*hose.velocityVariance;
            float volume = Mathf.Clamp01(Mathf.Abs(Mathf.Sin((time+offset)*2f)));
            return new Mozzarella.Point() {
                position = hose.transform.position,
                prevPosition = hose.transform.position - velocity,
                volume = volume,
            };
        }
    }

    public void Fire() {
        var container = GetComponentInParent<GenericReagentContainer>();
        if (container != null) {
            Fire(container);
        }
    }
    public void Fire(GenericReagentContainer b) {
        if (b.volume <= 0f){ 
            return;
        }
        if (firing) {
            return;
        }
        if (fireRoutine != null) {
            StopCoroutine(fireRoutine);
        }
        switch(type) {
            case OutputType.Spray:
            case OutputType.Hose:
            effect.Play();
            break;
            case OutputType.Squirt: break;
        }
        fireRoutine = StartCoroutine(FireRoutine(b));
    }
    public void StopFiring() {
        streams.Clear();
        effect.Stop();
        volumeSprayedThisFire = 0f;
        firing = false;
        if (fireRoutine != null) {
            StopCoroutine(fireRoutine);
        }
    }
    public override void FixedUpdate() {
        for (int i=streams.Count-1;i>=0;i--) {
            if (streams[i] is SquirtStream && (streams[i] as SquirtStream).IsFinished()) {
                streams.RemoveAt(i);
            }
        }
        base.FixedUpdate();
    }
    public override void Awake() {
        base.Awake();
        effect.Stop();
        waitForSeconds = new WaitForSeconds(squirtDuration*1.25f);
        mozzarellaRenderer = GetComponent<MozzarellaRenderer>();
        fluidHitListener = GetComponent<FluidHitListener>();
    }
    IEnumerator FireRoutine(GenericReagentContainer b) {
        firing = true;
        float logscale = Mathf.Log(1f+Mathf.Min(b.volume*0.5f, volumePerSecond));
        int wantedStreamCount = Mathf.RoundToInt(logscale*2f);
        mozzarellaRenderer.SetPointRadius(logscale*0.16f);
        fluidHitListener.decalSize = logscale*0.16f;
        if (type == OutputType.Squirt) {
            velocityMultiplier = 0.04f+logscale*0.02f;
            squirtDuration = Mathf.Min(0.1f + logscale*0.4f,1f);
            velocityVariance = logscale*0.04f;
        }
        while(volumeSprayedThisFire < volumeSprayedPerFire && b.volume > 0f) {
            Color c = b.GetColor();
            fluidHitListener.erasing = b.IsCleaningAgent();
            effect.SetVector4("Color", c);
            mozzarellaRenderer.material.color = c;
            fluidHitListener.projector.color = c;
            particlesPerSecondPerStream = Mathf.RoundToInt(50f+((float)wantedStreamCount)*42f*particleMultiplier);
            switch(type) {
                case OutputType.Squirt: {
                    SplashTransfer(b, volumePerSecond*squirtDuration);
                    volumeSprayedThisFire += volumePerSecond * squirtDuration;
                    for(int i=streams.Count;i<wantedStreamCount;i++) {
                        streams.Add(new SquirtStream(Time.time, squirtDuration, Time.time*i*100));
                    }
                    mozzarella.SetVisibleUntil(Time.time + squirtDuration + 5f);
                    yield return new WaitForSeconds(squirtDuration);
                    StopFiring();
                    break;
                }
                case OutputType.Hose: {
                    SplashTransfer(b, volumePerSecond*Time.deltaTime);
                    volumeSprayedThisFire += volumePerSecond * Time.deltaTime;
                    for(int i=streams.Count;i<wantedStreamCount;i++) {
                        streams.Add(new HoseStream(Time.time + i*5f));
                    }
                    mozzarella.SetVisibleUntil(Time.time + 5f);
                    break;
                }
                case OutputType.Spray: {
                    SplashTransfer(b, volumePerSecond*Time.deltaTime);
                    volumeSprayedThisFire += volumePerSecond*Time.deltaTime;
                    for(int i=streams.Count;i<wantedStreamCount;i++) {
                        streams.Add(new HoseStream(Time.time + i*5f));
                    }
                    mozzarella.SetVisibleUntil(Time.time + 5f);
                    break;
                }
            }
            yield return null;
        }
        StopFiring();
    }
    void SplashTransfer(GenericReagentContainer b, float amount) {
        fluidHitListener.transferContents.AddMix(b.Spill(amount));
    }
    void OnValidate() {
        volumeCurve.postWrapMode = WrapMode.Clamp;
        volumeCurve.preWrapMode = WrapMode.Clamp;
        velocityCurve.postWrapMode = WrapMode.Clamp;
        velocityCurve.preWrapMode = WrapMode.Clamp;
    }
    public void SetVolumePerSecond(float newVPS) {
        volumePerSecond = newVPS;
    }
}
