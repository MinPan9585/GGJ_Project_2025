using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    public GameObject bubblePrefab; // 泡泡预制体
    public GameObject soapPrefab;   // 肥皂预制体
    public int gridWidth = 10;      // 场景宽度
    public int gridHeight = 8;     // 场景高度
    public int soapCount = 5;       // 肥皂数量

    private GameObject[,] bubbles;  // 泡泡网格

    void Start()
    {
        InitializeBubbles();
        //SpawnSoaps();
    }

    void InitializeBubbles()
    {
        bubbles = new GameObject[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject bubble = Instantiate(bubblePrefab, new Vector3(x, 0, y), Quaternion.identity);
                bubbles[x, y] = bubble;
            }
        }
    }

    void SpawnSoaps()
    {
        for (int i = 0; i < soapCount; i++)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            Instantiate(soapPrefab, new Vector3(x, 0.1f, y), Quaternion.identity); // 生成肥皂
        }
    }
}

