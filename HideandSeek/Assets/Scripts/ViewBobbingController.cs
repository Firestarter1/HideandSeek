using UnityEngine;

public class ViewBobbingController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] GameObject held;
    [SerializeField] float viewBobFrequency = 1.0f;
    [SerializeField] float viewBobAmplitude = 1.0f;
    [SerializeField] float recenterSpeed = 3.0f;
    
    float bobDelta;
    Vector3 cameraOrigin;
    Vector3 heldItemOrigin;
    private void Awake()
    {
        cameraOrigin = Camera.main.transform.localPosition;
        heldItemOrigin = held.gameObject.transform.localPosition;
    }

    private void Update()
    {
        ViewBobbing();
    }

    void ViewBobbing()
    {
        if ((controller.velocity.x != 0 || controller.velocity.z != 0) && controller.isGrounded)
        {
            bobDelta += Time.deltaTime * controller.velocity.magnitude;
            Camera.main.transform.localPosition = cameraOrigin + HeadViewBob(bobDelta);
            held.transform.localPosition = heldItemOrigin + ItemViewBob(bobDelta);
        }
        else
        {
            
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, cameraOrigin, recenterSpeed * Time.deltaTime);
            held.transform.localPosition = Vector3.Lerp(held.transform.localPosition, heldItemOrigin, recenterSpeed * Time.deltaTime);
        }
        //Debug.Log("cameraMainTransformLocalPosition=" + Camera.main.transform.localPosition + " cameraOrigin=" + cameraOrigin);
    }

    Vector3 HeadViewBob(float t)
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(t * viewBobFrequency) * viewBobAmplitude;
        pos.x = Mathf.Cos(t * viewBobFrequency / 2.1f) * viewBobAmplitude;
        return pos;
    }

    Vector3 ItemViewBob(float t)
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(t * viewBobFrequency) * viewBobAmplitude / 5f;
        pos.x = -Mathf.Cos(t * viewBobFrequency / 2.1f) * viewBobAmplitude / 5f;
        return pos;
    }
}
