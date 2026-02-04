using UnityEngine;

public class PatternMove : MonoBehaviour
{
    public float speed = 2f; // 말씀하신 대로 속도 2
    public float destroyX = -20f; // 화면 왼쪽으로 완전히 벗어나는 지점

    void Update()
    {
        // 1. 왼쪽으로 이동
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 2. 삭제 로직: 패턴의 중심이 아니라 '가장 우측'까지 나갔는지 체크하려면 
        // 넉넉하게 화면 밖 지점(예: -20)을 기준으로 잡는 것이 안전합니다.
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}