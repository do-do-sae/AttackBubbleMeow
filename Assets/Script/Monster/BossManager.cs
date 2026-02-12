using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;

public class BossManager : MonoBehaviour
{
    // ======================================================
    // 1. 공격 프리팹 설정
    // ======================================================
    [Header("공격 프리팹 설정")]
    public GameObject straightPaperPrefab;
    public GameObject wavePaperPrefab;
    public GameObject targetedPaperPrefab;
    public GameObject fastPaperPrefab;
    public GameObject explosivePaperPrefab;
    public GameObject assistantPrefab;
    public Transform firePoint;

    // ======================================================
    // 2. 보스 이미지 설정
    // ======================================================
    [Header("보스 이미지 설정")]
    public Sprite normalSprite;
    public Sprite attackPaperSprite;
    public Sprite summonSprite;

    // ======================================================
    // 3. 공격 / 소환 쿨타임
    // ======================================================
    [Header("공격 주기 설정")]
    public float basicAttackCooldown = 2.0f;
    public float summonSkillCooldown = 8.0f;

    // ======================================================
    // 4. 보스 설정
    // ======================================================
    [Header("보스 설정")]
    public int scorePerHit = 100;
    private SpriteRenderer spriteRenderer;
    private CinemachineImpulseSource impulseSource;

    // ======================================================
    // 5. 둥둥 움직임 (DOTween)
    // ======================================================
    [Header("둥둥 움직임 (DOTween)")]
    public float floatAmount = 4.5f;
    public float floatDuration = 1f;

    // ======================================================
    // 6. 반동 설정 (DOTween)
    // ======================================================
    [Header("반동 설정 (DOTween)")]
    public float recoilDistance = 0.8f;
    public float recoilTime = 0.15f;

    private Vector3 startPos;
    private bool isGameStarted = false;

    
    // ======================================================
    // 7. 점수 기반 난이도
    // ======================================================
    public enum DifficultyTier
    {
        Tier1,   // 0 ~ 1000
        Tier2,   // 1000 ~ 5000
        Tier3,   // 5000 ~ 10000
        Tier4    // 10000+
    }

    DifficultyTier GetDifficultyTier(int score)
    {
        if (score < 1000) return DifficultyTier.Tier1;
        if (score < 5000) return DifficultyTier.Tier2;
        if (score < 10000) return DifficultyTier.Tier3;
        return DifficultyTier.Tier4;
    }

    float GetAttackCooldown(DifficultyTier tier)
    {
        switch (tier)
        {
            case DifficultyTier.Tier1: return 2.0f;
            case DifficultyTier.Tier2: return 1.6f;
            case DifficultyTier.Tier3: return 1.2f;
            case DifficultyTier.Tier4: return 0.8f;
        }
        return 2f;
    }

    float GetSummonCooldown(DifficultyTier tier)
    {
        switch (tier)
        {
            case DifficultyTier.Tier1: return 10f;
            case DifficultyTier.Tier2: return 8f;
            case DifficultyTier.Tier3: return 6f;
            case DifficultyTier.Tier4: return 4f;
        }
        return 10f;
    }

    // ======================================================
    // 8. 초기화
    // ======================================================
    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        spriteRenderer.sprite = normalSprite;

        // 둥둥 움직임
        transform.position = startPos + Vector3.down * (floatAmount * 0.5f);
        transform.DOMoveY(startPos.y + (floatAmount * 0.5f), floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ======================================================
    // 9. 전투 시작
    // ======================================================
    public void StartBossAction()
    {
        if (isGameStarted) return;
        isGameStarted = true;

        StartCoroutine(BasicAttackLoop());
        StartCoroutine(SummonSkillLoop());
    }

    // ======================================================
    // 10. 기본 공격 루프
    // ======================================================
    IEnumerator BasicAttackLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            int score = UIManager.instance.CurrentScore;
            DifficultyTier tier = GetDifficultyTier(score);
            float cooldown = GetAttackCooldown(tier);

            if (tier <= DifficultyTier.Tier2)
            {
                yield return StartCoroutine(RandomSinglePattern());
            }
            else if (tier == DifficultyTier.Tier3)
            {
                StartCoroutine(RandomSinglePattern());
                yield return StartCoroutine(RandomSinglePattern());
            }
            else
            {
                StartCoroutine(RandomSinglePattern());
                StartCoroutine(RandomSinglePattern());
                yield return StartCoroutine(RandomSinglePattern());
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    // ======================================================
    // 11. 랜덤 패턴 선택
    // ======================================================
    IEnumerator RandomSinglePattern()
    {
        int pattern = Random.Range(0, 5);

        switch (pattern)
        {
            case 0: yield return TripleStraightPattern(); break;
            case 1: yield return WaveAttackPattern(); break;
            case 2: yield return FanTargetPattern(); break;
            case 3: yield return FastSnipePattern(); break;
            case 4: yield return ExplosiveAttackPattern(); break;
        }
    }

    // ======================================================
    // 12. 개별 공격 패턴
    // ======================================================

    IEnumerator TripleStraightPattern()
    {
        spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        float[] yOffsets = { -1.8f, 0f, 1.8f };
        foreach (float y in yOffsets)
        {
            Instantiate(straightPaperPrefab,
                firePoint.position + new Vector3(0, y, 0),
                Quaternion.identity);
        }

        yield return new WaitForSeconds(0.8f);
        spriteRenderer.sprite = normalSprite;
    }

    IEnumerator WaveAttackPattern()
    {
        spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        Vector3 spawnPos = new Vector3(firePoint.position.x, 0, 0);

        GameObject wave1 = Instantiate(wavePaperPrefab, spawnPos, Quaternion.identity);
        wave1.GetComponent<Paper>().waveAmplitude = 2.8f;

        GameObject wave2 = Instantiate(wavePaperPrefab, spawnPos, Quaternion.identity);
        wave2.GetComponent<Paper>().waveAmplitude = -2.8f;

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = normalSprite;
    }

    IEnumerator FanTargetPattern()
    {
        spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        Instantiate(targetedPaperPrefab, firePoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        spriteRenderer.sprite = normalSprite;
    }

    IEnumerator FastSnipePattern()
    {
        spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        float[] yPositions = { 2.5f, 0f, -2.5f };
        foreach (float y in yPositions)
        {
            Instantiate(fastPaperPrefab,
                new Vector3(firePoint.position.x, y, 0),
                Quaternion.identity);
        }

        yield return new WaitForSeconds(0.6f);
        spriteRenderer.sprite = normalSprite;
    }

    IEnumerator ExplosiveAttackPattern()
    {
        spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil();

        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            float y = Random.Range(-2.5f, 2.5f);
            Instantiate(explosivePaperPrefab,
                new Vector3(firePoint.position.x, y, 0),
                Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.6f);
        spriteRenderer.sprite = normalSprite;
    }

    // ======================================================
    // 13. 조교 소환 루프
    // ======================================================
    IEnumerator SummonSkillLoop()
    {
        while (true)
        {
            int score = UIManager.instance.CurrentScore;
            DifficultyTier tier = GetDifficultyTier(score);

            yield return new WaitForSeconds(GetSummonCooldown(tier));
            yield return StartCoroutine(SummonAssistantPatternRoutine());
        }
    }

    IEnumerator SummonAssistantPatternRoutine()
    {
        transform.DOScale(Vector3.one * 1.2f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        spriteRenderer.sprite = summonSprite;
        TriggerSummonShake(0.8f);
        PlayRecoil();

        Instantiate(assistantPrefab,
            new Vector3(transform.position.x - 1f, Random.Range(-2f, 2f), 0),
            Quaternion.identity);

        yield return new WaitForSeconds(0.8f);
        spriteRenderer.sprite = normalSprite;
    }

    // ======================================================
    // 14. 보조 연출
    // ======================================================
    void PlayRecoil()
    {
        transform.DOMoveX(startPos.x + recoilDistance, recoilTime)
            .OnComplete(() =>
                transform.DOMoveX(startPos.x, recoilTime * 2f));
    }

    void TriggerSummonShake(float intensity)
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulseWithVelocity(
                impulseSource.DefaultVelocity * intensity);
    }

    // ======================================================
    // 15. 피격 처리 (점수)
    // ======================================================
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (!collision.CompareTag("Bubble")) return;
     
    //    UIManager.instance.AddScore(scorePerHit);
    //    Destroy(collision.gameObject);
    //}

    // ======================================================
    // 16. 타격 이펙트 처리
    // ======================================================
    public void OnHit()
    {
        StartCoroutine(HitFlash());

        if (SoundManager.instance != null)
            SoundManager.instance.PlayBossHit();
    }
    IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = Color.white;
    }
}
