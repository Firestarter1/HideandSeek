using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    public string musicName;
    public float volume = 1.0f;

    private void Start()
    {
        SoundManager.Instance.PlayMusic(musicName, volume);
        Destroy(gameObject);
    }
}
