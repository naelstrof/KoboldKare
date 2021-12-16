using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCustomParts : MonoBehaviour
{
    public static Vector3 caveLaunchpadPos = new Vector3(74.6600037f, -62.1015511f, -18.0100002f);
 
    private static bool IsLoaded(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == name)
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityScriptableSettings.ScriptableSettingsManager sm = GetComponent<UnityScriptableSettings.ScriptableSettingsManager>();
        GameObject ob = gameObject.transform.Find("ReiikzManager").gameObject;
        if(ob == null) Debug.LogError("ReiikzManager Missing from GameManager");
        UnityScriptableSettings.ScriptableSetting[] settings = ob.GetComponent<MySettings>().settings;
        UnityScriptableSettings.ScriptableSetting[] newSettings = new UnityScriptableSettings.ScriptableSetting[sm.settings.Length + settings.Length];
        for(int x = 0; x < newSettings.Length; x++){
            if(x < sm.settings.Length){
                newSettings[x] = sm.settings[x];
            }else{
                newSettings[x] = settings[x - sm.settings.Length];
            }
        }
        sm.settings = newSettings;
        runMapCustoms();
        GameObject versionNumber = GameObject.Find("VersionNumber");
        if(versionNumber != null){
            TMPro.TextMeshProUGUI txt = versionNumber.GetComponent<TMPro.TextMeshProUGUI>();
            txt.text += "\n(PENIS)";
        }else{
            Debug.LogWarning("Could not find Version number game object");
        }
    }

    void Awake(){
        runMapCustoms();
    }

    void runMapCustoms(){
        if(IsLoaded("MainMap")){
            customizeMainMap();
            if(!IsLoaded("ReiikzMainMapAditions")) {
                SceneManager.LoadScene("ReiikzMainMapAditions", LoadSceneMode.Additive);
            }
        }
    }

    void customizeMainMap(){
        GameObject foundation = GameObject.Find("PlayerHouseConcreteFoundation");
        Destroy(foundation.transform.Find("default").GetComponent<MeshCollider>());
        Launchpad[] ls = (Launchpad[]) GameObject.FindObjectsOfType(typeof(Launchpad));
        if(ls.Length > 0){
            foreach(Launchpad l in ls){
                GameObject go = l.gameObject;
                if(Vector3.Distance(go.transform.position, caveLaunchpadPos) > 60){
                    GameObject.Destroy(go);
                }
            }
        }else{
            Debug.Log("No launchpads found");
        }
    }

    // Update is called once per frame
    // void Update()
    // {
        
    // }
}
