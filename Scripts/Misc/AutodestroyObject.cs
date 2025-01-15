using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutodestroyObject : MonoBehaviour
{
    public float time = 5f;
    public bool startTimerOnEnabled;

    private void OnEnable()
    {
        if(startTimerOnEnabled)
        {
            Invoke("Destroy", time);
        }
    }

    private void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}
