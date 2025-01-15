using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InputManager : StaticSerializedMonoBehaviour<UI_InputManager>
{
    private MainPlayerInputActions ui_input;
    public MainPlayerInputActions UI_Input { get { return ui_input; } }

#if UNITY_EDITOR
    [ReadOnly] public bool debug_ui_input_enabled;
#endif

    protected override void Awake()
    {
        base.Awake();
        ui_input = new MainPlayerInputActions();
        ui_input.Enable();
    }

    public void DisableUIInputs()
    {
        ui_input.Disable();
    }

    public void EnableUIInputs()
    {
        ui_input.Enable();
    }
}
