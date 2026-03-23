using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 20;
    public float cellSize = 1f;

    private bool[,] isOccupied;
    private GameObject[,] gridObjects;

    private void Awake()
    {
        isOccupied = new bool[width, height];
        gridObjects = new GameObject[width, height];
    }

    public void SetOccupied(int x, int y, bool occupied, GameObject obj = null)
    {
        if (IsValidPos(x, y))
        {
            isOccupied[x, y] = occupied;
            gridObjects[x, y] = obj;
        }
    }

    public bool CanPlaceBlock(int x, int y)
    {
        if (x < 0 || x >= width || y < 0) return false;
        if (y >= height) return true; // 천장 위는 생성 허용

        return !isOccupied[x, y];
    }

    public void CheckAndClearLine()
    {
        for (int y = 0; y < height; y++)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                ShiftLinesDown(y);
                y--; // 줄이 내려왔으므로 현재 높이 다시 검사
            }
        }
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (!isOccupied[x, y]) return false;
        }
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (gridObjects[x, y] != null)
            {
                Destroy(gridObjects[x, y]);
            }
            isOccupied[x, y] = false;
            gridObjects[x, y] = null;
        }
    }

    private void ShiftLinesDown(int clearedY)
    {
        for (int y = clearedY + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (isOccupied[x, y])
                {
                    // 데이터 한 칸 아래로 복사
                    isOccupied[x, y - 1] = true;
                    gridObjects[x, y - 1] = gridObjects[x, y];

                    // 비주얼 이동
                    if (gridObjects[x, y - 1] != null)
                    {
                        gridObjects[x, y - 1].transform.position += Vector3.down * cellSize;
                    }

                    // 원래 칸 비우기
                    isOccupied[x, y] = false;
                    gridObjects[x, y] = null;
                }
            }
        }
    }

    public bool BreakBlockAtWorldPos(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        if (IsValidPos(gridPos.x, gridPos.y))
        {
            if (isOccupied[gridPos.x, gridPos.y])
            {
                if (gridObjects[gridPos.x, gridPos.y] != null)
                    Destroy(gridObjects[gridPos.x, gridPos.y]);

                isOccupied[gridPos.x, gridPos.y] = false;
                gridObjects[gridPos.x, gridPos.y] = null;

                return true;
            }
        }
        return false;
    }

    // 헬퍼 함수: 좌표 변환
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(relativePos.x / cellSize),
            Mathf.FloorToInt(relativePos.y / cellSize)
        );
    }

    private bool IsValidPos(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f); // 약간 투명한 시안색
        for (int x = 0; x <= width; x++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(x * cellSize, 0, 0),
                            transform.position + new Vector3(x * cellSize, height * cellSize, 0));
        }
        for (int y = 0; y <= height; y++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, y * cellSize, 0),
                            transform.position + new Vector3(width * cellSize, y * cellSize, 0));
        }
    }
}