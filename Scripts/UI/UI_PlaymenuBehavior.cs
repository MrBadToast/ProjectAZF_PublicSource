using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public enum PlaymenuType // 플레이메뉴 타입
{
    Null,
    Inventory,
    Quest,
    Sailboat,
    Worldmap,
    Settings,
    ControlHelp
}

public class UI_PlaymenuBehavior : StaticSerializedMonoBehaviour<UI_PlaymenuBehavior>
{
    //===============================
    //
    // [싱글턴 오브젝트]
    // 인벤토리나, 지도, 기록 같은 플레이 메뉴 UI를 모두 관리합니다.
    // 메뉴 관련 요소들은 하위 오브젝트가 아닌 여기서 호출하세요!
    //
    //===============================  

    private struct PlaymenuElement
    {
        public PlaymenuType assignedType;
        public GameObject windowObject;
        public LocalizedString titleString;
    }

    [SerializeField] private PlaymenuElement[] playmenues;

    [SerializeField] EventReference sound_Open;         // 소리 : 메뉴 오픈시 소리
    [SerializeField] EventReference sound_Close;        // 소리 : 메뉴 닫을시 소리

    [SerializeField] private GameObject visualGroup;
    [SerializeField] private GameObject titleLineObject;

    public bool IsTitlelineSelected { get { return EventSystem.current.gameObject.Equals(titleLineObject); } }
    [SerializeField] TextMeshProUGUI titleLineTextmesh;
    [SerializeField] TextMeshProUGUI nextTextmesh;
    [SerializeField] TextMeshProUGUI prevTextemesh;

    MainPlayerInputActions input;

    private int activePlayemenuIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        input = UI_InputManager.Instance.UI_Input;
        input.Player.Enable();
        input.Player.OpenPlaymenu.performed += OnOpenKeydown;
        input.UI.SecondaryNav.performed += OnSecondaryNavigate;
    }

    private void Start()
    {
        visualGroup.SetActive(false);
    }

    private void OnEnable()
    {
        
    }

    public void EnableInput()
    {
        input.Player.Enable();
    }

    public void DisableInput()
    {
        input.Player.Disable();
    }

    public void OnOpenKeydown(InputAction.CallbackContext context)
    {
        if (visualGroup.activeInHierarchy == false)
        {
            OpenPlaymenu(playmenues[activePlayemenuIndex].assignedType);
        }
        else
        {
            ClosePlaymenu();
        }
    }

    public void OnSecondaryNavigate(InputAction.CallbackContext context)
    {
        if (visualGroup.activeInHierarchy == false) return;

        if (context.ReadValue<float>() > 0)
            BrowseNext();
        else if (context.ReadValue<float>() < 0)
            BrowsePrev();

    }

    public void BrowseNext()
    {
        if (activePlayemenuIndex < playmenues.Length-1) activePlayemenuIndex++;
        else activePlayemenuIndex = 0;

        titleLineTextmesh.GetComponent<DOTweenAnimation>().DORestartById("FromRight");
        ChangePlaymenu(playmenues[activePlayemenuIndex].assignedType);
    }

    public void BrowsePrev()
    {
        if (activePlayemenuIndex > 0) activePlayemenuIndex--;
        else activePlayemenuIndex = playmenues.Length-1;

        titleLineTextmesh.GetComponent<DOTweenAnimation>().DORestartById("FromLeft");
        ChangePlaymenu(playmenues[activePlayemenuIndex].assignedType);

    }


    /// <summary>
    /// 플레이 메뉴를 엽니다.
    /// </summary>
    /// <param name="playmenu">메뉴 종류</param>
    public void OpenPlaymenu(PlaymenuType playmenu = PlaymenuType.Settings)
    {
        visualGroup.SetActive(true);
        CursorLocker.Instance.DisableFreelook();
        PlayerCore gameplayer = PlayerCore.Instance;
        if (gameplayer != null) { gameplayer.DisableControls(); }


        RuntimeManager.PlayOneShot(sound_Open);
        ChangePlaymenu(playmenu);

    }

    /// <summary>
    /// 플레이 메뉴를 닫습니다.
    /// </summary>
    public void ClosePlaymenu()
    {
        CloseAllWindowObject();
        visualGroup.SetActive(false);
        CursorLocker.Instance.EnableFreelook();

        PlayerCore gameplayer = PlayerCore.Instance;
        if (gameplayer != null) { gameplayer.EnableControls(); }

        RuntimeManager.PlayOneShot(sound_Close);

    }

    private void ChangePlaymenu(PlaymenuType playmenuType = PlaymenuType.Settings)
    {
        CloseAllWindowObject();

        PlaymenuElement menu = FindElement(playmenuType);
        if (menu.assignedType != PlaymenuType.Null)
        {
            menu.windowObject.SetActive(true);
        }
        else
        {
            return;
        }

        // 인벤토리
        if (playmenuType == PlaymenuType.Inventory)
        {
            PlayerInventoryContainer inventoryContainer = PlayerInventoryContainer.Instance;
            if (inventoryContainer == null) { Debug.Log("인벤토리 열기를 시도했지만 PlayterInventoryContainer를 찾을 수 없었습니다."); return; }

            UI_InventoryBehavior inventory = UI_InventoryBehavior.Instance;
            inventory.SetInventory(inventoryContainer.InventoryData);
            inventory.SetMoney(inventoryContainer.Money);
        }

        UpdatePlayemenuTitle();

    }

    private void CloseAllWindowObject()
    {
        foreach (var m in playmenues)
        {
            m.windowObject.SetActive(false);
        }
    }


    private PlaymenuElement FindElement(PlaymenuType playmenu)
    {
        foreach(var m in playmenues)
        {
            if (m.assignedType == playmenu) 
                return m;
        }

        Debug.LogWarning("Failed to file PlaymenuE  lement : " + playmenu.ToString() + " | FindElement will return NullElement");
        PlaymenuElement nullElement = new PlaymenuElement();
        nullElement.assignedType = PlaymenuType.Null;
        return nullElement;
    }

    private void UpdatePlayemenuTitle()
    {
        titleLineTextmesh.text = playmenues[activePlayemenuIndex].titleString.GetLocalizedString();

        if (activePlayemenuIndex + 1 >= playmenues.Length)
            nextTextmesh.text = playmenues[0].titleString.GetLocalizedString();
        else
            nextTextmesh.text = playmenues[activePlayemenuIndex+1].titleString.GetLocalizedString();

        if (activePlayemenuIndex - 1 < 0)
            prevTextemesh.text = playmenues[playmenues.Length-1].titleString.GetLocalizedString();
        else
            prevTextemesh.text = playmenues[activePlayemenuIndex - 1].titleString.GetLocalizedString();

    }
}
