using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
public class ParameterByDistance : MonoBehaviour
{
    [SerializeField] private float maxDistace = 5f;
    [SerializeField] private bool inversed = false;
    [SerializeField] private string parameterID;

    StudioEventEmitter sound;

    private void Awake()
    {
        sound = GetComponent<StudioEventEmitter>();
    }

    private void Update()
    {
        if (!PlayerCore.IsInstanceValid) return;

        if (!inversed)
            sound.SetParameter(parameterID, Mathf.InverseLerp(0, maxDistace, Vector3.Distance(transform.position, PlayerCore.Instance.transform.position)));
        else
            sound.SetParameter(parameterID, Mathf.InverseLerp(maxDistace, 0, Vector3.Distance(transform.position, PlayerCore.Instance.transform.position)));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistace);
    }
}
