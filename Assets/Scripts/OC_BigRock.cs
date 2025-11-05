using UnityEngine;

public class OC_BigRock : MonoBehaviour
{

    OC_Board board;
    public Vector2Int gridPos;

    Vector3 dragStartWorld;
    Vector2Int startCell;

    public void Init(OC_Board b, Vector2Int p)
    {
        board = b;
        gridPos = p;
        transform.position = board.ToWorld(gridPos);
    }

    void CommitMove(Vector2Int target)
    {
        board.UnregisterBlock(gridPos);   // free old cell
        gridPos = target;
        board.RegisterBlock(gridPos);     // occupy new cell
        transform.position = board.ToWorld(gridPos);

        if (board.PathExists())
            FindAnyObjectByType<OC_GameController>()?.Win();
    }

    void OnMouseDown()
    {
        dragStartWorld = transform.position;
        startCell = gridPos;
    }
    void OnMouseDrag()
    {
        
        var w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;
        transform.position = w;
    }
    void OnMouseUp()
    {
        Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;
        Vector2Int target = new(
            Mathf.RoundToInt(w.x / board.cellSize),
            Mathf.RoundToInt(w.y / board.cellSize));

        bool oneStep = Mathf.Abs(target.x - startCell.x) + Mathf.Abs(target.y - startCell.y) == 1;

        if (oneStep && board.grid.ContainsKey(target) && !board.IsWall(target))
        {
           
            bool targetHasRock = board.dynamicBlocks.Contains(target);
            if (!targetHasRock)
            {
                
                board.UnregisterBlock(gridPos);
                gridPos = target;
                board.RegisterBlock(gridPos);

                transform.position = board.ToWorld(gridPos);

                board.ShowGuidance();

                var gc = FindAnyObjectByType<OC_GameController>();
                if (gc && board.PathExists()) gc.Win();
                else board.onChanged?.Invoke();
                return;
            }
        }

        // illegal move -> snap back to original snapped cell
        transform.position = board.ToWorld(startCell);
    }

}
