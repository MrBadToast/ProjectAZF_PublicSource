using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerCore player;

    public void FootstepEvent()
    {
        player.FootstepEvent();
    }

    public void OnPlayerPickupEnd()
    {
        player.Input.Enable();
    }
}
