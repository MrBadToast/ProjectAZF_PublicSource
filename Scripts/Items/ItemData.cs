using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItemData",menuName = "새 아이템 데이터 추가",order = 1)]
public class ItemData : ScriptableObject
{
    //================================================
    //
    // 아이템 정보를 담는 스크립터블 오브젝트입니다.
    //
    //================================================

    [SerializeField] private string itemID;                                         // 아이템 구분용 ID
    public string ItemID { get { return itemID; } }                         
    [SerializeField] private LocalizedString itemName;                              // 아이템의 이름 ( UI )
    public LocalizedString ItemName { get { return itemName; } }
    [SerializeField] private Sprite itemImage;                                      // 아이템의 이미지 ( UI )
    public Sprite ItemImage { get { return itemImage; } }
    [SerializeField] private Sprite itemPopUpImage;                                 // 아이템의 팝업이미지 ( UI )
    public Sprite ItemPopUpImage { get { return itemPopUpImage; } }
    [SerializeField] private LocalizedString itemDiscription;                       // 아이템 설명 ( UI )
    public LocalizedString ItemDiscription { get { return itemDiscription; } }
    [SerializeField] private string[] tags;                                         // 아이템 태그
    public string[] Tags { get { return tags; } }

    /// <summary>
    /// set에 있는 아이템들 중 ID값의 아이템이 있는지 확인합니다. 
    /// </summary>
    /// <param name="set"></param>
    /// <param name="ID"></param>
    /// <returns> 아이템 존재 여부 </returns>
    static bool TryItemFromSet(ItemData[] set, string ID)
    {
        for (int i = 0; i < set.Length; i++)
        {
            if (set[i].itemID == ID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// set에 있는 아이템들 중 ID값의 아이템이 있는지 확인합니다. 있다면 to 레퍼런스에 아이템을 저장합니다.
    /// </summary>
    /// <param name="set"></param>
    /// <param name="ID"></param>
    /// <param name="to"></param>
    /// <returns> 아이템 존재 여부 </returns>
    static bool TryItemFromSet(ItemData[] set, string ID, out ItemData to)
    {
        for (int i = 0; i < set.Length; i++)
        {
            if (set[i].itemID == ID)
            {
                to = set[i];
                return true;
            }
        }

        to = null;
        return false;
    }

    /// <summary>
    /// 이 아이템이 tag를 가지고 있는지 확인합니다.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns> 아이템 존재 여부 </returns>
    public bool HasTag(string tag)
    {
        for(int i = 0; i < tags.Length; i++)
        {
            if (tags[i] == tag) return true;
        }
        return false;
    }
}
