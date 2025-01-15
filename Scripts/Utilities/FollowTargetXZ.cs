using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//============================================
//
// 현재 Transform이 Target의 X값과 Z값만 따라가도록 합니다. 
//
//============================================

public class FollowTargetXZ : MonoBehaviour
{
    public Transform target;

    [SerializeField] private bool SetPositionToOffset = true;
    [SerializeField] private bool isCliped = false;     // 현재 오브젝트가 연속적으로 움직이길 원하면 false, clipSize에 따라 불연속적으로 움직이길 원하면 true
    [SerializeField] private float clipSize = 0.1f;     // 불연속적으로 움직일 때 움직이는 간격

    private Vector3 offset;

    private void Start()
    {
        if (SetPositionToOffset)
            offset = transform.position;
        else
            offset = Vector3.zero;
    }

    private void Update()
    {
        if (isCliped)
        {
            transform.position = new Vector3(
                (target.position.x + offset.x) - (target.position.x + offset.x) % clipSize,
                offset.y,
                (target.position.z + offset.z) - (target.position.z + offset.z) % clipSize
                );
        }
        else
        {
            transform.position = new Vector3(target.position.x + offset.x, transform.position.y + offset.y, target.position.z + offset.z);
        }
    }
}
