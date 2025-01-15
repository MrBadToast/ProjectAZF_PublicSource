using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionIndicator : SerializedMonoBehaviour
{
    //================================================
    //
    // 플레이어에게 방향 지시 표시를 할 수 있는 오브젝트에 관한 클래스입니다.
    //
    //================================================


    private Transform target_transform;
    private Vector3 target_vector;
    [SerializeField] private GameObject visualGroup;
    [SerializeField] private Transform directionTransformObject;

    bool isTransformMode = false;

    private void Update()
    {
        if (visualGroup.activeInHierarchy)
        {
            if (isTransformMode)
            {
                Vector3 direction = (target_transform.position - PlayerCore.Instance.transform.position);
                directionTransformObject.forward = new Vector3(direction.x, 0f, direction.z);
            }
            if (!isTransformMode)
            {
                Vector3 direction = (target_vector - PlayerCore.Instance.transform.position).normalized;
                directionTransformObject.forward = new Vector3(direction.x, 0f, direction.z);
            }
        }
    }

    /// <summary>
    /// target의 트랜스폼 위치를 향해 방향 지시가 설정되도록 합니다.
    /// </summary>
    /// <param name="target"></param>
    public void EnableAndSetIndicator(Transform target)
    {
        visualGroup.SetActive(true);
        isTransformMode = true;
        target_transform = target;
    }

    /// <summary>
    /// target의 월드 좌표를 향해 방향 지시가 설정되도록 합니다.
    /// </summary>
    /// <param name="target"></param>
    public void EnableAndSetIndicator(Vector3 target)
    {
        visualGroup.SetActive(true);
        isTransformMode = false;
        target_vector = target;
    }

    /// <summary>
    /// 방향 지시를 숨깁니다.
    /// </summary>
    /// <param name="target"></param>
    public void DisableIndicator()
    {
        visualGroup.SetActive(false);
    }


}
