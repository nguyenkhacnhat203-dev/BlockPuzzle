using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BlockDrag : MonoBehaviour
{
    public static System.Action<Block> OnBlockPlaced;

    [Header("Drag Settings")]
    public float dragYOffset = 1.0f;
    public float scaleWhileDragging = 1.0f;
    public float scaleInTray = 0.6f;

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera cam;
    private Board board;
    private Block block;
    private bool isPlaced = false;

    void Awake()
    {
        cam = Camera.main;
        board = FindObjectOfType<Board>();
        block = GetComponent<Block>();
        startPos = transform.position;
        transform.localScale = Vector3.one * scaleInTray;
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        block.SetCellsSortingOrder(10);
        transform.localScale = Vector3.one * scaleWhileDragging;
        Vector3 mousePos = GetMouseWorldPos();
        dragOffset = transform.position - mousePos;
        dragOffset.y += dragYOffset;
    }

    void OnMouseDrag()
    {
        if (isPlaced) return;
        transform.position = GetMouseWorldPos() + dragOffset;
        UpdateGhostAndHighlight();
    }

    void OnMouseUp()
    {
        if (isPlaced) return;

        if (TryPlace())
        {
            isPlaced = true;
            board.ClearGhost();
            OnBlockPlaced?.Invoke(block);
            Destroy(gameObject);
        }
        else
        {
            board.ResetHighlightedCells();
            ReturnToTray();
        }
    }

    Vector3 GetMouseWorldPos() { Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition); pos.z = 0; return pos; }

    void UpdateGhostAndHighlight()
    {
        List<Vector2Int> cells = GetBoardCellsUnderMouse();
        if (board.CanPlaceBlock(cells))
        {
            board.ShowGhost(cells, block.GetCellPrefab());
            board.HighlightLines(cells, block.GetCellPrefab());
        }
        else
        {
            board.ClearGhost();
            board.ResetHighlightedCells();
        }
    }

    bool TryPlace()
    {
        List<Vector2Int> cells = GetBoardCellsUnderMouse();
        if (board.CanPlaceBlock(cells))
        {
            AudioManager.Instance.DropBlock();

            board.PlaceBlock(cells, transform, block.GetCellPrefab());
            return true;
        }
        return false;
    }

    void ReturnToTray()
    {
        block.SetCellsSortingOrder(3);
        transform.position = startPos;
        transform.localScale = Vector3.one * scaleInTray;
        board.ClearGhost();
    }

    List<Vector2Int> GetBoardCellsUnderMouse()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (Transform c in transform)
            cells.Add(board.WorldToCell(c.position));
        return cells;
    }
}