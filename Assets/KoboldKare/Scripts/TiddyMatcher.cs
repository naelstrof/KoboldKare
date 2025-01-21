using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiddyMatcher : MonoBehaviour
{
    [SerializeField]
    private Kobold kobold;
    [SerializeField]
    private Transform smallTransform;
    [SerializeField]
    private Transform bigTransform;
    [SerializeField]
    private Transform ownTransform;

    private float lastSize=-999f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition=new Vector3(ownTransform.localPosition.x,ownTransform.localPosition.y,ownTransform.localPosition.z);  
        initialRotation=new Quaternion(ownTransform.localRotation.x,ownTransform.localRotation.y,ownTransform.localRotation.z,ownTransform.localRotation.w);
  
        initialScale=new Vector3(ownTransform.localScale.x,ownTransform.localScale.y,ownTransform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(kobold.GetGenes().breastSize!= lastSize)
        {   float newSize=kobold.GetGenes().breastSize;
            if(newSize<20)
                {
                ownTransform.localPosition=Vector3.Lerp(smallTransform.localPosition,initialPosition,newSize/20f);
                ownTransform.localRotation=Quaternion.Lerp(smallTransform.localRotation,initialRotation,Mathf.Clamp(newSize/20f,0,1));
                ownTransform.localScale=Vector3.Lerp(smallTransform.localScale,initialScale,newSize/20f);
                }
            else
                {
                ownTransform.localPosition=Vector3.Lerp(initialPosition,bigTransform.localPosition,(newSize-20)/20f);
                ownTransform.localRotation=Quaternion.Lerp(initialRotation,bigTransform.localRotation,Mathf.Clamp((newSize-20)/20f,0,1));
                ownTransform.localScale=Vector3.Lerp(initialScale,bigTransform.localScale,(newSize-20)/20f);
                }
            lastSize=newSize;
        }
        
    }

}
