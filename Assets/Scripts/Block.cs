using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class Block : MonoBehaviour
{
    [Header("Cell Prefabs (Colors)")]
    public List<GameObject> cellPrefabs;
    public float cellSize = 1f;

    public static List<Vector2Int[]> AllPossibleShapes = new List<Vector2Int[]>
    {
        new Vector2Int[]{ new Vector2Int(0,0) },
        new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
        new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,0) },
        new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0) },
        new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1) },
        new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) }
    };

    private GameObject selectedCellPrefab;
    private BoxCollider2D boxCollider;
    private List<Vector2Int> logicalShape = new List<Vector2Int>();

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    public void SetupBlock(int shapeIndex)
    {
        ClearOldCells();
        selectedCellPrefab = cellPrefabs[Random.Range(0, cellPrefabs.Count)];

        Vector2Int[] chosenShape = AllPossibleShapes[shapeIndex];
        logicalShape.Clear();
        logicalShape.AddRange(chosenShape);

        foreach (var p in logicalShape)
        {
            GameObject cell = Instantiate(selectedCellPrefab, transform);
            cell.name = selectedCellPrefab.name;
            cell.transform.localPosition = new Vector3(p.x * cellSize, p.y * cellSize, 0);
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 3;
        }

        CenterBlock(logicalShape);
        UpdateCollider();
    }

    void CenterBlock(List<Vector2Int> shape)
    {
        Vector2 center = Vector2.zero;
        foreach (var p in shape) center += p;
        center /= shape.Count;

        foreach (Transform c in transform)
            c.localPosition -= (Vector3)(center * cellSize);
    }

    void UpdateCollider()
    {
        if (transform.childCount == 0) return;
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;
        foreach (Transform c in transform)
        {
            Vector2 p = c.localPosition;
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        boxCollider.size = (max - min) + Vector2.one * cellSize;
        boxCollider.offset = (min + max) / 2f;
    }

    void ClearOldCells()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    public void SetCellsSortingOrder(int order)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
        }
    }

    public GameObject GetCellPrefab() => selectedCellPrefab;
    public List<Vector2Int> GetShape() => logicalShape;
}