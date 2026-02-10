using UnityEngine;
using System.Collections;
using DG.Tweening;

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
    public float basicAttackCooldown = 2.0f;
    public float summonSkillCooldown = 10.0f;

    [Header("보스 설정")]
    public int scorePerHit = 100;
    private SpriteRenderer spriteRenderer;

    [Header("둥둥 움직임 (DOTween)")]
    public float floatAmount = 3f;
    public float floatDuration = 1f;

    [Header("반동 설정 (DOTween)")]
    public float recoilDistance = 0.8f;
    public float recoilTime = 0.15f;

    private Vector3 startPos;
    private bool isGameStarted = false; // [추가] 중복 실행 방지용

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (normalSprite != null) spriteRenderer.sprite = normalSprite;

        // 보스 위아래 둥실거리는 움직임 초기화 (타임라인 도중에도 움직임)
        transform.position = startPos + Vector3.down * (floatAmount * 0.5f);
        transform.DOMoveY(transform.position.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // [수정] Start에서 더 이상 공격 코루틴을 바로 실행하지 않습니다.
    }

    // [추가] GameManager가 타임라인 종료 시 이 함수를 호출합니다.
    public void StartBossAction()
    {
        if (isGameStarted) return;
        isGameStarted = true;

        StartCoroutine(BasicAttackLoop());
        StartCoroutine(SummonSkillLoop());
    }

    IEnumerator BasicAttackLoop()
    {
        yield return new WaitForSeconds(1f); // 활성화 후 약간의 대기
        while (true)
        {
            int pattern = Random.Range(0, 2);

            if (pattern == 0) yield return StartCoroutine(TripleStraightPattern());
            else yield return StartCoroutine(WaveAttackPattern());

            yield return new WaitForSeconds(basicAttackCooldown);
        }
    }

    IEnumerator SummonSkillLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(summonSkillCooldown);
            yield return StartCoroutine(SummonAssistantPatternRoutine());
        }
    }

    void PlayRecoil()
    {
        transform.DOMoveX(startPos.x + recoilDistance, recoilTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOMoveX(startPos.x, recoilTime * 2f).SetEase(Ease.InOutQuad);
            });
    }

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

    IEnumerator SummonAssistantPatternRoutine()
    {
        transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);

        if (summonSprite != null) spriteRenderer.sprite = summonSprite;
        PlayRecoil();

        Vector3 spawnPos = new Vector3(transform.position.x - 1f, Random.Range(-2f, 2f), 0);
        Instantiate(assistantPrefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(0.8f);
        spriteRenderer.sprite = normalSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bubble"))
        {
            transform.DOShakePosition(0.2f, 0.3f, 10, 90, false, true);

            if (UIManager.instance != null) UIManager.instance.AddScore(scorePerHit);
            Destroy(collision.gameObject);
        }
    }
}