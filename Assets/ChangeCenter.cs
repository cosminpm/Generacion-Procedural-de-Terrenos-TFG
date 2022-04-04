using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCenter : MonoBehaviour
{
    // Start is called before the first frame update


    public bool Recalculate;
    private void OnValidate()
    {
        GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
    }
}
