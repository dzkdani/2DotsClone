using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // public Camera uiCamera;
    // public LayerMask dotLayer;

    public LineConnector lineConnector;
    private GridManager gridManager;
    private List<Dot> connectedDots = new List<Dot>();
    private DotColor currentColor;
    private bool isDragging = false;

    void Start()
    {
        gridManager = GetComponent<GridManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartConnection();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            TryContinueConnection();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndConnection();
        }
    }

    void TryStartConnection()
    {
        Dot dot = GetDotUnderMouse();
        if (dot != null)
        {
            connectedDots.Clear();
            connectedDots.Add(dot);
            currentColor = dot.dotColor;
            isDragging = true;
            lineConnector.ResetLine();
            lineConnector.AddDot(dot);
        }
    }

    void TryContinueConnection()
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
        if (connectedDots.Count >= 3)
        {
            foreach (var dot in connectedDots)
            {
                lineConnector.ResetLine();
                gridManager.ClearDotAt(dot.column, dot.row);
                // Optional: Play scale down animation
            }
        }

        connectedDots.Clear();
        isDragging = false;

        //end connection logic, e.g. check for game over, update score, or refill the grid
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
            lineConnector.SetLineColor(dot.GetColor());
            if (dot != null) return dot;
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
