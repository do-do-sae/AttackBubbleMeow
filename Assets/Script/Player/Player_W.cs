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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return; // 무적일 때는 충돌 무시

        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject); // 조교와 부딪히면 조교 제거
        }
        else if (collision.CompareTag("EnemyAttack"))
        {
            TakeDamage(20f);
            Destroy(collision.gameObject); // 종이 제거
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
            // 1. 애니메이터의 Hit 트리거 발동
            anim.SetTrigger("Hit");
            // 2. 무적 및 깜빡임 코루틴 시작
            StartCoroutine(BecomeInvincible());
        }
    }

    void Die()
    {
        Debug.Log("게임 오버");
        speed = 0;
        // 나중에 사망 애니메이션(좌절)이 생기면 여기에 추가
    }

    // 무적 상태 및 시각적 피드백 코루틴
    System.Collections.IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        float duration = 1.0f; // 1초간 무적
        float blinkSpeed = 0.1f; // 깜빡이는 간격
        float timer = 0;

        while (timer < duration)
        {
            // 알파값(투명도)을 조절하여 깜빡임 표현
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            yield return new WaitForSeconds(blinkSpeed);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed * 2;
        }

        spriteRenderer.color = Color.white; // 혹시 모르니 투명도 원복
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