using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractionInfo : StaticSerializedMonoBehaviour<UI_InteractionInfo>
{
    //===============================
    //
    // [싱글턴 오브젝트]
    // Interaction이 가능한 오브젝트에 가까이가면 띄우는 UI들을 관리합니다.
    //
    //===============================  
    [SerializeField] private CanvasGroup visualGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI textMesh;

    private Transform worldObjectTarget = null;

    private void Start()
    {
        visualGroup.gameObject.SetActive(false);
    }

    const float yOffsetTarget = 100f;
    float yOffset = 0;

    private void FixedUpdate()
    {
        if (visualGroup.gameObject.activeInHierarchy && worldObjectTarget != null)
        {
            if (PlayerCore.IsInstanceValid)
            {
                if (PlayerCore.Instance.IsHoldingSomething || !PlayerCore.Instance.Input.Player.enabled) HideCurrentInfo();
            }

            if (yOffset <= yOffsetTarget - 1f)
                yOffset = Mathf.Lerp(yOffset, yOffsetTarget, 0.5f);
            else
                yOffset = yOffsetTarget;

            if (worldObjectTarget != null)
                panelRect.position = Camera.main.WorldToScreenPoint(worldObjectTarget.position) + Vector3.up * yOffset;

        }
    }

    public void SetNewInteractionInfo(Transform target, string infoName)
    {
        yOffset = 0f;
        visualGroup.gameObject.SetActive(true);
        worldObjectTarget = target;
        textMesh.text = infoName;
        Canvas.ForceUpdateCanvases();
    }

    public bool CompareCurrentTarget(Transform target)
    {
        if (worldObjectTarget == target) return true;
        else return false;
    }

    public void HideCurrentInfo()
    {
        yOffset = 0f;
        visualGroup.gameObject.SetActive(false);
        worldObjectTarget = null;
    }
}
