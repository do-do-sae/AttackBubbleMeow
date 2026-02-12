using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 13f;

    [Header("Ground Check 설정")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("HP & Shield 설정")]
    public float maxHp = 500f;
    public float currentHp;
    public bool hasShield = false;

    [Header("대쉬(Dash) 설정")]
    public float dashSpeed = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 5f;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("애니메이션 설정")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;

    private Rigidbody2D rb;
    private bool isGrounded;

    // 게임 시작 전 입력 제한
    public bool isControlEnabled = false;

    // ✅ Clamp를 “월드 경계” 기준으로 바꿈
    [Header("맵 경계 제한(월드 기준)")]
    public bool useClamp = true;

    [Tooltip("플레이어가 왼쪽으로 못 넘어가게 할 경계(Transform). 예: LeftWall")]
    public Transform leftLimit;

    [Tooltip("플레이어가 오른쪽으로 못 넘어가게 할 경계(Transform). 예: RightWall")]
    public Transform rightLimit;

    [Tooltip("플레이어가 위로 못 넘어가게 할 경계(Transform). 예: TopWall")]
    public Transform topLimit;

    [Tooltip("플레이어 콜라이더 반지름/여유. 값이 크면 더 안쪽에서 막힘")]
    public float paddingX = 0.4f;

    public float paddingTop = 0.4f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        currentHp = maxHp;

        if (UIManager.instance != null)
            UIManager.instance.UpdateHPBar(currentHp, maxHp);
    }

    void Update()
    {
        if (currentHp <= 0 || isDashing || !isControlEnabled)
        {
            if (!isControlEnabled && rb != null)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical"); // 아래 방향키 감지를 위해 추가

        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (x < 0) transform.localScale = new Vector3(-1, 1, 1);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // ✅ 일방향 플랫폼 아래로 점프 (아래 방향키 + 점프)
        if (Input.GetButtonDown("Jump") && isGrounded && y < 0)
        {
            StartCoroutine(DownJump());
        }
        // 기존 점프 로직 (일반 점프)
        else if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (SoundManager.instance != null)
                SoundManager.instance.PlayJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
            if (SoundManager.instance != null)
                SoundManager.instance.PlayDash();
        }
    }

    void LateUpdate()
    {
        if (!useClamp) return;
        if (rb == null) return;

        // 경계 Transform이 하나도 안 들어왔으면 Clamp 하지 않음(에러 방지)
        if (leftLimit == null && rightLimit == null && topLimit == null) return;

        Vector3 pos = transform.position;
        Vector2 vel = rb.linearVelocity;

        // ✅ X Clamp: left/rightLimit 기준
        if (leftLimit != null)
        {
            float minX = leftLimit.position.x + paddingX;
            if (pos.x < minX)
            {
                pos.x = minX;
                vel.x = 0f; // 밀림/떨림 방지
            }
        }

        if (rightLimit != null)
        {
            float maxX = rightLimit.position.x - paddingX;
            if (pos.x > maxX)
            {
                pos.x = maxX;
                vel.x = 0f;
            }
        }

        // ✅ Top Clamp
        if (topLimit != null)
        {
            float maxY = topLimit.position.y - paddingTop;
            if (pos.y > maxY)
            {
                pos.y = maxY;
                vel.y = 0f;
            }
        }

        rb.linearVelocity = vel;
        transform.position = pos;
    }
    // ✅ 플랫폼 아래로 내려가기 로직
    IEnumerator DownJump()
    {
        // 현재 플레이어가 밟고 있는 플랫폼을 찾습니다.
        Collider2D platformCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (platformCollider != null)
        {
            Collider2D playerCollider = GetComponent<Collider2D>();

            // 플레이어 콜라이더와 플랫폼 콜라이더 간의 충돌을 무시합니다.
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

            // 플랫폼을 충분히 빠져나갈 시간 동안 대기 (0.3초 정도면 충분합니다)
            yield return new WaitForSeconds(0.3f);

            // 다시 충돌이 발생하도록 복구합니다.
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }
    IEnumerator Dash()
    {
        if (UIManager.instance != null)
            UIManager.instance.StartDashCooldownUI(dashCooldown + dashTime);

        canDash = false;
        isDashing = true;
        isInvincible = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
        isInvincible = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return;

        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("EnemyAttack"))
        {
            TakeDamage(20f);
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            StartCoroutine(BecomeInvincible());
            return;
        }

        currentHp -= damage;

        if (UIManager.instance != null)
            UIManager.instance.UpdateHPBar(currentHp, maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null) anim.SetTrigger("Hit");
            StartCoroutine(BecomeInvincible());
        }

        if (SoundManager.instance != null)
            SoundManager.instance.PlayHit();
    }

    void Die()
    {
        if (speed == 0 && rb.bodyType == RigidbodyType2D.Kinematic) return;

        isControlEnabled = false;

        AutoAttack aa = GetComponent<AutoAttack>();
        if (aa != null) aa.canAttack = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (anim != null) anim.SetTrigger("Die");

        StartCoroutine(GameOverAfterDelay(1f));
    }

    IEnumerator GameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameManager gm = GameObject.FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.OnGameOver();
    }

    IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        float duration = 1.0f;
        float blinkSpeed = 0.1f;
        float timer = 0;

        while (timer < duration)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            yield return new WaitForSeconds(blinkSpeed);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed * 2;
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
