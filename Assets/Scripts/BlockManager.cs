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

    void Awake()
    {
        Instance = this;
    }

    void OnEnable() => BlockDrag.OnBlockPlaced += HandleBlockPlaced;
    void OnDisable() => BlockDrag.OnBlockPlaced -= HandleBlockPlaced;

    // ❌ KHÔNG spawn ở Start nữa
    void Start() { }

    public void SpawnThreeBlocks()
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
        GameObject obj = Instantiate(blockPrefab, point.position, Quaternion.identity, transform);
        currentBlocks.Add(obj.GetComponent<Block>());
        yield return null;
    }

    void HandleBlockPlaced(Block placedBlock)
    {
        if (currentBlocks.Contains(placedBlock))
            currentBlocks.Remove(placedBlock);

        if (currentBlocks.Count == 0)
            SpawnThreeBlocks();
        else
            Invoke(nameof(CheckLose), 0.1f);
    }

    public void CheckLose()
    {
        StartCoroutine(Check());
    }

    IEnumerator Check()
    {
        yield return new WaitForSeconds(0.3f);
        if (currentBlocks.Count == 0) yield break;


        if (!board.CanPlaceAnyBlock(currentBlocks))
        {
            Debug.LogError("❌ GAME OVER! Không còn nước đi nào!");
        }

    }


    public void ClearAllBlocks()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        currentBlocks.Clear();
    }

}
