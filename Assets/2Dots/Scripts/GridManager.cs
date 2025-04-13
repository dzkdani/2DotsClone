using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    [Header("Grid Properties")]
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    public GameObject dotPrefab;
    public GameObject bombPrefab;
    public Transform gridPanel;
    public List<Color> colors;
    [SerializeField] private int poolSize = 100;
    private Queue<GameObject> dotPool = new Queue<GameObject>();
    private Dot[,] dots;

    void Start()
    {
        dots = new Dot[width, height];
        InitPool();
        InitGrid();
    }

    void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject dotObj = Instantiate(dotPrefab, gridPanel);
            dotObj.SetActive(false);
            dotPool.Enqueue(dotObj);
        }
    }

    GameObject GetDotFromPool()
    {
        if (dotPool.Count > 0)
        {
            GameObject obj = dotPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Expand pool if needed
            GameObject newObj = Instantiate(dotPrefab, gridPanel);
            return newObj;
        }
    }

    void ReturnDotToPool(GameObject dotObj)
    {
        dotObj.SetActive(false);
        dotPool.Enqueue(dotObj);
    }

    void InitGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject dotObj = GetDotFromPool();
                dotObj.transform.SetParent(gridPanel, false);

                Dot dot = dotObj.GetComponent<Dot>();

                int colorIndex = Random.Range(0, colors.Count);
                dot.SetColor((DotColor)colorIndex, colors[colorIndex]);
                dot.column = x;
                dot.row = y;

                RectTransform rt = dot.GetComponent<RectTransform>();
                rt.anchoredPosition = GetPositionFromGrid(x, y);

                dots[x, y] = dot;
            }
        }
    }

    public void SpawnBomb(int endX, int endY)
    {
        // Instantiate bomb object
        GameObject bombObj = Instantiate(bombPrefab, gridPanel);
        
        // Get references once
        Dot dot = bombObj.GetComponent<Dot>();
        Bomb bomb = bombObj.GetComponent<Bomb>();
        RectTransform rt = bombObj.GetComponent<RectTransform>();

        // Setup the dot properties
        dot.column = endX;
        dot.row = endY;
        dot.isBomb = true; // ✅ Important to set BEFORE assigning to dots[,]

        // Assign to grid
        dots[endX, endY] = dot;

        // Visuals
        rt.anchoredPosition = GetPositionFromGrid(endX, endY);
        bombObj.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8, 1);

        // Activate bomb behavior if any
        bomb.column = endX;
        bomb.row = endY;
        bomb.ActivateBomb();

        // Debug
        Debug.Log($"[SpawnBomb] Assigned bomb at ({endX},{endY}) | isBomb: {dot.isBomb} | InGrid: {dots[endX, endY] == dot}");

    }


    public void RefillGrid()
    {
        Debug.Log($"[RefillGrid Start] dots[0,0]: {(dots[0,0] == null ? "null" : (dots[0,0].isBomb ? "bomb" : "dot"))}");

        float moveDuration = 0.25f;
        float fallDelay = 0.05f;
        
        // Fall down existing dots
        for (int x = 0; x < width; x++)
        {
            float columnDelay = x * fallDelay;

            for (int y = height - 1; y >= 0; y--)
            {
                if (dots[x, y] == null)
                {
                    for (int ny = y - 1; ny >= 0; ny--)
                    {
                        if (dots[x, ny] != null)
                        {
                            Dot fallingDot = dots[x, ny];
                            dots[x, y] = fallingDot;
                            dots[x, ny] = null;
                            fallingDot.row = y;

                            RectTransform rt = fallingDot.GetComponent<RectTransform>();
                            Vector2 targetPos = GetPositionFromGrid(x, y);

                            rt.DOAnchorPos(targetPos, moveDuration)
                                .SetEase(Ease.InOutSine)
                                .SetDelay(columnDelay);
                            break;
                        }
                    }
                }
            }
        }

        // Spawn and fall new ones
        float refillStartDelay = (width - 1) * fallDelay + moveDuration;
        DOVirtual.DelayedCall(refillStartDelay, () =>
        {
            Debug.Log($"Before refill, dots[0,0]: {(dots[0,0] == null ? "null" : dots[0,0].isBomb ? "bomb" : "dot")}");

            for (int x = 0; x < width; x++)
            {
                float columnDelay = x * fallDelay;

                for (int y = 0; y < height; y++)
                {
                    if (dots[x, y] == null)
                    {
                        // Normal spawn
                        SpawnNewDot(x, y, columnDelay, moveDuration);
                    }
                    else if (dots[x, y].isBomb)
                    {
                        // Skip bombs completely
                        Debug.LogWarning($"Skipping spawn at ({x},{y}) because it's a bomb.");
                        continue;
                    }
                }
            }
        });
    }

    void SpawnNewDot(int x, int y, float delay, float duration)
    {
        GameObject dotObj = GetDotFromPool();
        dotObj.transform.SetParent(gridPanel, false);
        Dot dot = dotObj.GetComponent<Dot>();

        int colorIndex = Random.Range(0, colors.Count);
        dot.SetColor((DotColor)colorIndex, colors[colorIndex]);
        dot.column = x;
        dot.row = y;

        RectTransform rt = dot.GetComponent<RectTransform>();
        Vector2 spawnPos = GetPositionFromGrid(x, y - height);
        Vector2 targetPos = GetPositionFromGrid(x, y);

        rt.anchoredPosition = spawnPos;
        rt.DOAnchorPos(targetPos, duration)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);

        dots[x, y] = dot; 
    }

    Vector2 GetPositionFromGrid(int x, int y)
    {
        float cellSize = 64f;
        return new Vector2(x * cellSize, -y * cellSize);
    }

    public void ClearDotAt(int x, int y, bool spawnBomb = false)
    {
        float clearDuration = 0.25f;

        if (dots[x, y] != null)
        {
            Dot dot = dots[x, y];
            RectTransform rt = dot.GetComponent<RectTransform>(); 

            rt.DOScale(Vector3.zero, clearDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (dot.isBomb)
                    {
                        Debug.LogWarning($"[ClearDotAt] Tried to clear a bomb at ({x},{y}) — skipping!");
                        return;
                    }
                    ReturnDotToPool(dot.gameObject);
                    dots[x, y] = null;
                });
        }

        if (spawnBomb)
        {
            DOVirtual.DelayedCall(0.05f, () => SpawnBomb(x, y));
        }
        DOVirtual.DelayedCall(clearDuration, () => RefillGrid());  
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public Dot GetDotAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return dots[x, y];
        }
        return null;
    }
}

