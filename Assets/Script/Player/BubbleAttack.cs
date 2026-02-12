using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    private float direction = 1f; // 1이면 오른쪽, -1이면 왼쪽
    public GameObject hitEffectPrefab;   // ParticleSystem 프리팹(Inspector에 넣기)
    public float hitEffectScale = 0.2f;    // 너무 크면 0.2~0.6 같은 값으로 조절
    public float hitEffectLife = 2f;     // 파티클 지속시간보다 약간 길게

    void Start()
    {
        // 2. 사운드 재생 호출
        SoundManager.instance.PlayBubble();
        Destroy(gameObject, lifeTime);
    }

    // 외부(AutoAttack)에서 방향을 정해줄 함수
    public void SetDirection(float dir)
    {
        direction = dir;

        // 버블 이미지도 고양이가 보는 방향에 맞춰 뒤집어줍니다.
        transform.localScale = new Vector3(dir, 1, 1);
    }

    void Update()
    {
        // 설정된 방향(1 또는 -1)으로 이동합니다.
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Boss")) return;

        BossManager boss = collision.GetComponent<BossManager>();
        if (boss != null)
        {
            // 1) 명중 지점(보스 콜라이더 기준으로, 버블 위치에 가장 가까운 점)
            Vector3 hitPos = collision.ClosestPoint(transform.position);

            // 2) 이펙트 생성(월드 좌표)
            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);

                // 크기 보정(너무 크면 hitEffectScale을 낮추세요)
                fx.transform.localScale = Vector3.one * hitEffectScale;

                // 혹시 Play On Awake가 꺼져있어도 재생되게
                var ps = fx.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play(true);

                Destroy(fx, hitEffectLife);
            }

            // 3) 보스 피격 연출(플래시/사운드만)
            boss.OnHit();

            // 4) 점수 처리(기존 BossManager가 하던걸 여기로 옮김)
            if (UIManager.instance != null)
                UIManager.instance.AddScore(boss.scorePerHit);
        }

        // 5) 버블 제거
        Destroy(gameObject);
    }
}