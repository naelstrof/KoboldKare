using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
public class FoxComReskin : MonoBehaviour
{



    #region Inspector related

    static bool doNotSpawn; 

    [SerializeField] GameObject selfSpawner;
    [SerializeField] GameObject inventoryTabSpawner;
    [SerializeField] GameObject questFillerSpawner;
    [SerializeField] GameObject checkerObjectSpawner; 
    [SerializeField] TMPro.TMP_FontAsset fontAsset;
    [SerializeField] RuntimeAnimatorController questANIM;
    [Space(20)]

    [SerializeField] List<UIElements> elements;
    [Space(20)]
    public Sprite[] keyIcons;
    public Sprite[] hudIcons;
    [Space(20)]
    [Header("DEBUG")]
    [SerializeField] Sprite softBlackBox;

    #endregion
   



    void Start()
    { 
        if (!doNotSpawn)
        {
            doNotSpawn = true;
            Instantiate(selfSpawner);
            print("[FoxCom] Component set doNotSpawn = true");
            Destroy(GetComponent<FoxComReskin>());
            return;
        }
        if (gameObject.name == "Fox's Enhancer Loaded")
        {
            Destroy(GetComponent<FoxComReskin>());
            Debug.LogWarning("[FoxCom] Found a clone! Deleting");
        }
        gameObject.name = "Fox's Enhancer Loaded";
        DontDestroyOnLoad(gameObject);
        StartCoroutine(ActivateEnhancer());
    }   //Might look nicer, but naaah, It works


    IEnumerator ActivateEnhancer()
    {
        GameObject checker;
        while (true)
        {
            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu");
            yield return new WaitForSeconds(2);
            yield return new WaitUntil(() => GameObject.Find("Canvas - FPSView") != null);
            RedrawFoxUI();
            checker = Instantiate(checkerObjectSpawner, GameObject.Find("Canvas - FPSView").transform);

            print("[FoxCom] Applied to UI");                     //DBG

            yield return new WaitUntil(()=> UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu" || checker == null);

            print("[FoxCom] UI WAS DELETED");                    //DBG

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu") 
            {
                print("[FoxCom] SCENE WAS SWITCHED");           //DBG
                checker = null; 
            }
            else
            {
                print("[FoxCom] CANWAS WAS DELETED");           //DBG
                yield return new WaitUntil(() => GameObject.Find("Canvas - FPSView") != null);
            }
        }
    } //uhh... make it look nicer                         !!! REMEMBUR !!!






    void RedrawFoxUI()
    {
        FoxComMidScreenActionReskin.foxReskin = GetComponent<FoxComReskin>();
        FoxComKeytipReskin.foxReskin = GetComponent<FoxComReskin>();

        GameObject.Find("ObjectivePanel").GetComponent<Animator>().runtimeAnimatorController = questANIM;
        GameObject spawned = Instantiate(inventoryTabSpawner, GameObject.Find("LeftPanel").transform, false);
        spawned.transform.SetSiblingIndex(4);
        spawned = Instantiate(questFillerSpawner, GameObject.Find("TitleRoll").transform, false);
      //Spawn spawnables blah bluh


        GameObject current;
        GameObject childObj;

        GameObject.Find("StatsPanel").transform.SetSiblingIndex(3);         //////////////////////////////////////////
        GameObject.Find("ObjectivePanel").transform.SetSiblingIndex(3);     //    Move things befind Inventory UI   //
        GameObject.Find("ChatPanel").transform.SetSiblingIndex(3);          //////////////////////////////////////////

        foreach (UIElements element in elements)                            //Make UI editor later, so peoples will be able to change anything.
        {
            current = GameObject.Find(element._elementName);

            if (element._rect._edited) ApplyRect(current, element._rect);
            if (element._image._edited) ApplyImage(current, element._image);
            if (element._layout._edited) ApplyLayoutGroups(current, element._layout);
            if (element._text._edited) ApplyText(current.transform.GetChild(element._text._textID).gameObject, element._text);
            if (element._dynamics._edited) ApplyDynamics(current, element._dynamics);
            foreach (UIChildElement child in element._kids)
            {
                childObj = current.transform.GetChild(child._childID).gameObject;
                if (child._childRect._edited) ApplyRect(childObj, child._childRect);
                if (child._childImage._edited) ApplyImage(childObj, child._childImage);
                if (child._childLayout._edited) ApplyLayoutGroups(childObj, child._childLayout);
            }//Do same thing with the kids.
        }
        UpdateFoxKeybindingTips();
    }   //Apply all params to FPS Canvas



    public void UpdateFoxKeybindingTips()
    {
        ActionHint[] tooltips = FindObjectsOfType<ActionHint>(true);

        foreach (ActionHint tip in tooltips)
        {
            tip.GetComponent<RectTransform>().sizeDelta = new Vector2(38, 34);
            if (tip.GetComponent<Image>() != null)
            {
                tip.GetComponent<Image>().sprite = softBlackBox;
                tip.GetComponent<Image>().color = Color.white;
                tip.GetComponent<Image>().pixelsPerUnitMultiplier = 7;
            }
            if (tip.transform.GetChild(0).GetComponent<Image>() != null&& tip.transform.GetChild(0).GetComponent<Image>().sprite !=null)
            {
                foreach (Sprite sprite in hudIcons)
                {
                    if (sprite.name == tip.transform.GetChild(0).GetComponent<Image>().sprite.name)
                    {
                        tip.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                        tip.transform.GetChild(0).GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
                        tip.transform.GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
                        tip.transform.GetChild(0).GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                        tip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(38, 34);
                        continue;
                    }
                }
            }

            tip.image.GetComponent<RectTransform>().sizeDelta = new Vector2(37, 37);
            tip.image.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            tip.image.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 2);
            tip.gameObject.AddComponent<FoxComKeytipReskin>();
        }
    }   //Add component to all objects containing KK's keybinder and replace it.



    void UpdateInteractalbeIcons()
    {
        FindObjectsOfType<BedMachine>(true);

    }   //TEST



    #region Appliers
    /// <summary>
    /// Apply changes to RectTransform component. Also allows to set a parent.
    /// </summary> 
    void ApplyRect(GameObject uiObject, FoxUIRect rectProperties)
    {
        if (rectProperties._parentName != "")
        {
            uiObject.transform.parent = GameObject.Find(rectProperties._parentName).transform;
            uiObject.GetComponent<RectTransform>().anchoredPosition = rectProperties._position;
            uiObject.GetComponent<RectTransform>().sizeDelta = rectProperties._scale;
            if (rectProperties._parentHierarchy != -1) uiObject.transform.SetSiblingIndex(rectProperties._parentHierarchy);
        }//Move object to it's new parent.

        if (rectProperties._advanced)
        {
            uiObject.GetComponent<RectTransform>().anchorMax = rectProperties._anchorMax;
            uiObject.GetComponent<RectTransform>().anchorMin = rectProperties._anchorMin;
            uiObject.GetComponent<RectTransform>().pivot = rectProperties._pivot;
            //return;
        }//Only apply advanced settings if setted.
        uiObject.GetComponent<RectTransform>().anchoredPosition = rectProperties._position;
        uiObject.GetComponent<RectTransform>().sizeDelta = rectProperties._scale;
    }

    /// <summary>
    /// Apply changes to Image component if presented, otherwithe create one.
    /// </summary> 
    void ApplyImage(GameObject uiObject, FoxUIImage imageProperties)
    {
        if (imageProperties._removeImageComponent) 
        {
            uiObject.GetComponent<Image>().enabled = false;
            return;
        }

        if (uiObject.GetComponent<Image>() == null)
        {
            uiObject.AddComponent<Image>();
        }

        uiObject.GetComponent<Image>().sprite = imageProperties._sprite;
        uiObject.GetComponent<Image>().type = imageProperties._imageType;
        uiObject.GetComponent<Image>().pixelsPerUnitMultiplier = imageProperties._pixelMultiplier;
        uiObject.GetComponent<Image>().color = imageProperties._color;
    }

    /// <summary>
    /// Apply changes to Layout component if needed.
    /// </summary> 
    void ApplyLayoutGroups(GameObject uiObject, UILayoutChange layoutProperties)
    {
        if (layoutProperties._isVertical)
        {
            if (layoutProperties._remove)
            {
                Destroy(uiObject.GetComponent<VerticalLayoutGroup>());
                return;
            }                                                                       //Vertical
            uiObject.GetComponent<VerticalLayoutGroup>().spacing = layoutProperties._layoutSpacing;
            uiObject.GetComponent<VerticalLayoutGroup>().padding = layoutProperties._layoutPadding;

        }
        else
        {
            if (layoutProperties._remove)
            {
                Destroy(uiObject.GetComponent<HorizontalLayoutGroup>());
                return;
            }                                                                       //Horizontal
            uiObject.GetComponent<HorizontalLayoutGroup>().spacing = layoutProperties._layoutSpacing;
            uiObject.GetComponent<HorizontalLayoutGroup>().padding = layoutProperties._layoutPadding;
        }
    }

    /// <summary>
    /// Apply changes to TextMeshPro component if needed.
    /// </summary>
    void ApplyText(GameObject uiObject, UITextElementer textProperties)
    {
        uiObject.GetComponent<TMPro.TMP_Text>().enableAutoSizing = false;
        uiObject.GetComponent<TMPro.TMP_Text>().font = fontAsset;
        uiObject.GetComponent<TMPro.TMP_Text>().color = textProperties._fontColor;
        uiObject.GetComponent<TMPro.TMP_Text>().fontSize = textProperties._fontSize;
        if (textProperties._disableWrapping) uiObject.GetComponent<TMPro.TMP_Text>().enableWordWrapping = false;
        if (!textProperties._isBold) uiObject.GetComponent<TMPro.TMP_Text>().textStyle = TMPro.TMP_Style.NormalStyle;
        if (textProperties._textPos != Vector2.zero) uiObject.gameObject.GetComponent<RectTransform>().anchoredPosition = textProperties._textPos;
    }

    /// <summary>
    /// Add dynamics component that makes UI react on mouse movement.
    /// </summary>
    void ApplyDynamics(GameObject uiObject, UIDynamicsFox dynamicsProperties)
    {
        uiObject.AddComponent<FoxComUIDynamics>().lag = dynamicsProperties._lag;
        uiObject.GetComponent<FoxComUIDynamics>().boldTRA = GameObject.Find("PlayerController(Clone)").transform.parent.transform;
    }

    #endregion


    /// <summary>
    /// Since it's a mod and I cannot edit any Existing files (only work with stuff i already have)
    /// </summary>
    #region Initial Mod Injection

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        if (!doNotSpawn) GameObject.Find("Main Camera").AddComponent<FoxComReskin>();        
    }

    [RuntimeInitializeOnLoadMethod]
    static void OnSecondRuntimeMethodLoad()
    {
        Debug.Log("[FoxCom] Game Started, Script Activated");
    }

    #endregion
}



#region Sub classes

//////////////////////////////////////////////////////////////////////
//                                                                  //
//                     My Custom Configurators                      //
//                                                                  //
//   since i'm a big dum dum and don't know how to make it normaly  //
//                                                                  //
//////////////////////////////////////////////////////////////////////


[System.Serializable]
class UIElements
{
    [Header("Object Name")]
    public string _elementName;
    [Space(20)]

    [Header("Transfrom")]
    public FoxUIRect _rect;
    [Space(20)]

    [Header("Image")]
    public FoxUIImage _image;
    [Space(20)]

    [Header("Child Objects")]
    public List<UIChildElement> _kids;
    [Space(20)]

    [Header("Text Mesh Pro")]
    public UITextElementer _text;
    [Space(20)]

    [Header("LayoutGroup")]
    public UILayoutChange _layout;
    [Space(20)]

    [Header("UI Dynamics")]
    public UIDynamicsFox _dynamics;
}



[System.Serializable]
class FoxUIRect
{
    public bool _edited;
    public Vector2 _position;       //Simple X Y positioning.
    public Vector2 _scale;          //Simple X Y scaling.
    [Space(7)]
    public bool _advanced;          //Makes use of _advancedRect instead of _position and _scale. 
    public Vector2 _anchorMin;
    public Vector2 _anchorMax;
    public Vector2 _pivot;
    [Space(7)]
    public string _parentName;      //Move into parent if have any.
    public int _parentHierarchy = -1;//Set object's order in hierarchy.
}


[System.Serializable]
class FoxUIImage
{
    public bool _edited;
    [Space(7)]
    public Sprite _sprite;
    public Color _color = Color.white;
    [Space(7)]
    public Image.Type _imageType;
    public int _pixelMultiplier = 7;
    [Space(7)]
    public bool _removeImageComponent;
}


[System.Serializable]
class UIChildElement
{
    public int _childID;
    [Space(7)]
    public FoxUIRect _childRect;
    public FoxUIImage _childImage;
    public UILayoutChange _childLayout; 
}


[System.Serializable]
class UITextElementer
{
    public bool _edited;       //Make sure if 
    [Space(7)]
    public int _textID;             //Text's hierarchy number.
    public bool _isBold;
    public int _fontSize;
    [Space(4)]

    public bool _disableWrapping;   //Removes text's borders.
    public Vector2 _textPos;        //Simple X Y positioning.
    public Color _fontColor;        //Colorizing  RGBA (0f - 1f).
}


[System.Serializable]
class UILayoutChange
{
    public bool _edited;
    [Space(7)]
    public bool _isVertical; 
    public int _layoutSpacing;
    public RectOffset _layoutPadding;
    public bool _remove;
}


[System.Serializable]
class UIDynamicsFox
{
    public bool _edited;
    public Vector2 _lag; 
}
#endregion