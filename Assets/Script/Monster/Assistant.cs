using UnityEngine;

public class Assistant : MonoBehaviour
{
    public float speed = 4f;
    public float waveAmplitude = 2f;   // 흔들림 폭
    public float waveFrequency = 5f;     // 흔들림 속도

    private Transform playerTransform;
    private float startTime;
    void Start()
    {
        startTime = Time.time;
        // "Player" 태그를 가진 오브젝트를 찾아 추적 대상으로 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // 1. 플레이어까지의 방향 벡터 계산
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // 2. 방향 벡터에 수직인 벡터 계산 (위아래 흔들림을 주기 위함)
            // 2D이므로 방향이 (x, y)라면 수직은 (-y, x)가 됩니다.
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

            // 3. 사인파를 이용한 흔들림 계산
            float wave = Mathf.Sin((Time.time - startTime) * waveFrequency) * waveAmplitude;

            // 4. 최종 이동: 플레이어 방향(전진) + 수직 방향(흔들림)
            transform.position += (direction * speed + perpendicular * wave) * Time.deltaTime;

            // (선택 사항) 진행 방향에 따라 좌우 반전
            if (direction.x > 0) transform.localScale = new Vector3(-1, 1, 1);
            else transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어가 쏜 버블(Bubble)에 맞았을 때
        if (collision.CompareTag("Bubble"))
        {
            Debug.Log("조교님 처치!");

            // 1. 점수 추가
            if (UIManager.instance != null)
            {
                UIManager.instance.AddScore(50);
            }

            // 2. 버블 탄환 삭제
            Destroy(collision.gameObject);

            // 3. 조교 자신 삭제 (즉시 사라짐)
            Destroy(gameObject);

            // (나중에 펑! 하는 이펙트를 넣고 싶다면 여기에 Instantiate(effect, ...)를 추가하면 됩니다.)
        }
    }
}