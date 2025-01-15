using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemObtainInfo : StaticSerializedMonoBehaviour<ItemObtainInfo>
{
    [SerializeField] private GameObject visualGroup;
    [SerializeField] private EventReference sound_obtainSound;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI quantityText;

    MainPlayerInputActions input;

    protected override void Awake()
    {
        base.Awake();
        input = UI_InputManager.Instance.UI_Input;
    }

    public IEnumerator Cor_OpenWindow(ItemData item, int quantity = 1)
    {
        RuntimeManager.PlayOneShot(sound_obtainSound);

        visualGroup.SetActive(true);

        itemImage.sprite = item.ItemImage;
        itemNameText.text = item.ItemName.GetLocalizedString();
        itemDescriptionText.text = item.ItemDiscription.GetLocalizedString();

        if (quantity > 1) quantityText.text = "x" + quantity.ToString();
        else quantityText.text = string.Empty;

        yield return new WaitForSecondsRealtime(2.0f);

        yield return new WaitUntil(() => input.UI.Positive.IsPressed());

        visualGroup.SetActive(false);
    }

}
