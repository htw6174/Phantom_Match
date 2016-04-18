using UnityEngine;
using System.Collections;

public class BlockGrid : MonoBehaviour {

    public int width;
    public int height;

    private Block[,] blocks;

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
}
