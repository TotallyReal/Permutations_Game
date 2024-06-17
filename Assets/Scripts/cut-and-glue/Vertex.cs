using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{

    public struct VertexId
    {
        public Polygon polygon;
        public int vertexID;
        public Vector3 position;
    }

    public VertexId VertexInfo { get; set; }

}
