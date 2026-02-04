using UnityEngine;
using System.Collections;
using DG.Tweening; // 반드시 추가!

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

        // 이 방식이 가장 정확합니다:
        // 현재 위치에서 아래로(floatAmount/2) 내려간 뒤, 
        // 위로(floatAmount)만큼 무한 왕복하게 설정
        transform.position = startPos + Vector3.down * (floatAmount * 0.5f);

        transform.DOMoveY(transform.position.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        StartCoroutine(PatternLoop());
    }

    // 이제 Update()는 필요 없습니다! DOTween이 내부적으로 처리합니다.
    // void Update() { }

    IEnumerator PatternLoop()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            int pattern = Random.Range(0, 3);
            if (pattern == 0) yield return StartCoroutine(TripleStraightPattern());
            else if (pattern == 1) yield return StartCoroutine(WaveAttackPattern());
            else yield return StartCoroutine(SummonAssistantPatternRoutine());

            yield return new WaitForSeconds(2.5f);
        }
    }

    // 2. [반동 로직] DOTween으로 훨씬 간단하게 구현
    void PlayRecoil()
    {
        // 0.15초 동안 오른쪽으로 슥 밀렸다가 다시 제자리로 돌아오는 연출
        // 펀치(Punch) 효과를 써도 되지만, DOMoveX를 두 번 쓰는 게 더 정교합니다.
        transform.DOMoveX(startPos.x + recoilDistance, recoilTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOMoveX(startPos.x, recoilTime * 2f).SetEase(Ease.InOutQuad);
            });
    }

    IEnumerator TripleStraightPattern()
    {
        if (attackPaperSprite != null) spriteRenderer.sprite = attackPaperSprite;
        PlayRecoil(); // 반동 실행

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
            // [보너스] 맞았을 때 살짝 흔들리는 효과 (DOTween 무료 버전 기능)
            transform.DOShakePosition(0.2f, 0.3f, 10, 90, false, true);

            if (UIManager.instance != null) UIManager.instance.AddScore(scorePerHit);
            Destroy(collision.gameObject);
        }
    }
}