using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class Interactable_Base : SerializedMonoBehaviour
{
    //================================================
    //
    // Interactable 계열 스크립트들의 베이스 클래스입니다.
    // 플레이어가 가까이 오면서 Trigger를 건드리면 상호작용 UI를 띄우고, 상호작용 키를 누르면 오버라이드된 Interact를 호출합니다.
    //
    //================================================

    [SerializeField, LabelText("텍스트 활성화")] protected bool enableInteractionUI = true;
    [SerializeField, LabelText("상호작용 텍스트"),ShowIf("enableInteractionUI")] protected LocalizedString interactionUIText;    // 가까이 접근 시 상호작용 UI에 띄울 텍스트
    protected MainPlayerInputActions input;
    protected bool isEnabled = true;

    /// <summary>
    /// 오버라이드 하여 적절한 상호작용을 하게 만드세요
    /// </summary>
    public virtual void Interact() { }

    /// <summary>
    /// 인풋 이벤트
    /// </summary>
    /// <param name="context"></param>
    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (!isEnabled) return;

        if(PlayerCore.IsInstanceValid)
        {
            if (PlayerCore.Instance.IsHoldingSomething)
                return;
        }

        if(UI_InteractionInfo.IsInstanceValid)
        {
            UI_InteractionInfo.Instance.HideCurrentInfo();
        }

        Interact();
    }

    public void OnTriggerEnter(Collider other)
    {
        if(!isEnabled) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            input = other.GetComponentInParent<PlayerCore>().Input;
            input.Player.Interact.performed += OnInteractInput;

            if (interactionUIText.GetLocalizedString() != string.Empty)
            {
                if (enableInteractionUI)
                {
                    if (UI_InteractionInfo.IsInstanceValid)
                    {
                        var infoUI = UI_InteractionInfo.Instance;

                        if (!infoUI.CompareCurrentTarget(transform))
                        {
                            infoUI.SetNewInteractionInfo(transform, interactionUIText.GetLocalizedString());
                        }
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (input != null)
                input.Player.Interact.performed -= OnInteractInput;
            input = null;

            if (enableInteractionUI)
            {
                if (UI_InteractionInfo.IsInstanceValid)
                {
                    var infoUI = UI_InteractionInfo.Instance;

                    if (infoUI.CompareCurrentTarget(transform))
                    {
                        infoUI.HideCurrentInfo();
                    }
                }
            }
        }
    }

    public void OnDisable()
    {
        if (input != null)
        {
            input.Player.Interact.performed -= OnInteractInput;
            input = null;
        }

        if (UI_InteractionInfo.IsInstanceValid)
        {
            var infoUI = UI_InteractionInfo.Instance;

            if (infoUI.CompareCurrentTarget(transform))
            {
                infoUI.HideCurrentInfo();
            }
        }
    }
}
