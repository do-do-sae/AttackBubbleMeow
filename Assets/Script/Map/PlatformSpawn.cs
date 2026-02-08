using UnityEngine;

public class PatternSpawner : MonoBehaviour
{
    public GameObject[] patternPrefabs; // 조립된 패턴 프리팹들
    public float spawnInterval = 5f;    // 패턴 길이에 맞춰 간격을 조절하세요 
    public float spawnX = 15f;          // 화면 오른쪽 밖 소환 지점

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnPattern();
            timer = 0;
        }
    }

    void SpawnPattern()
    {
        if (patternPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, patternPrefabs.Length);

        // Y값을 0으로 고정하여 프리팹에 설정된 높이를 그대로 유지합니다.
        Vector3 spawnPos = new Vector3(spawnX, 0, 0);

        Instantiate(patternPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }
}