using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public Round[] rounds;
    [System.NonSerialized] public Round currentRound;
    int currentRoundIndex = 0;

    public int timeBetweenRounds = 30;

    int currentWaveMobs = 0;
    int currentRoundMobs = 0;

    float initialBuffer = 0.0f;

    [SerializeField] List<Spawn> spawnLocations = new List<Spawn>();

    Dictionary<Wave, Coroutine> pendingWaves = new Dictionary<Wave, Coroutine>();

    [System.NonSerialized] public UnityEvent roundStarted;
    [System.NonSerialized] public UnityEvent<int> roundCountdown;
    [System.NonSerialized] public UnityEvent<int> remainingEnemiesUpdated;
    [System.NonSerialized] public UnityEvent allMobsKilled;

    bool roundActive = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        roundStarted = new UnityEvent();
        roundCountdown = new UnityEvent<int>();
        remainingEnemiesUpdated = new UnityEvent<int>();
        allMobsKilled = new UnityEvent();
        
    }

    IEnumerator PreRoundCountdown()
    {
        
        currentRound = rounds[currentRoundIndex];
        roundStarted.Invoke();
        int time = timeBetweenRounds;
        while (time > 0)
        {
            roundCountdown.Invoke(time);
            time--;
            yield return new WaitForSeconds(1);
        }
        StartRound();
    }

    private void Update()
    {
        initialBuffer += Time.deltaTime;
        if (initialBuffer >= 1.0f && currentWaveMobs <= 0 && pendingWaves.Count > 0)
        {
            Wave shortestWave = pendingWaves.Keys.OrderBy(w => w.delay).FirstOrDefault();
            if (shortestWave != null && pendingWaves.TryGetValue(shortestWave, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                pendingWaves.Remove(shortestWave);
                SpawnWave(shortestWave);
            }
        }
        else if (pendingWaves.Count <= 0 && currentRoundMobs <= 0 && roundActive)
        {
            currentRoundIndex++;
            allMobsKilled.Invoke();
            roundActive = false;
            if (currentRoundIndex == rounds.Length)
            {
                GameManager.Instance.WinState();
            } else
            {
                StartCoroutine(PreRoundCountdown());
            }
            
        }
    }

    private void Start()
    {
        StartCoroutine(PreRoundCountdown());
    }

    public void StartRound()
    {
        roundActive = true;
        roundStarted.Invoke();
        for (int i = 0; i < currentRound.waves.Length; i++)
        {
            if (currentRound.waves[i].delay <= 0)
            {
                SpawnWave(currentRound.waves[i]);
            }
            else
            {
                Coroutine c = StartCoroutine(SpawnWave(currentRound.waves[i], currentRound.waves[i].delay));
                pendingWaves.Add(currentRound.waves[i], c);
            }
        }
        currentRoundMobs = currentRound.GetTotalEnemiesInWave();
        remainingEnemiesUpdated.Invoke(currentRoundMobs);
    }

    void SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.GetEnemyCount(); i++)
        {
            StartCoroutine(SpawnMob(wave.GetEnemy(i), wave.spawnLocation, i * wave.delayBetweenEnemies));
            currentWaveMobs++;

        }
    }
    IEnumerator SpawnWave(Wave wave, int delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        for (int i = 0; i < wave.GetEnemyCount(); i++)
        {
            StartCoroutine(SpawnMob(wave.GetEnemy(i), wave.spawnLocation, i * wave.delayBetweenEnemies));
            currentWaveMobs++;
        }
        pendingWaves.Remove(wave);
    }
    IEnumerator SpawnMob(EnemeyAI enemy, int location, float delay)
    {
        yield return new WaitForSeconds(delay);

        CreateMobAtLocation(enemy, GetSpawnLocation(location));
        //Debug.Log("Spawning " + enemy.name);
    }

    Spawn GetSpawnLocation(int id)
    {
        if (id == -1)
        {
            return spawnLocations[Mathf.RoundToInt(Random.Range(0, spawnLocations.Count - 1))];
        }
        return spawnLocations[id];
    }

    void CreateMobAtLocation(EnemeyAI enemy, Spawn position)
    {
        Instantiate(enemy, position.spawnPosition, Quaternion.identity);
    }

    public void MobDeath()
    {
        currentWaveMobs--;
        currentRoundMobs--;
        remainingEnemiesUpdated.Invoke(currentRoundMobs);
    }

    public int GetCurrentRoundIndex()
    {
        for (int i = 0; i < rounds.Length; i++)
        {
            if (rounds[i] == currentRound) return i+1;
        }
        return -1;
    }
}
