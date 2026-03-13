using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 originalPosition;
    private GridManager gridManager;

    // 현재 블록이 어느 칸에 있는지 저장하는 변수
    private int currentGridX = -1;
    private int currentGridY = -1;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        originalPosition = transform.position;

        // 시작할 때 현재 위치를 그리드 좌표로 계산해서 점유 표시
        UpdateCurrentGridPosition();
    }

    private void OnMouseDown()
    {
        // 1. 드래그를 시작하면 현재 밟고 있는 칸을 '비어있음'으로 설정
        if (currentGridX != -1 && currentGridY != -1)
        {
            gridManager.SetOccupied(currentGridX, currentGridY, false);
            Debug.Log($"({currentGridX}, {currentGridY}) 칸을 비웠습니다.");
        }

        Vector3 mousePos = GetMouseWorldPos();
        offset = transform.position - mousePos;
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    private void OnMouseUp()
    {
        Vector3 relativePos = transform.position - gridManager.transform.position;
        int newGridX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        int newGridY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        // 2. 새로운 칸이 유효하고 비어있는지 확인
        if (gridManager.CanPlaceBlock(newGridX, newGridY))
        {
            Vector3 snapPos = gridManager.transform.position +
                new Vector3(newGridX * gridManager.cellSize + gridManager.cellSize * 0.5f,
                            newGridY * gridManager.cellSize + gridManager.cellSize * 0.5f, 0);

            transform.position = snapPos;
            originalPosition = snapPos;

            // 새로운 좌표 저장 및 점유 설정
            currentGridX = newGridX;
            currentGridY = newGridY;
            gridManager.SetOccupied(currentGridX, currentGridY, true);

            Debug.Log($"({currentGridX}, {currentGridY}) 칸에 새로 배치되었습니다.");
        }
        else
        {
            // 3. 이동할 수 없는 곳이라면 원래 위치로 되돌리고 다시 점유
            transform.position = originalPosition;
            if (currentGridX != -1 && currentGridY != -1)
            {
                gridManager.SetOccupied(currentGridX, currentGridY, true);
            }
            Debug.Log("이동 불가! 원래 위치로 되돌아갑니다.");
        }
    }

    // 현재 위치를 기반으로 그리드 좌표를 갱신하는 함수
    private void UpdateCurrentGridPosition()
    {
        Vector3 relativePos = transform.position - gridManager.transform.position;
        currentGridX = Mathf.FloorToInt(relativePos.x / gridManager.cellSize);
        currentGridY = Mathf.FloorToInt(relativePos.y / gridManager.cellSize);

        // 시작 위치가 그리드 안이라면 점유 표시
        if (currentGridX >= 0 && currentGridX < gridManager.width &&
            currentGridY >= 0 && currentGridY < gridManager.height)
        {
            gridManager.SetOccupied(currentGridX, currentGridY, true);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}