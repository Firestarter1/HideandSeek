using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Spawn")]
public class Spawn : ScriptableObject
{
    public Vector3 spawnPosition;
    public int desiredArea;
}
