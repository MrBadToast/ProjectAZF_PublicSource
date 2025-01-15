using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerDampedPoint : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float damp = 0.5f;
    [SerializeField, Required] private Transform playerTF;

    private void Start()
    {
        transform.parent = null;
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, playerTF.position, damp);
    }
}
