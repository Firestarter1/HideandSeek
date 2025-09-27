using UnityEngine;
[CreateAssetMenu]
public class MusicSO : ScriptableObject
{
    public AudioClip clip;
    public float bpm;
    public int barsPerLoop;

    double SecondsPerBar()
    {
        return (60.0 / bpm) * 4.0f;
    }

    public double GetLoopTime()
    {
        return SecondsPerBar() * barsPerLoop;
    }
}
