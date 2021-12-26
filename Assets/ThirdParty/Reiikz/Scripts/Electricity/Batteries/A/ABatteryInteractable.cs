using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABatteryInteractable : GenericUsable
{
    public Sprite icon;
    public GameObject rootObject;
    public GameObject ABatteryCan;
    public GameObject ABatteryLid;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Sprite GetSprite(Kobold k) {
        return icon;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        Instantiate(ABatteryCan, rootObject.transform.position, rootObject.transform.rotation);
        Destroy(rootObject);
    }
}
