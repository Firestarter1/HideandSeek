using UnityEngine;
[CreateAssetMenu()]
public class ScreenShakeSettings : ScriptableObject
{
    public float impulseTime = 0.2f;
    public float impulseForce = 1f;
    public Vector3 defaultVelocity = new Vector3(0f,-1f,0f);
    public bool randomVelocity = false;
    public AnimationCurve curve;

    [Space(10)]

    public float listenerAmplitude = 1f;
    public float listenerFrequency = 1f;
    public float listenerDuration = 1f;

}
