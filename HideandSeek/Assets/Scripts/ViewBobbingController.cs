using UnityEngine;

public class ViewBobbingController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] GameObject held;
    [SerializeField] float viewBobFrequency = 1.0f;
    [SerializeField] float viewBobAmplitude = 1.0f;
    [SerializeField] float recenterSpeed = 3.0f;
    [SerializeField] float footStepYOffset = -1.5f;
    
    float bobDelta;
    Vector3 cameraOrigin;
    Vector3 heldItemOrigin;

    float prevBobY = 0f;
    bool footstepAudioPrimed = false;
    const float primeDepth = 0.6f;
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

            float y = Camera.main.transform.localPosition.y - cameraOrigin.y;

            bool descending = y < prevBobY;
            if (descending && y < -viewBobAmplitude * primeDepth)
            {
                footstepAudioPrimed = true;
            }

            bool ascending = y > prevBobY;
            if (ascending && footstepAudioPrimed)
            {
                footstepAudioPrimed = false;
                SoundManager.Instance?.PlaySoundFXClip(SoundType.Footstep, new Vector3(transform.position.x, transform.position.y + footStepYOffset, transform.position.z) , AudioGroup.SFX, 0.33f, 0.1f, 1f, 0.1f);
            }
            prevBobY = y;
        }
        else
        {
            
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, cameraOrigin, recenterSpeed * Time.deltaTime);
            held.transform.localPosition = Vector3.Lerp(held.transform.localPosition, heldItemOrigin, recenterSpeed * Time.deltaTime);
            footstepAudioPrimed = false;
            prevBobY = 0f;
        }
        //Debug.Log("cameraMainTransformLocalPosition=" + Camera.main.transform.localPosition + " cameraOrigin=" + cameraOrigin);
    }

    Vector3 HeadViewBob(float t)
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(t * viewBobFrequency) * viewBobAmplitude;

        //Play footstep sound if we're at the bottom y (since that would probably be when it would make sense for the player to take a step idk bro)
        

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
