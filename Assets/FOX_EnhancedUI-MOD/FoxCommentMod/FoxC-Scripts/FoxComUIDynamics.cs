using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoxComUIDynamics : MonoBehaviour
{
    private RectTransform tra;
    public Transform boldTRA;
    private Vector2 initPos;
    private Vector2 boldPos;
    public Vector2 lag;
    private void Start()
    {
        tra = GetComponent<RectTransform>();
        initPos = tra.anchoredPosition;
    } 
    private void FixedUpdate()
    {
        tra.anchoredPosition = initPos + (boldPos - new Vector2(Mathf.Abs(boldTRA.position.x) + Mathf.Abs(boldTRA.position.z), boldTRA.position.y)) * lag;
        boldPos = Vector2.Lerp(boldPos,             new Vector2(Mathf.Abs(boldTRA.position.x) + Mathf.Abs(boldTRA.position.z), boldTRA.position.y), .1f) ;
    }
}
