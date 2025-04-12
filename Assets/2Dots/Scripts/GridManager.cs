using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotParent;
    [SerializeField] private List<Color> colors;

    private Dot[,] dots;
    private Queue<GameObject> dotPool = new Queue<GameObject>();
    private int poolSize = 100; // Adjust based on expected usage
    private bool movedDot = false;

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
            GameObject dotObj = Instantiate(dotPrefab, dotParent);
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
            // Optional: Expand pool if needed
            GameObject newObj = Instantiate(dotPrefab, dotParent);
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
                dotObj.transform.SetParent(dotParent, false);

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

    public void RefillGrid(float moveDuration = 0.25f, float waveDelay = 0.05f)
    {
        // Fall down existing dots (with wave delay)
        for (int x = 0; x < width; x++)
        {
            float columnDelay = x * waveDelay;

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

                            movedDot = true;
                            break;
                        }
                    }
                }
            }
        }

        // Delay for fall animation before spawning new ones
        float refillStartDelay = (width - 1) * waveDelay + moveDuration;
        DOVirtual.DelayedCall(refillStartDelay, () =>
        {
            for (int x = 0; x < width; x++)
            {
                float columnDelay = x * waveDelay;

                for (int y = 0; y < height; y++)
                {
                    if (dots[x, y] == null)
                    {
                        GameObject dotObj = GetDotFromPool();
                        dotObj.transform.SetParent(dotParent, false);
                        Dot dot = dotObj.GetComponent<Dot>();

                        int colorIndex = Random.Range(0, colors.Count);
                        dot.SetColor((DotColor)colorIndex, colors[colorIndex]);
                        dot.column = x;
                        dot.row = y;

                        RectTransform rt = dot.GetComponent<RectTransform>();
                        Vector2 spawnPos = GetPositionFromGrid(x, y - height);
                        Vector2 targetPos = GetPositionFromGrid(x, y);

                        rt.anchoredPosition = spawnPos;

                        rt.DOAnchorPos(targetPos, moveDuration)
                            .SetEase(Ease.OutBack)
                            .SetDelay(columnDelay);

                        dots[x, y] = dot;
                    }
                }
            }
        });
    }


    IEnumerator MoveDot(Dot dot, Vector2 targetPosition, float duration)
    {
        RectTransform rt = dot.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = targetPosition;
    }


    Vector2 GetPositionFromGrid(int x, int y)
    {
        float cellSize = 64f;
        return new Vector2(x * cellSize, -y * cellSize);
    }

    public void ClearDotAt(int x, int y, float clearDuration = 0.2f)
    {
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

        DOVirtual.DelayedCall(0.25f, () => RefillGrid());
    }
}

