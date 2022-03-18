using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Extras : MonoBehaviour
{
    private Mesh _mesh;
    private List<Vector3> _vertices;
    private float _sizeOfCube;

    public void Preprocess()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _vertices = new List<Vector3>(_mesh.vertices);
        _sizeOfCube = GetSizeOfCube();
    }

    private float GetSizeOfCube()
    {
        float resut = Mathf.Infinity;
        for (int i = 1; i < _vertices.Count; i++)
        {
            float size = Mathf.Abs(_vertices[0].x - _vertices[i].x);
            if (size!= 0 && size < resut)
            {
                resut = size;
            }
        }
        return resut;
    }

    public void CubeWord(GameObject obj)
    {
        GameObject parent = new GameObject();
        parent.name = "CubeWord";
        List<GameObject> objects = new List<GameObject>();

        foreach (var v in _vertices)
        {
            objects.Add(DrawVertex(v, _sizeOfCube, parent, obj));
        }

        
        foreach (var o in objects)
        {
         EscaladoObjetoSegunEntorno(o);
        }
        
    }

    private void EscaladoObjetoSegunEntorno(GameObject o)
    {
        Renderer component = o.GetComponent<Renderer>();
        var bounds = component.bounds;
        Vector3 sizeDesired = bounds.size;

        float newSize = sizeDesired.z * 4;
        Debug.Log("BBBB:"+sizeDesired);
        Debug.Log("AAAA:"+newSize);
       
        Collider[] colliders = Physics.OverlapBox(bounds.center, new Vector3(newSize,newSize,newSize)/2);
        foreach (var c in colliders)
        {
            Debug.Log(c.transform.name);
        }

        if (colliders.Length > 0)
        {
            o.transform.localScale = rescaleOnNewSize(o, newSize);
        }
        
        Debug.Log("COLLIDERS LENGTH:"+colliders.Length);
    }


    private GameObject DrawVertex(Vector3 point, float sizeOfCube, GameObject parent, GameObject objToSpawn)
    {
        GameObject obj = Instantiate(objToSpawn, parent.transform, true);
        float size = obj.GetComponent<Renderer>().bounds.size.y;
        float rescale = obj.transform.localScale.x;

        rescale = sizeOfCube * rescale / size;
        obj.transform.localScale = new Vector3(rescale, rescale, rescale);
        obj.transform.position = point;
        return obj;
    }

    private Vector3 rescaleOnNewSize(GameObject obj, float newSize)
    {
        float size = obj.GetComponent<Renderer> ().bounds.size.x;
        float rescale = obj.transform.localScale.x;
        rescale = newSize * rescale / size;

        return new Vector3(rescale,rescale,rescale);
    }
    
}