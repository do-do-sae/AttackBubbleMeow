using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Globalization;

public class ResultPanelUI : MonoBehaviour
{
    [Header("이번 점수")]
    public TextMeshProUGUI scoreText;

    [Header("Top10 텍스트(1~10 순서대로)")]
    public List<TextMeshProUGUI> rankTexts = new List<TextMeshProUGUI>();

    [Header("NEW RECORD 배지 오브젝트")]
    public GameObject newRecordBadge;

    public void Show(int lastScore)
    {
        // 1) 이번 점수 표시(콤마)
        if (scoreText != null)
            scoreText.text = lastScore.ToString("N0", CultureInfo.InvariantCulture);

        // 2) 랭킹 저장
        if (RankingManager.instance != null)
            RankingManager.instance.AddScore(lastScore);

        // 3) 랭킹 UI 그리기 + 내 점수 강조
        RefreshRankingUI(lastScore);

        // 4) NEW RECORD 배지(최고기록이면 켬)
        int best = PlayerPrefs.GetInt("BEST_SCORE", 0);
        bool isNew = (lastScore >= best) && lastScore > 0;

        if (newRecordBadge != null)
            newRecordBadge.SetActive(isNew);
    }

    void RefreshRankingUI(int lastScore)
    {
        if (rankTexts == null || rankTexts.Count == 0) return;

        List<int> list = (RankingManager.instance != null)
            ? RankingManager.instance.GetScores()
            : new List<int>();

        // 내 점수 강조는 "첫 번째로 같은 점수" 하나만
        bool highlighted = false;

        for (int i = 0; i < rankTexts.Count; i++)
        {
            if (rankTexts[i] == null) continue;

            if (i < list.Count)
            {
                int s = list[i];
                string line = $"{i + 1}. {s.ToString("N0", CultureInfo.InvariantCulture)}";

                if (!highlighted && s == lastScore && lastScore > 0)
                {
                    line = $"<color=#4169E1><b>★ {line}</b></color>";
                    highlighted = true;
                }

                rankTexts[i].text = line;
            }
            else
            {
                rankTexts[i].text = $"{i + 1}. ---";
            }
        }
    }

    // RESTART 버튼
    public void OnRestart()
    {
        GameManager.instance.RestartToStart();
    }

    // EXIT 버튼
    public void OnExit()
    {
        GameManager.instance.ExitGame();
    }
}
