using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField] CinemachineImpulseListener listener;

    CinemachineImpulseDefinition definition;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void ScreenShakeFromSettings(ScreenShakeSettings settings, CinemachineImpulseSource source)
    {
        ApplyScreenShakeSettings(settings, source);
        source.GenerateImpulseWithForce(settings.impulseForce);
    }

    void ApplyScreenShakeSettings(ScreenShakeSettings settings, CinemachineImpulseSource source)
    {
        definition = source.ImpulseDefinition;

        definition.ImpulseDuration = settings.impulseTime;
        Vector3 velocity = settings.defaultVelocity;
        if (settings.randomVelocity)
        {
            velocity.x *= Random.value > 0.5f ? 1 : -1;
            velocity.y *= Random.value > 0.5f ? 1 : -1;
            velocity.z *= Random.value > 0.5f ? 1  : -1;
        }
        source.DefaultVelocity = velocity;
        definition.CustomImpulseShape = settings.curve;

        source.ImpulseDefinition = definition;

        listener.ReactionSettings.AmplitudeGain = settings.listenerAmplitude;
        listener.ReactionSettings.FrequencyGain = settings.listenerFrequency;
        listener.ReactionSettings.Duration = settings.listenerDuration;
    }
}
