using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectedEdge : MonoBehaviour
{
    public static Color[] edgeColors = new Color[] {
        Color.blue, Color.red, Color.green, Color.yellow,
        Color.magenta, Color.black, Color.white
    }; // TODO: inside colors should be final.

    [SerializeField] private Color color;
    [SerializeField] private SpriteRenderer line;
    [SerializeField] private SpriteRenderer triangle;

    private DirectedEdge pairedEdge { get; set; }

    private EdgeType edgeType = new EdgeType() { edgePairType = 0, isClockwise = true };

    public Vector3 fromPosition { get; private set; } = Vector3.zero;
    public Vector3 toPosition { get; private set; } = Vector3.up;

    private void OnValidate()
    {
        SetColor(color);
        //SetEdgePoints(fromObject.position, toObject.position);
    }

    public void SetColor(Color color)
    {
        line.color = color;
        triangle.color = color;
    }

    public struct EdgeType
    {
        public EdgeType(int edgePairType, bool isClockwise)
        {
            this.edgePairType = edgePairType;
            this.isClockwise = isClockwise;
        }

        public int edgePairType;
        public bool isClockwise;
    }

    public void SetEdgeType(EdgeType type)
    {
        this.edgeType = type;
        color = DirectedEdge.edgeColors[type.edgePairType];
        SetColor(color);
        triangle.transform.localRotation = Quaternion.Euler(0, 0, type.isClockwise ? 0 : 180);
    }

    public EdgeType GetEdgeType()
    {
        return edgeType; // TODO: return a copy
    }


    public void SetVertexTo(Vector3 toV)
    {
        SetEdgePoints(fromPosition, toV);
    }

    public void SetVertexFrom(Vector3 fromV)
    {
        SetEdgePoints(fromV, toPosition);
    }

    public void SetEdgePoints(Vector3 fromV, Vector3 toV)
    {
        fromPosition = fromV;
        toPosition = toV;

        transform.position = (fromV + toV) / 2f;
        Vector2 dir = toV - fromV;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.x, -dir.y) * 180/Mathf.PI);
        line.transform.localScale = new Vector3(0.1f, dir.magnitude, 1);
    }

    public void Translate(Vector3 v)
    {
        SetEdgePoints(fromPosition + v, toPosition + v);
    }

    public void ApplyMatrix(Matrix4x4 matrix)
    {
        SetEdgePoints(matrix.MultiplyPoint3x4(fromPosition), matrix.MultiplyPoint3x4(toPosition));
    }
}
