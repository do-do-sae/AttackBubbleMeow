using UnityEngine;

public class CheatManager : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public int addSmall = 100;
    public int addBig = 1000;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            AddScore(addSmall);

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            AddScore(-addSmall);

        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            AddScore(addBig);
    }

    void AddScore(int amount)
    {
        if (UIManager.instance != null)
            UIManager.instance.AddScore(amount);
    }
#endif
}
