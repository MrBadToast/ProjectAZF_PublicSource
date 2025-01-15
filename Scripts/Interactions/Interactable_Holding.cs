using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Holding : Interactable_Base
{
    //================================================
    //
    // Interactable_Base의 자식 클래스,  Interact를 오버라이드
    // Interact시 플레이어가 이 오브젝트를 듭니다.
    // 이 때 PlayerCore의 HoldItem이 호출됩니다.
    //
    //================================================

    public Transform leftHandPoint;         // 플레이어가 아이템을 잡을 때 왼손의 위치와 각도를 나타냅니다.
    public Transform rightHandPoint;        // 플레이어가 아이템을 잡을 때 오른손의 위치와 각도를 나타냅니다.

    private bool isHolding = false;
#pragma warning disable CS0108
    [SerializeField] private Rigidbody rigidbody;
#pragma warning restore CS0108
    [SerializeField] private Collider collision;

    public override void Interact()
    {
        PlayerCore player = PlayerCore.Instance;

        if (isHolding) return;        
        if (player == null) return;

        Hold(player);
    }
    
    public void Hold(PlayerCore player)
    {
        if (player.HoldItem(leftHandPoint, rightHandPoint, this))
        {
            isHolding = true;
            rigidbody.isKinematic = true;
            collision.enabled = false;
            base.isEnabled = false;
        }
    }

    public void Release()
    {
        isHolding = false;
        rigidbody.isKinematic = false;
        collision.enabled = true;
        base.isEnabled = true;
    }
}
