using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewDickData", menuName = "Data/DickData", order = 1)]
public class DickData : ScriptableObject {
    public AnimationCurve ballsScaleCurve;
    public AnimationCurve ballsRotateCurve;
    public AnimationCurve ballsTranslateCurve;
    public AnimationCurve bouncyCurve;
    public float ballsTranslationOffset;
    public float ballsVolumeDivisor;
    public AnimationCurve zeroToOneLogCurve;
    public AnimationCurve fullFlaccidCurve;
    public string fullFlaccidName;
    public AnimationCurve partialFlaccidCurve;
    public string partialFlaccidName;
    public AnimationCurve jiggleElasticityCurve;
    public AnimationCurve jiggleFrictionCurve;
    public Vector3 jiggleGravity;
    public AnimationCurve arousalCurve;
    public AnimationCurve arousalBounceCurve;
    public string biggerBallsBlendshapeName;
    public string confineBlendshapeName;
    public string squishBlendshapeName;
    public string pullBlendshapeName;
    public string cumBlendshapeName;
    public List<AudioClip> pumpingSounds;
    public List<AudioClip> plappingSounds;
    public AudioClip slidingSound;
    public string penetratorLayerName;
    public GameObject outputStreamPrefab;
    public Material strandMaterial;
    public AnimationCurve lodScreenTransitionHeightCurve;
    public float dickScaleVolumeDivisor;
    public AnimationCurve dickScaleCurve;
    public AnimationCurve dickRotateCurve;
    public AnimationCurve dickTranslateCurve;
    public AnimationCurve dildoJiggleFrictionCurve;
    public AnimationCurve dildoJiggleElasticCurve;
}
