using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{
    [Header("Board Size")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;

    [Header("Grid Prefab")]
    public GameObject CellInGridPrefab;
    public Transform cellParent;

    [Header("Ghost Preview")]
    public float ghostAlpha = 0.6f;

    private GameObject[,] gridObjects;
    private Vector2 origin;
    private List<GameObject> ghostCells = new List<GameObject>();

    private List<GameObject> highlightedCells = new List<GameObject>();
    private Dictionary<GameObject, Sprite> originalSprites = new Dictionary<GameObject, Sprite>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    void Awake()
    {
        gridObjects = new GameObject[width, height];
        origin = (Vector2)transform.position - new Vector2(width * cellSize / 2f, height * cellSize / 2f);
        GenerateCells();
    }

    void GenerateCells()
    {
        if (cellParent == null)
        {
            cellParent = new GameObject("Cells").transform;
            cellParent.SetParent(transform);
            cellParent.localPosition = Vector3.zero;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 worldPos = CellToWorld(x, y);
                if (CellInGridPrefab)
                {
                    GameObject cell = Instantiate(CellInGridPrefab, worldPos, Quaternion.identity, cellParent);
                    cell.name = $"Cell_{x}_{y}";
                    if (cell.GetComponent<SpriteRenderer>()) cell.GetComponent<SpriteRenderer>().sortingOrder = 0;
                }
            }
        }
    }

    public Vector2 CellToWorld(int x, int y) => origin + new Vector2(x * cellSize + cellSize / 2f, y * cellSize + cellSize / 2f);
    public Vector2Int WorldToCell(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x + cellSize / 2f) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - origin.y + cellSize / 2f) / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsInside(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public bool IsEmpty(int x, int y) => IsInside(x, y) && gridObjects[x, y] == null;

    public bool CanPlaceBlock(List<Vector2Int> cells)
    {
        foreach (var c in cells)
            if (!IsInside(c.x, c.y) || !IsEmpty(c.x, c.y)) return false;
        return true;
    }

    public void PlaceBlock(List<Vector2Int> cellCoords, Transform blockTransform, GameObject cellPrefab)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in blockTransform) children.Add(child);

        for (int i = 0; i < cellCoords.Count; i++)
        {
            Vector2Int coord = cellCoords[i];
            GameObject cellObj = children[i].gameObject;
            cellObj.GetComponent<SpriteRenderer>().sortingOrder = 2;
            gridObjects[coord.x, coord.y] = cellObj;
            cellObj.transform.SetParent(cellParent);
            cellObj.transform.position = CellToWorld(coord.x, coord.y);
        }

        ResetHighlightedCells();
        StartCoroutine(CheckLinesRoutine(cellPrefab));
    }

    public void HighlightLines(List<Vector2Int> potentialCells, GameObject cellPrefab)
    {
        ResetHighlightedCells();
        if (cellPrefab == null) return;

        SpriteRenderer prefabSr = cellPrefab.GetComponent<SpriteRenderer>();
        Sprite targetSprite = prefabSr.sprite;
        Color targetColor = prefabSr.color;

        List<int> rows = new List<int>();
        List<int> cols = new List<int>();

        bool[,] tempGrid = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tempGrid[x, y] = gridObjects[x, y] != null;

        foreach (var c in potentialCells)
            if (IsInside(c.x, c.y)) tempGrid[c.x, c.y] = true;

        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++) if (!tempGrid[x, y]) { full = false; break; }
            if (full) rows.Add(y);
        }
        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++) if (!tempGrid[x, y]) { full = false; break; }
            if (full) cols.Add(x);
        }

        foreach (int y in rows) for (int x = 0; x < width; x++) ApplyVisualToCell(x, y, targetSprite, targetColor);
        foreach (int x in cols) for (int y = 0; y < height; y++) ApplyVisualToCell(x, y, targetSprite, targetColor);
    }

    void ApplyVisualToCell(int x, int y, Sprite newSprite, Color newColor)
    {
        GameObject obj = gridObjects[x, y];
        if (obj != null)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (!originalSprites.ContainsKey(obj))
            {
                originalSprites.Add(obj, sr.sprite);
                originalColors.Add(obj, sr.color);
                highlightedCells.Add(obj);
            }
            sr.sprite = newSprite;
            sr.color = newColor;
        }
    }

    public void ResetHighlightedCells()
    {
        foreach (var obj in highlightedCells)
        {
            if (obj != null)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                sr.sprite = originalSprites[obj];
                sr.color = originalColors[obj];
            }
        }
        highlightedCells.Clear();
        originalSprites.Clear();
        originalColors.Clear();
    }

    IEnumerator CheckLinesRoutine(GameObject cellPrefab)
    {
        List<int> rows = new List<int>();
        List<int> cols = new List<int>();

        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++) if (gridObjects[x, y] == null) { full = false; break; }
            if (full) rows.Add(y);
        }
        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++) if (gridObjects[x, y] == null) { full = false; break; }
            if (full) cols.Add(x);
        }

        if (rows.Count > 0 || cols.Count > 0)
        {
            Sprite targetSprite = cellPrefab.GetComponent<SpriteRenderer>().sprite;
            Color targetColor = cellPrefab.GetComponent<SpriteRenderer>().color;

            foreach (int y in rows) for (int x = 0; x < width; x++) UpdateCellVisual(x, y, targetSprite, targetColor);
            foreach (int x in cols) for (int y = 0; y < height; y++) UpdateCellVisual(x, y, targetSprite, targetColor);

            yield return new WaitForSeconds(0.15f);

            foreach (int y in rows) for (int x = 0; x < width; x++) ClearCell(x, y);
            foreach (int x in cols) for (int y = 0; y < height; y++) ClearCell(x, y);
        }
    }

    void UpdateCellVisual(int x, int y, Sprite s, Color c)
    {
        if (gridObjects[x, y] != null)
        {
            var sr = gridObjects[x, y].GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.color = c;
        }
    }

    void ClearCell(int x, int y)
    {
        if (gridObjects[x, y] != null) { Destroy(gridObjects[x, y]); gridObjects[x, y] = null; }
    }

    public void ShowGhost(List<Vector2Int> cells, GameObject cellPrefab)
    {
        ClearGhost();
        if (cellPrefab == null) return;
        foreach (var c in cells)
        {
            if (!IsInside(c.x, c.y)) continue;
            GameObject ghost = Instantiate(cellPrefab, CellToWorld(c.x, c.y), Quaternion.identity, cellParent);
            SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, ghostAlpha);
            sr.sortingOrder = 1;
            ghostCells.Add(ghost);
        }
    }

    public void ClearGhost() { foreach (var g in ghostCells) if (g != null) Destroy(g); ghostCells.Clear(); }
    public bool CanPlaceAnyBlock(List<Block> blocks) {  return true; }
}