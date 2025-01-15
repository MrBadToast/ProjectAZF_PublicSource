using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlotSingle : MonoBehaviour
{
    [SerializeField,ReadOnly()] private ItemData assignedItem; 
    public ItemData AssignedItem { get { return assignedItem; } }

    [SerializeField] private UI_InventoryBehavior inventoryManager;
    public UI_InventoryBehavior InventoryManager { get { return inventoryManager; } }

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    [SerializeField] private GameObject popUpWindow;
    [SerializeField] private Image popUpImage;
    [SerializeField] private TextMeshProUGUI popUpText;

    private MainPlayerInputActions input;
    private Sprite debug_imageError;


    private void Awake()
    {
        debug_imageError = itemImage.sprite;
        input = new MainPlayerInputActions();
        input.UI.Enable();
    }

    private void Update()
    {
        if (input.UI.Cancel.IsPressed() || input.UI.Positive.IsPressed() || input.UI.Navigate.IsPressed() )
        {
            CloseItemPopUp();
        }
    }

    public void InitializeSlot(UI_InventoryBehavior inventory, ItemData item, int quantity = 1)
    {
        assignedItem = item;
        itemImage.sprite = item.ItemImage;
        inventoryManager = inventory;
        if(quantity >= 1) quantityText.text = quantity.ToString();
    }

    public void OnItemUsed()
    {
        PlayerInventoryContainer inventoryContainer = PlayerInventoryContainer.Instance;
        if (inventoryContainer == null) { Debug.Log("인벤토리정보에 접근하려 했으나 PlayerInventoryConatiner가 없습니다."); return; }

        inventoryContainer.RemoveItem(assignedItem);
    }

    public void ClearSlot()
    {
        assignedItem = null;
        itemImage.sprite = debug_imageError;
        quantityText.text = string.Empty;
    }

    public void ButtonClickEvent()
    {
        if (assignedItem.ItemPopUpImage == null)
        {
        }
        else 
        {
            OpenItemPopUp();
        }
        
    }

    public void OpenItemPopUp()
    {
        //assignedItem = item;
        popUpWindow.SetActive(true);
        popUpText.text = assignedItem.ItemDiscription.GetLocalizedString();
        popUpImage.sprite = assignedItem.ItemPopUpImage;
    }

    public void CloseItemPopUp()
    {
        popUpText.text = "";
        popUpImage.sprite = null;
        popUpWindow.SetActive(false);
    }
}
