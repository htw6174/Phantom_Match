using UnityEngine;
using System.Collections;

public class BlockGrid : MonoBehaviour {

    public int width;
    public int height;

    public float boardSize;

    public GameObject[] blockPrefabs;

    private Block[,] blocks;
    private Vector3[,] blockPositions;

    void Start()
    {
        FillBlockGrid();
        FillBlockPositions();
        SetBlockPositions();
    }

    void Update()
    {

    }

    /// <summary>
    /// Returns the Block at position
    /// </summary>
    /// <param name="x">Horizontal postion</param>
    /// <param name="y">Vertical position</param>
    /// <param name="clampRange">
    /// true: returns closest valid block to x, y
    /// false: reutrns null when x, y is outside of range
    /// </param>
    /// <returns></returns>
    public Block GetBlock(int x, int y, bool clampRange = true)
    {
        if (clampRange)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            return blocks[x, y];
        }
        else
        {
            if (x < 0 || x < 0 || x > width - 1 || x > height - 1)
            {
                return null;
            }
            else
            {
                return blocks[x, y];
            }

        }
    }

    private void FillBlockGrid()
    {
        blocks = new Block[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blocks[x, y] = RandomBlockInstance();
            }
        }
    }

    private void FillBlockPositions()
    {
        Vector3[,] newPositions = new Vector3[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                newPositions[x, y] = new Vector3(Mathf.Lerp(-boardSize / 2f, boardSize / 2f, x / (float)(width - 1)), Mathf.Lerp(-boardSize / 2f, boardSize / 2f, y / (float)(width - 1)), 0f);
            }
        }
        blockPositions = newPositions;
    }

    private void SetBlockPositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blocks[x, y].transform.position = blockPositions[x, y];
            }
        }
    }

    private Block RandomBlockInstance()
    {
        GameObject blockPrefab = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)]);
        Debug.Log(blockPrefab.name);
        return blockPrefab.GetComponent<Block>();
    }
}
