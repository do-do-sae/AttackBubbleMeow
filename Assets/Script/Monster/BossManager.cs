using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween 필수

public class BossManager : MonoBehaviour
{
    [Header("공격 프리팹 설정")]
    public GameObject straightPaperPrefab;
    public GameObject wavePaperPrefab;
    public GameObject assistantPrefab;
    public Transform firePoint;

    [Header("보스 이미지 설정")]
    public Sprite normalSprite;
    public Sprite attackPaperSprite;
    public Sprite summonSprite;

    [Header("공격 주기 설정")]
    public float basicAttackCooldown = 2.0f; // 책 던지기 간격
    public float summonSkillCooldown = 10.0f; // 조교 소환 간격 (스킬)

    [Header("보스 설정")]
    public int scorePerHit = 100;
    private SpriteRenderer spriteRenderer;

    [Header("둥둥 움직임 (DOTween)")]
    public float floatAmount = 3f;      // 위아래 이동 폭
    public float floatDuration = 1f;    // 한 번 움직이는 데 걸리는 시간

    [Header("반동 설정 (DOTween)")]
    public float recoilDistance = 0.8f; // 뒤로 밀리는 거리
    public float recoilTime = 0.15f;    // 밀리는 속도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (normalSprite != null) spriteRenderer.sprite = normalSprite;

        // 보스 위아래 둥실거리는 움직임 초기화
        transform.position = startPos + Vector3.down * (floatAmount * 0.5f);
        transform.DOMoveY(transform.position.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 두 개의 루프를 병렬로 실행
        StartCoroutine(BasicAttackLoop());   // 기본 공격 (랜덤 책 던지기)
        StartCoroutine(SummonSkillLoop());   // 특수 스킬 (주기적 조교 소환)
    }

    // 1. 기본 공격 루프 (책 던지기 2종 중 랜덤)
    IEnumerator BasicAttackLoop()
    {
        yield return new WaitForSeconds(2f); // 게임 시작 후 첫 공격 대기
        while (true)
        {
            int pattern = Random.Range(0, 2); // 0 또는 1 선택

            if (pattern == 0) yield return StartCoroutine(TripleStraightPattern());
            else yield return StartCoroutine(WaveAttackPattern());

            yield return new WaitForSeconds(basicAttackCooldown);
        }
    }

    // 2. 특수 스킬 루프 (일정 시간마다 조교 소환)
    IEnumerator SummonSkillLoop()
    {
        while (true)
        {
            // 설정한 스킬 쿨타임만큼 대기
            yield return new WaitForSeconds(summonSkillCooldown);

            // 조교 소환 패턴 실행
            yield return StartCoroutine(SummonAssistantPatternRoutine());
        }
    }

    // --- 반동 연출 ---
    void PlayRecoil()
    {
        transform.DOMoveX(startPos.x + recoilDistance, recoilTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOMoveX(startPos.x, recoilTime * 2f).SetEase(Ease.InOutQuad);
            });
    }

    // --- 패턴 1: 3줄 직선 공격 ---
    IEnumerator TripleStraightPattern()
    {
        if (attackPaperSprite != null) spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        float[] yOffsets = { -1.8f, 0f, 1.8f };
        foreach (float y in yOffsets)
        {
            Vector3 spawnPos = firePoint.position + new Vector3(0, y, 0);
            Instantiate(straightPaperPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.8f);
        spriteRenderer.sprite = normalSprite;
    }

    // --- 패턴 2: 웨이브 공격 ---
    IEnumerator WaveAttackPattern()
    {
        if (attackPaperSprite != null) spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        Instantiate(wavePaperPrefab, firePoint.position, Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        Instantiate(wavePaperPrefab, firePoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = normalSprite;
    }

    // --- 패턴 3: 조교 소환 (스킬) ---
    IEnumerator SummonAssistantPatternRoutine()
    {
        // 스킬 발동 시 시각적 강조 (통통 튀기)
        transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);

        if (summonSprite != null) spriteRenderer.sprite = summonSprite;
        PlayRecoil();

        // 조교 소환 위치 랜덤 설정
        Vector3 spawnPos = new Vector3(transform.position.x - 1f, Random.Range(-2f, 2f), 0);
        Instantiate(assistantPrefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(0.8f);
        spriteRenderer.sprite = normalSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bubble"))
        {
            // 맞았을 때 피드백 (흔들림)
            transform.DOShakePosition(0.2f, 0.3f, 10, 90, false, true);

            if (UIManager.instance != null) UIManager.instance.AddScore(scorePerHit);
            Destroy(collision.gameObject);
        }
    }
}