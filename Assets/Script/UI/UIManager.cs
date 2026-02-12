using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Globalization;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HP")]
    public Image hpFillImage;
    public float lerpSpeed = 5f;
    float targetFill = 1f;
    public TextMeshProUGUI hpNumberText;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    int currentScore = 0;
    public int CurrentScore => currentScore;

    [Header("Dash UI")]
    public Image dashCooldownGauge;
    Coroutine dashCoroutine;

    [Header("StartPanel Best")]
    public TextMeshProUGUI bestScoreText;
    private const string BEST_KEY = "BEST_SCORE";

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // 게임 멈춰도 UI는 자연스럽게 보이게 unscaledDeltaTime
        if (hpFillImage != null)
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.unscaledDeltaTime * lerpSpeed);
    }

    // PlayerMove가 호출
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        targetFill = currentHP / maxHP;

        if (hpNumberText != null)
        {
            int cur = Mathf.CeilToInt(currentHP);
            int max = Mathf.CeilToInt(maxHP);
            hpNumberText.text = $"{cur} / {max}";
        }
    }

    void RefreshScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = currentScore.ToString("N0", CultureInfo.InvariantCulture); // 1,000 형식
    }
    public void AddScore(int amount)
    {
        currentScore += amount;

        RefreshScoreText();

        if (scoreText != null)
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    }

    public int GetBestScore()
    {
        return PlayerPrefs.GetInt(BEST_KEY, 0);
    }

    public void RefreshBestText()
    {
        if (bestScoreText == null) return;

        int best = PlayerPrefs.GetInt("BEST_SCORE", 0);
        bestScoreText.text = $"BEST : {best:N0}";
    }

    // PlayerMove가 호출
    public void StartDashCooldownUI(float duration)
    {
        if (dashCooldownGauge == null) return;

        if (dashCoroutine != null) StopCoroutine(dashCoroutine);
        dashCoroutine = StartCoroutine(DashCooldownCoroutine(duration));
    }

    IEnumerator DashCooldownCoroutine(float duration)
    {
        float t = 0f;
        dashCooldownGauge.fillAmount = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 멈춰도 진행
            dashCooldownGauge.fillAmount = Mathf.Clamp01(t / duration);
            yield return null;
        }

        dashCooldownGauge.fillAmount = 1f;
        dashCooldownGauge.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f).SetUpdate(true);
    }

    public void ResetUI()
    {
        currentScore = 0;
        RefreshScoreText();
        if (scoreText != null) scoreText.text = "0";

        targetFill = 1f;
        if (hpFillImage != null) hpFillImage.fillAmount = 1f;

        if (dashCooldownGauge != null) dashCooldownGauge.fillAmount = 1f;
    }
}
