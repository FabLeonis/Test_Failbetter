using UnityEngine;
using System.Collections.Generic;       // for List


public class Plotter : MonoBehaviour
{
    public GameObject PointPrefab;
    public GameObject BufferPrefab;
    public PolygonCollider2D Collider;
    public float BufferDistance = 40;

    // point trimming (basic threshold system)
    public float PointMinAngle = 0f;        // angle threshold for point generation
    public float PointMinDistance = 0f;     // distance threshold for point generation

    // keep a list of instantiated points and buffer points
    private List<GameObject> Points = new List<GameObject>();
    private List<GameObject> Buffer = new List<GameObject>();

    // constants
    private const float OverlapMercy = .9f;


    // simple clear implementation, no memorization over scene/project reload
    public void Clear()
    {
        Debug.Log("Clear");

        // clear points
        foreach (GameObject obj in Points)
            DestroyImmediate(obj);

        Points.Clear();

        // clear buffer
        foreach (GameObject obj in Buffer)
            DestroyImmediate(obj);

        Buffer.Clear();
    }


    public void PlotPoints()
    {
        // clear everything (this implementation uses points as base for the buffer)
        Clear();

        Debug.Log("PlotPoints");

        Vector2 lastpoint = Vector2.zero;
        int idx = 0;

        // use collider geometry as base
        foreach (Vector2 point in Collider.points)
        {
            // plot 1st point, then plot only significant enough points (angle & distance > threshold). Simple trimming technics as points optimization not a requirement here
            if (idx == 0 || (idx < Collider.points.Length - 1 && Mathf.Abs(Vector2.Angle((Collider.points[idx + 1] - Collider.points[idx]), (Collider.points[idx] - lastpoint))) >= PointMinAngle && (Collider.points[idx] - lastpoint).magnitude >= PointMinDistance))
            {
                GameObject obj = GameObject.Instantiate(PointPrefab, point, Quaternion.identity, Collider.transform);
                Points.Add(obj);
                lastpoint = point;
            }

            idx++;
        }

    }


    public void PlotBuffer()
    {
        // clear only buffer elements.
        foreach (GameObject obj in Buffer)
            DestroyImmediate(obj);
        Buffer.Clear();

        Debug.Log("PlotBuffer");

        // base on the plotted points
        for (int i = 0; i < Points.Count; i++)
        {
            // compute tangent vector to the 2 direct neighbors
            int prev = (i == 0 ? Points.Count - 1 : i - 1);
            int next = (i == Points.Count - 1 ? 0 : i + 1);
            Vector2 tandir = -Vector2.Perpendicular(Points[next].transform.position - Points[prev].transform.position).normalized;
            // pick a position at BufferDistance in that direction
            Vector2 position = (Vector2)(Points[i].transform.position) + tandir * BufferDistance;

            // don't instantiate anything that would be within the border
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, BufferDistance * OverlapMercy);      // in the glorious days before Collider2D.ClosestPoint existed...
            bool overlap = false;
            foreach (Collider2D collider in colliders)
            {
                if (collider == Collider)
                {
                    overlap = true;
                    break;
                }
            }
            if (overlap)
                continue;

            // instantiate buffer point
            GameObject obj = GameObject.Instantiate(BufferPrefab, position, Quaternion.identity, Collider.transform);
            Buffer.Add(obj);
        }

    }
}