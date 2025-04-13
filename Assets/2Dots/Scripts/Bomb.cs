using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Bomb : MonoBehaviour, IPointerClickHandler
{
    public int column, row;
    private Image image;
    private bool isActive;

    // Add any visual or audio effects for when the bomb is clicked
    // public GameObject explosionEffect;  // Prefab for the explosion effect

    void Awake()
    {
        image = GetComponent<Image>();
        isActive = false;  // Start with inactive state
    }

    // This function will be called to activate the bomb after clearing the 6 dots
    public void ActivateBomb()
    {
        isActive = true;
        image.color = Color.red; // Change color to indicate bomb
        // Optional: Add a bomb animation (like a pulse)
        transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    // Trigger explosion when clicked
    public void OnBombClick()
    {
        if (!isActive) return;

        // Stop pulse effect
        transform.DOKill();

        // Show explosion effect (optional, could be a particle system)
        // Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Destroy surrounding 3x3 area
        DestroyArea();
    }

    // Destroy all tiles in the 3x3 area around the bomb
    void DestroyArea()
    {
        // For simplicity, let's assume `GridManager` is in the same context
        GridManager gridManager = FindObjectOfType<GridManager>();

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
        Destroy(gameObject); // Destroy the bomb itself
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
