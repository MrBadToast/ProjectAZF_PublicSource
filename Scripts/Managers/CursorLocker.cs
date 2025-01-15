using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLocker : StaticSerializedMonoBehaviour<CursorLocker>
{
    //================================================
    //
    // 커서를 숨기거나 표시하는 시스템 스크립트입니다.
    //
    //================================================

    public enum CursorMode
    {
        Freelook,
        CursorVisible
    }

    [SerializeField,ReadOnly] private CursorMode cursorState = CursorMode.Freelook;
    /// <summary>
    /// 현재 커서 모드입니다.
    /// </summary>
    public CursorMode CurrentCursorState { get { return cursorState; } }

    private MainPlayerInputActions input;

    private int duplicateStack = 0;

    protected override void Awake()
    {
        base.Awake();
        input = new MainPlayerInputActions();
        input.Enable();
    }

    private void Update()
    {
        if(cursorState == CursorMode.CursorVisible)
        {
            if(Cursor.visible == false)
            {
                DisableFreelook();
            }
        }
        else
        {
            if (Cursor.visible == true)
            {
                EnableFreelook();
            }
        }
    }

    /// <summary>
    /// 커서를 숨깁니다. ( Freelook 모드로 전환합니다. )
    /// </summary>
    public void EnableFreelook()
    {
        if(duplicateStack != 0)
        {
            duplicateStack--; return;
        }

        cursorState = CursorMode.Freelook;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 커서를 표시합니다. ( Freelook 모드를 중지합니다. )
    /// </summary>
    public void DisableFreelook()
    {
        if(cursorState == CursorMode.CursorVisible)
        {
            duplicateStack++;
        }

        cursorState = CursorMode.CursorVisible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
