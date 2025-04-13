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
    private bool isRefilling;
    public bool IsRefilling => isRefilling; // Make it readable outside
    private bool isShuffling;
    private float cellSize = 64f;


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

    public void ShuffleGrid()
    {
        if (isShuffling || isRefilling) return; // avoid overlaps
        isShuffling = true;

        int animationsLeft = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (dots[x, y] != null)
                {
                    ReturnDotToPool(dots[x, y].gameObject);
                    dots[x, y] = null;

                    //Bugged code
                    // animationsLeft++;

                    // int tempX = x;
                    // int tempY = y;
                    // Dot dot = dots[tempX, tempY];
                    // RectTransform rt = dot.GetComponent<RectTransform>();

                    // rt.DOScale(Vector3.zero, 0.1f)
                    //     .SetEase(Ease.InBack)
                    //     .OnComplete(() =>
                    //     {
                    //         ReturnDotToPool(dot.gameObject);
                    //         dots[tempX, tempY] = null;

                    //         animationsLeft--;
                    //         if (animationsLeft == 0)
                    //         {
                    //             isClearing = false;
                    //             RefillGrid();
                    //         }
                    //     });
                }
            }
        }

        if (animationsLeft == 0)
        {
            isShuffling = false;
            RefillGrid(); // grid already empty, just in case
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
        dot.isBomb = true; 

        // Assign to grid
        dots[endX, endY] = dot;

        // Visuals
        rt.anchoredPosition = GetPositionFromGrid(endX, endY);
        bombObj.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8, 1);

        // Activate bomb behavior if any
        bomb.column = endX;
        bomb.row = endY;
        bomb.SetBomb();

        Debug.Log($"[SpawnBomb] Assigned bomb at ({endX},{endY}) | isBomb: {dot.isBomb} | InGrid: {dots[endX, endY] == dot}");
    }


    public void RefillGrid()
    {
        if (isRefilling) return;
        isRefilling = true;
     
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
            for (int x = 0; x < width; x++)
            {
                float columnDelay = x * fallDelay;

                for (int y = 0; y < height; y++)
                {
                    if (dots[x, y] == null)
                    {
                        SpawnNewDot(x, y, columnDelay, moveDuration);
                    }
                }
            }
        });

        float totalDelay = (width - 1) * fallDelay + moveDuration + 0.1f;
        DOVirtual.DelayedCall(totalDelay, () => { isRefilling = false; });
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

    public List<Dot> FindHint()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && dots[x, y] != null)
                {
                    List<Dot> cluster = new List<Dot>();
                    DotColor targetColor = dots[x, y].dotColor;
                    FloodFill(x, y, targetColor, visited, cluster);

                    if (cluster.Count >= 3)
                        return cluster;
                }
            }
        }
        Debug.Log("No possible match found!");
        return null;
    }

    void FloodFill(int startX, int startY, DotColor color, bool[,] visited, List<Dot> result)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y])
                continue;

            Dot dot = dots[x, y];
            if (dot == null || dot.dotColor != color)
                continue;

            visited[x, y] = true;
            result.Add(dot);

            // Check all 4 adjacent tiles
            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public Dot[,] GetDots()
    {
        return dots;
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

