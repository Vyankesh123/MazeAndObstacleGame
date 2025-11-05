using System;
using System.Collections.Generic;
using UnityEngine;

public class OC_Board : MonoBehaviour
{
    [Header("Assets")]
    public GameObject tilePrefab;
    public Sprite groundSprite, blockSprite;

    public GameObject smallRockPrefab;
    public GameObject bigRockPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;

    [Header("Levels")]
    public LevelData levelData;
    public int levelIndex;

    [Header("Grid")]
    public Transform tileRoot;
    public Transform entityRoot;
    public float cellSize = 1f;


    public Dictionary<Vector2Int, char> grid = new Dictionary<Vector2Int, char>();
    public Vector2Int startPos, endPos;
    public Dictionary<Vector2Int, SpriteRenderer> tileSRs = new();

    public Action onChanged;


    public void Build()
    {
        // clear old
        foreach (Transform c in tileRoot) Destroy(c.gameObject);
        foreach (Transform c in entityRoot) Destroy(c.gameObject);
        grid.Clear();

        //read map
        var raw = levelData.levels[levelIndex].Replace("\r", "");
        var lines = raw.Split('\n');
        int h = lines.Length;
        int w = lines[0].Length;

        for(int y =0; y < h; y++)
        {
            var line = lines[y];

            for(int x =0; x < w; x++)
            {
                char c = line[x];
                var p = new Vector2Int(x, (h - 1) - y);
                grid[p] = c;

                // tiles
                var tile = Instantiate(tilePrefab, tileRoot);
                tile.transform.position = ToWorld(p);

                var sr = tile.GetComponent<SpriteRenderer>();
                if (!sr) sr = tile.AddComponent<SpriteRenderer>();

                //tile.GetComponent<SpriteRenderer>().sprite = (c == '#') ? blockSprite : groundSprite;
                sr.sprite = (c == '#') ? blockSprite : groundSprite;
                tileSRs[p] = sr;
                
                if (c == 's')
                {
                    var e = Instantiate(smallRockPrefab, entityRoot);
                    e.transform.position = ToWorld(p);
                    e.GetComponent<OC_SmallRock>().Init(this, p);
                    RegisterBlock(p);
                }
                else if (c == 'B')
                {
                    var e = Instantiate(bigRockPrefab, entityRoot);
                    e.transform.position = ToWorld(p);
                    e.GetComponent<OC_BigRock>().Init(this, p);
                    RegisterBlock(p);
                }
                else if (c == 'S')
                {
                    Instantiate(startPrefab, ToWorld(p), Quaternion.identity, entityRoot);
                    startPos = p;
                }
                else if (c == 'G')
                {
                    Instantiate(goalPrefab, ToWorld(p), Quaternion.identity, entityRoot);
                    endPos = p;
                }
            }
        }
        onChanged?.Invoke();
    }

    public void ClearTileTints()
    {
        foreach (var kv in tileSRs)
        {
            var sr = kv.Value;
            if (!sr) continue;
            sr.color = Color.white; // reset
        }
    }

    public void TintTiles(IEnumerable<Vector2Int> cells, Color color, float alpha = 0.35f)
    {
        var c = new Color(color.r, color.g, color.b, 1f);
        foreach (var p in cells)
        {
            if (tileSRs.TryGetValue(p, out var sr) && sr && !IsWall(p))
                sr.color = Color.Lerp(Color.white, c, alpha);
        }
    }

    public Vector3 ToWorld(Vector2Int g) => new(g.x * cellSize, g.y * cellSize, 0);

    public bool IsWall(Vector2Int p) => grid.TryGetValue(p, out char c) && c == '#';

    public bool PathExists()
    {
        var q = new Queue<Vector2Int>();
        var seen = new HashSet<Vector2Int>();
        q.Enqueue(startPos);
        seen.Add(startPos);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            if (p == endPos) return true;

            foreach (var d in dirs)
            {
                var n = p + d;
                if (seen.Contains(n)) continue;

            
                if (!IsBlocked(n))  
                {
                    seen.Add(n);
                    q.Enqueue(n);
                }
            }
        }

        return false;
    }

    // Tracks every grid cell currently blocked by a rock
    public HashSet<Vector2Int> dynamicBlocks = new HashSet<Vector2Int>();

    public void RegisterBlock(Vector2Int p) => dynamicBlocks.Add(p);
    public void UnregisterBlock(Vector2Int p) => dynamicBlocks.Remove(p);

    public bool IsBlocked(Vector2Int p)
    {
        bool wall = grid.TryGetValue(p, out char c) && c == '#';
        return wall || dynamicBlocks.Contains(p);
    }

    public List<Vector2Int> IdealPath()
    {
        var came = new Dictionary<Vector2Int, Vector2Int>();
        var q = new Queue<Vector2Int>();
        var seen = new HashSet<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        q.Enqueue(startPos); seen.Add(startPos);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            if (p == endPos)
            {
                // reconstruct
                var path = new List<Vector2Int>();
                var cur = p;
                while (cur != startPos) { path.Add(cur); cur = came[cur]; }
                path.Add(startPos);
                path.Reverse();
                return path;
            }
            foreach (var d in dirs)
            {
                var n = p + d;
                if (seen.Contains(n)) continue;
                if (!grid.ContainsKey(n) || IsWall(n)) continue;
                seen.Add(n);
                came[n] = p;
                q.Enqueue(n);
            }
        }
        return new List<Vector2Int>();
    }

    // Call this whenever level builds or rocks change
    public void ShowGuidance()
    {
        ClearTileTints();

        var path = IdealPath();
        if (path.Count == 0) return;

        // softly tint the ideal route
        TintTiles(path, new Color(0f, 1f, 0.8f), 0.35f);

        
        foreach (var cell in path)
        {
            if (dynamicBlocks.Contains(cell))
            {
                
                PulseRockAt(cell);
                return;
            }
        }
    }

    // Find a rock at a cell and start pulsing it
    void PulseRockAt(Vector2Int cell)
    {
        foreach (Transform t in entityRoot)
        {
            var s = t.GetComponent<OC_SmallRock>();
            if (s != null && GetGridPos(s) == cell) { AddOrPingPulse(t.gameObject, 1.1f); return; }

            var b = t.GetComponent<OC_BigRock>();
            if (b != null && b.gridPos == cell) { AddOrPingPulse(t.gameObject, 1.1f); return; }
        }
    }

    
    Vector2Int GetGridPos(OC_SmallRock s)
    {
        
        return (Vector2Int)typeof(OC_SmallRock).GetField("gridPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
    }

    void AddOrPingPulse(GameObject go, float scale = 1.1f)
    {
        var p = go.GetComponent<Pulse>();
        if (!p) p = go.AddComponent<Pulse>();
        p.popAmount = scale;
        p.DoPing();
    }

}
