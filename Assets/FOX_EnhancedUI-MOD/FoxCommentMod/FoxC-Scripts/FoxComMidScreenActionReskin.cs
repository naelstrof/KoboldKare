using UnityEngine;

public class FoxComMidScreenActionReskin : MonoBehaviour
{
    public static FoxComReskin foxReskin;
    private void OnEnable() => Invoke("Apply", .1f);
    private void Apply()
    {
        foreach (Sprite sprite in foxReskin.hudIcons)
        {
            if (sprite.name == transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite.name)
            {
                transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = sprite; 
            }
        }
    }
}
