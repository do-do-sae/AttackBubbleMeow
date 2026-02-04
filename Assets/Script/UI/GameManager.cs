using UnityEngine;
using UnityEngine.UI; // UI를 제어하기 위해 추가
using DG.Tweening; // DOTween을 쓰기 위해 꼭 필요!

public class GameManager : MonoBehaviour
{
    public GameObject startPanel; // 시작 화면
    public GameObject gameUI;    // 점수, HP UI
    public GameObject gameWorld; // 보스, 캐릭터 묶음

    [Header("시작 화면 연출")]
    public RectTransform decoBG; // 둥둥 떠다닐 Deco_BG를 여기에 연결
    public float floatDistance = 20f; // 움직일 거리
    public float floatDuration = 1.5f; // 움직이는 속도 (초)

    [Header("페이드 효과")]
    public Image fadeImage; // 1단계에서 만든 FadeImage 연결

    void Start()
    {
        // 처음엔 게임 세상을 아예 꺼버립니다. (보스가 동작 안 함)
        gameWorld.SetActive(false);
        gameUI.SetActive(false);
        startPanel.SetActive(true);
        // 초기 설정: 투명하게 비활성화
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);
        }
        // --- 추가된 코드: Deco_BG 둥둥 떠다니기 ---
        if (decoBG != null)
        {
            // 현재 위치에서 Y축으로 floatDistance만큼 무한 왕복(Yoyo)
            decoBG.DOAnchorPosY(decoBG.anchoredPosition.y + floatDistance, floatDuration)
                .SetEase(Ease.InOutSine) // 부드러운 가속/감속
                .SetLoops(-1, LoopType.Yoyo); // -1은 무한 반복
        }
    }

    // 시작 버튼에 연결할 함수
    public void GameStart()
    {
        // 1. 페이드 이미지 활성화
        fadeImage.gameObject.SetActive(true);

        // 2. [연출 시작] 기존의 Scale 효과와 Fade 효과를 동시에 실행
        // 패널은 작아지고, 화면은 검게 변합니다.
        startPanel.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack);
        fadeImage.DOFade(1f, 0.5f).OnComplete(() => {

            // 3. [화면이 완전히 검어졌을 때] 실제 오브젝트 교체
            startPanel.SetActive(false);
            gameWorld.SetActive(true);
            gameUI.SetActive(true);

            // 4. [다시 밝아지기] 0.5초 동안 투명하게 만든 후 비활성화
            fadeImage.DOFade(0f, 0.5f).OnComplete(() => {
                fadeImage.gameObject.SetActive(false);
            });
        });
    }
}
