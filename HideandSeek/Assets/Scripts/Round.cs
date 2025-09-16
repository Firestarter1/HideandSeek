using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Round")]

public class Round : ScriptableObject
{
    [SerializeField] public Wave[] waves;

    public int GetTotalEnemiesInWave()
    {
        int i = 0;
        foreach (Wave w in waves)
        {
            i += w.GetEnemyCount();
        }
        return i;
    }

    public List<EnemeyAI> GetEnemiesInWave()
    {
        List<EnemeyAI> enemies = new List<EnemeyAI>();
        foreach (Wave w in waves)
        {
            EnemeyAI[] arr = w.GetEnemies();
            for (int i = 0; i < arr.Length; i++)
            {
                enemies.Add(arr[i]);
            }
        }
        return enemies;
    }
}
