using UnityEngine;

public class CreditsScroller : MonoBehaviour {
    [SerializeField]
    private float speed;
    [SerializeField]
    private float height;
    private float startTime = 0f;
    private RectTransform target;
    private float startY;
    private void OnEnable() {
        startTime = Time.unscaledTime;
        target = GetComponent<RectTransform>();
        startY = target.anchoredPosition.y;
    }

    void Update() {
        float currentPosition = Mathf.Repeat((Time.unscaledTime-startTime) * speed, height);
        target.anchoredPosition = new Vector2(target.anchoredPosition.x, startY+currentPosition);
        //transform.position = transform.position.With(y:currentPosition);
    }
}
