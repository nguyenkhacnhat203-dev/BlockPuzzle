using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Size")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1.28f;

    [Header("Grid Prefab")]
    public GameObject CellInGridPrefab;
    public Transform cellParent;

    [Header("Save / Load")]
    public List<GameObject> allCellPrefabs = new List<GameObject>();

    [Header("Intro Effect")]
    public bool playIntroEffect = true;
    public float introTotalTime = 0.1f;

    private GameObject[,] gridObjects;
    private Vector2 origin;

    private List<GameObject> ghostCells = new List<GameObject>();
    private List<GameObject> previewClearCells = new List<GameObject>();
    private Dictionary<Vector2Int, SpriteRenderer> hiddenRealCells = new Dictionary<Vector2Int, SpriteRenderer>();
    private HashSet<Vector2Int> willClearCells = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, GameObject> savedCellPrefabs = new Dictionary<Vector2Int, GameObject>();

    #region UNITY
    void Awake()
    {
        gridObjects = new GameObject[width, height];
        origin = (Vector2)transform.position -
                 new Vector2(width * cellSize / 2f, height * cellSize / 2f);

        GenerateCells();
    }

    void Start()
    {
        LoadBoardDataOnly();

        if (playIntroEffect)
            StartCoroutine(IntroBoardEffect());
        else
        {
            RestoreSavedCellsInstant();
            BlockManager.Instance.SpawnThreeBlocks();
        }
    }
    #endregion

    #region GRID
    void GenerateCells()
    {
        if (cellParent == null)
            cellParent = new GameObject("Cells").transform;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (CellInGridPrefab)
                    Instantiate(CellInGridPrefab, CellToWorld(x, y), Quaternion.identity, cellParent);
    }
    #endregion

    #region INTRO EFFECT
    IEnumerator IntroBoardEffect()
    {
        AudioManager.Instance.Reload();

        List<Vector2Int> allCells = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                allCells.Add(new Vector2Int(x, y));

        for (int i = 0; i < allCells.Count; i++)
        {
            int r = Random.Range(i, allCells.Count);
            (allCells[i], allCells[r]) = (allCells[r], allCells[i]);
        }

        float delay = introTotalTime / allCells.Count;

        foreach (var pos in allCells)
        {
            GameObject prefab = savedCellPrefabs.ContainsKey(pos)
                ? savedCellPrefabs[pos]
                : allCellPrefabs[Random.Range(0, allCellPrefabs.Count)];

            GameObject cell = Instantiate(prefab, CellToWorld(pos.x, pos.y), Quaternion.identity, cellParent);
            cell.name = prefab.name;
            gridObjects[pos.x, pos.y] = cell;

            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(introTotalTime);

        foreach (var pos in allCells)
        {
            if (savedCellPrefabs.ContainsKey(pos)) continue;
            ClearCell(pos.x, pos.y);
            yield return new WaitForSeconds(delay);
        }

        BlockManager.Instance.SpawnThreeBlocks();
    }
    #endregion

    #region SAVE / LOAD
    void LoadBoardDataOnly()
    {
        if (!PlayerPrefs.HasKey("PuzzleSaveKey")) return;

        GameSaveData data = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString("PuzzleSaveKey"));

        foreach (var sc in data.placedCells)
        {
            GameObject prefab = allCellPrefabs.Find(p => p.name == sc.name);
            if (prefab != null)
                savedCellPrefabs[new Vector2Int(sc.x, sc.y)] = prefab;
        }
    }

    void RestoreSavedCellsInstant()
    {
        foreach (var kv in savedCellPrefabs)
        {
            GameObject obj = Instantiate(kv.Value, CellToWorld(kv.Key.x, kv.Key.y), Quaternion.identity, cellParent);
            obj.name = kv.Value.name;
            gridObjects[kv.Key.x, kv.Key.y] = obj;
        }
    }

    public void SaveBoardData()
    {
        GameSaveData data = new GameSaveData();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (gridObjects[x, y] != null)
                    data.placedCells.Add(new SavedCellData
                    {
                        x = x,
                        y = y,
                        name = gridObjects[x, y].name
                    });

        PlayerPrefs.SetString("PuzzleSaveKey", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    #endregion

    #region GHOST & PREVIEW
    public void ShowGhost(List<Vector2Int> cells, GameObject prefab)
    {
        ClearGhost();

        foreach (var c in cells)
        {
            if (!IsInside(c.x, c.y)) continue;

            GameObject ghost = Instantiate(prefab, CellToWorld(c.x, c.y), Quaternion.identity, cellParent);
            SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.35f);
            sr.sortingOrder = 2;
            ghostCells.Add(ghost);
        }
    }

    public void ClearGhost()
    {
        foreach (var g in ghostCells)
            if (g != null) Destroy(g);
        ghostCells.Clear();
        ClearPreview();
    }

    public void ShowClearPreview(GameObject prefab)
    {
        ClearPreview();
        SpriteRenderer src = prefab.GetComponent<SpriteRenderer>();

        foreach (var pos in willClearCells)
        {
            GameObject real = gridObjects[pos.x, pos.y];
            if (real != null)
            {
                SpriteRenderer rs = real.GetComponent<SpriteRenderer>();
                if (rs != null)
                {
                    rs.enabled = false;
                    hiddenRealCells[pos] = rs;
                }
            }

            GameObject preview = Instantiate(prefab, CellToWorld(pos.x, pos.y), Quaternion.identity, cellParent);
            SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
            sr.sprite = src.sprite;
            sr.color = src.color;
            sr.sortingOrder = 6;

            previewClearCells.Add(preview);
        }
    }

    void ClearPreview()
    {
        foreach (var p in previewClearCells)
            if (p != null) Destroy(p);
        previewClearCells.Clear();

        foreach (var kv in hiddenRealCells)
            if (kv.Value != null) kv.Value.enabled = true;
        hiddenRealCells.Clear();
    }
    #endregion

    #region CORE GAMEPLAY
    public void CalculateWillClearCells(List<Vector2Int> potentialCells)
    {
        willClearCells.Clear();
        bool[,] temp = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                temp[x, y] = gridObjects[x, y] != null;

        foreach (var c in potentialCells)
            if (IsInside(c.x, c.y))
                temp[c.x, c.y] = true;

        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
                if (!temp[x, y]) { full = false; break; }

            if (full)
                for (int x = 0; x < width; x++)
                    willClearCells.Add(new Vector2Int(x, y));
        }

        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++)
                if (!temp[x, y]) { full = false; break; }

            if (full)
                for (int y = 0; y < height; y++)
                    willClearCells.Add(new Vector2Int(x, y));
        }
    }

    public void PlaceBlock(List<Vector2Int> cells, Transform blockTransform, GameObject prefab)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform c in blockTransform) children.Add(c);

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int p = cells[i];
            GameObject cell = children[i].gameObject;

            cell.name = prefab.name;
            cell.transform.SetParent(cellParent);
            cell.transform.position = CellToWorld(p.x, p.y);
            cell.GetComponent<SpriteRenderer>().sortingOrder = 3;

            gridObjects[p.x, p.y] = cell;
        }

        ApplyPlacedColorToWillClearCells(prefab);

        StartCoroutine(ClearRoutine());
    }

    void ApplyPlacedColorToWillClearCells(GameObject prefab)
    {
        SpriteRenderer psr = prefab.GetComponent<SpriteRenderer>();
        if (psr == null) return;

        foreach (var pos in willClearCells)
        {
            GameObject cell = gridObjects[pos.x, pos.y];
            if (cell == null) continue;

            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.sprite = psr.sprite;
            sr.color = psr.color;
        }
    }

    IEnumerator ClearRoutine()
    {
        yield return new WaitForSeconds(0.15f);

        foreach (var p in willClearCells)
            ClearCell(p.x, p.y);

        SaveBoardData();
    }
    #endregion

    #region UTILS
    void ClearCell(int x, int y)
    {
        if (gridObjects[x, y] != null)
        {
            Destroy(gridObjects[x, y]);
            gridObjects[x, y] = null;
        }
    }

    public Vector2 CellToWorld(int x, int y)
    {
        return origin + new Vector2(
            x * cellSize + cellSize / 2f,
            y * cellSize + cellSize / 2f
        );
    }

    public Vector2Int WorldToCell(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((pos.x - origin.x + cellSize / 2f) / cellSize),
            Mathf.FloorToInt((pos.y - origin.y + cellSize / 2f) / cellSize)
        );
    }

    public bool IsInside(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public bool IsEmpty(int x, int y) => IsInside(x, y) && gridObjects[x, y] == null;
    public bool CanPlaceBlock(List<Vector2Int> cells)
    {
        foreach (var c in cells)
            if (!IsInside(c.x, c.y) || !IsEmpty(c.x, c.y))
                return false;
        return true;
    }

    public bool CanPlaceAnyBlock(List<Block> blocks)
    {
        foreach (Block block in blocks)
        {
            List<Vector2Int> shape = block.GetShape();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (CanPlaceShapeAt(shape, x, y))
                        return true; 
                }
            }
        }

        return false; 
    }

    bool CanPlaceShapeAt(List<Vector2Int> shape, int startX, int startY)
    {
        foreach (var c in shape)
        {
            int x = startX + c.x;
            int y = startY + c.y;

            if (!IsInside(x, y) || !IsEmpty(x, y))
                return false;
        }
        return true;
    }

    #endregion




    public void ResetGameFromPopup()
    {
        StopAllCoroutines();
        ClearGhost();
        ClearAllCellsImmediate();
        PlayerPrefs.DeleteKey("PuzzleSaveKey");

        StartCoroutine(NewGameIntroRoutine());
    }


    void ClearAllCellsImmediate()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                ClearCell(x, y);
    }


    IEnumerator NewGameIntroRoutine()
    {
        BlockManager.Instance.ClearAllBlocks();
        AudioManager.Instance.Reload();
        List<Vector2Int> allCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                allCells.Add(new Vector2Int(x, y));

        for (int i = 0; i < allCells.Count; i++)
        {
            int r = Random.Range(i, allCells.Count);
            (allCells[i], allCells[r]) = (allCells[r], allCells[i]);
        }

      
        float spawnDelay = introTotalTime / allCells.Count;

        foreach (var pos in allCells)
        {
            GameObject prefab = allCellPrefabs[Random.Range(0, allCellPrefabs.Count)];
            GameObject cell = Instantiate(
                prefab,
                CellToWorld(pos.x, pos.y),
                Quaternion.identity,
                cellParent
            );

            cell.name = prefab.name;
            gridObjects[pos.x, pos.y] = cell;

            yield return new WaitForSeconds(spawnDelay);
        }

       
        float clearDelay = introTotalTime / allCells.Count;

        foreach (var pos in allCells)
        {
            ClearCell(pos.x, pos.y);
            yield return new WaitForSeconds(clearDelay);
        }

  
        BlockManager.Instance.SpawnThreeBlocks();
    }



}
[System.Serializable]
public class SavedCellData
{
    public int x, y;
    public string name;
}

[System.Serializable]
public class GameSaveData
{
    public List<SavedCellData> placedCells = new List<SavedCellData>();
}
