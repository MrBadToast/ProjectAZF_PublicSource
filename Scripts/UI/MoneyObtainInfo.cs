using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyObtainInfo : StaticSerializedMonoBehaviour<MoneyObtainInfo>
{
    [SerializeField] float textInterval = 0.05f;
    [SerializeField] EventReference sound_textChangeTick;
    [SerializeField] DOTweenAnimation openAnimation;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] GameObject addMoneyText;

    public void MoneyChanged(int from, int add)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_MoneyChanged(from, add));
    }

    public IEnumerator Cor_MoneyChanged(int from, int add)
    {
        addMoneyText.GetComponent<CanvasGroup>().alpha = 1f;
        if (add >= 0)
        {
            addMoneyText.GetComponent<TextMeshProUGUI>().text = "+" + add.ToString();
        }
        else
        {
            addMoneyText.GetComponent<TextMeshProUGUI>().text = "-" + add.ToString();
        }
        openAnimation.DORestartById("MoneyInfo_Open");
        Tween tw = openAnimation.GetTweens()[0];
        yield return tw.WaitForCompletion();

        addMoneyText.GetComponent<DOTweenAnimation>().DORestart();

        if(Mathf.Abs(add) < 100)
        {
            if (add >= 0)
            {
                for (int i = 0; i < add; i++)
                {
                    amountText.text = (from + i).ToString();
                    RuntimeManager.PlayOneShot(sound_textChangeTick);
                    yield return new WaitForSecondsRealtime(textInterval);
                }
            }
            else
            {
                for (int i = from; i > from + add; i--)
                {
                    amountText.text = (from + i).ToString();
                    RuntimeManager.PlayOneShot(sound_textChangeTick);
                    yield return new WaitForSecondsRealtime(textInterval);
                }
            }
        }
        else
        {
            if (add >= 0)
            {
                for (float i = 0; i < add; i += add/100f)
                {
                    amountText.text = ((int)(from + i)).ToString();
                    RuntimeManager.PlayOneShot(sound_textChangeTick);
                    yield return new WaitForSecondsRealtime(textInterval);
                }
            }
            else
            {
                for (float i = from; i > from + add; i -= add/100f)
                {
                    amountText.text = ((int)(from + i)).ToString();
                    RuntimeManager.PlayOneShot(sound_textChangeTick);
                    yield return new WaitForSecondsRealtime(textInterval);
                }
            }
        }

        amountText.text = (from + add).ToString();
        yield return new WaitForSecondsRealtime(1.0f);

        openAnimation.DORestartById("MoneyInfo_Close");

    }
}
