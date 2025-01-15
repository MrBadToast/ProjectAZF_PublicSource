using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroWarpPlayer : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;

    public void WarpPlayer()
    {
        if (!PlayerCore.IsInstanceValid) return;

        PlayerCore.Instance.transform.position = targetTransform.position;
        PlayerCore.Instance.transform.forward = targetTransform.forward;
        PlayerCore.Instance.DisableControls();
    }

    public void EnablePlayer()
    {
        PlayerCore.Instance.EnableControls();
    }
}
