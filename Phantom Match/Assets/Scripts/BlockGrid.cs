using UnityEngine;
using System.Collections;

public class BlockGrid : MonoBehaviour {

    public int width;
    public int height;

    public float blockSize;

    public float blockSpeed = 2f;

    public GameObject blockColliderPrefab;
    public GameObject[] blockPrefabs;

    public Block defaultBlock;

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
    /// Returns the Block at position; Returns new block with BlockType of 'none' if out of range
    /// </summary>
    /// <param name="x">Horizontal postion</param>
    /// <param name="y">Vertical position</param>
    /// <returns></returns>
    public Block GetBlock(int x, int y)
    {
        if (x < 0 || y < 0 || x > width - 1 || y > height - 1)
        {
            return null;
        }
        else
        {
            //x = Mathf.Clamp(x, 0, width - 1);
            //y = Mathf.Clamp(y, 0, height - 1);
            return blocks[x, y];
        }
    }

    /// <summary>
    /// Sets block at [x, y] to newBlock and returns true on success
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="newBlock"></param>
    /// <returns></returns>
    public bool SetBlock(int x, int y, Block newBlock)
    {
        if (x < 0 || y < 0 || x > width - 1 || y > height - 1)
        {
            Debug.Log("Tried to assign block to space outside the grid!", newBlock.gameObject);
            return false;
        }
        else
        {
            blocks[x, y] = newBlock;
            return true;
        }
    }

    public BlockType GetBlockType(int x, int y)
    {
        Block returned = GetBlock(x, y);
        if (returned == null) return BlockType.None;
        else return returned.type;
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
        CheckMatches();
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
        Block temp = GetBlock(x1, y1);
        MoveBlockTo(GetBlock(x2, y2), x1, y1);
        MoveBlockTo(temp, x2, y2);
    }

    /// <summary>
    /// Reassigns blocks[x, y] to given block, and
    /// smoothly interpolates block position from current to grid position given by [x, y]
    /// </summary>
    /// <param name="block"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void MoveBlockTo(Block block, int x, int y)
    {
        block.SetPosition(x, y);
        StartCoroutine(block.MoveBlock(blockPositions[x, y], blockSpeed));
        SetBlock(x, y, block);
    }

    /// <summary>
    /// Loop through each block in the grid and run CheckMatch() on it
    /// </summary>
    private void CheckMatches()
    {
        Block[,] matchedBlocks = new Block[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CheckMatch(x, y))
                {
                    matchedBlocks[x, y] = GetBlock(x, y);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            int nullCount = 0;
            for (int y = 0; y < height; y++)
            {
                if (matchedBlocks[x, y] != null)
                {
                    matchedBlocks[x, y].DestroyBlock();
                    nullCount++;
                }
                else
                {
                    //Cause block in position to fall
                    MoveBlockTo(GetBlock(x, y), x, y - nullCount);
                    //Spawn new block at y of height + nullCount
                }
            }

            int heightAbove = nullCount;
            for (int y = 0; y < nullCount; y++)
            {
                SpawnBlock(x, height - y - 1, heightAbove);
                heightAbove--;
            }
        }
    }

    /// <summary>
    /// Check for chains of blocks matching the type of block at [x, y] horizontally and vertically
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private bool CheckMatch(int x, int y)
    {
        BlockType type = GetBlockType(x, y);
        int verticalLength = 1;
        int horizontalLength = 1;

        verticalLength += CheckMatchInDirection(x, y, Direction.Up, type);
        verticalLength += CheckMatchInDirection(x, y, Direction.Down, type);
        horizontalLength += CheckMatchInDirection(x, y, Direction.Right, type);
        horizontalLength += CheckMatchInDirection(x, y, Direction.Left, type);

        if (verticalLength >= 3 || horizontalLength >= 3)
        {
            GetBlock(x, y).transform.localScale = Vector3.one * 0.5f;
            Debug.Log(type + " was part of a " + verticalLength + "x" + horizontalLength + " match!");
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Checks if block in [direction] is equal in type to the block at [x, y]
    /// Add one to the return value and repeat if true
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="direction"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private int CheckMatchInDirection(int x, int y, Direction direction, BlockType type)
    {
        BlockType toCheck = GetBlockType(x + direction.ToXInt(), y + direction.ToYInt());
        if (type != BlockType.None && toCheck == type)
        {
            //TODO: Flag block for destruction after match checking is done
            return CheckMatchInDirection(x + direction.ToXInt(), y + direction.ToYInt(), direction, type) + 1;
        }
        else return 0;
    }

    private void SpawnBlock(int x, int y, int rise)
    {
        Block newBlock = RandomBlockInstance();
        newBlock.transform.position = blockPositions[x, height + rise - 1];
        MoveBlockTo(newBlock, x, y);
    }

    private void FillBlockGrid()
    {
        blocks = new Block[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SetBlock(x, y, RandomBlockInstance());
                GetBlock(x, y).SetPosition(x, y);
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
        int heightTimes2 = height * 2;
        Vector3[,] newPositions = new Vector3[width, heightTimes2];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height * 2; y++)
            {
                float newX = Mathf.Lerp(-BoardWidth / 2f, BoardWidth / 2f, x / (float)(width - 1));
                float newY = Mathf.Lerp(-BoardHeight / 2f, (BoardHeight / 2f) + BoardHeight, y / (float)(heightTimes2 - 2));
                newPositions[x, y] = new Vector3(newX, newY, 0f);
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
                GetBlock(x, y).transform.position = blockPositions[x, y];
                blockColliders[x, y].transform.position = blockPositions[x, y];
            }
        }
    }

    private Block RandomBlockInstance()
    {
        return Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)]).GetComponent<Block>();
    }
}
