using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 20;
    public float cellSize = 1f;

    private bool[,] isOccupied;
    private GameObject[,] gridObjects; // 각 칸에 있는 실제 오브젝트 저장

    private void Awake()
    {
        isOccupied = new bool[width, height];
        gridObjects = new GameObject[width, height];
    }

    // 블록이 안착할 때 정보 등록
    public void SetOccupied(int x, int y, bool occupied, GameObject obj = null)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            isOccupied[x, y] = occupied;
            gridObjects[x, y] = obj;
        }
    }

    // 해당 칸이 비어있는지 확인
    public bool CanPlaceBlock(int x, int y)
    {
        // 그리드 범위를 벗어나면 (바닥 아래 등) 설치 불가
        if (x < 0 || x >= width || y < 0) return false;
        // 천장 위는 일단 허용 (생성 지점 때문)
        if (y >= height) return true;

        return !isOccupied[x, y];
    }

    // 가로줄이 꽉 찼는지 확인하고 삭제
    public void CheckAndClearLine()
    {
        for (int y = 0; y < height; y++)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                ShiftLinesDown(y);
                y--;
            }
        }
    }
    private void ShiftLinesDown(int clearedY)
    {
        // 지워진 줄(clearedY)의 바로 윗줄부터 가장 위(height-1)까지 검사
        for (int y = clearedY + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (isOccupied[x, y])
                {
                    // 1. 데이터를 한 칸 아래(y-1)로 이동
                    isOccupied[x, y - 1] = true;
                    gridObjects[x, y - 1] = gridObjects[x, y];

                    // 2. 실제 게임 오브젝트의 위치를 한 칸 아래로 이동
                    if (gridObjects[x, y - 1] != null)
                    {
                        gridObjects[x, y - 1].transform.position += Vector3.down * cellSize;
                    }

                    // 3. 원래 칸(y)은 비워줌
                    isOccupied[x, y] = false;
                    gridObjects[x, y] = null;
                }
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
                Destroy(gridObjects[x, y]); // 실제 블록 조각 파괴
            }
            isOccupied[x, y] = false;
            gridObjects[x, y] = null;
        }
        Debug.Log($"{y}번 줄 삭제!");
    }
    private void OnDrawGizmos() // 그리드 그리기
    {
        Gizmos.color = Color.cyan;
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