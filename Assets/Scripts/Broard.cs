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

    private GameObject[,] gridObjects;
    private Vector2 origin;

    private List<GameObject> ghostCells = new List<GameObject>();

    private List<GameObject> previewClearCells = new List<GameObject>();
    private Dictionary<Vector2Int, SpriteRenderer> hiddenRealCells =
        new Dictionary<Vector2Int, SpriteRenderer>();

    private HashSet<Vector2Int> willClearCells = new HashSet<Vector2Int>();

    void Awake()
    {
        gridObjects = new GameObject[width, height];
        origin = (Vector2)transform.position -
                 new Vector2(width * cellSize / 2f, height * cellSize / 2f);

        GenerateCells();
    }

    void Start()
    {
        Invoke(nameof(LoadBoardData), 0.01f);
    }

    void GenerateCells()
    {
        if (cellParent == null)
            cellParent = new GameObject("Cells").transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CellInGridPrefab)
                {
                    Instantiate(
                        CellInGridPrefab,
                        CellToWorld(x, y),
                        Quaternion.identity,
                        cellParent
                    );
                }
            }
        }
    }


    public void CalculateWillClearCells(List<Vector2Int> potentialCells)
    {
        willClearCells.Clear();

        bool[,] tempGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tempGrid[x, y] = gridObjects[x, y] != null;

        foreach (var c in potentialCells)
            if (IsInside(c.x, c.y))
                tempGrid[c.x, c.y] = true;

        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
                if (!tempGrid[x, y]) { full = false; break; }

            if (full)
                for (int x = 0; x < width; x++)
                    willClearCells.Add(new Vector2Int(x, y));
        }

        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++)
                if (!tempGrid[x, y]) { full = false; break; }

            if (full)
                for (int y = 0; y < height; y++)
                    willClearCells.Add(new Vector2Int(x, y));
        }
    }


    public void ShowClearPreview(GameObject cellPrefab)
    {
        ClearPreview();

        SpriteRenderer src = cellPrefab.GetComponent<SpriteRenderer>();

        foreach (var pos in willClearCells)
        {
            if (!IsInside(pos.x, pos.y)) continue;

            GameObject real = gridObjects[pos.x, pos.y];
            if (real != null)
            {
                SpriteRenderer realSR = real.GetComponent<SpriteRenderer>();
                if (realSR != null)
                {
                    realSR.enabled = false;
                    hiddenRealCells[pos] = realSR;
                }
            }

            GameObject preview = Instantiate(
                cellPrefab,
                CellToWorld(pos.x, pos.y),
                Quaternion.identity,
                cellParent
            );

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
            if (kv.Value != null)
                kv.Value.enabled = true;

        hiddenRealCells.Clear();
    }


    public void ShowGhost(List<Vector2Int> cells, GameObject cellPrefab)
    {
        ClearGhost();

        foreach (var c in cells)
        {
            if (!IsInside(c.x, c.y)) continue;

            GameObject ghost = Instantiate(
                cellPrefab,
                CellToWorld(c.x, c.y),
                Quaternion.identity,
                cellParent
            );

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


    public void PlaceBlock(
        List<Vector2Int> cellCoords,
        Transform blockTransform,
        GameObject cellPrefab
    )
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform c in blockTransform)
            children.Add(c);

        for (int i = 0; i < cellCoords.Count; i++)
        {
            Vector2Int pos = cellCoords[i];
            GameObject cell = children[i].gameObject;

            cell.name = cellPrefab.name;
            cell.transform.SetParent(cellParent);
            cell.transform.position = CellToWorld(pos.x, pos.y);
            cell.GetComponent<SpriteRenderer>().sortingOrder = 3;

            gridObjects[pos.x, pos.y] = cell;
        }

        ApplyFinalColor(cellPrefab);
        StartCoroutine(ClearRoutine());
    }

    void ApplyFinalColor(GameObject cellPrefab)
    {
        SpriteRenderer src = cellPrefab.GetComponent<SpriteRenderer>();

        foreach (var pos in willClearCells)
        {
            GameObject cell = gridObjects[pos.x, pos.y];
            if (cell != null)
            {
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                sr.sprite = src.sprite;
                sr.color = src.color;
            }
        }
    }

    IEnumerator ClearRoutine()
    {
        yield return new WaitForSeconds(0.15f);

        foreach (var pos in willClearCells)
            ClearCell(pos.x, pos.y);

        SaveBoardData();
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

    public void LoadBoardData()
    {
        if (!PlayerPrefs.HasKey("PuzzleSaveKey")) return;

        GameSaveData data =
            JsonUtility.FromJson<GameSaveData>(
                PlayerPrefs.GetString("PuzzleSaveKey")
            );

        foreach (var sc in data.placedCells)
        {
            GameObject prefab =
                allCellPrefabs.Find(p => p.name == sc.name);

            if (prefab != null)
            {
                GameObject obj = Instantiate(
                    prefab,
                    CellToWorld(sc.x, sc.y),
                    Quaternion.identity,
                    cellParent
                );
                obj.name = prefab.name;
                gridObjects[sc.x, sc.y] = obj;
            }
        }
    }


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

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsEmpty(int x, int y)
    {
        return IsInside(x, y) && gridObjects[x, y] == null;
    }

    public bool CanPlaceBlock(List<Vector2Int> cells)
    {
        foreach (var c in cells)
            if (!IsInside(c.x, c.y) || !IsEmpty(c.x, c.y))
                return false;
        return true;
    }

    public bool CanPlaceAnyBlock(List<Block> blocks) => true;
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
