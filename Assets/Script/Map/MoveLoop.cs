using UnityEngine;

public class MoveLoop : MonoBehaviour
{
    [Header("설정값")]
    public float speed = 2f;          // 이동 속도
    public float repeatWidth = 25f;   // 배경 한 세트의 가로 길이 (25 입력)

    private Vector3 startPos;

    void Start()
    {
        // 시작할 때의 초기 위치를 기억합니다.
        startPos = transform.position;
    }

    void Update()
    {
        // Mathf.Repeat(현재시간 * 속도, 반복거리)
        // 0부터 25 사이의 값을 무한히 반복해서 뿜어냅니다.
        float offset = Mathf.Repeat(Time.time * speed, repeatWidth);

        // 초기 위치에서 계산된 거리만큼 왼쪽(Vector3.left)으로 밀어줍니다.
        transform.position = startPos + Vector3.left * offset;
    }
}