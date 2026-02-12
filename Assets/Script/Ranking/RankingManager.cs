using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RankingManager : MonoBehaviour
{
    public static RankingManager instance;

    private List<int> scores = new List<int>();
    private const string SAVE_KEY = "RANK_SCORES";

    void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void AddScore(int score)
    {
        // 0점 저장이 싫으면 여기서 막아도 됨
        // if (score <= 0) return;

        scores.Add(score);

        scores = scores
            .OrderByDescending(s => s)
            .Take(10)
            .ToList();

        Save();
    }

    public List<int> GetScores()
    {
        return new List<int>(scores);
    }

    public void Reload()
    {
        Load();
    }

    void Save()
    {
        string data = string.Join(",", scores);
        PlayerPrefs.SetString(SAVE_KEY, data);
        PlayerPrefs.Save();
    }

    void Load()
    {
        scores.Clear();

        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string data = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(data)) return;

        string[] split = data.Split(',');
        foreach (string s in split)
        {
            if (int.TryParse(s, out int v))
                scores.Add(v);
        }

        scores = scores
            .OrderByDescending(x => x)
            .Take(10)
            .ToList();
    }
}
