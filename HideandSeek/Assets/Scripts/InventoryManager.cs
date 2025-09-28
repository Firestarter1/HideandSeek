using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Item[] startItems;

    [SerializeField] int maxStackedItems;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    int selectedSlot = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeSelectedSlot(0);
        foreach (var item in startItems)
            AddItem(item);
        StartCoroutine(DelayedRefresh());
    }

    private void Update()
    {
        if (Input.inputString != null)
        {
            int num = GetPressedNumber();

            if (num <= 9 && num > 0)
            {
                ChangeSelectedSlot(num - 1);
                ChangeHeldItem();
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            int newValue = selectedSlot < 9 ? selectedSlot + 1 : 0;
            ChangeSelectedSlot(newValue);
            ChangeHeldItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            int newValue = selectedSlot > 0 ? selectedSlot - 1 : 9;
            ChangeSelectedSlot(newValue);
            ChangeHeldItem();
        }
    }

    int GetPressedNumber()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + i)))
            {
                return i;
            }
        }
        return 0;
    }

    void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
            inventorySlots[selectedSlot].Deselect();

        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }

    public bool AddItem(Item item)
    {
        // Check if any slots have the same item with count lower than max
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable == true)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                StartCoroutine(DelayedRefresh());
                return true;
            }
        }

        // Find an empty slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot, i);
                StartCoroutine(DelayedRefresh());
                return true;
            }
        }
        
        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot, int slotIndex)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item, slotIndex);
    }

    public Item GetSelectedItem(bool use)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;

            if (use == true)
            {
                ItemAction(item, itemInSlot);
                
            }

            return item;
        }

        return null;
    }

    public void Consume(InventoryItem itemInSlot)
    {
        itemInSlot.count--;

        if (itemInSlot.count <= 0)
        {
            Destroy(itemInSlot.gameObject);
        }
        else
        {
            itemInSlot.RefreshCount();
        }
        StartCoroutine(DelayedRefresh());
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;
        ChangeHeldItem();
    }

    public void ItemAction(Item item, InventoryItem itemInSlot)
    {
        if (item.itemType == Item.ItemType.HealthPack)
        {
            GameManager.Instance.playerScript.Heal(item.healAmount);

            if (item.actionType == Item.ActionType.Consumable)
                Consume(itemInSlot);
        }
        else if (item.actionType == Item.ActionType.Gun)
        {
            GunStates currGun = (GunStates)GetSelectedItem(false);

            if (currGun.ammoCurr > 0)
            {
                GameManager.Instance.playerScript.shoot();
                
            }
        }
    }

    public bool HasItemInInventory(Item item)
    {
        foreach (InventorySlot slot in  inventorySlots)
        {
            if (slot.GetComponentInChildren<InventoryItem>() != null && slot.GetComponentInChildren<InventoryItem>().item == item)
            {
                return true;
            }
        }
        return false;
    }

    public void ChangeHeldItem()
    {
        GameManager.Instance.playerScript.ChangeItem();
    }
}
