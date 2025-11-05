using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public MazeBuilder maze;
    public GameController game;
    public float moveSpeed = 8f;

    const float SWIPE_THRESHOLD_PX = 25f;
    const float TAP_THRESHOLD_PX = 15f;

    Vector2Int gridPos;
    bool moving;
    Vector2 swipeStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridPos = maze.startPos; transform.position = maze.ToWorld(gridPos);
    }

    // Update is called once per frame
    void Update()
    {
        if (moving) { return; }

        Vector2Int dir = ReadInput();

        if (dir == Vector2Int.zero) { return; }

        var next = gridPos + dir;
        if (!maze.Walkable(next)) { return; }

        StartCoroutine(MoveTo(next));

    }

    Vector2Int ReadInput()
    {
        // --- TOUCH (mobile) ---
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                swipeStart = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                Vector2 delta = t.position - swipeStart;

                // SWIPE
                if (delta.magnitude >= SWIPE_THRESHOLD_PX)
                    return DirFromDelta(delta);

                // TAP
                if (delta.magnitude <= TAP_THRESHOLD_PX)
                    return DirFromTap(t.position);
            }
        }
        //Swipe
        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - swipeStart;

            // SWIPE
            if (delta.magnitude >= SWIPE_THRESHOLD_PX)
                return DirFromDelta(delta);

            // TAP
            if (delta.magnitude <= TAP_THRESHOLD_PX)
                return DirFromTap(Input.mousePosition);
        }

        // keyboard
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) return Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) return Vector2Int.right;
        return Vector2Int.zero;
    }

    Vector2Int DirFromDelta(Vector2 delta)
    {
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? (delta.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (delta.y > 0 ? Vector2Int.up : Vector2Int.down);
    }

    // Convert screen tap to a world direction from the player, then snap to cardinal
    Vector2Int DirFromTap(Vector2 screenPos)
    {
        var cam = Camera.main;
        if (!cam) return Vector2Int.zero;

        Vector3 tapWorld = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));

        // if you track gridPos, use the tile center; else use transform.position
        Vector3 playerWorld = maze.ToWorld(gridPos);

        Vector2 v = (Vector2)(tapWorld - playerWorld);
        if (v.sqrMagnitude < 0.0001f) return Vector2Int.zero;

        // choose the dominant axis to get a clean cardinal
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? (v.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (v.y > 0 ? Vector2Int.up : Vector2Int.down);
    }

    IEnumerator MoveTo(Vector2Int dist)
    {
        moving = true;
        Vector3 a = transform.position;
        Vector3 b = maze.ToWorld(dist);
        float t = 0f;
        while (t < 1f) 
        { 
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(a, b, t); 
            yield return null; 
        }
        gridPos = dist;


        GetComponent<Pop>()?.DoPop();

        if (maze.TypeAt(gridPos) == CellType.Goal)
        {
            Instantiate(maze.goalBurstPrefab, transform.position, Quaternion.identity);
            game.NextLevel();   // moves to next map
        }

        moving = false;
    }
}