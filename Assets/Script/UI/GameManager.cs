using UnityEngine;
using UnityEngine.UI; // UI를 제어하기 위해 추가
using DG.Tweening; // DOTween을 쓰기 위해 꼭 필요!
using UnityEngine.SceneManagement; // 씬 재시작을 위해 추가
public class GameManager : MonoBehaviour
{
    public GameObject startPanel; // 시작 화면
    public GameObject gameUI;    // 점수, HP UI
    public GameObject gameWorld; // 보스, 캐릭터 묶음
    public GameObject gameOverPanel; // 게임 오버 패널 연결용

    [Header("시작 화면 연출")]
    public RectTransform decoBG; // 둥둥 떠다닐 Deco_BG를 여기에 연결
    public float floatDistance = 20f; // 움직일 거리
    public float floatDuration = 1.5f; // 움직이는 속도 (초)

    [Header("페이드 효과")]
    public Image fadeImage; // 1단계에서 만든 FadeImage 연결

    void Start()
    {
        // 게임 시작 시 시간은 정상 속도(1)여야 합니다.
        Time.timeScale = 1f;

        // 처음엔 게임 세상을 아예 꺼버립니다. (보스가 동작 안 함)
        gameWorld.SetActive(false);
        gameUI.SetActive(false);
        startPanel.SetActive(true);
        // 초기 설정 시 게임오버 패널도 꺼줍니다.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
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
        gameWorld.SetActive(false);
        gameUI.SetActive(false);
        startPanel.SetActive(true);
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
    // --- 게임 오버 함수 ---
    public void OnGameOver()
    {
        // 1. 게임 오버 UI 표시
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
        }

        // 2. 시간 멈춤 (물리 연산, 자동 공격 등 중지)
        // .SetUpdate(true)는 DOTween이 시간이 멈춰도 동작하게 해줍니다.
        Time.timeScale = 0f;

        // 3. 모든 사운드 일시정지
        AudioListener.pause = true;

        if (UIManager.instance != null)
        {
            UIManager.instance.OnGameOverWithFade();
        }
    }
    // 다시 시작 버튼(Restart Button)에 연결할 함수
    public void RestartGame()
    {
        // 다시 시작할 때 모든 상태 초기화
        Time.timeScale = 1f;       // 시간 복구
        AudioListener.pause = false; // 사운드 복구
        DOTween.KillAll();         // 실행 중인 트윈 제거

        // 현재 씬 재로드 (기록 등은 씬이 새로 읽히며 초기값으로 돌아감)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // 메인 메뉴로 돌아가기 버튼 등에 연결
    public void GoToMain()
    {
        DOTween.KillAll();
        // 씬 이름이 'MainScene'인 경우 (본인의 씬 이름에 맞게 수정)
        SceneManager.LoadScene("SampleScene");
    }
}
