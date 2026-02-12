using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxPlayer;   // 효과음 전용
    public AudioSource bgmPlayer;   // BGM 전용

    [Header("SFX Clips")]
    public AudioClip bubbleSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip hitSound;
    public AudioClip BosshitSound;
    public AudioClip ClickBtn;


    [Header("BGM Clips")]
    public AudioClip mainBGM;
    public AudioClip gameOverBGM;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬 전환해도 유지
    }

    // =========================
    //  BGM
    // =========================

    public void PlayMainBGM()
    {
        if (bgmPlayer.clip == mainBGM && bgmPlayer.isPlaying)
            return;

        bgmPlayer.Stop();
        bgmPlayer.clip = mainBGM;
        bgmPlayer.loop = true;
        bgmPlayer.Play();
    }

    public void PlayGameOverBGM()
    {
        bgmPlayer.Stop();
        bgmPlayer.clip = gameOverBGM;
        bgmPlayer.Play();
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    // =========================
    //  SFX
    // =========================

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxPlayer.PlayOneShot(clip);
    }

    public void PlayBubble()
    {
        PlaySFX(bubbleSound);
    }

    public void PlayJump()
    {
        PlaySFX(jumpSound);
    }

    public void PlayDash()
    {
        PlaySFX(dashSound);
    }

    public void PlayHit()
    {
        PlaySFX(hitSound);
    }

    public void PlayBossHit()
    {
        PlaySFX(BosshitSound);
    }
    public void Click()
    {
        PlaySFX(ClickBtn);
    }
}
