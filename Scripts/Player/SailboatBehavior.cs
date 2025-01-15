using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailboatBehavior : MonoBehaviour
{
    [Title("BottomPlane")]
    [SerializeField] private Transform floatingPoint1;
    [SerializeField] private Transform floatingPoint2;
    [SerializeField] private Transform floatingPoint3;

    private Plane surfacePlane;
    public Plane SurfacePlane { get { return surfacePlane; } }
    [SerializeField,ReadOnly] private float submergeRate = 0.0f;
    public float SubmergeRate { get { return submergeRate; } }

    private void Start()
    {
        if (GlobalOceanManager.Instance == null)
        {
            Debug.Log("SailboatBehavior를 사용하려면 Global Ocean Manager를 생성하세요!");
        }

        surfacePlane = new Plane(Vector3.up, 0f);
    }

    const int oceanLayerMask = 1 << 3;
    const int waterLayerMask = 1 << 4;

    private void FixedUpdate()
    {

        RaycastHit hitWater;

        submergeRate = float.PositiveInfinity;

        if ( Physics.Raycast(transform.position + Vector3.up*5f, Vector3.down, out hitWater, 10.0f, waterLayerMask))
        {
            surfacePlane = new Plane(Vector3.up, 0f);

            float distance = 0;
            distance = transform.position.y - hitWater.point.y;
            submergeRate = distance;
        }
        if (Physics.Raycast(transform.position, Vector3.up, float.PositiveInfinity, oceanLayerMask) ||
            Physics.Raycast(transform.position, Vector3.down, float.PositiveInfinity, oceanLayerMask))
        {

            float average = 0;
            float[] surface = new float[3];

            surface[0] = GlobalOceanManager.Instance.GetWaveHeight(floatingPoint1.position);
            surface[1] = GlobalOceanManager.Instance.GetWaveHeight(floatingPoint2.position);
            surface[2] = GlobalOceanManager.Instance.GetWaveHeight(floatingPoint3.position);

            average = (floatingPoint1.position.y - surface[0] + floatingPoint2.position.y - surface[1] + floatingPoint3.position.y - surface[2]) / 3f;

            if (Vector3.Dot(transform.up, Vector3.down) > 0.5f) transform.up = Vector3.up;

            if (submergeRate > average)
            {
                submergeRate = average;

                surfacePlane = new Plane(
                    new Vector3(floatingPoint1.position.x, surface[0], floatingPoint1.position.z),
                    new Vector3(floatingPoint2.position.x, surface[1], floatingPoint2.position.z),
                    new Vector3(floatingPoint3.position.x, surface[2], floatingPoint3.position.z));
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, surfacePlane.normal);
    }
}
