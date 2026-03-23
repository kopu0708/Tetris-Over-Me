using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float xRange = 4.5f; // 이동 제한 범위

    [Header("레이캐스트 설정")]
    public float rayDistance = 0.6f; // 캐릭터 중심에서 아래로 쏘는 거리
    public LayerMask groundLayer;    // 지면 레이어

    [Header("대시 설정")]
    public float dashSpeed = 15f; // 대시 속도
    public float dashDuration = 0.2f; // 대시 지속 시간
    public float dashCooldown = 1.5f; // 대시 쿨다운 시간 
    private bool isDashing = false; // 대시 중인지 여부
    private float dashTimeLeft; // 대시 남은 시간
    private float lastDashTime; // 마지막 대시 시간

    [Header("대시 파괴 설정")]
    public float dashDestroyRadius = 0.5f; // 대시 시 파괴 범위
    public LayerMask blocklayer; // 파괴할 블록 레이어
    private GridManager gridManager;
    private float lastMoveDirection = 1f; // 마지막 이동 방향 (1: 오른쪽, -1: 왼쪽)

    private Rigidbody2D rb;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        gridManager = FindFirstObjectByType<GridManager>();
    }

    void Update()
    {
        // 1. 좌우 입력 받기 (A, D / 좌우 방향키)
        moveInput = Input.GetAxisRaw("Horizontal");
        if(moveInput != 0) lastMoveDirection = moveInput; // 이동 입력이 있을 때마다 마지막 이동 방향 업데이트

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - lastDashTime >= dashCooldown)
        {
            Dash();
        }
        if(isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            CheckAndBreakBlocks(); // 대시 중에 블록 파괴 체크

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                lastDashTime = Time.time;
                rb.gravityScale = 2; // 대시가 끝나면 중력 효과를 다시 적용
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y); // 대시가 끝나면 수평 속도를 0으로 만들어서 자연스럽게 멈추도록 함
            }
        }
        // 2. 점프 입력 (바닥에 닿아 있을 때만)
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            Jump();
        }

        // 3. 이동 제한 (xRange)
        float clampedX = Mathf.Clamp(transform.position.x, -xRange, xRange);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    void FixedUpdate()
    {
        if(!isDashing)
        {
            // 4. 물리 이동 (velocity를 직접 제어하여 빠릿빠릿한 이동 구현)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
       
    }

    private void Jump()
    {
        // 점프 시 기존 수직 속도를 0으로 초기화하면 항상 일정한 높이로 점프함
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private bool IsGrounded()
    {
        Vector2 rayStart = transform.position;
        rayStart.y -= 0.5f;

        Debug.DrawRay(rayStart, Vector2.down * 0.2f, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 0.2f, groundLayer);

        return hit.collider != null;
    }

    private void Dash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        rb.linearVelocity = new Vector2(lastMoveDirection * dashSpeed, 0);
        rb.gravityScale = 0;
    }

    private void CheckAndBreakBlocks()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("[ERROR] 씬에 GridManager 오브젝트가 없습니다!");
                return;
            }
        }

        Vector2 breakPos = (Vector2)transform.position + new Vector2(lastMoveDirection * 0.5f, 0);
        Collider2D hitBlock = Physics2D.OverlapCircle(breakPos, dashDestroyRadius, blocklayer);

        if (hitBlock != null)
        {
            Debug.Log($"블록이 감지되었습니다:  + {hitBlock.name}, 레이어: {LayerMask.LayerToName(hitBlock.gameObject.layer)}");
            if (gridManager.BreakBlockAtWorldPos(hitBlock.transform.position))
            {
                Debug.Log($"{hitBlock.name} 한 칸 파괴 성공!");
                // 대시 중 중복 파괴 방지를 위해 이 프레임에서는 종료
                return;
            }
        }
    }

    // 디버그용: 파괴 범위 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float dir = Application.isPlaying ? lastMoveDirection : 1f;
        Vector2 breakPos = (Vector2)transform.position + new Vector2(dir * 0.5f, 0);
        Gizmos.DrawWireSphere(breakPos, dashDestroyRadius);
    }
}