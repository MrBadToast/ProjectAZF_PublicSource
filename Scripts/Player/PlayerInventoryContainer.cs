using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryContainer : StaticSerializedMonoBehaviour<PlayerInventoryContainer>
{
    //============================================
    //
    // [싱글턴 오브젝트]
    // 플레이어 인벤토리 "정보"를 관리하는 클래스입니다. "표시"를 관리하는 클래스는 UI/InventoryBehavior.cs를 확인하세요
    // 
    //============================================

    [SerializeField, ReadOnly] private Dictionary<ItemData, int> inventoryData;             // 실질적으로 인벤토리 정보가 담기는 곳입니다.
    public Dictionary<ItemData, int> InventoryData { get { return inventoryData; } }        // (읽기 전용) 현재 인벤토리 정보를 가져옵니다.

    private Dictionary<ItemData, int> debug_items;

    [SerializeField,ReadOnly] private int money;
    public int Money { get { return money; } }                                              // 현재 가지고 있는 조개 정보를 가져옵니다.

    protected override void Awake()
    {
        base.Awake();

        inventoryData = new Dictionary<ItemData, int>();
        if(debug_items != null)
        {
            foreach(var item in debug_items)
            {
                inventoryData.Add(item.Key, item.Value);
            }
        }
    }

    /// <summary>
    /// 아이템을 하나 추가합니다
    /// </summary>
    /// <param name="item">아이템</param>
#if UNITY_EDITOR
    [Button(ButtonSizes.Small), DisableInEditorMode]
#endif
    public void AddItem(ItemData item)
    {
        if (inventoryData.ContainsKey(item))
        {
            inventoryData[item]++;
        }
        else
        {
            inventoryData.Add(item, 1);
        }

        TryResetInventoryUI();
    }

    /// <summary>
    /// 아이템을 quantity 만큼 추가합니다
    /// </summary>
    /// <param name="item">아이템</param>
    /// <param name="quantity">수량</param>
    public void AddItem(ItemData item, int quantity)
    {
        if (quantity < 0) { Debug.LogWarning("InventoryContainer : 음수의 아이템 추가 시도"); return; }

        if (inventoryData.ContainsKey(item))
        {
            inventoryData[item] += quantity;
        }
        else
        {
            inventoryData.Add(item, quantity);
        }

        TryResetInventoryUI();
    }

    /// <summary>
    /// 아이템 창을 여는 코루틴 입니다
    /// </summary>
    /// <param name="item">아이템</param>
    /// <param name="quantity">개수</param>
    /// <returns></returns>
    public IEnumerator Cor_ItemWindow(ItemData item,int quantity)
    {
        ItemObtainInfo info = ItemObtainInfo.Instance;
        if (info == null) yield break;

        yield return info.StartCoroutine(info.Cor_OpenWindow(item,quantity));
    }

    /// <summary>
    /// 아이템을 제거합니다.
    /// </summary>
    /// <param name="item"></param>
    /// <returns> 아이템 제거 실패 시 false </returns>
    public bool RemoveItem(ItemData item)
    {
        if (inventoryData.ContainsKey(item))
        {
            if (inventoryData[item] <= 1)
            {
                inventoryData.Remove(item);
            }
            else
            {
                inventoryData[item]--;
            }

            TryResetInventoryUI();

            return true;
        }
        else
        {
            return false;
        }


    }

    /// <summary>
    /// 아이템을 quantity만큼 제거합니다.
    /// </summary>
    /// <param name="item">아이템</param>
    /// <param name="quantity">수량</param>
    /// <returns>아이템 제거 실패 시 false</returns>
    public bool RemoveItem(ItemData item, int quantity)
    {
        if (inventoryData.ContainsKey(item))
        {
            if (inventoryData[item] == quantity)
            {
                inventoryData.Remove(item);
                TryResetInventoryUI();
                return true;
            }
            else if (inventoryData[item] < quantity)
            {
                return false;
            }
            else
            {
                inventoryData[item] -= quantity;
                TryResetInventoryUI();
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// itemID가 인벤토리에 있는지 확인합니다.
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public bool HasItem(string itemID)
    {
        var items = inventoryData.Keys;

        foreach (var item in items)
        {
            if (item.ItemID == itemID) return true;
        }
        return false;
    }

    /// <summary>
    /// 조개를 추가합니다
    /// </summary>
    /// <param name="value">수량</param>
    public void AddMoney(int value)
    {
        int le = money;
        money += value;

        MoneyObtainInfo info = MoneyObtainInfo.Instance;
        if (info != null) { info.MoneyChanged(le, value); return; }
    }

    /// <summary>
    /// 조개를 제거합니다.
    /// </summary>
    /// <param name="value">수량</param>
    /// <returns></returns>
    public bool UseMoney(int value)
    {
        if (money < value) return false;
        else
        {
            int le = money;
            money -= value;

            MoneyObtainInfo info = MoneyObtainInfo.Instance;
            if (info != null) { info.MoneyChanged(le, -value); }

            TryResetInventoryUI();

            return true;
        }
    }

    private void TryResetInventoryUI()
    {
        UI_InventoryBehavior uiInventory = UI_InventoryBehavior.Instance;
        if (uiInventory != null) 
        { 
            uiInventory.SetInventory(inventoryData);
            uiInventory.SetMoney(money);
        }
    }
}
