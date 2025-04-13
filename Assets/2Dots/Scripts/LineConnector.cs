using System.Collections.Generic;
using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public RectTransform canvasRect;
    private LineRenderer lineRenderer;
    private List<Dot> connectedDots = new List<Dot>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (connectedDots.Count == 0) return;
        if (Input.GetMouseButton(0)) DrawLine();
        else if (Input.GetMouseButtonUp(0)) ResetLine();
    }

    public void AddDot(Dot dot)
    {
        if (!connectedDots.Contains(dot))
        {
            connectedDots.Add(dot);
        }
    }

    void DrawLine()
    {
        Vector3[] points = new Vector3[connectedDots.Count + 1];

        for (int i = 0; i < connectedDots.Count; i++)
        {
            Vector3 worldPos = connectedDots[i].transform.position;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Camera.main.WorldToScreenPoint(worldPos), Camera.main, out localPos);
            points[i] = localPos;
        }

        Vector2 mouseLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, Camera.main, out mouseLocalPos);
        points[points.Length - 1] = mouseLocalPos;

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public void ResetLine()
    {
        connectedDots.Clear();
        lineRenderer.positionCount = 0;
    }

    public void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (lineRenderer.material != null)
        {
            lineRenderer.material.color = color;
        }
    }

}
