using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class UI_FairwindInfo : StaticSerializedMonoBehaviour<UI_FairwindInfo>
{
    [SerializeField] private LocalizedString message_succeed;
    [SerializeField] private LocalizedString message_failTimeout;
    [SerializeField] private LocalizedString message_failRouteout;

    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject visualGroup;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject fairwnindObject;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI fairwindCountdown_integer;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI fairwindCountdown_frac;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject alertObject;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI alertCountdown_integer;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI alertCountdown_frac;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject successUI;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI successUI_text;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject failedUI;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI failedUI_text;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private GameObject additinalTime;
    [SerializeField, Required, FoldoutGroup("ChildReferences")]
    private TextMeshProUGUI additinalTime_text;

    private void Start()
    {
        visualGroup.SetActive(false);
        successUI.SetActive(false);
        failedUI.SetActive(false);

        fairwindCountdown_integer.text = "00";
        fairwindCountdown_frac.text = "00";
        alertCountdown_integer.text = "00";
        alertCountdown_frac.text = "00";
    }

    public void ToggleFairwindUI(bool value)
    {
        if (value == true)
        {
            visualGroup.SetActive(true);
        }
        else
        {
            successUI.SetActive(false);
            failedUI.SetActive(false);
            ToggleAlertUI(false);
            visualGroup.SetActive(false);
        }
    }

    public void ToggleAlertUI(bool value)
    {
        if(value != alertObject.activeInHierarchy)
            alertObject.SetActive(value);
    }

    public void SetFairwindCountdown(float time)
    {
        if (time < 0f) time = 0f;
        if (!visualGroup.activeInHierarchy) return;
        fairwindCountdown_integer.text = ((int)time).ToString();
        fairwindCountdown_frac.text = (((int)(time*100))%100).ToString();
    }

    public void SetAlertCountdown(float time)
    {
        if (time < 0f) time = 0f;
        if (!alertObject.activeInHierarchy) return;
        alertCountdown_integer.text = ((int)time).ToString();
        alertCountdown_frac.text = (((int)(time * 100)) % 100).ToString();
    }

    public void OnFairwindSuccessed()
    {
        ToggleAlertUI(false);       
        successUI.SetActive(true);
        successUI_text.text = message_succeed.GetLocalizedString();
    }

    public void OnFairwindTimeoutFailed()
    {
        ToggleAlertUI(false);
        failedUI.SetActive(true);
        failedUI_text.text = message_failTimeout.GetLocalizedString();
    }

    public void OnFairwindRouteoutFailed()
    {
        ToggleAlertUI(false);
        failedUI.SetActive(true);
        failedUI_text.text = message_failRouteout.GetLocalizedString();
    }

    public void OnAdditionalTime(float time)
    {
        additinalTime.SetActive(false);
        additinalTime.SetActive(true);
        additinalTime_text.text = "+ " + ((int)time).ToString();
    }
}
