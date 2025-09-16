using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ScribtableObject/Item")]
public class Item : ScriptableObject
{
    public GameObject model;
    public enum ItemType { HealthPack, Grenade, Gun }
    public enum ActionType { Consumable, Gun }

    public string itemName;
    public ItemType itemType;
    public Sprite image;
    public ActionType actionType;
    public int healAmount;
    public bool stackable = true;
}
