using UnityEngine;

public class OC_Player : MonoBehaviour
{
    OC_Board board;
    Vector2Int pos;
    float moveDelay = 0.35f; // movement speed
    float timer;

    public void Init(OC_Board b, Vector2Int start)
    {
        board = b;
        pos = start;
        transform.position = board.ToWorld(pos);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < moveDelay) return;
        timer = 0f;

        TryStepForward();
    }

    void TryStepForward()
    {
        Vector2Int next = pos + Vector2Int.right; // always move Right
        if (board.IsWall(next)) return; // blocked

        // move
        pos = next;
        transform.position = board.ToWorld(pos);

        if (next == board.endPos)
        {
            FindAnyObjectByType<OC_GameController>().Win();
        }
    }
}
