using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UI_SailboatSkillInfo : StaticSerializedMonoBehaviour<UI_SailboatSkillInfo>
{
    [SerializeField] private CanvasGroup boosterGroup;
    [SerializeField] private Image boosterRing;
    [SerializeField] private DOTweenAnimation boosterAnimation;
    [SerializeField] private CanvasGroup leapupGroup;
    [SerializeField] private Image leapupRing;
    [SerializeField] private DOTweenAnimation leapupAnimation;
    [SerializeField] private DOTweenAnimation tweenAnimation;

    bool boosterAvailable = true;
    bool leapupAvailable = true;

    float disabledAlpha = 0.5f;

    public void Update()
    {
        boosterGroup.alpha = Mathf.Lerp(boosterGroup.alpha, boosterAvailable ? 1f : disabledAlpha, 0.5f);
        leapupGroup.alpha = Mathf.Lerp(leapupGroup.alpha, leapupAvailable ? 1f : disabledAlpha, 0.5f);
    }

    public void ToggleInfo(bool value)
    {
        if (value)
        {
            tweenAnimation.DORestartAllById("Open");
        }
        else
        {
            tweenAnimation.DORestartAllById("Close");
        }
    }

    public void SetBoosterAvailable(bool value)
    {
        boosterAvailable = value;
    }

    public void SetBoosterRing(float value)
    {
        value = Mathf.Clamp01(value);
        boosterRing.fillAmount = value;
    }

    public void AnimateBoosterRing()
    {
        boosterAnimation.DORestart();
    }

    public void SetLeapupAvailable(bool value)
    {
        leapupAvailable = value;
    }

    public void SetLeapupRing(float value)
    {
        value = Mathf.Clamp01(value);
        leapupRing.fillAmount = value;
    }

    public void AnimateLeapupRing()
    {
        leapupAnimation.DORestart();
    }
}
