using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    public GameObject bubblePrefab;
    public Transform firePoint;

    // [추가] 타임라인 종료 후 활성화될 변수
    public bool canAttack = false;

    // 애니메이션 이벤트에서 호출되는 함수
    public void FireBubble()
    {
        // 게임 시작 전이면 버블 생성을 무시합니다.
        if (!canAttack || bubblePrefab == null || firePoint == null) return;

        GameObject bubbleObj = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Bubble bubbleScript = bubbleObj.GetComponent<Bubble>();

        if (bubbleScript != null)
        {
            float lookDirection = transform.localScale.x;
            bubbleScript.SetDirection(lookDirection);
        }
    }
}