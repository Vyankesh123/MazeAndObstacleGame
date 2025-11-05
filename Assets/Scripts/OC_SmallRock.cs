using UnityEngine;
using UnityEngine.EventSystems;

public class OC_SmallRock : MonoBehaviour, IPointerClickHandler
{
    OC_Board board;
    Vector2Int gridPos;

    public void Init(OC_Board b, Vector2Int p)
    {
        board = b;
        gridPos = p;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        board.UnregisterBlock(gridPos);
    
        board.ShowGuidance();
        Destroy(gameObject);


        if (board.PathExists())
        {
            FindAnyObjectByType<OC_GameController>()?.Win();
        }
    }
}
