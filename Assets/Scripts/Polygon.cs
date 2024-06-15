using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using EdgeType = DirectedEdge.EdgeType;

public class Polygon : MonoBehaviour
{
    [Header("Vertices")]
    [SerializeField] private Transform vertexContainer;
    [SerializeField] private Transform vertexPrefab;
    private Transform[] vertices;
    [Range(0, 10)]
    private int numOfVertices = 4;
    [SerializeField] private float vertexRadius;

    [Header("Edges")]
    [SerializeField] private Transform edgeContainer;
    [SerializeField] private DirectedEdge edgePrefab;
    private DirectedEdge[] edges;

    [Header("Mesh")]
    [SerializeField] private PolygonMesh polyMesh;

    private List<Vector3> vertexPositions; // TODO: Do I really need this list?

    void Awake()
    {
    }

    #region --------------------- Creation ---------------------

    public void GenerateFromEdges(List<DirectedEdge> edges)
    {
        DirectedEdge lastEdge = edges.Last();
        Vector3 lastVertexPosition = lastEdge.toPosition;
        Vector3 v = lastVertexPosition;

        List<Vector3> verPositions = new List<Vector3>();

        foreach (DirectedEdge edge in edges)
        {
            if (Vector3.Distance(edge.fromPosition,v)>0.001)
            {
                throw new Exception($"Edge {edge} did not start where the last edge finished.");
            }
            verPositions.Add(v);
            v = edge.toPosition;
        }
        vertexPositions = verPositions;
        numOfVertices = vertexPositions.Count;

        polyMesh.GenerateMesh(verPositions);
        CreateVertices(verPositions);
        SetEdges(edges);
    }

    public void GenerateFromVertices(List<Vector3> vertices)
    {
        if (vertices.Count >= 3)
        {
            numOfVertices = vertices.Count;
            this.vertexPositions = vertices; // TODO: copy each vertex and not the list
            polyMesh.GenerateMesh(vertices);
            CreateVertices(vertices);
            CreateEdgesFromVertexPositions();
        }
    }

    private void CreateVertices(IEnumerable<Vector3> vertexPositions)
    {
        foreach (Transform vertex in vertexContainer)
        {
            Destroy(vertex.gameObject);
        }

        int index = 0;
        vertices = new Transform[numOfVertices];
        foreach (Vector3 v in vertexPositions)
        {
            Transform vertex = Instantiate(vertexPrefab, vertexContainer);
            vertices[index] = vertex;
            vertex.name = $"vertex {index++}";
            vertex.localPosition = v;
            vertex.localScale = new Vector3(vertexRadius, vertexRadius, 1);
        }
    }

    public void CreateEdgesFromVertexPositions()
    {
        edges = new DirectedEdge[numOfVertices];
        for (int i = 0; i<numOfVertices-1; i++)
        {
            edges[i] = Instantiate(edgePrefab);
            edges[i].SetEdgePoints(vertexPositions[i], vertexPositions[i + 1]);
        }

        edges[numOfVertices - 1] = Instantiate(edgePrefab);
        edges[numOfVertices - 1].SetEdgePoints(vertexPositions[numOfVertices - 1], vertexPositions[0]);

        SetEdges(edges);
    }

    #endregion

    #region --------------------- Update ---------------------

    private void SetEdges(IEnumerable<DirectedEdge> edges)
    {
        foreach (Transform edge1 in edgeContainer)
        {
            Destroy(edge1.gameObject);
        }
        int index = 0;
        foreach (DirectedEdge edge2 in edges) { 
            edge2.name = $"edge {index++}";
            edge2.transform.parent = edgeContainer;
        }
        this.edges = edges.ToArray();
    }
    
    // TODO: Consider either removing this method, or moving it to another place.
    public void SetEdgeTypes(List<EdgeType> edgeTypes)
    {
        if (edgeTypes.Count != numOfVertices)
            return;

        int index = 0;
        foreach (EdgeType edgeType in edgeTypes)
        {
            edges[index++].SetEdgeType(edgeType);
        }
    }

    public void UpdateVertices(List<Vector3> positions)
    {
        if (positions.Count != numOfVertices)
            throw new Exception("Wrong number of vertex positions");

        vertexPositions = positions;
        polyMesh.GenerateMesh(vertexPositions);

        foreach (var (vertex, position) in vertices.Zip(vertexPositions, (v, p) => (v, p)))
        {
            vertex.localPosition = position; // TODO: local or global?
        }

        for (int i = 1; i < numOfVertices; i++)
        {
            edges[i-1].SetEdgePoints(positions[i - 1], positions[i]);
        }
        edges[numOfVertices - 1].SetEdgePoints(positions[numOfVertices - 1], positions[0]);
    }

    public void Regularize()
    {
        UpdateVertices(PolygonMesh.RegularPolygon(numOfVertices, 3));
    }

    #endregion

    #region --------------------- transformations ---------------------

    public void Translate(Vector3 translation)
    {
        // TODO: remember that Vector3 is a struct, so in iterations it is copied, and I cannot change the original element.
        // try to read : https://stackoverflow.com/questions/58069669/can-i-foreach-over-an-array-of-structs-without-copying-the-elements-in-c-sharp-8

        vertexPositions = (from v in vertexPositions select v + translation).ToList();

        foreach (Transform vertex in vertices)
        {
            vertex.localPosition += translation; // TODO: local or global?
        }

        polyMesh.UpdateVertices(vertexPositions);

        foreach (DirectedEdge edge in edges)
        {
            edge.Translate(translation);
        }

    }

    public void RotateAround(Vector3 center, float angle)
    {
        Matrix4x4 map = Matrix4x4.Translate(center) * Matrix4x4.Rotate(new Quaternion(0, 0, Mathf.Sin(angle/2), Mathf.Cos(angle / 2))) * Matrix4x4.Translate(-center);
        ApplyMatrix(map);
    }

    public void ReflectThrough(Vector3 point, Vector3 perp)
    {
        perp.Normalize();
        Vector3 v = Vector3.up;

        Matrix4x4 reflection = new Matrix4x4(
            new Vector4(1 - perp.x * perp.x,     perp.y * perp.x,     perp.z * perp.x, 0),
            new Vector4(    perp.x * perp.y, 1 - perp.y * perp.y,     perp.z * perp.y, 0),
            new Vector4(    perp.x * perp.z,     perp.y * perp.z, 1 - perp.z * perp.z, 0),
            new Vector4(0                  , 0                  , 0                  , 1)
            );

        Matrix4x4 map = Matrix4x4.Translate(point) * reflection * Matrix4x4.Translate(-point);
    }

    public void ApplyMatrix(Matrix4x4 matrix)
    {
        // TODO: remember that Vector3 is a struct, so in iterations it is copied, and I cannot change the original element.
        // try to read : https://stackoverflow.com/questions/58069669/can-i-foreach-over-an-array-of-structs-without-copying-the-elements-in-c-sharp-8

        vertexPositions = (from v in vertexPositions select matrix.MultiplyPoint3x4(v)).ToList();

        polyMesh.UpdateVertices(vertexPositions);

        foreach (var (vertex, position) in vertices.Zip(vertexPositions, (v,p)=>(v,p)))
        {
            vertex.localPosition = position; // TODO: local or global?
        }

        foreach (DirectedEdge edge in edges)
        {
            edge.ApplyMatrix(matrix);
        }



    }

    #endregion

    public struct ClosestVertexInfo
    {
        public Vector3 vertex;
        public int vertexId;
        public float distance;
    }

    public ClosestVertexInfo GetClosestVertex(Vector2 p)
    {

        /*float minDist = 100000f;
        Vector3 closestVertex = Vector3.zero;
        int index = 0;*/
        return vertexPositions
            .Select((v, index) => new ClosestVertexInfo(){ vertex = v, vertexId = index, distance = Vector2.Distance(v, p) })
            .Aggregate((min, current) => current.distance < min.distance ? current : min);


        /*foreach (Vector3 v in vertexPositions)
        {
            float dist = Vector2.Distance(p, v);
            if (dist < minDist)
            {
                minDist = dist;
                closestVertex = v;
            }
            index += 1;
        }
        return new Vector3(closestVertex.x, closestVertex.y, closestVertex.z);*/
    }

    public int GetNumberOfVertices()
    {
        return numOfVertices;
    }

    public Vector3[] GetVertices() // TODO: return a copy instead
    {
        return vertexPositions.ToArray();
    }

    public EdgeType[] GetEdgeTypes()
    {
        return (from edge in edges select edge.GetEdgeType()).ToArray();
    }

    public IEnumerable<DirectedEdge> GetEdges()
    {
        return edges;
    }

    /*[ContextMenu("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
    public void CreateMesh()
    {

        Debug.Log("Creating mesh ... 3");

        Mesh mesh = new Mesh();

        Vector2[] uv = new Vector2[numOfVertices];

        int[] triangles = new int[3 * numOfVertices];
        int index = 0;
        for (int i = 2; i < numOfVertices; i++)
        {
            triangles[index] = 0;
            triangles[index + 1] = i;
            triangles[index + 2] = i - 1;
            index += 3;
        }

        mesh.vertices = RegularPolygon(numOfVertices, radius).ToArray();
        mesh.uv = uv;
        mesh.triangles = triangles;

        AssetDatabase.CreateAsset(mesh, "Assets/Mesh/temp.mesh");
    }*/
}
