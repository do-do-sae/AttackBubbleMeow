using UnityEngine;

public class Paper : MonoBehaviour
{
    public enum MoveType { Straight, SineWave, Accelerate, Targeted, Explosive }

    [Header("이동 설정")]
    public MoveType moveType;
    public float speed = 10f; //

    [Header("파도 공격 설정")]
    public float waveAmplitude = 2.8f;
    public float waveFrequency = 3f;

    [Header("가속 설정")]
    public float acceleration = 0f;

    [Header("폭발 공격 설정")]
    public float explodeRange = 0.8f;     // 플레이어와 이 거리 안에 들어오면
    public float explodeDelay = 1.0f;     // 도착 후 폭발까지 대기 시간
    public float explodeRadius = 1f;    // 폭발 범위
    public int damage = 10;
    public GameObject explodeEffect;
    [Header("폭발 경고 색상")]
    public Color safeColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    [Header("폭발 경고 펄스")]
    public float pulseScale = 1.15f;
    public float pulseSpeed = 6f;

    private Vector3 baseScale;
    private SpriteRenderer sr;
    private float explodeTimerMax;

    private float startTime;
    private Vector3 startPos;
    private Vector3 moveDirection = Vector2.left; // 기본값 왼쪽

    private bool isExploding = false;
    private float explodeTimer = 0f;
    private Transform player;

    // Paper.cs의 Start 함수 부분 수정
    void Start()
    {
        baseScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();

        if (moveType == MoveType.Explosive)
        {
            explodeTimerMax = explodeDelay;
            sr.color = safeColor;
        }

        startTime = Time.time;
        startPos = transform.position;

        if (moveType == MoveType.Targeted || moveType == MoveType.Explosive)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                moveDirection = (player.position - transform.position).normalized;

                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    void Update()
    {
        if (moveType == MoveType.Explosive)
        {
            ExplosiveMove();
            return;
        }

        // 기존 이동
        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveType == MoveType.SineWave)
        {
            float sinMovement = Mathf.Sin((Time.time - startTime) * waveFrequency) * waveAmplitude;
            transform.position = new Vector3(transform.position.x, startPos.y + sinMovement, 0);
        }

        if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 10f)
            Destroy(gameObject);
    }
    void ExplosiveMove()
    {
        if (player == null) return;

        if (!isExploding)
        {
            // 플레이어를 계속 추적
            moveDirection = (player.position - transform.position).normalized;
            transform.position += moveDirection * speed * Time.deltaTime;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= explodeRange)
            {
                isExploding = true;
                explodeTimer = explodeDelay;
            }
        }
        else
        {
            explodeTimer -= Time.deltaTime;

            // 0 → 도착 직후 / 1 → 폭발 직전
            float t = 1f - (explodeTimer / explodeTimerMax);

            if (t < 0.5f)
            {
                // 흰색 → 노랑
                sr.color = Color.Lerp(safeColor, warningColor, t * 2f);
            }
            else
            {
                // 노랑 → 빨강
                sr.color = Color.Lerp(warningColor, dangerColor, (t - 0.5f) * 2f);
            }

            if (explodeTimer <= 0f)
            {
                Explode();
            }
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            transform.localScale = Vector3.Lerp(
                baseScale,
                baseScale * pulseScale,
                t * pulse
            );
        }
    }
    void Explode()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;

        GameObject fx = Instantiate(explodeEffect, pos, Quaternion.identity);

        // 핵심: 폭발 반경에 맞게 이펙트 크기 고정
        float diameter = explodeRadius * 0.5f;
        fx.transform.localScale = Vector3.one * diameter;

        // 데미지 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explodeRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerMove>()?.TakeDamage(damage);
            }
        }

        // 파티클 끝난 뒤 제거
        Destroy(fx, 1.4f);

        Destroy(gameObject);
    }
    void OnDrawGizmosSelected()
    {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}