using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ClosedSplineUtilities
{
    /// <summary>
    /// Spine의 내부에 있는지 확인하는 함수입니다. *반드시 스플라인이 "시계방향"으로 진행해야 합니다!!
    /// 출처 : https://forum.unity.com/threads/figure-out-whether-a-point-is-inside-a-unity-spline-new-spline-system.1508984/
    /// </summary>
    /// <param name="point">지점</param>
    /// <param name="splineContainer">스플라인 영역</param>
    /// <param name="nearestPointInSpline">(out)플레이어에서 가장 가까운 Spline경계면 위치</param>
    /// <returns></returns>
    public static bool IsInsideSpline(Vector3 point, SplineContainer splineContainer, out Vector3 nearestPointInSpline,int resolution = 2, int iterations = 1)
    {
        Vector3 pointPositionLocalToSpline = splineContainer.transform.InverseTransformPoint(point);
        Bounds splineBounds = splineContainer.Spline.GetBounds();

        SplineUtility.GetNearestPoint(splineContainer.Spline, pointPositionLocalToSpline, out var splinePoint, out var t,resolution,iterations);
        splinePoint.y = pointPositionLocalToSpline.y;


        if (Vector3.Distance(point, splineContainer.transform.TransformPoint(splineBounds.center)) < Vector3.Distance(splinePoint, splineBounds.center))
        {
            // If point is inside of the spline...
            nearestPointInSpline = point;
            return true;
        }
        else
        {
            nearestPointInSpline = splineContainer.transform.TransformPoint(splinePoint);
            return false;
        }
    }
}
