using UnityEngine;
public class FoxComKeytipReskin : MonoBehaviour
{ 
    public static FoxComReskin foxReskin;
    private void OnEnable() => Invoke("Apply", .2f); 
    private void Apply()
    {
        foreach (Sprite sprite in foxReskin.keyIcons)
        {
            if (sprite.name == GetComponent<ActionHint>().image.sprite.name)
            {
                GetComponent<ActionHint>().image.sprite = sprite;
                if (GetComponent<ActionHint>().image.sprite.texture.height != 512) GetComponent<ActionHint>().image.GetComponent<RectTransform>().sizeDelta = new Vector2(52, 37);
                if (gameObject.name == "ErectionPanel") GetComponent<RectTransform>().sizeDelta = new Vector2(50,34);
                Destroy(GetComponent<ActionHint>());
                Destroy(GetComponent<FoxComKeytipReskin>());  
            }            
        } 
    }
}
