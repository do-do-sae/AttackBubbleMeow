using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트메시프로 사용을 위해 필수!

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HP 설정")]
    public Image hpFillImage;       // 현재 체력 이미지
    public Image hpEffectImage;     // 뒤따라오는 하얀 잔상 이미지 (선택 사항)
    public float lerpSpeed = 5f;    // 깎이는 속도

    private float targetFill = 1f;

    [Header("점수 설정")]
    public TextMeshProUGUI scoreText; // Hierarchy에 있는 ScoreText를 여기에 드래그하세요.
    private int currentScore = 0;

    void Awake() { instance = this; }

    void Update()
    {
        // 실시간으로 hpFillImage의 fillAmount를 목표치까지 부드럽게 이동
        hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);

        // 잔상 효과를 쓰고 싶다면 다른 속도로 따라오게 함
        if (hpEffectImage != null)
        {
            hpEffectImage.fillAmount = Mathf.Lerp(hpEffectImage.fillAmount, targetFill, Time.deltaTime * (lerpSpeed * 0.5f));
        }
    }

    public void UpdateHPBar(float currentHP, float maxHP)
    {
        // 직접 값을 바꾸지 않고 목표치(target)만 설정
        targetFill = currentHP / maxHP;
    }

   
    public void AddScore(int amount)
    {
        currentScore += amount;
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}