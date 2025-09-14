using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WaveDisplay : MonoBehaviour
{
    WaveManager waveManager;

    [Header("Round Count Label")]
    [SerializeField] TextMeshProUGUI roundLabelText;
    [SerializeField] Color roundLabelColor;
    [Space(10)]
    [SerializeField] TextMeshProUGUI roundCountText;
    [SerializeField] Color roundCurrentNumberColor;
    [SerializeField] Color roundTotalNumberColor;

    [Header("Next Wave Timer Label")]
    [SerializeField] TextMeshProUGUI nextWaveTimerLabelText;
    [SerializeField] Color nextWaveTimerLabelColor;
    [Space(10)]
    [SerializeField] TextMeshProUGUI nextWaveTimerCounterText;
    [SerializeField] Color nextWaveTimerCounterColor;

    [Header("Remaining Enemies Label")]
    [SerializeField] Color remainingLabelColor;
    [Space(10)]
    [SerializeField] Color remainingCounterColor;

    private void Start()
    {
        waveManager = WaveManager.instance;
        waveManager.roundStarted.AddListener(SetRoundText);
        waveManager.roundCountdown.AddListener(SetNextWaveTimerText);
        waveManager.remainingEnemiesUpdated.AddListener(SetRemainingText);
    }

    private void OnDisable()
    {
        waveManager.roundStarted.RemoveListener(SetRoundText);
        waveManager.roundCountdown.RemoveListener(SetNextWaveTimerText);
        waveManager.remainingEnemiesUpdated.RemoveListener(SetRemainingText);
    }

    void SetRoundText()
    {
        int currentRound = waveManager.GetCurrentRoundIndex();
        int totalRounds = waveManager.rounds.Length;

        roundLabelText.text = "<color=#" + roundLabelColor.ToHexString() + "> ROUND:</color>";

        roundCountText.text = "<color=#" + roundCurrentNumberColor.ToHexString() + ">" + currentRound + " </color>" +
            "<color=#" + roundLabelColor.ToHexString() + "> / </color>" +
            "<color=#" + roundTotalNumberColor.ToHexString() + ">" + totalRounds + "</color>";
    }

    void SetNextWaveTimerText(int timeRemaining)
    {
        nextWaveTimerLabelText.text = "<color=#" + nextWaveTimerLabelColor.ToHexString() + "> NEXT WAVE:</color>";

        nextWaveTimerCounterText.text = "<color=#" + nextWaveTimerCounterColor.ToHexString() + "> " + timeRemaining + "</color>";
    }

    void SetRemainingText(int remainingEnemies)
    {
        nextWaveTimerLabelText.text = "<color=#" + remainingLabelColor.ToHexString() + "> REMAINING:</color>";

        nextWaveTimerCounterText.text = "<color=#" + remainingCounterColor.ToHexString() + "> " + remainingEnemies + "</color>";
    }
}
