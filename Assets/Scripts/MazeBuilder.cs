using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;


public enum CellType 
{    Wall,
     Path,
     Start,
     Goal,
     ArrowU,
     ArrowD,
     ArrowL,
     ArrowR,
     Star 
}


public class MazeBuilder : MonoBehaviour
{
    [Header("Assets")]
    public GameObject tilePrefabs;
    public Sprite wallSprite, pathSprite, startSprite, goalSprite, arrowSprite, starSprite;

    [Header("Level")]
    public LevelData levelData;
    public int levelIndex;

    [Header("Grid")]
    public float cellSize = 1f;
    public Transform tileRoot;

    public Dictionary<Vector2Int, CellType> cells = new Dictionary<Vector2Int, CellType>();
    public Vector2Int startPos, endPos;


    public GameObject goalBurstPrefab;
 


    public void Build()
    {
        Clear();
        var raw = levelData.levels[levelIndex].Replace("\r","");

        var lines = raw.Split('\n');
        int w = lines[0].Length;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length != w)
                Debug.LogWarning($"Line {i} length {lines[i].Length} != {w}. Fill with '.' or remove extras.");
        }
        int h = lines.Length;

        for (int y = 0; y < h; y++)
        {
            string line = lines[y];

            for (int x = 0; x < w; x++)
            {

                char c = line[x];

                // 4) warn on unexpected chars
                if ("#.SGLRUD*".IndexOf(c) == -1)
                    Debug.LogWarning($"Unexpected char '{c}' at ({x},{y})");

                var t = Parse(line[x]);
                var p = new Vector2Int(x, (h - 1) - y);

                cells[p] = t;
                var go = GameObject.Instantiate(tilePrefabs, tileRoot);

                go.transform.position = ToWorld(p);
                go.GetComponent<SpriteRenderer>().sprite = SpriteOf(t);


                if(t == CellType.Start) startPos = p;
                if (t == CellType.Goal) endPos = p;
                

                }
            }
    }

    public void Clear()
    {
        cells.Clear();
        for(int i = tileRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(tileRoot.GetChild(i).gameObject);
        }
    }

    public Vector3 ToWorld(Vector2Int g) => new(g.x * cellSize, g.y * cellSize, 0);
    public bool Walkable(Vector2Int p) => cells.TryGetValue(p, out var t) && t != CellType.Wall;

    public CellType TypeAt(Vector2Int p)
    {
        return cells.TryGetValue(p, out var t) ? t : CellType.Wall;
    }

    CellType Parse(char c) => c switch
    {
        '#' => CellType.Wall,
        '.' => CellType.Path,
        'S' => CellType.Start,
        'G' => CellType.Goal,
        'U' => CellType.ArrowU,
        'D' => CellType.ArrowD,
        'L' => CellType.ArrowL,
        'R' => CellType.ArrowR,
        '*' => CellType.Star,
        _ => CellType.Path
    };

    Sprite SpriteOf(CellType t) => t switch
    {
        CellType.Wall => wallSprite,
        CellType.Path => pathSprite,
        CellType.Start => startSprite ? startSprite : pathSprite,
        CellType.Goal => goalSprite,
        CellType.ArrowU or CellType.ArrowD or CellType.ArrowL or CellType.ArrowR => arrowSprite ? arrowSprite : pathSprite,
        CellType.Star => starSprite,
        _ => pathSprite
    };

}
