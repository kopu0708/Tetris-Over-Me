using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] blockPrefabs; // 여러 모양의 블록 프리팹들
    public float spawnInterval = 3f;   // 블록 생성 간격
    public float spawnHeight = 15f;    // 생성될 Y 높이

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnBlock();
            timer = 0f;
        }
    }

    void SpawnBlock()
    {
        if (blockPrefabs.Length == 0) return;

        // 무작위 블록 선택
        int randomIndex = Random.Range(0, blockPrefabs.Length);

        // 일단 화면 중앙 상단에서 생성 (나중에 FallingShapeBlock이 플레이어를 추적함)
        Vector3 spawnPos = new Vector3(transform.position.x, spawnHeight, 0);

        Instantiate(blockPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }
}
