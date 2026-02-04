using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance; // 어디서든 접근 가능한 싱글톤

    [Header("오디오 소스")]
    public AudioSource sfxPlayer; // 효과음 전용 플레이어

    [Header("효과음 파일(Clip)")]
    public AudioClip bubbleSound; // 버블 발사 소리

    void Awake()
    {
        // 싱글톤 설정
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 효과음을 재생하는 공용 함수
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxPlayer.PlayOneShot(clip);
        }
    }
}
