using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Blackout : StaticSerializedMonoBehaviour<UI_Blackout>
{
    [SerializeField] AnimationCurve defaultCurve;
    [SerializeField, FoldoutGroup("ChildRefernce")] private CanvasGroup visualGroup;

    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_FadeOut(duration));
    }

    public void FadeOut(float duration, AnimationCurve curve)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_FadeOut(duration, curve));
    }

    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_FadeIn(duration));
    }

    public void FadeIn(float duration, AnimationCurve curve)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_FadeIn(duration));
    }

    public IEnumerator Cor_FadeOut(float duration)
    {
        for(float time = 0f; time < duration; time += Time.fixedDeltaTime)
        {
            float t = time / duration;
            visualGroup.alpha = defaultCurve.Evaluate(t);
            yield return new WaitForFixedUpdate();
        }
        visualGroup.alpha = 1f;
    }

    public IEnumerator Cor_FadeIn(float duration)
    {
        for(float time = 0f; time < duration; time += Time.fixedDeltaTime)
        {
            float t = time / duration;
            visualGroup.alpha = defaultCurve.Evaluate(1f-t);
            yield return new WaitForFixedUpdate();
        }
        visualGroup.alpha = 0f;
    }

    public IEnumerator Cor_FadeOut(float duration, AnimationCurve curve)
    {
        Debug.Log(curve == null);
        for (float time = 0f; time < duration; time += Time.fixedDeltaTime)
        {
            float t = time / duration;
            visualGroup.alpha = curve.Evaluate(t);
            yield return new WaitForFixedUpdate();
        }
        visualGroup.alpha = 1f;
    }

    public IEnumerator Cor_FadeIn(float duration, AnimationCurve curve)
    {
        for (float time = 0f; time < duration; time += Time.fixedDeltaTime)
        {
            float t = time / duration;
            visualGroup.alpha = curve.Evaluate(1f-t);
            yield return new WaitForFixedUpdate();
        }
        visualGroup.alpha = 0f;
    }
}
