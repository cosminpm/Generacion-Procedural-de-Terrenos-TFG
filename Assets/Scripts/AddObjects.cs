using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class AddObjects : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;

    [HideInInspector] public float minHeight, maxHeight, heightToSpawn;

    // Generate list of random objects to cast
    private List<Vector3> GenerateListOfRandomVectors(int nElements)
    {
        Vector3[] corners = GetCornersVectorAndMaxHeight();
        List<Vector3> r = new List<Vector3>();
        for (int i = 0; i < nElements; i++)
        {
            Vector3 v = new Vector3(Random.Range(corners[0].x, corners[2].x), heightToSpawn,
                Random.Range(corners[0].z, corners[1].z));
            r.Add(v);
        }

        return r;
    }

    private Quaternion Rotate(bool rotate, Vector3 normal)
    {
        if (rotate)
            return Quaternion.FromToRotation(Vector3.up, normal);
        else
            return Quaternion.Euler(Vector3.up);
    }

    // Raycast an object
    public GameObject PositionRayCast(Vector3 origin, GameObject objectToSpawn, bool rotate, GameObject parent,
        bool avoidDoubleCollision, float[] rangeBetween, bool randomRotate)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity))
        {
            if (avoidDoubleCollision && !hit.transform.CompareTag("Terrain"))
                return null;

            var spawnRotation = Rotate(rotate, hit.normal);
            var o = Instantiate(objectToSpawn, hit.point, spawnRotation);

            if (rangeBetween != null)
            {
                float scale = Random.Range(rangeBetween[0], rangeBetween[1]);
                o.transform.localScale *= scale;
            }

            if (randomRotate)
            {
                float rotation = Random.Range(0, 360);
                o.transform.Rotate(Vector3.up, rotation);
            }

            o.transform.parent = parent.transform;
            return o;
        }
        else
            return null;
    }


    private void RemoveColliding(List<GameObject> collisionObjects)
    {
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < collisionObjects.Count; i++)
        {
            for (int j = 0; j < collisionObjects.Count; j++)
            {
                if (j == i)
                    continue;

                if (collisionObjects[i].GetComponent<Collider>().bounds
                    .Intersects(collisionObjects[j].GetComponent<Collider>().bounds))
                {
                    result.Add(collisionObjects[j]);
                    collisionObjects.Remove(collisionObjects[j]);
                }
            }
        }
        foreach (var o in result)
        {
            DestroyImmediate(o);
        }
    }


    // Cast multiple raycasts to place objects
    public void MultipleRaycast(int nElements, GameObject objectToSpawn, bool rotate, bool avoidDoubleCollision,
        float[] rangeScale, bool rdmRotate, float[] heights)
    {
        GameObject parent = new GameObject();
        parent.name = objectToSpawn.name;
        parent.transform.parent = transform;
        List<Vector3> corners = GenerateListOfRandomVectors(nElements);

        List<GameObject> objects = new List<GameObject>();
        foreach (var c in corners)
        {
            
            GameObject o = PositionRayCast(c, objectToSpawn, rotate, parent, avoidDoubleCollision, rangeScale,
                rdmRotate);

            if (heights[0] == 0 && heights[1] == 0 && o != null)
            { 
                objects.Add(o);
                Debug.Log("COLOCE OBJETO");
            }
            
            else if (heights[0] != 0 && heights[1] != 0 && o.transform.position.y > heights[0] && o.transform.position.y < heights[1] && o != null)
            {
                objects.Add(o);
                Debug.Log("COLOCE OBJETO");
            }
            else
            {
                DestroyImmediate(o);
                Debug.Log("NOOOO");
            }

            
        }

        if (avoidDoubleCollision)
            RemoveColliding(objects);
    }

    public void MultipleRaycast(int nElements, List<GameObject> objectsToSpawn, bool rotate, bool avoidDoubleCollision,
        float[] rangeScale, bool rdmRotate, float[] heights)
    {
        int sizeOfList = objectsToSpawn.Count;
        foreach (var o in objectsToSpawn)
        {
            MultipleRaycast(nElements / sizeOfList, o, rotate, avoidDoubleCollision, rangeScale, rdmRotate, heights);
        }
    }

    private void GetComponents()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _vertices = _mesh.vertices;
    }

    public void Preprocess()
    {
        GetComponents();
        GetCornersVectorAndMaxHeight();
        GetMinAndMaxHeight();
        transform.tag = "Terrain";
    }

    public void AddObjectsPN(float limiterValue, float perlinNoiseMultiplier, GameObject objectToSpawn, bool rotate,
        int numberOfElements, bool avoidDoubleCollision, float[] rangeScale, bool rdmRotate)
    {
        GameObject parent = new GameObject();
        parent.name = objectToSpawn.name;
        parent.transform.parent = transform;
        int breaker = 0;

        List<GameObject> objects = new List<GameObject>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (breaker > numberOfElements)
                break;

            float result = Mathf.PerlinNoise(_vertices[i].x * perlinNoiseMultiplier,
                _vertices[i].z * perlinNoiseMultiplier);
            if (result > limiterValue)
            {
                float[] randoms = GetRandomBetweenSquareSize();
                Vector3 origin = new Vector3(_vertices[i].x + randoms[0], heightToSpawn, _vertices[i].z + randoms[1]);
                GameObject o = PositionRayCast(origin, objectToSpawn, rotate, parent, avoidDoubleCollision, rangeScale,
                    rdmRotate);
                breaker += 1;
                if (o != null)
                    objects.Add(o);
            }
        }

        if (avoidDoubleCollision)
            RemoveColliding(objects);
    }

    public void AddObjectsPN(float limiterValue, float perlinNoiseMultiplier, List<GameObject> objects, bool rotate,
        int numberOfElements, bool avoidDoubleCollision, float[] rangeScale, bool rdmRotate)
    {
        GameObject parent = new GameObject();
        parent.name = "EmptyObject";
        parent.transform.parent = transform;
        int breaker = 0;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (breaker > numberOfElements)
                return;

            float result = Mathf.PerlinNoise(_vertices[i].x * perlinNoiseMultiplier,
                _vertices[i].z * perlinNoiseMultiplier);
            if (result > limiterValue)
            {
                float[] randoms = GetRandomBetweenSquareSize();
                Vector3 origin = new Vector3(_vertices[i].x + randoms[0], heightToSpawn, _vertices[i].z + randoms[1]);

                int randomRange = Random.Range(0, objects.Count);
                PositionRayCast(origin, objects[randomRange], rotate, parent, avoidDoubleCollision, rangeScale,
                    rdmRotate);
                breaker += 1;
            }
        }
    }


    public void AddObjectsFromEmpty(int modeAddObjects, GameObject empty, int nElements, bool rotate, bool avoidDC,
        float limiterValue, float PNmultiplier, float[] rangeScale, bool rdmRotate, float[] heights)
    {
        List<GameObject> objects = new List<GameObject>();
        int nChild = empty.transform.childCount;
        for (int i = 0; i < nChild; i++)
        {
            objects.Add(empty.transform.GetChild(i).gameObject);
        }

        if (modeAddObjects == 0)
            MultipleRaycast(nElements, objects, rotate, avoidDC, rangeScale, rdmRotate, heights);
        if (modeAddObjects == 1)
            AddObjectsPN(limiterValue, PNmultiplier, objects, rotate, nElements, avoidDC, rangeScale, rdmRotate);
    }


    private float[] GetRandomBetweenSquareSize()
    {
        float[] squareSizes = GetSizeSquare();
        float randX = Random.Range(0, squareSizes[0]);
        float randZ = Random.Range(0, squareSizes[1]);
        return new[] {randX, randZ};
    }

    private float[] GetSizeSquare()
    {
        float xSizeSquare = Mathf.Abs(_vertices[0].x - _vertices[1].x);
        float zSizeSquare = Mathf.Abs(_vertices[1].z - _vertices[1].z);
        return new[] {xSizeSquare, zSizeSquare};
    }

    private void GetMinAndMaxHeight()
    {
        maxHeight = -Mathf.Infinity;
        minHeight = Mathf.Infinity;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (maxHeight < _vertices[i].y)
                maxHeight = _vertices[i].y;
            if (minHeight > _vertices[i].y)
                minHeight = _vertices[i].y;
        }

        heightToSpawn = Mathf.Abs(maxHeight) + Mathf.Abs(minHeight);
    }

    private Vector3[] GetCornersVectorAndMaxHeight()
    {
        Vector3 topRight = _vertices[0];
        Vector3 botLeft = _vertices[0];
        Vector3 topLeft = _vertices[0];
        Vector3 botRight = _vertices[0];
        for (int i = 1; i < _vertices.Length; i++)
        {
            if (_vertices[i].x >= topRight.x && _vertices[i].z >= topRight.z)
                topRight = _vertices[i];
            if (_vertices[i].x <= botLeft.x && _vertices[i].z <= botLeft.z)
                botLeft = _vertices[i];
            if (_vertices[i].x >= topLeft.x && _vertices[i].z <= topLeft.z)
                topLeft = _vertices[i];
            if (_vertices[i].x <= botRight.x && _vertices[i].z >= botRight.z)
                botRight = _vertices[i];
        }
        
        var position = transform.position;
        Vector3[] r = { topRight,  topLeft, botRight,  botLeft};
        
        for (var index = 0; index < r.Length; index++)
        {
            r[index] = transform.TransformPoint(r[index]);
        }
        
        return r;
    }
    
}