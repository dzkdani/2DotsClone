using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; 

public enum DotColor { Red, Green, Blue, Yellow, Pink }

public class Dot : MonoBehaviour, IPointerClickHandler
{
    public int row, column;
    public DotColor dotColor;
    private Image image;

    // List of colors you want to cycle through
    private readonly Color[] colors = new Color[]
    {
        Color.red, Color.blue, Color.green,  Color.yellow, Color.magenta
    };

    void Awake()
    {
        image = GetComponent<Image>();
    }

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

        // Get the new color to cycle to
        Color targetColor = colors[nextIndex];

        // Update the enum immediately after animation starts, so it reflects the next color
        dotColor = (DotColor)nextIndex;

        // Animate the color transition
        image.DOColor(targetColor, 0.3f).OnKill(() => SetColor(dotColor, colors[nextIndex]));  // Animation duration is 0.3s
        image.transform.DOScale(1.2f, 0.1f).OnKill(() => image.transform.DOScale(1f, 0.1f));
        image.DOFade(0, 0.2f).OnKill(() => image.DOFade(1, 0.2f).OnKill(() => image.DOColor(targetColor, 0.3f)));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click to cycle through colors
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CycleColor();
        }
    }
}

