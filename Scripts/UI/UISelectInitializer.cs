using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UISelectInitializer : MonoBehaviour
{
    public GameObject FirstButton;
    public GameObject SelectOnDisable;
    public UnityEvent OnCancel;

    string gamepadSchemeName = "Gamepad";

    MainPlayerInputActions input;

    private void Awake()
    {
        input = UI_InputManager.Instance.UI_Input;
    }

    private void OnEnable()
    {
        if(UI_PlaymenuBehavior.IsInstanceValid)
        {
            if (UI_PlaymenuBehavior.Instance.IsTitlelineSelected) return;
        }

        if (FirstButton != null)
        {
            if (PlayerInput.all[0].currentControlScheme == gamepadSchemeName)
            {
                //EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(FirstButton);
                Debug.Log("EventSystem.current Changed : " + FirstButton.name);
            }
        }

        if (OnCancel != null)
        {
            if (input.Player.OpenPlaymenu.IsPressed())
                input.UI.Cancel.Reset();
            else
                input.UI.Cancel.performed += CloseThis;
        }
    }

    public void CloseThis(InputAction.CallbackContext context)
    {
        OnCancel.Invoke();
    }

    private void OnDisable()
    {
        if (UI_PlaymenuBehavior.IsInstanceValid)
        {
            if (UI_PlaymenuBehavior.Instance.IsTitlelineSelected) return;
        }

        if (SelectOnDisable != null)
        {
            if (PlayerInput.all[0].currentControlScheme == gamepadSchemeName)
            {
                //EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(SelectOnDisable);
                Debug.Log("EventSystem.current Changed : " + SelectOnDisable.name);
            }
        }

        if(OnCancel != null)
        {
            input.UI.Cancel.performed -= CloseThis; 
        }
    }
}
