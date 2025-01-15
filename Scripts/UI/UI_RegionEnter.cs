using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class UI_RegionEnter : StaticSerializedMonoBehaviour<UI_RegionEnter>
{
    // 플래이어가 새로운 섬 지역에 들어갔을 때 띄우는 UI 입니다.

    [SerializeField] private GameObject visualGroup;
    [SerializeField] private TextMeshProUGUI regionText;
    [SerializeField] new private Animation animation;

    public void OnRegionEnter(string regionName)
    {
        visualGroup.SetActive(true);
        regionText.text = regionName;
        animation.Play();
    }

    public void OnRegionEnter(LocalizedString regionName)
    {
        Debug.Log("regionEnter");
        visualGroup.SetActive(true);
        regionText.text = regionName.GetLocalizedString();
        animation.Play();
    }

    public void OnAnimationEnd()
    {
        visualGroup.SetActive(false);
    }
}
