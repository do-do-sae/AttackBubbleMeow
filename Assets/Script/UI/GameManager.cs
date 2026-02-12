using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using DG.Tweening;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gameWorld;     // 보스/플레이어/플랫폼 등 실제 월드
    public GameObject gameUI;        // HP/점수/대시 UI
    public GameObject gameOverPanel; // GameOver UI
    public GameObject resultPanel;   // Result UI

    [Header("Timeline / Camera")]
    public PlayableDirector startTimeline;
    public CinemachineCamera vcamMain;

    [Header("Fade (Black Overlay Image)")]
    public Image fadeImage;

    public int LastScore { get; private set; } = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitToStartState();
    }

    // =========================
    // START FLOW
    // =========================
    public void OnClickStart()
    {
        // BGM
        if (SoundManager.instance != null)
            SoundManager.instance.PlayMainBGM();
        
        if (SoundManager.instance != null)
            SoundManager.instance.Click();

        // 1) 검정 오버레이를 즉시 켜서 "딜레이 체감" 제거
        SetBlackOverlay(true);

        // 2) StartPanel 끄고 월드 켜기 (검정이라 안 보임)
        if (startPanel != null) startPanel.SetActive(false);
        if (gameWorld != null) gameWorld.SetActive(true);

        // 3) 타임라인/게임 시작
        PlayTimelineOrStartGameplay();

        // 4) 살짝 딜레이 후 검정 페이드아웃
        if (fadeImage != null)
        {
            fadeImage.DOFade(0f, 0.35f)
                .SetDelay(0.08f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    SetBlackOverlay(false);
                });
        }
        else
        {
            SetBlackOverlay(false);
        }
    }

    void PlayTimelineOrStartGameplay()
    {
        if (startTimeline != null)
        {
            if (vcamMain != null) vcamMain.Priority = 10;

            startTimeline.stopped -= OnTimelineStopped;
            startTimeline.stopped += OnTimelineStopped;
            startTimeline.Play();
        }
        else
        {
            StartGameplay();
        }
    }

    void OnTimelineStopped(PlayableDirector d)
    {
        if (startTimeline != null)
            startTimeline.stopped -= OnTimelineStopped;

        StartGameplay();
    }

    void StartGameplay()
    {
        if (gameUI != null) gameUI.SetActive(true);

        // 플레이어 조작/자동공격 활성화
        PlayerMove player = FindFirstObjectByType<PlayerMove>();
        if (player != null) player.isControlEnabled = true;

        if (player != null)
        {
            AutoAttack aa = player.GetComponent<AutoAttack>();
            if (aa != null) aa.canAttack = true;
        }

        // 보스 공격 시작
        BossManager boss = FindFirstObjectByType<BossManager>();
        if (boss != null) boss.StartBossAction();
    }

    // =========================
    // GAME OVER FLOW
    // =========================
    public void OnGameOver()
    {
        // 점수 먼저 저장 (UI가 꺼져도 안전)
        LastScore = (UIManager.instance != null) ? UIManager.instance.CurrentScore : 0;

        // BestScore 저장 (UIManager에 GetBestScore 없어도 됨)
        int best = PlayerPrefs.GetInt("BEST_SCORE", 0);
        if (LastScore > best)
        {
            PlayerPrefs.SetInt("BEST_SCORE", LastScore);
            PlayerPrefs.Save();
        }

        // 정지
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // 검정 페이드 인 -> GameOverPanel 표시
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.transform.SetAsLastSibling();

            fadeImage.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    gameOverPanel.transform.SetAsLastSibling();
                }
            });
        }
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            // GameOver BGM
            if (SoundManager.instance != null)
                SoundManager.instance.PlayGameOverBGM();
        }
    }

    // GameOverPanel 버튼에서 호출 (Result로 이동)
    // 결과 패널에서도 "검정 배경 유지" 버전
    public void GoToResultWithFade()
    {
        // 이미 GameOver에서 화면이 검정(알파1) 상태이므로
        // 그냥 검정 유지 + 패널만 교체하면 됨
        SetBlackOverlay(true);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            // 검정(fade) 위에 resultPanel이 보이도록 순서 보장
            if (fadeImage != null) fadeImage.transform.SetAsLastSibling();
            resultPanel.transform.SetAsLastSibling();

            ResultPanelUI rui = resultPanel.GetComponent<ResultPanelUI>();
            if (rui != null) rui.Show(LastScore);
        }
        if (SoundManager.instance != null)
            SoundManager.instance.Click();
    }

    // =========================
    // RESULT BUTTONS
    // =========================
    public void RestartToStart()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.Click();
        // 가장 안정: 씬 리로드
        Time.timeScale = 1f;
        AudioListener.pause = false;
        DOTween.KillAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.Click();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // =========================
    // INIT
    // =========================
    void InitToStartState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (gameWorld != null) gameWorld.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        if (startPanel != null)
        {
            startPanel.SetActive(true);
            startPanel.transform.localScale = Vector3.one;
        }

        SetBlackOverlay(false);

        if (UIManager.instance != null)
        {
            UIManager.instance.ResetUI();
            UIManager.instance.RefreshBestText();
        }

        // 시작화면 랭킹 최신화가 필요하면 (StartPanel이 랭킹을 보여주는 경우)
        if (RankingManager.instance != null)
            RankingManager.instance.Reload();
        if (SoundManager.instance != null)
            SoundManager.instance.PlayMainBGM();
    }

    // =========================
    // Black Overlay Helper
    // =========================
    void SetBlackOverlay(bool on)
    {
        if (fadeImage == null) return;

        if (on)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1f);
            fadeImage.transform.SetAsLastSibling();
        }
        else
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);
        }
    }
}
