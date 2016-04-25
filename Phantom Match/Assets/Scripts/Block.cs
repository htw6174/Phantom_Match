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

    /// <summary>
    /// Tell a block to move from its current position to newPosition, at a rate of [speed] units per second
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="speed"></param>
    public IEnumerator MoveBlock(Vector3 newPosition, float speed = 1f)
    {
        WaitForSeconds frameDelay = new WaitForSeconds(1f / 60f);
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, newPosition);
        int steps = (int)(distance * (60f / speed));
        for (int i = 0; i < steps; i++)
        {
            yield return frameDelay;
            transform.position = Vector3.Lerp(startPosition, newPosition, (float)i / steps);
        }
        transform.position = newPosition;
    }

    public void DestroyBlock()
    {
        StopAllCoroutines();
        Destroy(gameObject, 0.2f);
    }
}
