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
        if (gridManager.IsRefilling) return; // Block input during refill

        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<Dot> hint = gridManager.FindHint();
            if (hint != null)
            {
                foreach (var dot in hint)
                {
                    dot.Highlight(); 
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            gridManager.ShuffleGrid();
        }

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
        if (dot != null)
        {
            Dot lastDot = connectedDots[connectedDots.Count - 1];

            // Allow backtracking
            if (connectedDots.Count >= 2 && dot == connectedDots[connectedDots.Count - 2])
            {
                connectedDots.RemoveAt(connectedDots.Count - 1);
                lineConnector.RemoveLastPoint(); // Implement this in LineConnector
                return;
            }

            if (!connectedDots.Contains(dot) &&
                dot.dotColor == currentColor &&
                AreAdjacent(dot, lastDot))
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
            lineConnector.ResetLine();
            for (int i = 0; i < connectedDots.Count; i++)
            {
                Dot dot = connectedDots[i];
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
        return (a.column == b.column && Mathf.Abs(a.row - b.row) == 1) ||
            (a.row == b.row && Mathf.Abs(a.column - b.column) == 1);
    }

}
