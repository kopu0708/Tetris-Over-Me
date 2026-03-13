using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;

    private bool[,] isOccupied; // true: 점유, false: 비어있음
    
    private void Awake()
    {
        isOccupied = new bool[width, height]; // 초기화: 모든 칸이 비어있음
    }

    public bool CanPlaceBlock(int x, int y) // 해당 칸에 블록을 놓을 수 있는지 체크
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.Log("그리드 범위를 벗어났습니다.");
            return false; // 그리드 범위를 벗어남      
        }
        return !isOccupied[x, y]; // 해당 칸이 비어있는지 확인
    }

    public void SetOccupied(int x, int y, bool occupied) // 해당 칸의 점유 상태 설정
    {
        if(x >= 0 && x < width && y >= 0 && y < height)
        {
            isOccupied[x, y] = occupied;
            Debug.Log($"({x}, {y}) 칸의 점유 상태가 {(occupied ? "점유됨" : "비어있음")}으로 설정되었습니다.");
        }
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