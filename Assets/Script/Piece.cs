using UnityEngine;
using System.Collections.Generic;

public class FallingShapeBlock : MonoBehaviour
{
    private GridManager gridManager;
    private Transform playerTransform;

    [Header("낙하 설정")]
    public float fallSpeed = 3f;
    public float followSpeed = 3f;

    [Header("블록 모양 설정")]
    public List<Vector2Int> occupiedOffsets = new List<Vector2Int>();

    private bool isFixed = false;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        UpdateOffsetsFromChildren();
    }

    private void Update()
    {
        if (isFixed) return;

        if (Input.GetKeyDown(KeyCode.R)) RotateBlock();

        MoveAndFall(); 
    }

    void MoveAndFall()
    {
        // 1. X축 추적 (플레이어 위치를 기반으로 격자단위 이동)
        if (playerTransform != null && transform.position.y > 1.0f)
        {
            float relativeX = playerTransform.position.x - gridManager.transform.position.x; // 플레이어 위치를 그리드 기준으로 변환
            float gridIndexX = Mathf.FloorToInt(relativeX / gridManager.cellSize); // 가장 가까운 격자 단위로 이동
            // 그 칸의 중앙 월드 좌표 계산
            float targetWorldX = gridManager.transform.position.x +
                                (gridIndexX * gridManager.cellSize) +
                                (gridManager.cellSize * 0.5f);

            if (CanMoveTo(targetWorldX, transform.position.y))
            {
                transform.position = new Vector3(targetWorldX, transform.position.y, 0);
            }
        }

        // 2. Y축 낙하
        float nextY = transform.position.y - (fallSpeed * Time.deltaTime);

        if (CanMoveTo(transform.position.x, nextY))
        {
            transform.position = new Vector3(transform.position.x, nextY, 0);
        }
        else
        {
            SnapToGrid();
        }
    }

    bool CanMoveTo(float x, float y)
    {
        Vector3 relativePos = new Vector3(x, y, 0) - gridManager.transform.position;
        int rootX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int rootY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        foreach (Vector2Int offset in occupiedOffsets)
        {
            if (!gridManager.CanPlaceBlock(rootX + offset.x, rootY + offset.y))
                return false;
        }
        return true;
    }

    void SnapToGrid()
    {
        isFixed = true;

        Vector3 relativePos = transform.position - gridManager.transform.position;
        int rootX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int rootY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        transform.position = gridManager.transform.position +
            new Vector3(rootX * gridManager.cellSize + gridManager.cellSize * 0.5f,
                        rootY * gridManager.cellSize + gridManager.cellSize * 0.5f, 0);

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform) children.Add(child);


        for (int i = 0; i < children.Count; i++)
        {
            Vector2Int offset = occupiedOffsets[i]; 
            GameObject childObj = children[i].gameObject;
            childObj.transform.SetParent(null);

            gridManager.SetOccupied(rootX + offset.x, rootY + offset.y, true, childObj);
        }

        gridManager.CheckAndClearLine();
        CheckPlayerGameOver(rootX, rootY);
        Destroy(gameObject);
    }

    void RotateBlock()
    {
        if (gameObject.name.Contains("Square")) return;

        // 1. 회전 후의 오프셋을 미리 계산
        List<Vector2Int> nextOffsets = new List<Vector2Int>();
        foreach (Vector2Int offset in occupiedOffsets)
        {
            nextOffsets.Add(new Vector2Int(offset.y, -offset.x));
        }

        // 현재 블록의 그리드 인덱스 위치 추출
        Vector3 relativePos = transform.position - gridManager.transform.position;
        int rootX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int rootY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        // 2. 검사 루프: 제자리 -> 왼쪽으로 한 칸 밀기 -> 오른쪽으로 한 칸 밀기
        if (IsValidRotation(rootX, rootY, nextOffsets))
        {
            ApplyRotation(nextOffsets);
        }
        else if (IsValidRotation(rootX - 1, rootY, nextOffsets)) // 월 킥: 왼쪽 시도
        {
            transform.position += Vector3.left * gridManager.cellSize;
            ApplyRotation(nextOffsets);
        }
        else if (IsValidRotation(rootX + 1, rootY, nextOffsets)) // 월 킥: 오른쪽 시도
        {
            transform.position += Vector3.right * gridManager.cellSize;
            ApplyRotation(nextOffsets);
        }
        else
        {
            // 모든 방향으로 밀어도 공간이 없으면 회전하지 않음
            Debug.Log("공간이 부족하여 회전할 수 없습니다.");
        }
    }

    // 특정 위치에서 해당 오프셋들이 유효한지(그리드 안쪽 & 비어있음) 체크
    bool IsValidRotation(int rX, int rY, List<Vector2Int> offsets)
    {
        foreach (Vector2Int os in offsets)
        {
            if (!gridManager.CanPlaceBlock(rX + os.x, rY + os.y))
                return false;
        }
        return true;
    }

    // 실제 회전 적용 및 데이터 갱신
    void ApplyRotation(List<Vector2Int> nextOffsets)
    {
        transform.Rotate(0, 0, -90);
        occupiedOffsets = nextOffsets;
    }

    private void CheckPlayerGameOver(int rootX, int rootY)
    {
        foreach (Vector2Int offset in occupiedOffsets)
        {
            Vector3 cellPos = gridManager.transform.position +
                new Vector3((rootX + offset.x) * gridManager.cellSize + gridManager.cellSize * 0.5f,
                            (rootY + offset.y) * gridManager.cellSize + gridManager.cellSize * 0.5f, 0);

            Collider2D hit = Physics2D.OverlapCircle(cellPos, 0.4f);
            if (hit != null && hit.CompareTag("Player"))
            {
                Debug.Log("플레이어가 깔렸습니다! GAME OVER");
            }
        }
    }

    public void UpdateOffsetsFromChildren()
    {
        occupiedOffsets.Clear();
        foreach (Transform child in transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x);
            int y = Mathf.RoundToInt(child.localPosition.y);
            occupiedOffsets.Add(new Vector2Int(x, y));
        }
    }

    
}