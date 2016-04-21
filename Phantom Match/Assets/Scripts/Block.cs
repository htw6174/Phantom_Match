using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

    public GridPosition gridPos;

    public BlockType type;

    public void SetPosition(int newX, int newY)
    {
        gridPos.x = newX;
        gridPos.y = newY;
    }
}
