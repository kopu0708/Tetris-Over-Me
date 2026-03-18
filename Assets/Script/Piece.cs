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
        gridManager = FindObjectOfType<GridManager>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        UpdateOffsetsFromChildren();
    }

    private void Update()
    {
        if (isFixed) return;

        if (Input.GetKeyDown(KeyCode.R)) RotateBlock();

        MoveAndFall(); // 이제 이 함수가 아래 내용을 진짜 실행합니다.
    }

    // --- 여기서부터 함수들을 하나씩 밖으로 독립시켰습니다 ---

    void MoveAndFall()
    {
        // 1. X축 추적 (부드러운 이동)
        if (playerTransform != null && transform.position.y > 2.0f)
        {
            float targetX = playerTransform.position.x;
            float newX = Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime);

            if (CanMoveTo(newX, transform.position.y))
            {
                transform.position = new Vector3(newX, transform.position.y, 0);
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
        transform.Rotate(0, 0, -90);
        for (int i = 0; i < occupiedOffsets.Count; i++)
        {
            int oldX = occupiedOffsets[i].x;
            int oldY = occupiedOffsets[i].y;
            occupiedOffsets[i] = new Vector2Int(oldY, -oldX);
        }
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