using UnityEngine; 
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; 

public enum DotColor { Red, Blue, Green, Yellow, Magenta }

public class Dot : MonoBehaviour, IPointerClickHandler
{
    [Header("Dot Properties")]
    public int column;
    public int row;
    public DotColor dotColor;
    public bool isBomb;

    private Image image;
    private readonly Color[] colors = new Color[]
    {
        Color.red, Color.blue, Color.green,  Color.yellow, Color.magenta
    };

    void Awake()
    {
        image = GetComponent<Image>();
        isBomb = false;
    }

    public Vector2Int GridPos => new Vector2Int(column, row);

    public void SetColor(DotColor color, Color actualColor)
    {
        dotColor = color;
        image.color = actualColor;
    }

    public Color GetColor()
    {
        return image.color;
    }

    void CycleColor()
    {
        int currentIndex = (int)dotColor;
        int nextIndex = (currentIndex + 1) % colors.Length;
        Color targetColor = colors[nextIndex];
        dotColor = (DotColor)nextIndex;

        image.transform.DOScale(1.2f, 0.1f).OnKill(() => image.transform.DOScale(1f, 0.1f));
        image.DOFade(0, 0.2f).OnKill(() => image.DOFade(1, 0.2f).OnKill(() => image.DOColor(targetColor, 0.3f)));
        image.DOColor(targetColor, 0.3f).OnKill(() => SetColor(dotColor, colors[nextIndex])); 
    }

    public void Highlight()
    {
        GetComponent<RectTransform>().DOPunchScale(Vector3.one * 0.5f, 0.5f, 5, 1f);
        if (image != null)
        {
            Color originalColor = image.color;
            Color flashColor = Color.yellow;

            Sequence flashSeq = DOTween.Sequence();
            flashSeq.Append(image.DOColor(flashColor, 0.1f));
            flashSeq.Append(image.DOColor(originalColor, 0.1f));
            flashSeq.SetLoops(3, LoopType.Yoyo); 
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CycleColor();
        }
    }
}

