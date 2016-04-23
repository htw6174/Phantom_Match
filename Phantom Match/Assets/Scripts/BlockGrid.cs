using UnityEngine;
using System.Collections;

public class BlockGrid : MonoBehaviour {

    public int width;
    public int height;

    public float blockSize;

    public GameObject blockColliderPrefab;
    public GameObject[] blockPrefabs;

    private Vector3[,] blockPositions;
    private Block[,] blocks;
    private Block[,] blockColliders;

    private Block selected;

    public float BoardWidth
    {
        get
        {
            return blockSize * (width - 1);
        }
    }

    public float BoardHeight
    {
        get
        {
            return blockSize * (height - 1);
        }
    }

    void Start()
    {
        FillBlockGrid();
        FillColliderGrid();
        FillBlockPositions();
        SetInitialBlockPositions();
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

    public void SelectBlock(Vector3 position)
    {
        Ray cameraToGrid = new Ray(position, Vector3.forward * 10);
        Debug.DrawRay(position, Vector3.forward * 9.7f);
        RaycastHit pointerHit;
        if (Physics.Raycast(cameraToGrid, out pointerHit))
        {
            GridPosition colPos = pointerHit.transform.gameObject.GetComponent<Block>().gridPos;
            if (selected == null)
            {
                //Debug.Log(colPos.x + ", " + colPos.y);
                selected = blocks[colPos.x, colPos.y];
                selected.transform.localScale = Vector3.one * 0.8f;
                //Debug.Log(selected.gridPos.x + ", " + selected.gridPos.y);
            }
            else if (selected)
            {
                SwapBlock(selected.gridPos.x, selected.gridPos.y, FindGridDirection(selected.gridPos.x, selected.gridPos.y, colPos.x, colPos.y));
            }
        }
    }

    public void DeselectBlock()
    {
        selected.transform.localScale = Vector3.one;
        selected = null;
    }

    private Direction FindGridDirection(int x1, int y1, int x2, int y2)
    {
        //Debug.Log(x1 - x2);
        //Debug.Log(y1 - y2);

        Direction direction;
        if (x1 - x2 == 0 && y1 - y2 == -1)
        {
            direction = Direction.Up;
        }
        else if (x1 - x2 == 0 && y1 - y2 == 1)
        {
            direction = Direction.Down;
        }
        else if (x1 - x2 == -1 && y1 - y2 == 0)
        {
            direction = Direction.Right;
        }
        else if (x1 - x2 == 1 && y1 - y2 == 0)
        {
            direction = Direction.Left;
        }
        else direction = Direction.None;

        //Debug.Log(direction);
        return direction;
    }

    private void SwapBlock(int x, int y, Direction direction)
    {
        switch (direction)
        {
            case Direction.None:
                break;
            case Direction.Up:
                SwapGridBlock(x, y, x, y + 1);
                break;
            case Direction.Down:
                SwapGridBlock(x, y, x, y - 1);
                break;
            case Direction.Left:
                SwapGridBlock(x, y, x - 1, y);
                break;
            case Direction.Right:
                SwapGridBlock(x, y, x + 1, y);
                break;
        }
    }

    private void SwapGridBlock(int x1, int y1, int x2, int y2)
    {
        Block temp = blocks[x1, y1];
        blocks[x1, y1] = blocks[x2, y2];
        blocks[x2, y2] = temp;
        MoveBlockTo(blocks[x1, y1], x1, y1);
        MoveBlockTo(blocks[x2, y2], x2, y2);
    }

    /// <summary>
    /// Smoothly Interpolates block position from current to grid position given by [x, y]
    /// </summary>
    /// <param name="block"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void MoveBlockTo(Block block, int x, int y)
    {
        block.SetPosition(x, y);
        block.transform.position = blockPositions[x, y];
    }

    private bool CheckMatch(int x, int y, Direction direction = Direction.None, BlockType type = BlockType.Red, int chainLength = 1)
    {
        //Case for a call from outside the function
        if(direction == Direction.None)
        {

        }
        type = blocks[x, y].type;

        return false;
    }

    private void FillBlockGrid()
    {
        blocks = new Block[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blocks[x, y] = RandomBlockInstance();
                blocks[x, y].SetPosition(x, y);
            }
        }
    }

    private void FillColliderGrid()
    {
        blockColliders = new Block[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block newCollider = Instantiate(blockColliderPrefab).GetComponent<Block>();
                newCollider.SetPosition(x, y);
                blockColliders[x, y] = newCollider;
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
                newPositions[x, y] = new Vector3(Mathf.Lerp(-BoardWidth / 2f, BoardWidth / 2f, x / (float)(width - 1)), Mathf.Lerp(-BoardHeight / 2f, BoardHeight / 2f, y / (float)(height - 1)), 0f);
            }
        }
        blockPositions = newPositions;
    }

    private void SetInitialBlockPositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blocks[x, y].transform.position = blockPositions[x, y];
                blockColliders[x, y].transform.position = blockPositions[x, y];
            }
        }
    }

    private Block RandomBlockInstance()
    {
        return Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)]).GetComponent<Block>();
    }
}
