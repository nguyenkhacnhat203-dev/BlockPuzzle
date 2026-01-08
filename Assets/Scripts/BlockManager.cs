using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;

    [Header("Spawn Points")]
    public Transform po1, po2, po3;

    [Header("Block Prefab")]
    public GameObject blockPrefab;

    public Board board;

    private List<Block> currentBlocks = new List<Block>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable() => BlockDrag.OnBlockPlaced += HandleBlockPlaced;
    void OnDisable() => BlockDrag.OnBlockPlaced -= HandleBlockPlaced;

    void Start()
    {
        SpawnThreeBlocks();
    }

    void SpawnThreeBlocks()
    {
        currentBlocks.Clear();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return SpawnBlock(po1);
        yield return new WaitForSeconds(0.1f);
        yield return SpawnBlock(po2);
        yield return new WaitForSeconds(0.1f);
        yield return SpawnBlock(po3);

        CheckLose();
    }

    IEnumerator SpawnBlock(Transform point)
    {
        GameObject obj = Instantiate(
            blockPrefab,
            point.position,
            Quaternion.identity,
            this.transform   
        );
        Block block = obj.GetComponent<Block>();
        currentBlocks.Add(block);
        yield return null;
    }

    void HandleBlockPlaced(Block placedBlock)
    {
        if (currentBlocks.Contains(placedBlock))
        {
            currentBlocks.Remove(placedBlock);
        }

        if (currentBlocks.Count == 0)
        {
            SpawnThreeBlocks();
        }
        else
        {
            Invoke(nameof(CheckLose), 0.1f);
        }
    }

    public void CheckLose()
    {
        if (currentBlocks.Count == 0) return;

        bool canMove = board.CanPlaceAnyBlock(currentBlocks);

        if (!canMove)
        {
            Debug.LogError("❌ GAME OVER!");
        }
        
    }
}