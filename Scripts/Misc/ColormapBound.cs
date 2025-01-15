using StylizedGrass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrassColorMapRenderer),typeof(Collider))]
public class ColormapBound : MonoBehaviour
{
    GrassColorMapRenderer colormap;

    private void Awake()
    {
        colormap = GetComponent<GrassColorMapRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            colormap.ActivateColorMap();
        }
    }

    private void OnDrawGizmosSelected()
    {
        colormap = GetComponent<GrassColorMapRenderer>();

        colormap.ActivateColorMap();
    }
}
