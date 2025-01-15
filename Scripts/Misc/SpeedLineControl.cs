using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLineControl : StaticSerializedMonoBehaviour<SpeedLineControl>
{
    public Material speedLineMaterial;

    public void SetSpeedLine(float amount)
    {
        StopAllCoroutines();
        speedLineMaterial.SetFloat("_LineDensity",amount);
    }

    public void SetSpeedLine(float amount, float animateTime)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_SpeedLine(amount, animateTime));
    }

    private void OnDisable()
    {
        speedLineMaterial.SetFloat("_LineDensity", 0f);
    }

    private IEnumerator Cor_SpeedLine(float amount, float animateTime)
    {
        float prevValue = speedLineMaterial.GetFloat("_LineDensity");
        for(float t = 0; t < animateTime; t+= Time.fixedDeltaTime)
        {
            speedLineMaterial.SetFloat("_LineDensity", Mathf.InverseLerp(prevValue, amount, t / animateTime));
            yield return new WaitForFixedUpdate();
        }
    }
}
