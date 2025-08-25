using UnityEngine;

[CreateAssetMenu]

public class Item : ScriptableObject
{
    public enum ItemType { HealthPack, Grenade, Key}

    public string itemName;
    public ItemType itemType;
    [Range(0, 4)] public int itemTier;
    [Range(0, 99)] public int amount;
}
