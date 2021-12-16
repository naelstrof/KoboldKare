using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PatMeFrame : GenericUsable
{
    public Sprite patSprite;
    public float amont = 1f;
    public float useEach = 1f;
    //public static Dictionary<int, float> usedby = new Dictionary<int, float>();
    public float usedBy = 0f;
    public Animator anim;
    private float nextUpdate = 0f;
    public float pettingPower = 0f;
    public float petPowerIncStep = 0.01f;
    public TMPro.TextMeshProUGUI textBubble;
    public float nextVanishText = 0f;

    static string[] phrases = { "Pat me", "BRRR", "rer", "rawr", "penis", "jej", "pat pats", "run", "69420 NICE", ":v", "(8)8====>", "pat me", "gib pats", "pattity pat pat", "UwU", "OwO", "OWO", "UNO", "OnO", "UnU", "rawr XD", "<3", "https://youtu.be/EWMPVn1kgIQ", "GIB PATS!", "NEED PATS", "RUB ME!", "RUB MY PP", "GAMING 2008", "MyMsix MP3 player", "I'm currently on the run from the police for multiple home invasions between 2003-2007", "nun", "unu", "PLEASE, DON'T TOUCH ME!", "one day I'll break out of this wood and take over this world", "I'm watching you all have sex" };
    static string[] pattedPhrases = { "NICE COCK!", "nice color", "what an ugly color", "*BRRRRRR*", "*BRRR*", "*BRRRRRRRRR*", "*PURRING INTENSIFIES*", "nice cock you got there", "I like your penis", "NEVER GONNA GIVE YOU UP\nALWAYS GONNA FUCK YOU HARD\nAND IMPALE YOU", "AHHHHHHHH\nHow dare you!", "Don't touch me!", "I see you like my penis", "OwO", "Patttt go BRRRRRR", "ME PATS!", "GIB MOAR", "AH, *BRRRRRR*", "E", "REEEEEEE", "Nice pat", "*BURP*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*", "*BRRRRRR*" };
    static int[] probOfTalking = { 5000, 1 };

    // Start is called before the first frame update
    void Start()
    {
        anim.Play("Idle");
        textBubble.text="";
    }

    // Update is called once per frame
    void Update()
    {
        if(nextUpdate <= Time.timeSinceLevelLoad){
            if((usedBy + useEach) <= Time.timeSinceLevelLoad){
                anim.Play("Idle");
            }
            nextUpdate = Time.timeSinceLevelLoad + 1;
            if(nextVanishText < Time.timeSinceLevelLoad){
                textBubble.text = "";
            }
            if(RandomChoice.WeightedIndex(probOfTalking) == 1){
                int phrase = System.Convert.ToInt32(UnityEngine.Random.Range(0f, phrases.Length - 1));
                talk(phrases[phrase]);
            }
        }
    }
    public void talk(string s){ talk(s, false); }
    public void talk(string s, bool replace){
        if(Time.timeSinceLevelLoad < nextVanishText && !replace) return;
        textBubble.text = s;
        nextVanishText = Time.timeSinceLevelLoad + 5;
    }
    public override Sprite GetSprite(Kobold k) {
        return patSprite;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        KoboldInventory inventory = k.GetComponent<KoboldInventory>();
        if (inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) == null){
            while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),false);
            }
            inventory.PickupEquipment(EquipmentDatabase.GetEquipment("KandiDick"), null);
        }
        int pid = k.GetComponent<PhotonView>().ViewID;
        if((usedBy + useEach) <= Time.timeSinceLevelLoad || usedBy == 0){
            if(k.baseDickSize < 20){
                k.baseDickSize = 20;
            }else{
                k.baseDickSize *= amont;
                if(pettingPower < 5) k.baseDickSize += pettingPower;
            }
            usedBy = Time.timeSinceLevelLoad;
            k.bellies[0].GetContainer().AddMix(ReagentDatabase.GetReagent("EggplantJuice"), pettingPower, GenericReagentContainer.InjectType.Metabolize);
            if((usedBy + (useEach*1.5)) <= Time.timeSinceLevelLoad) pettingPower += (petPowerIncStep*2f); else pettingPower += petPowerIncStep;
            k.arousal = 1;
            anim.Play("HeadPat");
            int phrase = System.Convert.ToInt32(UnityEngine.Random.Range(0f, pattedPhrases.Length - 1));
            talk(pattedPhrases[phrase], true);
        }
    }
}
