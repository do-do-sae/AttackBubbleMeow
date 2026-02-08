using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("페이드 설정")]
    public Image fadeImage; // 시작 시 사용했던 그 FadeImage를 드래그해서 연결하세요.

    [Header("패널 설정")]
    public GameObject gameOverPanel; // 게임 오버 패널

    [Header("HP 설정")]
    public Image hpFillImage;
    public float lerpSpeed = 5f;
    private float targetFill = 1f;

    [Header("점수 설정")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    [Header("대시 설정")]
    public Image dashCooldownGauge;
    private Coroutine dashCoroutine;

    void Awake() { instance = this; }

    void Update()
    {
        hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
    }

    // --- 게임 오버 페이드 연출 ---
    public void OnGameOverWithFade()
    {
        if (fadeImage == null) return;

        // 1. 초기화: 색상은 검정, 알파는 0으로 세팅 후 활성화
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.gameObject.SetActive(true);

        // 2. 레이어 순서 최상단으로 (다른 UI보다 앞에 나오게)
        fadeImage.transform.SetAsLastSibling();

        // 3. 1초 동안 검은색으로 화면 덮기 (Alpha: 1)
        fadeImage.DOFade(1f, 1.0f).OnComplete(() => {

            // 4. 화면이 완전히 검게 변했을 때 패널 활성화
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                // 패널을 페이드 이미지보다 더 앞으로 가져와야 글자가 보입니다.
                gameOverPanel.transform.SetAsLastSibling();
            }
        });
    }

    // --- 기존 함수들 ---
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        targetFill = currentHP / maxHP;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }

    public void StartDashCooldownUI(float duration)
    {
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);
        dashCoroutine = StartCoroutine(DashCooldownCoroutine(duration));
    }

    private System.Collections.IEnumerator DashCooldownCoroutine(float duration)
    {
        float timer = 0f;
        dashCooldownGauge.fillAmount = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            dashCooldownGauge.fillAmount = timer / duration;
            yield return null;
        }
        dashCooldownGauge.fillAmount = 1f;
        dashCooldownGauge.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 10, 1f);
        dashCooldownGauge.DOColor(Color.white, 0.1f).OnComplete(() => {
            dashCooldownGauge.DOColor(Color.cyan, 0.2f);
        });
    }
}