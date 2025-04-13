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
        if (connectedDots.Count > 0 && Input.GetMouseButton(0))
        {
            UpdateMousePosition();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetLine();
        }
    }

    void UpdateMousePosition()
    {
        if (lineRenderer.positionCount < connectedDots.Count + 1)
            return;
        DrawLine();
    }

    public void AddDot(Dot dot)
    {
        if (!connectedDots.Contains(dot))
        {
            connectedDots.Add(dot);

            Vector3 worldPos = dot.transform.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Camera.main.WorldToScreenPoint(worldPos), Camera.main, out Vector2 localPos);

            lineRenderer.positionCount = connectedDots.Count + 1;
            lineRenderer.SetPosition(connectedDots.Count - 1, localPos);
            lineRenderer.SetPosition(connectedDots.Count, localPos); // default mouse pos
        }
    }

    void DrawLine()
    {
        Vector2 mouseLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, Camera.main, out mouseLocalPos);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mouseLocalPos);
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

    public void RemoveLastPoint()
    {
        if (connectedDots.Count > 0)
        {
            connectedDots.RemoveAt(connectedDots.Count - 1);
            lineRenderer.positionCount = Mathf.Max(0, lineRenderer.positionCount - 1);
        }
    }

}
