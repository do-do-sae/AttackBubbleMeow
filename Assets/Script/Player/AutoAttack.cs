using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    public GameObject bubblePrefab;
    public Transform firePoint;

    // 1. Update 함수와 fireRate 관련 변수들은 더 이상 필요 없으므로 삭제합니다.
    // 애니메이션 재생 속도가 곧 공격 속도가 되기 때문입니다.

    // 2. 이 함수가 애니메이션 이벤트에서 호출될 핵심 함수입니다.
    // 반드시 'public'으로 선언해야 애니메이션 창에서 보입니다.
    public void FireBubble()
    {
        if (bubblePrefab == null || firePoint == null) return;

        // 거품 생성
        GameObject bubbleObj = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Bubble bubbleScript = bubbleObj.GetComponent<Bubble>();

        if (bubbleScript != null)
        {
            // 고양이의 현재 방향(1 또는 -1)을 버블에게 전달
            float lookDirection = transform.localScale.x;
            bubbleScript.SetDirection(lookDirection);
        }
    }

    // (기존 Shoot 함수는 FireBubble로 이름을 바꾸거나 통합해서 관리하면 됩니다.)
}