using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Image))]
public class RandomImageLoaderUI : MonoBehaviour {
    [SerializeField]
    private Sprite[] sprites;
    private void OnEnable() {
        GetComponent<Image>().sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
