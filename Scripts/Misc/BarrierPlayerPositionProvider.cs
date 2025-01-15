using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierPlayerPositionProvider : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField] private Transform splineTrackObject;
    private float distanceTollerance = 30f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        meshRenderer.material.SetFloat("_DistantAlpha", 0f);
    }

    private void FixedUpdate()
    {
        if (!PlayerCore.IsInstanceValid) return;

        if(splineTrackObject != null)
        {
            if (Vector3.Distance(splineTrackObject.position, PlayerCore.Instance.transform.position) > distanceTollerance)
                return;
        }

        Vector4 pos = new Vector4(PlayerCore.Instance.transform.position.x, PlayerCore.Instance.transform.position.y, PlayerCore.Instance.transform.position.z,0f);

        meshRenderer.material.SetVector("_PlayerPosition", pos);

    }
}
