using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] CanvasGroup group;
    [SerializeField] float defaultDuration = 0.35f;
    [SerializeField] float defaultDelay = 0.1f;
    [SerializeField] float overlap = 0.5f;
    [SerializeField] RectTransform[] leftRects;
    [SerializeField] RectTransform[] rightRects;

    bool ready;

    void Awake()
    {
        if (!Application.isPlaying) return;

        if (Instance && Instance != this) { 
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!group)
        {
            group = GetComponentInChildren<CanvasGroup>(true);
        }
        

        //group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        SceneManager.sceneLoaded += OnSceneLoaded;

        ready = true;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (!ready) return;
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut(float duration = -1)
    {
        duration = duration > 0f ? duration : defaultDuration;
        return Fade(1000f, 0f, duration);
    }

    public IEnumerator FadeIn(float duration = -1)
    {
        yield return null;
        duration = duration > 0f ? duration : defaultDuration;
        yield return Fade(0,1000f,duration,true);
    }

        

    IEnumerator Fade(float to, float from, float dur, bool reversed = false)
    {
        yield return new WaitForSecondsRealtime(defaultDelay);
        float t = 0f;
        group.blocksRaycasts = true;
        group.interactable = true;
        Vector2 size;

        int count = leftRects.Length;
        for (int i = 0; i < count; i++)
        {
            size = leftRects[i].sizeDelta;
            size.x = from;
            leftRects[i].sizeDelta = size;
            rightRects[i].sizeDelta = size;
        }

        float span = 1f + (count - 1) * (1f - overlap);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float percentage = (t / dur) * span;

            for (int i = 0; i < count; i++)
            {
                int band = reversed ? (count - 1 - i) : i;
                float start = i * (1f - overlap);
                float segment = percentage - start;

                size = leftRects[band].sizeDelta;
                size.x = Mathf.Lerp(from, to, segment);
                leftRects[band].sizeDelta = size;
                rightRects[band].sizeDelta = size;
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 final = leftRects[i].sizeDelta; 
            final.x = to;
            leftRects[i].sizeDelta = final;
            rightRects[i].sizeDelta = final;
        }

        /*t = 0f;
        for (int i = 0; i < count; i++)
        {
            size = leftRects[i].sizeDelta;
            size.x = to;
            leftRects[i].sizeDelta = size;
            rightRects[i].sizeDelta = size;
        }
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            int index = Mathf.Min(Mathf.FloorToInt((t / dur) * count), count - 1);
            index = (count - 1) - index;
            size = leftRects[index].sizeDelta;
            float segment = ((t / dur) * count) - Mathf.Floor(index);
            size.x = Mathf.Lerp(to, from, segment);
            leftRects[index].sizeDelta = size;
            rightRects[index].sizeDelta = size;
            yield return null;
        }*/
        group.blocksRaycasts = false;
        group.interactable = false;
    }

}
