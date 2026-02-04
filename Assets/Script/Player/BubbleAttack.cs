using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    private float direction = 1f; // 1이면 오른쪽, -1이면 왼쪽

    void Start()
    {
        // 2. 사운드 재생 호출
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(SoundManager.instance.bubbleSound);
        }
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

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}