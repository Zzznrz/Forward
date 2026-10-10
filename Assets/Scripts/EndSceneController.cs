using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndSceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        if (resultText != null)
            resultText.text = GameSession.LevelCompleted ? "LEVEL COMPLETE" : "GAME OVER";
        if (levelText != null)
            levelText.text = $"LEVEL {GameSession.SelectedLevel}";

        AddListener(retryButton, Retry);
        AddListener(mainMenuButton, MainMenu);
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.AddListener(() =>
        {
            AudioManager.GetOrCreate().PlayClick();
            action();
        });
    }

    private static void Retry()
    {
        GameSession.StartLevel(GameSession.SelectedLevel);
        SceneManager.LoadScene(GameSession.GameSceneName);
    }

    private static void MainMenu()
    {
        SceneManager.LoadScene(GameSession.StartSceneName);
    }
}
