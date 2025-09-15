using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSliderSetup : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    public string channelName;
    private void OnEnable()
    {
        mixer.GetFloat(channelName, out float db);
        float level = Mathf.Pow(10f, db / 20f);
        GetComponent<Slider>().value = level;
    }
}
