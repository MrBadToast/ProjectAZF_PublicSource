using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Quickexit : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private Image ringImg;
    [SerializeField] private float time;

    float progress = 0;

    private void FixedUpdate()
    {
        if (Keyboard.current.backquoteKey.isPressed)
        {
            progress += Time.fixedDeltaTime;
            canvas.alpha = Mathf.Lerp(canvas.alpha, 1f, 0.5f);
            if (time < progress)
            { 
                Application.Quit();
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
        }
        else
        {
            if (progress > 0)
                progress -= Time.fixedDeltaTime * 3f;
            canvas.alpha = Mathf.Lerp(canvas.alpha, 0f, 0.5f);
        }

        ringImg.fillAmount = progress / time;
    }
}
