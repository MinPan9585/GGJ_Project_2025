using UnityEngine;
using System.Collections.Generic; // 引入以使用 List

public class SceneSpawner : MonoBehaviour
{
    public List<GameObject> bubblePrefabs; // 泡泡的Prefab列表
    public GameObject bubbleParent, soapParent, mopPrefab; // 泡泡、肥皂的父物体和拖把Prefab
    public GameObject soapPrefab; // 肥皂的Prefab
    public float minX = -8f; // 平面区域的最小X值
    public float maxX = 8f; // 平面区域的最大X值
    public float minZ = -5f; // 平面区域的最小Z值
    public float maxZ = 5f; // 平面区域的最大Z值
    public float fixedY = 0f; // 泡泡、肥皂和拖把的固定Y值
    public int rows = 5; // 网格的行数
    public int columns = 5; // 网格的列数
    public float bubblePositionOffset = 0.5f; // 泡泡在网格单元内的随机偏移范围
    public float soapPositionOffset = 0.2f; // 肥皂在网格单元内的随机偏移范围
    public float minBubbleScale = 1f; // 泡泡的最小缩放比例
    public float maxBubbleScale = 1.5f; // 泡泡的最大缩放比例
    public int soapCount = 5; // 肥皂的数量

    // 拖把生成范围
    public float mopMinX = -6f; // 拖把生成区域的最小X值
    public float mopMaxX = 6f; // 拖把生成区域的最大X值
    public float mopMinZ = -3f; // 拖把生成区域的最小Z值
    public float mopMaxZ = 3f; // 拖把生成区域的最大Z值

    void Start()
    {
        // 生成泡泡和肥皂
        SpawnBubblesAndSoaps();

        // 生成拖把
        SpawnMop();
    }

    // 生成泡泡和肥皂
    public void SpawnBubblesAndSoaps()
    {
        // 计算平面区域的宽度和高度
        float planeWidth = maxX - minX;
        float planeHeight = maxZ - minZ;

        // 计算每个网格单元的大小
        float cellWidth = planeWidth / columns;
        float cellHeight = planeHeight / rows;

        // 创建一个二维数组来标记哪些网格单元已经生成了肥皂
        bool[,] soapGrid = new bool[rows, columns];

        // 随机生成肥皂的位置
        for (int i = 0; i < soapCount; i++)
        {
            int randomRow, randomColumn;

            // 确保肥皂不会生成在同一个网格单元
            do
            {
                randomRow = Random.Range(1, rows - 1);
                randomColumn = Random.Range(1, columns - 1);
            } while (soapGrid[randomRow, randomColumn]);

            // 标记该网格单元已经生成了肥皂
            soapGrid[randomRow, randomColumn] = true;

            // 计算该网格单元的中心点
            Vector3 cellCenter = new Vector3(
                minX + cellWidth * (randomColumn + 0.5f), // X方向
                fixedY, // 固定Y值
                minZ + cellHeight * (randomRow + 0.5f) // Z方向
            );

            // 在网格单元内随机生成肥皂的位置
            Vector3 randomOffset = new Vector3(
                Random.Range(-soapPositionOffset, soapPositionOffset),
                0, // Y方向保持不变
                Random.Range(-soapPositionOffset, soapPositionOffset)
            );

            Vector3 soapPosition = cellCenter + randomOffset;

            // 确保肥皂不会超出定义的平面范围
            soapPosition.x = Mathf.Clamp(soapPosition.x, minX, maxX);
            soapPosition.z = Mathf.Clamp(soapPosition.z, minZ, maxZ);

            // 实例化肥皂Prefab
            GameObject soap = Instantiate(soapPrefab, soapPosition, soapPrefab.transform.rotation);
            // 设置肥皂的父物体为当前Spawner，便于管理
            soap.transform.parent = soapParent.transform;
        }

        // 遍历每个网格单元并生成泡泡
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                // 如果该网格单元已经生成了肥皂，则跳过
                if (soapGrid[row, column])
                {
                    continue;
                }

                // 计算网格单元的中心点
                Vector3 cellCenter = new Vector3(
                    minX + cellWidth * (column + 0.5f), // X方向
                    fixedY, // 固定Y值
                    minZ + cellHeight * (row + 0.5f) // Z方向
                );

                // 在网格单元内随机生成泡泡的位置
                Vector3 randomOffset = new Vector3(
                    Random.Range(-bubblePositionOffset, bubblePositionOffset),
                    0, // Y方向保持不变
                    Random.Range(-bubblePositionOffset, bubblePositionOffset)
                );

                Vector3 bubblePosition = cellCenter + randomOffset;

                // 确保泡泡不会超出定义的平面范围
                bubblePosition.x = Mathf.Clamp(bubblePosition.x, minX, maxX);
                bubblePosition.z = Mathf.Clamp(bubblePosition.z, minZ, maxZ);

                // **随机选择一个泡泡Prefab**
                GameObject randomBubblePrefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)];

                // 实例化泡泡Prefab
                GameObject bubble = Instantiate(randomBubblePrefab, bubblePosition, randomBubblePrefab.transform.rotation);
                // 设置泡泡的父物体为当前Spawner，便于管理
                bubble.transform.parent = bubbleParent.transform;

                // 随机缩放泡泡
                float randomScale = Random.Range(minBubbleScale, maxBubbleScale);
                bubble.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            }
        }
    }

    // 生成拖把
    public void SpawnMop()
    {
        // 随机生成拖把的位置，使用单独的范围
        float mopX = Random.Range(mopMinX, mopMaxX);
        float mopZ = Random.Range(mopMinZ, mopMaxZ);

        // 确保拖把的Y值固定
        Vector3 mopPosition = new Vector3(mopX, fixedY, mopZ);

        // 实例化拖把Prefab
        GameObject mop = Instantiate(mopPrefab, mopPosition, mopPrefab.transform.rotation);
        // 设置拖把的父物体为当前Spawner，便于管理
        mop.transform.parent = this.transform;
    }
}
