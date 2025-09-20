using System.Collections;
using TMPro;
using UnityEngine;

public class WalletScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    Coroutine updateCoroutine;
    [SerializeField] float updateSpeed = 0.25f;
    
    void Start()
    {
        GameManager.Instance.playerScript.walletUpdated.AddListener(UpdateWallet);
    }
    
    void UpdateWallet(int amount)
    {
        int current = int.Parse(text.text);
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(UpdateCoroutine(current, amount));
    }

    IEnumerator UpdateCoroutine(int from, int to)
    {
        float timer = 0f;
        while (timer < updateSpeed)
        {
            timer += Time.deltaTime;
            int newNum = Mathf.CeilToInt(Mathf.Lerp(from, to, Mathf.Clamp01(timer/updateSpeed)));
            text.text = newNum.ToString();
            yield return null;
        }
        text.text = to.ToString();
        updateCoroutine = null;
    }
}
