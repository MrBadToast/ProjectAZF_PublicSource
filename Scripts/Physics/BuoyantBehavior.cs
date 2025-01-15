using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

//================================================
//
// Rigidbody 컴포넌트에 바다의 부력을 통한 물리적인 힘을 추가해줄 수 있습니다.
// GlobalOceanManager가 없으면 사용할 수 없습니다.
// 방향을 가진 물체가 뒤집히지 않기 위해서는 최소 3개의 부력 지점이 필요합니다.
// 
//================================================

[RequireComponent(typeof(Rigidbody))]
public class BuoyantBehavior : MonoBehaviour
{
    [SerializeField] float bouyancyPower = 1.0f;        // 부력 세기
    [SerializeField] Transform[] floatingPoint;         // 부력을 받는 지점
    [SerializeField] float submergeOffset = 0f;

    [SerializeField, ReadOnly] private float submergeRate = 0.0f;
    public float SubmergeRateZeroClamped { get { return Mathf.Clamp(submergeRate, float.NegativeInfinity, 0.0f); } }     // 물체가 얼마나 침수됐는지 표현합니다.( 0 미만으로 내려가지 않습니다. )
    public float SubmergeRate01 { get { return Mathf.Clamp01(submergeRate); } }                                     // 물체가 얼마나 침수됐는지 표현합니다.( 0과 1사이의 값으로 표현됩니다. )
    public float SubmergeRate { get { return submergeRate; } }                                                      // 물체가 얼마나 침수됐는지 표현합니다.
    private bool waterDetected = false;
    public bool WaterDetected { get { return waterDetected; } }                                                        // 물체의 아래나 위에 연산 가능한 물이 있는지 나타냅니다.

    Rigidbody rbody;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (GlobalOceanManager.Instance == null)
        {
            Debug.Log("BuoyancyBehavior를 사용하려면 Global Ocean Manager를 생성하세요!");
            this.enabled = false;
        }
    }

    const int oceanLayerMask = 1 << 3;
    const int waterLayerMask = 1 << 4;
    const float bouyancymagnitude = 50f;

    private void FixedUpdate()
    {
        waterDetected = false;
        
        RaycastHit hitWater;

        submergeRate = float.PositiveInfinity;
        waterDetected = false;

        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hitWater, 10.0f, waterLayerMask))
        {
            waterDetected = true;
            float distance = 0;
            distance = transform.position.y - hitWater.point.y + submergeOffset;

            if (distance < 0)
            {
                rbody.AddForce(Vector3.up * -Mathf.Clamp( distance, -1f, 0f) * bouyancymagnitude, ForceMode.Acceleration);
            }

            submergeRate = distance;
        }
        if (Physics.Raycast(transform.position, Vector3.up, float.PositiveInfinity, oceanLayerMask) ||
            Physics.Raycast(transform.position, Vector3.down, float.PositiveInfinity, oceanLayerMask))
        {
            waterDetected = true;

            float[] submerged = new float[floatingPoint.Length];
            float average = 0f;

            for (int i = 0; i < submerged.Length; i++)
            {
                submerged[i] = floatingPoint[i].position.y - GlobalOceanManager.Instance.GetWaveHeight(floatingPoint[i].position);
                if (submerged[i] < 0)
                {
                    rbody.AddForceAtPosition(Vector3.up * bouyancyPower / floatingPoint.Length * -Mathf.Clamp(submerged[i], -1f, 0f), floatingPoint[i].position, ForceMode.Acceleration);
                }

                average += submerged[i];
            }

            average /= submerged.Length;

            if (submergeRate > average)
                submergeRate = average;
        }

    }



    private void OnTriggerStay(Collider other)
    {
        //if (other.gameObject.layer == 4)
        //{
        //    float distance = 0;
        //    distance = other.bounds.max.y - transform.position.y;       

        //    submergeRate = -Mathf.Clamp(distance,0f,5f);
        //    rbody.AddForceAtPosition(Vector3.up * -submergeRate * bouyancyPower * Time.deltaTime * bouyancymagnitude, transform.position, ForceMode.Acceleration);
        //}   
    }
}

