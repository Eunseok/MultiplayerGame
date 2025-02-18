using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    // 애니메이션 매개변수
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    public float moveSpeed = 3f; // 이동 속도
    public float positionSyncInterval = 0.1f; // 서버와 위치 동기화 주기
    public float positionLerpSpeed = 10f; // 원격 플레이어 보간 속도

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection; // 로컬 플레이어의 이동 방향
    private Vector3 targetPosition; // 원격 플레이어의 목표 위치
    private float lastDirectionX = 1f; // 마지막 이동 방향
    private float lastSyncTime; // 마지막으로 서버와 동기화한 시간

    private void Start()
    {
        // 초기화
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
        
        // 카메라가 로컬 플레이어를 따라가도록 설정
        if (isLocalPlayer)
        {
            if (Camera.main != null) Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
        }

    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            HandleLocalPlayer(); // 로컬 플레이어 동작 처리
        }
        else
        {
            HandleRemotePlayer(); // 원격 플레이어 동작 처리
        }
    }
    
    /// 로컬 플레이어 입력, 이동, 서버 동기화 처리
    private void HandleLocalPlayer()
    {
        // 입력 처리
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        // 애니메이션 업데이트
        bool isMoving = moveDirection.magnitude > 0;
        UpdateAnimation(moveX, isMoving);

        // 이동 처리
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // 서버로 동기화
        if (Time.time - lastSyncTime > positionSyncInterval)
        {
            CmdSyncPosition(transform.position, isMoving, lastDirectionX > 0);
            lastSyncTime = Time.time;
        }
    }
    
    /// 원격 플레이어 동작 처리(보간 및 애니메이션 업데이트)
    private void HandleRemotePlayer()
    {
        // 원격 플레이어 위치 보간
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
    }
    
    /// 애니메이션 및 방향 업데이트
    private void UpdateAnimation(float moveX, bool isMoving)
    {
        // 이동 상태 반영
        animator.SetBool(IsMoving, isMoving);

        if (isMoving)
        {
            if (moveX != 0)
            {
                // 이동 방향에 따라 플립 처리
                spriteRenderer.flipX = moveX < 0;
                lastDirectionX = moveX; // 마지막 이동 방향 기억
            }
        }
        else
        {
            // 정지 중일 때 마지막 방향 유지
            spriteRenderer.flipX = lastDirectionX < 0;
        }
    }

    // ---- 네트워크 동기화 ----
    
    /// 서버에 위치 동기화 요청
    [Command]
    private void CmdSyncPosition(Vector3 position, bool isMoving, bool isFacingRight)
    {
        RpcSyncPosition(position, isMoving, isFacingRight);
    }
    
    // 동기화된 위치를 클라이언트들에게 전달
    [ClientRpc]
    private void RpcSyncPosition(Vector3 position, bool isMoving, bool isFacingRight)
    {
        // 로컬 플레이어는 동기화 필요 없음 (이미 처리)
        if (isLocalPlayer) return;

        targetPosition = position; // 원격 플레이어의 목표 위치 업데이트

        // 애니메이션 및 방향 업데이트
        spriteRenderer.flipX = !isFacingRight;
        animator.SetBool(IsMoving, isMoving);
    }
}