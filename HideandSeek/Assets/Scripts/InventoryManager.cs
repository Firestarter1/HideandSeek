using Meshy;
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
    }

    private void Update()
    {
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);

            if (isNumber && number <= 9 && number > 0)
            {
                ChangeSelectedSlot(number - 1);
                GameManager.Instance.playerScript.ChangeHeldItem();
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedSlot < 9)
        {
            int newValue = selectedSlot + 1;
            ChangeSelectedSlot(newValue);
            GameManager.Instance.playerScript.ChangeHeldItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedSlot > 0)
        {
            int newValue = selectedSlot - 1;
            ChangeSelectedSlot(newValue);
            GameManager.Instance.playerScript.ChangeHeldItem();
        }
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
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
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

}
