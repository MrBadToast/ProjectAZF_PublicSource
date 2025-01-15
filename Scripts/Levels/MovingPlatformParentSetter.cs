using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformParentSetter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic)
        {
            other.attachedRigidbody.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic)
        {
            other.attachedRigidbody.transform.SetParent(null);
        }
    }
}
