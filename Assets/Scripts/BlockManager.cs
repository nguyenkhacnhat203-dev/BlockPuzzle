using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    public Transform po1, po2, po3;
    public GameObject blockPrefab;
    public Board board;
    private List<Block> currentBlocks = new List<Block>();

    void Awake() { Instance = this; }
    void OnEnable() => BlockDrag.OnBlockPlaced += HandleBlockPlaced;
    void OnDisable() => BlockDrag.OnBlockPlaced -= HandleBlockPlaced;

    public void SpawnThreeBlocks()
    {
        currentBlocks.Clear();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        List<int> validIndices = board.GetAllValidShapeIndices(Block.AllPossibleShapes);

        int fIndex = (validIndices.Count > 0) ? validIndices[Random.Range(0, validIndices.Count)] : Random.Range(0, Block.AllPossibleShapes.Count);
        yield return SpawnBlock(po1, fIndex);
        yield return new WaitForSeconds(0.1f);

        yield return SpawnBlock(po2, Random.Range(0, Block.AllPossibleShapes.Count));
        yield return new WaitForSeconds(0.1f);
        yield return SpawnBlock(po3, Random.Range(0, Block.AllPossibleShapes.Count));

        CheckLose();
    }

    IEnumerator SpawnBlock(Transform point, int shapeIndex)
    {
        GameObject obj = Instantiate(blockPrefab, point.position, Quaternion.identity, transform);
        Block b = obj.GetComponent<Block>();
        b.SetupBlock(shapeIndex);
        currentBlocks.Add(b);
        yield return null;
    }

    void HandleBlockPlaced(Block placedBlock)
    {
        if (currentBlocks.Contains(placedBlock)) currentBlocks.Remove(placedBlock);
        if (currentBlocks.Count == 0) SpawnThreeBlocks();
        else Invoke(nameof(CheckLose), 0.1f);
    }

    public void CheckLose() { StartCoroutine(Check()); }
    IEnumerator Check()
    {
        yield return new WaitForSeconds(0.3f);
        if (currentBlocks.Count == 0) yield break;
        if (!board.CanPlaceAnyBlock(currentBlocks))
            Debug.LogError("GAME OVER!");
    }

    public void ClearAllBlocks()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        currentBlocks.Clear();
    }
}