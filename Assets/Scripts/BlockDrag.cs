using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BlockDrag : MonoBehaviour
{
    public static System.Action<Block> OnBlockPlaced;

    public float dragYOffset = 1f;
    public float scaleWhileDragging = 1f;
    public float scaleInTray = 0.6f;

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera cam;
    private Board board;
    private Block block;
    private bool isPlaced;

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
        if (PopupManager.Instance.isShowPopup == true) { return; }
        if (isPlaced) return;

        block.SetCellsSortingOrder(10);
        transform.localScale = Vector3.one * scaleWhileDragging;

        Vector3 mousePos = GetMouseWorldPos();
        dragOffset = transform.position - mousePos;
        dragOffset.y += dragYOffset;
    }

    void OnMouseDrag()
    {
        if(PopupManager.Instance.isShowPopup == true) {  return;  }
        if (isPlaced) return;

        transform.position = GetMouseWorldPos() + dragOffset;

        List<Vector2Int> cells = GetBoardCellsUnderMouse();

        if (board.CanPlaceBlock(cells))
        {
            board.ShowGhost(cells, block.GetCellPrefab());
            board.CalculateWillClearCells(cells);
            board.ShowClearPreview(block.GetCellPrefab());
        }
        else
        {
            board.ClearGhost();
        }
    }

    void OnMouseUp()
    {
        if (PopupManager.Instance.isShowPopup == true) { return; }

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
            ReturnToTray();
        }
    }

    bool TryPlace()
    {
        List<Vector2Int> cells = GetBoardCellsUnderMouse();

        if (board.CanPlaceBlock(cells))
        {
            board.PlaceBlock(cells, transform, block.GetCellPrefab());
            AudioManager.Instance.DropBlock();
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

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }

    List<Vector2Int> GetBoardCellsUnderMouse()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (Transform c in transform)
            cells.Add(board.WorldToCell(c.position));
        return cells;
    }
}
