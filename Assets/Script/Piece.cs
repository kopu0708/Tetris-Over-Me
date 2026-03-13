using UnityEngine;
using System.Collections.Generic;

public class ShapeBlock : MonoBehaviour
{
    private GridManager gridManager;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // 설치 실패 시 모양(좌표 리스트)을 되돌리기 위한 백업 리스트
    private List<Vector2Int> backupOffsets = new List<Vector2Int>();

    [Header("블록 모양 설정 (상대 좌표)")]
    public List<Vector2Int> occupiedOffsets = new List<Vector2Int>();

    private bool isDragging = false;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // 시작할 때 현재 모양 백업 및 그리드 점유
        SaveOffsetBackup();
        SetOccupancyAtCurrentPos(true);
    }

    private void Update()
    {
        // 드래그 중일 때만 R 키로 회전 가능
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateBlock();
        }
    }

    void RotateBlock()
    {
        // 시각적 회전 (시계 방향 90도)
        transform.Rotate(0, 0, -90);

        // 데이터(좌표 리스트) 회전 공식: (x, y) -> (y, -x)
        for (int i = 0; i < occupiedOffsets.Count; i++)
        {
            int oldX = occupiedOffsets[i].x;
            int oldY = occupiedOffsets[i].y;
            occupiedOffsets[i] = new Vector2Int(oldY, -oldX);
        }

        Debug.Log("블록 회전됨");
    }

    private void OnMouseDown()
    {
        isDragging = true;

        // 드래그 시작 시점의 위치, 회전, 모양 백업
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        SaveOffsetBackup();

        // 현재 점유 중인 칸들을 비워줌 (이동 중에 본인과 충돌 방지)
        SetOccupancyAtCurrentPos(false);
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        transform.position = new Vector3(mousePos.x, mousePos.y, 0);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        Vector3 relativePos = transform.position - gridManager.transform.position;
        int rootX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int rootY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        // 설치 가능한 위치인지 검사
        if (CanPlaceAt(rootX, rootY))
        {
            SnapToGrid(rootX, rootY);
            Debug.Log("배치 성공");
        }
        else
        {
            // 설치 실패 시 모든 상태 복구 (위치, 시각적 회전, 좌표 리스트)
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            RestoreOffsetBackup();

            SetOccupancyAtCurrentPos(true);
            Debug.Log("배치 실패 - 원래 위치로 복귀");
        }
    }

    private bool CanPlaceAt(int rootX, int rootY)
    {
        foreach (Vector2Int offset in occupiedOffsets)
        {
            if (!gridManager.CanPlaceBlock(rootX + offset.x, rootY + offset.y))
                return false;
        }
        return true;
    }

    private void SetOccupancyAtCurrentPos(bool occupied)
    {
        if (gridManager == null) return;

        Vector3 relativePos = transform.position - gridManager.transform.position;
        int rootX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int rootY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        foreach (Vector2Int offset in occupiedOffsets)
        {
            gridManager.SetOccupied(rootX + offset.x, rootY + offset.y, occupied);
        }
    }

    private void SnapToGrid(int rootX, int rootY)
    {
        Vector3 snapPos = gridManager.transform.position +
            new Vector3(rootX * gridManager.cellSize + gridManager.cellSize * 0.5f,
                        rootY * gridManager.cellSize + gridManager.cellSize * 0.5f, 0);

        transform.position = snapPos;
        // 새로운 기준 위치와 회전값 저장
        originalPosition = snapPos;
        originalRotation = transform.rotation;
        SaveOffsetBackup();

        SetOccupancyAtCurrentPos(true);
    }

    // 좌표 리스트 깊은 복사 (백업용)
    private void SaveOffsetBackup()
    {
        backupOffsets.Clear();
        foreach (var coord in occupiedOffsets)
        {
            backupOffsets.Add(new Vector2Int(coord.x, coord.y));
        }
    }

    // 좌표 리스트 복구
    private void RestoreOffsetBackup()
    {
        occupiedOffsets.Clear();
        foreach (var coord in backupOffsets)
        {
            occupiedOffsets.Add(new Vector2Int(coord.x, coord.y));
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}