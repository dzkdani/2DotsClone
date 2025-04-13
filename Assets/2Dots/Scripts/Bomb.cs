using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class Bomb : MonoBehaviour, IPointerClickHandler
{
    public int column, row;
    private bool isActive;
    private bool isColored;
    private GridManager gridManager;

    void Awake()
    {
        isActive = false;
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.Log("GridManager not found in the scene.");
        }  
    }

    public void SetBomb(bool colored = false)
    {
        isActive = true;
        isColored = colored;
        transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnBombClick()
    {
        if (!isActive) return;
        transform.DOKill();
        DestroyArea();
    }

    public void OnColoredBombConnected()
    {
        //colored bomb connected to a colored dot
        //check the color of the connected dot
        //destroy every dot of that color
        // DestroyDotWithColor();
    }

    void DestroyArea()
    {
        for (int x = column - 1; x <= column + 1; x++)
        {
            for (int y = row - 1; y <= row + 1; y++)
            {
                if (x >= 0 && x < gridManager.GetWidth() && y >= 0 && y < gridManager.GetHeight())
                {
                    gridManager.ClearDotAt(x, y);
                }
            }
        }
        Destroy(gameObject);
    }

    public void DestroyDotWithColor(DotColor color)
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isActive)
            {
                OnBombClick();
            }
        }
    }
}
