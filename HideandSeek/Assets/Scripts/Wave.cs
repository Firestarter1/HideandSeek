using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    [SerializeField] EnemeyAI[] enemies;
    public int delay = 0;
    public float delayBetweenEnemies = 0.5f;
    public int spawnLocation = 0;

    public EnemeyAI GetEnemy(int i)
    {
        return enemies[i];
    }

    public EnemeyAI[] GetEnemies()
    {
        return enemies;
    }

    public int GetEnemyCount()
    {
        return enemies.Length;
    }
}
