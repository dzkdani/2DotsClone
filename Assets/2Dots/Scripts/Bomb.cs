using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Linq;

public class Bomb : MonoBehaviour, IPointerClickHandler
{
    public int column, row;
    private bool isActive;
    private bool isColored;
    private GridManager gridManager;
    private Image image;
    Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };

    void Awake()
    {
        isActive = false;
        gridManager = FindObjectOfType<GridManager>();
        image = GetComponent<Image>();
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
        if (colored)
        {
            //flash bomb to every color
            if (image != null)
            {   
                Sequence colorSequence = DOTween.Sequence();
                for (int i = 0; i < colors.Length; i++)
                {
                    colorSequence.Append(image.DOColor(colors[i], 0.2f));
                }
                colorSequence.SetLoops(-1, LoopType.Yoyo);
            } 
        }
    }

    public void OnBombClick()
    {
        if (!isActive) return;
        transform.DOKill();
        DestroyArea();
    }

    public void OnColoredBombConnected(DotColor color)
    {
        //colored bomb connected to a colored dot
        //check the color of the connected dot
        //destroy every dot of that color
        DestroyDotWithColor(color);
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
        Destroy(gameObject, 0.5f);
    }

    public void DestroyDotWithColor(DotColor color)
    {
        //destroy every dot of that color
        for (int x = 0; x < gridManager.GetWidth(); x++)
        {
            for (int y = 0; y < gridManager.GetHeight(); y++)
            {
                Dot dot = gridManager.GetDotAt(x, y);
                if (dot != null && dot.dotColor == color)
                {
                    gridManager.ClearDotAt(x, y);
                }
            }
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public bool IsColored()
    {
        return isColored;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isActive && !isColored)
            {
                OnBombClick();
            }
        }
    }
}
