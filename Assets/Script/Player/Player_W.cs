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

    // [추가] 게임 시작 전 제어권 확인용
    public bool isControlEnabled = false;

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
        // [수정] 죽었거나, 대시 중이거나, 아직 게임 시작 신호가 안 왔을 때 입력 차단
        if (currentHp <= 0 || isDashing || !isControlEnabled)
        {
            if (!isControlEnabled && rb != null)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (x < 0) transform.localScale = new Vector3(-1, 1, 1);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    System.Collections.IEnumerator Dash()
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
        if (speed == 0 && rb.bodyType == RigidbodyType2D.Kinematic) return;

        Debug.Log("플레이어 사망 - 게임 오버 연출 시작");

        speed = 0;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // [주의] Animator에 "Die" 트리거 파라미터가 반드시 있어야 합니다.
        anim.SetTrigger("Die");

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