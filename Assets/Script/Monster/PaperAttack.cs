using UnityEngine;

public class Paper : MonoBehaviour
{
    public enum MoveType { Straight, SineWave }
    [Header("이동 방식 설정")]
    public MoveType moveType = MoveType.Straight;

    public float speed = 5f;

    [Header("파도 공격 설정 (SineWave 전용)")]
    public float waveAmplitude = 2.5f; // 위아래 진폭
    public float waveFrequency = 3f;  // 파동 속도

    private float startTime;
    private Vector3 startPos;

    void Start()
    {
        startTime = Time.time;
        startPos = transform.position;
    }

    void Update()
    {
        // 왼쪽으로 이동
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (moveType == MoveType.SineWave)
        {
            // 위아래 파동 계산
            float sinMovement = Mathf.Sin((Time.time - startTime) * waveFrequency) * waveAmplitude;
            transform.position = new Vector3(transform.position.x, startPos.y + sinMovement, 0);
        }

        if (transform.position.x < -15f) Destroy(gameObject);
    }
}