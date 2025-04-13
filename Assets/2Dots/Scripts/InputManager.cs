using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class InputManager : MonoBehaviour
{
    private LineConnector lineConnector;
    private GridManager gridManager;
    private List<Dot> connectedDots = new List<Dot>();
    private DotColor currentColor;
    private bool isDragging = false;

    void Start()
    {
        gridManager = GetComponent<GridManager>();
        lineConnector = FindObjectOfType<LineConnector>();
        if (lineConnector == null)
        {
            Debug.LogError("LineConnector not found in the scene.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartConnection();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueConnection();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndConnection();
        }
    }

    void StartConnection()
    {
        Dot dot = GetDotUnderMouse();
        if (dot != null)
        {
            lineConnector.SetLineColor(dot.GetColor());
            connectedDots.Clear();
            connectedDots.Add(dot);
            currentColor = dot.dotColor;
            isDragging = true;
            lineConnector.ResetLine();
            lineConnector.AddDot(dot);
        }
    }

    void ContinueConnection()
    {
        Dot dot = GetDotUnderMouse();
        if (dot != null && !connectedDots.Contains(dot))
        {
            Dot lastDot = connectedDots[connectedDots.Count - 1];
            if (dot.dotColor == currentColor && AreAdjacent(dot, lastDot))
            {
                connectedDots.Add(dot);
                lineConnector.AddDot(dot);
            }
        }
    }

    void EndConnection()
    {
        bool isBomb = false;
        isBomb = connectedDots.Count >= 6;
        if (connectedDots.Count >= 3)
        {
            for (int i = 0; i < connectedDots.Count; i++)
            {
                Dot dot = connectedDots[i];
                lineConnector.ResetLine();
                if (isBomb)
                {
                    gridManager.ClearDotAt(dot.column, dot.row, i == connectedDots.Count - 1);
                }
                else
                {
                    gridManager.ClearDotAt(dot.column, dot.row);
                }
            }

        }

        connectedDots.Clear();
        isDragging = false;
    }

    Dot GetDotUnderMouse()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);

        foreach (var hit in hits)
        {
            Dot dot = hit.gameObject.GetComponent<Dot>();
            if (dot == null)  Debug.Log("dot obj not found");
            else return dot;
        }

        return null;
    }

    bool AreAdjacent(Dot a, Dot b)
    {
        int dx = Mathf.Abs(a.column - b.column);
        int dy = Mathf.Abs(a.row - b.row);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
}
