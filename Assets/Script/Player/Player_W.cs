using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 13f;

    [Header("Ground Check 설정")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("HP & Shield 설정")]
    public float maxHp = 100f;
    public float currentHp;
    public bool hasShield = false;

    [Header("대시(Dash) 설정")]
    public float dashSpeed = 15f;      // 대시 속도
    public float dashTime = 0.2f;       // 대시 지속 시간
    public float dashCooldown = 5f;     // 쿨타임
    private bool canDash = true;        // 대시 가능 여부
    private bool isDashing = false;     // 대시 중 여부

    [Header("애니메이션 설정")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false; // 무적 상태 체크

    private Rigidbody2D rb;
    private bool isGrounded;

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
        // 죽었거나 대시 중일 때는 입력을 처리하지 않음
        if (currentHp <= 0 || isDashing) return;

        // 1. 좌우 이동
        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 2. 땅 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 3. 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. 대시 입력 (왼쪽 Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    // --- 대시 코루틴 ---
    System.Collections.IEnumerator Dash()
    {
        // UI 쿨타임 연출 시작
        if (UIManager.instance != null)
            UIManager.instance.StartDashCooldownUI(dashCooldown + dashTime);

        canDash = false;
        isDashing = true;
        isInvincible = true; // 대시 중 무적

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; // 중력 무시

        // 바라보는 방향으로 돌진
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f);

        yield return new WaitForSeconds(dashTime);

        // 원래 상태로 복구
        rb.gravityScale = originalGravity;
        isDashing = false;
        isInvincible = false;

        // 쿨타임 대기
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return; // 무적일 때는 충돌 무시 (대시 중 포함)

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

    void TakeDamage(float damage)
    {
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
            anim.SetTrigger("Hit");
            StartCoroutine(BecomeInvincible());
        }
    }

    void Die()
    {
        Debug.Log("게임 오버");
        speed = 0;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        GameManager gm = GameObject.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnGameOver();
        }
    }

    System.Collections.IEnumerator BecomeInvincible()
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