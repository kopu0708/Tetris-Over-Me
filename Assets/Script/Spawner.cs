using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("블록 설정")]
    public GameObject[] blockPrefabs; // 여러 모양의 블록 프리팹들
    public float spawnInterval = 3f;   // 블록 생성 간격
    public float spawnHeight = 15f;    // 생성될 Y 높이
    public float minSpawnInterval = 1f; // 최소 생성 간격 (난이도 상승용)
    public float difficultySpike = 0.05f; // 블록 생성 시마다 줄어드는 시간

    private float timer;

    void Start()
    {
        // 게임 시작하자마자 첫 블록 생성
        SpawnBlock();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnBlock();
            timer = 0f;

            // 점진적으로 난이도 상승 (선택 사항)
            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval -= difficultySpike;
            }
        }
    }

    public void SpawnBlock()
    {
        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogWarning("Spawner: blockPrefabs가 비어있습니다!");
            return;
        }

        // 1. 무작위 블록 선택
        int randomIndex = Random.Range(0, blockPrefabs.Length);

        // 2. 생성 위치 설정 (Y축은 고정, X축은 Spawner의 위치 기준)
        Vector3 spawnPos = new Vector3(transform.position.x, spawnHeight, 0);

        // 3. 생성
        GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPos, Quaternion.identity);

        // 4. 이름 정리 (Clone 글자 제거 - 나중에 이름으로 체크할 때 편함)
        newBlock.name = blockPrefabs[randomIndex].name;
    }
}