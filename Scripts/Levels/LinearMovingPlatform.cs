using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LinearMovingPlatform : SerializedMonoBehaviour
{
    //================================================
    //
    // 선형으로 경로를 찍으며 움직이는 플랫폼에 대한 스크립트입니다.
    //
    //================================================

    [SerializeField,LabelText("경로")] private Transform[] pathway;

    [SerializeField, LabelText("속도")] private float moveSpeed = 1f;
    [SerializeField, LabelText("애니메이션 커브")] private AnimationCurve curve;
    [SerializeField, LabelText("자동 시작")] private bool activateOnStart = false;
    [SerializeField, LabelText("자동 진행")] private bool continuous = true;
    [SerializeField, LabelText("중간 정차 시간"), ShowIf("continuous")] private float intermissionTime = 0f;
    [SerializeField, LabelText("충돌 레이어")] private LayerMask obstructLayer;


    private Rigidbody rBody;

    [ShowInInspector, ReadOnly] private bool isMoving = false;
    [ShowInInspector, ReadOnly] private bool moveStarterFlag = true;
    [ShowInInspector, ReadOnly] private bool obstructed = false;
    private Vector3 prevPosition = Vector3.zero;
    [ShowInInspector,ReadOnly] private Vector3 delta = Vector3.zero;


    /// <summary>
    /// 플랫폼이 정지 상태일 때 다음 지점으로 움직이도록 합니다.
    /// </summary>
    public void StartMove()
    {
        if (isMoving) return;

        if (!continuous && !moveStarterFlag)
        {
            moveStarterFlag = true;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Cor_PlatformMove());

    }

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (pathway == null || pathway.Length < 2)
        {
            Debug.LogWarning("움직이는 플랫폼의 pathway지점이 최소 2개 이상 필요합니다!");
            this.enabled = false;
        }


        if(activateOnStart)
        {
            StartMove();
        }
    }

    private void Update()
    {
    }

    private IEnumerator Cor_PlatformMove()
    {
        isMoving = true;
        moveStarterFlag = false;

        while (true)
        {
            for (int path = 0; path < pathway.Length - 1; path++)
            {
                Vector3 fromPath = pathway[path].position;
                Vector3 toPath = pathway[path+1].position;

                yield return StartCoroutine(Cor_MoveToPosition(fromPath, toPath));

                if(obstructed)
                {
                    yield return StartCoroutine(Cor_MoveToPosition(transform.position, fromPath));
                    obstructed = false;
                    path--;
                }

                yield return StartCoroutine(Cor_BeforeNext());
            }
            yield return StartCoroutine(Cor_MoveToPosition(pathway[pathway.Length-1].position, pathway[0].position));
            yield return StartCoroutine(Cor_BeforeNext());
        }
    }

    private IEnumerator Cor_MoveToPosition(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);

        for (float t = 0; Mathf.InverseLerp(from.x, to.x, transform.position.x) < 1.0f; t += Time.fixedDeltaTime * 20f * moveSpeed / distance)
        {
            transform.position = Vector3.Lerp(from, to, curve.Evaluate(t));
            yield return new WaitForFixedUpdate();
            if(obstructed)
            {
                yield break;
            }
        }

        transform.position = to;
    }

    private IEnumerator Cor_BeforeNext()
    {
        if (!continuous)
        {
            isMoving = false;
            yield return new WaitUntil(() => (moveStarterFlag == true));
            moveStarterFlag = false;
            isMoving = true;
        }
        else
        {
            if (intermissionTime > 0f)
            {
                yield return new WaitForSeconds(intermissionTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((1 << collision.gameObject.layer & obstructLayer) == 1)
        {
            if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) < 0.7f)
            {
                Debug.Log("Platform Obstructed");
                obstructed = true;
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {


    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (pathway == null || pathway.Length < 2) return;

            //pathway[0].position = transform.position;
            //transform.position = pathway[0].position;

            Gizmos.color = Color.yellow;

        for (int i = 0; i < pathway.Length-1; i++)
        {
            if (pathway[i] == null) continue;
            Gizmos.DrawWireSphere(pathway[i].position, 0.5f);
            Gizmos.DrawLine(pathway[i].position, pathway[i+1].position);
        }
        Gizmos.DrawWireSphere(pathway[pathway.Length-1].position, 0.5f);
        Gizmos.DrawLine(pathway[pathway.Length - 1].position, pathway[0].position);
    }

#endif
}
