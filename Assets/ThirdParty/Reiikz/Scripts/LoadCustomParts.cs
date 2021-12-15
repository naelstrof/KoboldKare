using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCustomParts : MonoBehaviour
{
 
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
    void Awake()
    {
        yield return new WaitForSeconds(2.5f);
        if(IsLoaded("MainMap")){
            if(!IsLoaded("HeadPat")) {
                SceneManager.LoadScene("HeadPat", LoadSceneMode.Additive);
                Debug.Log("PENIS!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
