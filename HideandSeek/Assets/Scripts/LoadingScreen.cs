using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Slider progress;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] float loadingTextCycleSpeed = 0.25f;
    [SerializeField] TextMeshProUGUI readyText;

    [SerializeField] float delaySeconds = 0.75f;


    AsyncOperation loadingOperation;
    bool waitingForUser;
    float startTime;

    Coroutine loadingTextCoroutine;

    void Awake()
    {
        progress.value = 0;
        loadingText.gameObject.SetActive(true);
        loadingTextCoroutine = StartCoroutine(CycleLoadingText());
        readyText.gameObject.SetActive(false);
    }

    IEnumerator CycleLoadingText()
    {
        float t = 0;
        while (!waitingForUser)
        {
            t += Time.deltaTime;
            if (t > loadingTextCycleSpeed)
            {
                t = 0;
                if (loadingText.text.Length >= 10)
                {
                    loadingText.text = "LOADING";
                } else
                {
                    loadingText.text += ".";
                }
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (waitingForUser)
        {
            if (Input.anyKeyDown)
            {
                Continue();
            }
        }
    }

    public void StartLoad(string sceneName)
    {
        gameObject.SetActive(true);
        startTime = Time.unscaledTime;
        StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        yield return LoadInternal(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single));
    }

    IEnumerator LoadInternal(AsyncOperation asyncOperation)
    {
        loadingOperation = asyncOperation;
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            this.progress.value = progress;

            if (loadingOperation.progress >= 0.9f && !waitingForUser)
            {
                float remaining = Mathf.Max(0, delaySeconds - (Time.unscaledTime - startTime));
                if (remaining > 0) yield return new WaitForSecondsRealtime(remaining);
                if (loadingTextCoroutine != null) StopCoroutine(loadingTextCoroutine);
                loadingText.gameObject.SetActive(false);
                
                readyText.gameObject.SetActive(true);

                waitingForUser = true;


            }

            if (waitingForUser)
            {
                yield return null;
            } else
            {
                yield return null;
            }
        }
    }

    public void Continue()
    {
        if (loadingOperation == null || waitingForUser == false) return;

        waitingForUser = false;

        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        yield return SceneFader.Instance.FadeOut();
        loadingOperation.allowSceneActivation = true;
    }
}
