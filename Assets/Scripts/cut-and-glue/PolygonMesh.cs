using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PolygonMesh : MonoBehaviour
{
    public static List<Vector3> RegularPolygon(int numOfVertices, float radius)
    {
        List<Vector3> vertices = new List<Vector3>();
        float angle = 0;
        for (int i = 0; i < numOfVertices; i++)
        {
            vertices.Add(new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)));
            angle += 2 * Mathf.PI / numOfVertices;
        }
        return vertices;
    }

    private MeshFilter meshFilter;
    private int numOfVertices = -1;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void GenerateMesh(List<Vector3> vertices)
    {
        if (vertices.Count < 3)
            return;

        numOfVertices = vertices.Count;

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

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

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public void UpdateMesh(List<Vector3> vertices)
    {
        if (vertices.Count != numOfVertices)
            return;

        meshFilter.mesh.vertices = vertices.ToArray();
    }

    public void UpdateVertices(List<Vector3> vertexPositions)
    {
        Mesh mesh = meshFilter.mesh;
        if (vertexPositions == null || vertexPositions.Count != mesh.vertices.Length)
            throw new Exception("Must have the same number of vertices as the existing mesh");

        mesh.vertices = vertexPositions.ToArray();
        mesh.RecalculateBounds();
    }
}
