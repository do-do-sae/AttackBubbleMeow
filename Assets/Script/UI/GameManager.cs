using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Unity.Cinemachine; // Unity 6 핵심 네임스페이스

public class GameManager : MonoBehaviour
{
    // static 인스턴스 추가 (PlayerMove에서 호출하기 위함)
    public static GameManager instance;

    public GameObject startPanel;
    public GameObject gameUI;
    public GameObject gameWorld;
    public GameObject gameOverPanel;

    [Header("타임라인 및 카메라 설정")]
    public PlayableDirector startTimeline;
    public CinemachineCamera vcamMain; // Unity 6에서는 CinemachineCamera를 사용합니다

    [Header("시작 화면 연출")]
    public RectTransform decoBG;
    public float floatDistance = 20f;
    public float floatDuration = 1.5f;

    [Header("페이드 효과")]
    public Image fadeImage;

    void Awake()
    {
        // 싱글톤 초기화
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        gameWorld.SetActive(false);
        gameUI.SetActive(false);
        startPanel.SetActive(true);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);
        }

        if (decoBG != null)
        {
            decoBG.DOAnchorPosY(decoBG.anchoredPosition.y + floatDistance, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void GameStart()
    {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();

        startPanel.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            startPanel.SetActive(false);
            gameWorld.SetActive(true);

            // [에러 없는 카메라 컷 방식]
            // 1. 시네머신 브레인을 찾습니다.
            var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null)
            {
                // 2. 브레인을 잠시 껐다가 다음 프레임에 바로 켜지도록 하여 
                // 이전 위치(갈색 화면)를 강제로 잊게 만듭니다.
                brain.enabled = false;

                // 3. 타임라인 시작 직전에 다시 켜서 첫 카메라를 즉시 잡게 합니다.
                brain.enabled = true;
            }

            if (startTimeline != null)
            {
                if (vcamMain != null) vcamMain.Priority = 10;

                startTimeline.Play();
                Invoke("OnTimelineFinished", (float)startTimeline.duration);
            }
            else
            {
                OnTimelineFinished();
            }
        });
    }

    // 함수 안의 중복 정의를 제거하고 하나로 합쳤습니다.
    void OnTimelineFinished()
    {
        // 1. UI 활성화
        gameUI.SetActive(true);

        // 2. 보스 공격 활성화
        BossManager boss = Object.FindFirstObjectByType<BossManager>();
        if (boss != null) boss.StartBossAction();

        // 3. 플레이어 제어 및 자동공격 활성화
        PlayerMove player = Object.FindFirstObjectByType<PlayerMove>();
        if (player != null)
        {
            player.isControlEnabled = true;
            AutoAttack aa = player.GetComponent<AutoAttack>();
            if (aa != null) aa.canAttack = true;
        }

        // 4. 연출 마무리 (암전 해제 등)
        fadeImage.DOFade(0f, 0.5f).OnComplete(() => {
            fadeImage.gameObject.SetActive(false);
        });
    }

    public void OnGameOver()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (UIManager.instance != null)
        {
            UIManager.instance.OnGameOverWithFade();
        }
        else
        {
            // 만약 UIManager가 없을 경우를 대비한 기본 실행
            OnGameOverWithFade();
        }
    }

    // GameManager 내부에서도 게임오버 연출이 가능하도록 유지
    public void OnGameOverWithFade()
    {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();

        fadeImage.DOFade(1f, 1.0f).SetUpdate(true).OnComplete(() => {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                gameOverPanel.transform.SetAsLastSibling();
                gameOverPanel.transform.localScale = Vector3.zero;
                gameOverPanel.transform.DOScale(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
            }
        });
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMain()
    {
        DOTween.KillAll();
        SceneManager.LoadScene("SampleScene"); // 실제 메인 씬 이름으로 확인하세요
    }
}