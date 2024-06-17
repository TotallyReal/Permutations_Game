using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using EdgeType = DirectedEdge.EdgeType;
using Unity.Mathematics;
using System.Data;

public class CuttableSurface : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private DirectedEdge edgePrefab;
    [SerializeField] private Polygon polygonPrefab;
    private int polygonCounter = 0;

    [Header("Initial Polygon")]
    //[SerializeField] private Polygon mainPolygon;
    [Range(3, 20)]
    [SerializeField] private int numOfVertices = 6;
    [Range(0, 10)]
    [SerializeField] private float radius = 3.0f;
    //private PlayerInput input;

    private Dictionary<int, GluedEdges> edgeDictionary;

    private List<Polygon> polygons;

    class GluedEdges
    {
        public DirectedEdge edge1;
        public Polygon polygon1;
        public DirectedEdge edge2;
        public Polygon polygon2;

        public void UpdateEdge(DirectedEdge edge, Polygon polygon)
        {
            if (edge == edge1)
            {
                polygon1 = polygon;
                return;
            }
            if (edge == edge2)
            {
                polygon2 = polygon;
                return;
            }
        }
    }

    private void Awake()
    {
        /*input = new PlayerInput();
        input.Player.Enable();*/

        edgeDictionary = new Dictionary<int, GluedEdges>();

        polygons = new List<Polygon> { CreateInitialPolygon() };

        // Don't put it in Enable\Disable, since the event generation is in this component also.
        OnEdgePressed += (sender, edge) => Glue(edge.GetEdgeType().edgePairType);
    }

    private Polygon CreateInitialPolygon()
    {
        Polygon polygon = Instantiate(polygonPrefab, transform);
        polygon.name = $"Polygon {polygonCounter++}";
        polygon.GenerateFromVertices(PolygonMesh.RegularPolygon(numOfVertices, radius));
        DirectedEdge[] edges = polygon.GetEdges().ToArray();
        /*int half = (int)(numOfVertices / 2);
        for (int i = 0; i < half; i++)
        {
            edges[i].SetEdgeType(new EdgeType() { edgePairType = i, isClockwise = (Random.Range(0, 2) == 0) });
            edges[i + half].SetEdgeType(new EdgeType() { edgePairType = i, isClockwise = (Random.Range(0, 2) == 0) });
            edgeDictionary[i] = new GluedEdges()
            {
                edge1 = edges[i], polygon1 = mainPolygon,
                edge2 = edges[i + half], polygon2 = mainPolygon
            };
        }*/

        edges[0].SetEdgeType(new EdgeType() { edgePairType = 0, isClockwise = true });
        edges[2].SetEdgeType(new EdgeType() { edgePairType = 0, isClockwise = false });
        edgeDictionary[0] = new GluedEdges()
        {
            edge1 = edges[0], polygon1 = polygon,
            edge2 = edges[2], polygon2 = polygon
        };

        edges[1].SetEdgeType(new EdgeType() { edgePairType = 1, isClockwise = true });
        edges[3].SetEdgeType(new EdgeType() { edgePairType = 1, isClockwise = false });
        edgeDictionary[1] = new GluedEdges()
        {
            edge1 = edges[1], polygon1 = polygon,
            edge2 = edges[3], polygon2 = polygon
        };

        edges[4].SetEdgeType(new EdgeType() { edgePairType = 2, isClockwise = true });
        edges[5].SetEdgeType(new EdgeType() { edgePairType = 2, isClockwise = false });
        edgeDictionary[2] = new GluedEdges()
        {
            edge1 = edges[4], polygon1 = polygon,
            edge2 = edges[5], polygon2 = polygon
        };

        return polygon;
    }

    #region --------------------- Enable \ Disable ---------------------

    private void OnEnable()
    {
        RaycastSelector2D.Instance.OnObjectPressed += ObjectPressed;
    }

    private void OnDisable()
    {
        RaycastSelector2D.Instance.OnObjectPressed -= ObjectPressed;
    }

    #endregion


    #region --------------------- Events ---------------------

    public event EventHandler<Vertex.VertexId> OnVertexPressed;
    public event EventHandler<DirectedEdge> OnEdgePressed;

    private void ObjectPressed(object sender, Transform objMouse)
    {
        if (objMouse.TryGetComponent<DirectedEdge>(out var edge))
        {
            // TODO: Event parameters should also contain polygon and id.
            OnEdgePressed?.Invoke(this, edge);
            return;
        }

        if (objMouse.TryGetComponent<Vertex>(out var vertex))
        {
            // TODO: Consider having a reference to the parent polygon inside the Vertex object.
            foreach (Polygon polygon in GetPolygons())
            {
                int vertexId = polygon.VertexId(vertex);
                if (vertexId >= 0)
                {
                    OnVertexPressed?.Invoke(this, new Vertex.VertexId() { polygon=polygon, vertexID = vertexId, position=vertex.transform.position});
                    return;
                }
            }            
        }
    }

    #endregion


    #region --------------------- Cut and Glue ---------------------

    public bool TryCutAt(Polygon polygon, int indexFrom, int indexTo)
    {
        if (!GetPolygons().Contains(polygon))
            return false;

        int numVertices = polygon.GetNumberOfVertices();
        if (Mathf.Abs(indexFrom - indexTo) <= 1 || Mathf.Abs(indexFrom - indexTo) == (numVertices - 1))
            return false;

        int smallerIndex = indexFrom;
        int largerIndex = indexTo;
        if (smallerIndex > largerIndex)
        {
            smallerIndex = indexTo;
            largerIndex = indexFrom;
        }

        int newPairType = 0;
        while (edgeDictionary.TryGetValue(newPairType, out GluedEdges gluedEdges))
        {
            newPairType++;
        }


        Vector3[] vertices = polygon.GetVertices();

        DirectedEdge[] mainPolEdges = polygon.GetEdges().ToArray();

        // --------- Create edges for the two new parts ---------

        List<DirectedEdge> edges1 = new List<DirectedEdge>();
        for (int i = smallerIndex; i < largerIndex; i++)
        {
            edges1.Add(mainPolEdges[i]);
        }

        List<DirectedEdge> edges2 = new List<DirectedEdge>();
        for (int i = largerIndex; i < numVertices; i++)
        {
            edges2.Add(mainPolEdges[i]);
        }
        for (int i = 0; i < smallerIndex; i++)
        {
            edges2.Add(mainPolEdges[i]);
        }

        DirectedEdge newEdge1 = Instantiate(edgePrefab);
        newEdge1.SetEdgePoints(vertices[largerIndex], vertices[smallerIndex]);
        newEdge1.SetEdgeType(new EdgeType() { edgePairType = newPairType, isClockwise = true });
        edges1.Add(newEdge1);

        DirectedEdge newEdge2 = Instantiate(edgePrefab);
        newEdge2.SetEdgePoints(vertices[smallerIndex], vertices[largerIndex]);
        newEdge2.SetEdgeType(new EdgeType() { edgePairType = newPairType, isClockwise = false });
        edges2.Add(newEdge2);

        // --------- Generate Polygons ---------

        Polygon part1 = Instantiate(polygonPrefab, transform);
        part1.name = $"Polygon {polygonCounter++}";
        Polygon part2 = Instantiate(polygonPrefab, transform);
        part2.name = $"Polygon {polygonCounter++}";
        polygons.Add(part1);
        polygons.Add(part2);

        part1.GenerateFromEdges(edges1);
        part2.GenerateFromEdges(edges2);

        // --------- Adjust their positions ---------

        Vector2 v1 = vertices[smallerIndex];
        Vector2 v2 = vertices[largerIndex];
        Vector2 dir = (v2 - v1).normalized;
        Vector3 perpDir = new Vector3(dir.y, -dir.x, 0);
        part1.Translate(perpDir);
        part2.Translate(-perpDir);

        // --------- Update Dictionary ---------

        edgeDictionary[newPairType] = new GluedEdges()
        {
            edge1 = newEdge1, polygon1 = part1,
            edge2 = newEdge2, polygon2 = part2,
        };

        UpdateDictionary(part1, edges1);
        UpdateDictionary(part2, edges2);

        // --------- Finally... ---------

        polygons.Remove(polygon);
        polygon.gameObject.SetActive(false);
        Destroy(polygon.gameObject);

        return true;
    }

    public void Glue(int edgeType)
    {
        if (!edgeDictionary.TryGetValue(edgeType, out GluedEdges gluedEdges))
        {
            throw new Exception($"Could not find edges of type {edgeType} which can be glued together");
        }
        if (gluedEdges.polygon1 == gluedEdges.polygon2)
        {
            Debug.Log($"Cannot glue two edges on the same polygon {gluedEdges.polygon1}.");
            return;
        }

        gluedEdges.polygon2.Scale(gluedEdges.edge1.Magnitude() / gluedEdges.edge2.Magnitude());
        if (gluedEdges.edge2.GetEdgeType().isClockwise == gluedEdges.edge1.GetEdgeType().isClockwise)
        {
            gluedEdges.polygon2.Translate(gluedEdges.edge1.fromPosition - gluedEdges.edge2.fromPosition);
            gluedEdges.polygon2.ReflectThrough(gluedEdges.edge1.fromPosition, Vector3.up);
            Vector3 dir1 = gluedEdges.edge1.toPosition - gluedEdges.edge1.fromPosition;
            Vector3 dir2 = gluedEdges.edge2.toPosition - gluedEdges.edge2.toPosition;
            gluedEdges.polygon2.RotateAround(gluedEdges.edge1.fromPosition, Vector3.Angle(dir2, dir1) * Mathf.PI / 180f);
        } else
        {
            // TODO: Assume for now that the edges have the same length. Change it later.

            // to and from positions, indicate the clockwise order around the polygon. So if the edges have opposite directions,
            // then we glue a "to" position to the "from" position.
            gluedEdges.polygon2.Translate(gluedEdges.edge1.fromPosition - gluedEdges.edge2.toPosition);
            Vector3 dir1 = gluedEdges.edge1.toPosition - gluedEdges.edge1.fromPosition;
            Vector3 dir2 = gluedEdges.edge2.fromPosition - gluedEdges.edge2.toPosition;
            gluedEdges.polygon2.RotateAround(gluedEdges.edge1.fromPosition, Vector3.SignedAngle(dir2, dir1, Vector3.forward) * Mathf.PI / 180f);
        }

        List<DirectedEdge> edges1 = RestOfEdges(gluedEdges.polygon1, gluedEdges.edge1);
        List<DirectedEdge> edges2 = RestOfEdges(gluedEdges.polygon2, gluedEdges.edge2);

        edges1.AddRange(edges2);

        Polygon polygon = Instantiate(polygonPrefab, transform);
        polygon.name = $"Polygon {polygonCounter++}";

        UpdateDictionary(polygon, edges1);
        edgeDictionary.Remove(edgeType);
        Debug.Log("Keys:");
        foreach (KeyValuePair<int,GluedEdges> kvp in edgeDictionary)
        {
            Debug.Log(kvp.Key);
        }


        polygon.GenerateFromEdges(edges1);

        polygon.gameObject.SetActive(true);

        gluedEdges.polygon1.gameObject.SetActive(false);
        gluedEdges.polygon2.gameObject.SetActive(false);

        //isCut = false;

        polygon.Regularize();

        polygons.Remove(gluedEdges.polygon1);
        polygons.Remove(gluedEdges.polygon2);
        polygons.Add(polygon);

        Destroy(gluedEdges.polygon1.gameObject);
        Destroy(gluedEdges.polygon2.gameObject);

    }

    private void UpdateDictionary(Polygon poly, IEnumerable<DirectedEdge> edges)
    {
        foreach (DirectedEdge edge in edges)
        {
            edgeDictionary[edge.GetEdgeType().edgePairType].UpdateEdge(edge, poly);
        }
    }

    private List<DirectedEdge> RestOfEdges(Polygon poly, DirectedEdge removeEdge)
    {

        List<DirectedEdge> edgesBefore = new List<DirectedEdge>();
        List<DirectedEdge> edgesAfter = new List<DirectedEdge>();
        List<DirectedEdge> edges = edgesBefore;
        foreach (DirectedEdge edge in poly.GetEdges())
        {
            if (edge == removeEdge)
            {
                edges = edgesAfter;
                continue;
            }
            edges.Add(edge);
        }
        edges.AddRange(edgesBefore);

        return edges;
    }

    #endregion

    public IEnumerable<Polygon> GetPolygons()
    {
        return polygons;
    }

}
