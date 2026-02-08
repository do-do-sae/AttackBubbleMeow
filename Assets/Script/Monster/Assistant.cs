using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween 필수

public class Assistant : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 4f;
    public float waveAmplitude = 2f;
    public float waveFrequency = 5f;

    [Header("폭주(Berserk) 설정")]
    public float berserkSpeedMultiplier = 1.5f; // 풀려났을 때 몇 배 빨라질지
    public Color berserkColor = new Color(1f, 0.4f, 0.4f); // 폭주 시 붉은 색상
    private bool isBerserk = false;

    [Header("상태 이미지")]
    public Sprite normalSprite;   // 원래 모습
    public Sprite trappedSprite;  // 버블에 갇힌 모습

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private float startTime;

    // 상태 변수
    private bool isTrapped = false;
    private float trapTimer = 0f;
    private const float trapDuration = 5f; // 5초 동안 갇힘

    void Start()
    {
        startTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // 1. 갇힌 상태일 때의 로직
        if (isTrapped)
        {
            UpdateTrappedState();
            return;
        }

        // 2. 이동 로직
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
            float wave = Mathf.Sin((Time.time - startTime) * waveFrequency) * waveAmplitude;

            // 폭주 상태라면 속도를 곱해줌
            float currentSpeed = isBerserk ? speed * berserkSpeedMultiplier : speed;

            transform.position += (direction * currentSpeed + perpendicular * wave) * Time.deltaTime;

            if (direction.x > 0) transform.localScale = new Vector3(-1, 1, 1);
            else transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void UpdateTrappedState()
    {
        trapTimer -= Time.deltaTime;

        if (trapTimer <= 0)
        {
            EscapeFromBubble();
        }
    }

    void GetTrapped()
    {
        isTrapped = true;
        trapTimer = trapDuration;

        if (trappedSprite != null) spriteRenderer.sprite = trappedSprite;

        // 갇힐 때는 원래 색상으로 (혹시 폭주 중이었더라도)
        spriteRenderer.color = Color.white;

        // [연출] 제자리 떨림 효과
        transform.DOShakePosition(0.5f, 0.2f, 10, 90, false, true).SetLoops(-1);
    }

    // 버블에서 탈출하는 함수
    void EscapeFromBubble()
    {
        isTrapped = false;
        transform.DOKill();
        if (normalSprite != null) spriteRenderer.sprite = normalSprite;

        // --- 폭주 로직 추가 ---
        EnterBerserkMode();

        startTime = Time.time;
    }

    // 폭주 모드 발동
    void EnterBerserkMode()
    {
        isBerserk = true;

        // 색상을 붉게 변경
        spriteRenderer.color = berserkColor;

        // [연출] 풀려날 때 살짝 커졌다 작아지는 효과로 경고 주기
        transform.DOPunchScale(Vector3.one * 0.3f, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bubble"))
        {
            Destroy(collision.gameObject);

            if (!isTrapped)
            {
                GetTrapped();
            }
            else
            {
                Die();
            }
        }
    }

    void Die()
    {
        transform.DOKill();
        if (UIManager.instance != null) UIManager.instance.AddScore(50);

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySFX(SoundManager.instance.bubbleSound);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}