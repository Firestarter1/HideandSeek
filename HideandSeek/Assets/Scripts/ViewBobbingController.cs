using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
public class ViewBobbingController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] GameObject held;
    [SerializeField] float viewBobFrequency = 1.0f;
    [SerializeField] float viewBobAmplitude = 1.0f;
    [SerializeField] float recenterSpeed = 3.0f;
    [SerializeField] float footStepYOffset = -1.5f;
    [SerializeField] float sprintFOV = 70.0f;
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] CinemachineImpulseListener impulseListener;

    float startingFOV;

    float bobDelta;
    Vector3 cameraOrigin;
    Vector3 heldItemOrigin;

    float prevBobY = 0f;
    bool footstepAudioPrimed = false;
    const float primeDepth = 0.6f;

    Tween fovTween;
    private void Awake()
    {
        cameraOrigin = cinemachineCamera.transform.localPosition;
        heldItemOrigin = held.gameObject.transform.localPosition;
        startingFOV = cinemachineCamera.Lens.FieldOfView;
    }

    private void LateUpdate()
    {
        ViewBobbing();
        AdjustFOV();
    }

    void AdjustFOV()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            fovTween?.Kill();
            fovTween = DOTween.To(() => cinemachineCamera.Lens.FieldOfView, x => cinemachineCamera.Lens.FieldOfView = x, sprintFOV, 0.3f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
               fovTween = null;
            });
        } else if (Input.GetButtonUp("Sprint"))
        {
            fovTween?.Kill();
            fovTween = DOTween.To(() => cinemachineCamera.Lens.FieldOfView, x => cinemachineCamera.Lens.FieldOfView = x, startingFOV, 0.3f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                fovTween = null;
            });
        }
    }

    void ViewBobbing()
    {
        if ((controller.velocity.x != 0 || controller.velocity.z != 0) && controller.isGrounded)
        {
            bobDelta += Time.deltaTime * controller.velocity.magnitude;
            cinemachineCamera.transform.localPosition = cameraOrigin + HeadViewBob(bobDelta);
            held.transform.localPosition = heldItemOrigin + ItemViewBob(bobDelta);

            float y = cinemachineCamera.transform.localPosition.y - cameraOrigin.y;

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
            if (!CinemachineImpulseManager.Instance.GetImpulseAt(transform.position, impulseListener.Use2DDistance, impulseListener.ChannelMask, out Vector3 ___, out Quaternion __))
            {
                cinemachineCamera.transform.localPosition = Vector3.Lerp(cinemachineCamera.transform.localPosition, cameraOrigin, recenterSpeed * Time.deltaTime);
            }
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
