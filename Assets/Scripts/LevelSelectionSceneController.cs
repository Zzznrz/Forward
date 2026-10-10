using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Handles level selection independently from the title screen.</summary>
public class LevelSelectionSceneController : MonoBehaviour
{
    [SerializeField] private Button levelOneButton;
    [SerializeField] private Button levelTwoButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        AddListener(levelOneButton, () => StartLevel(1));
        AddListener(levelTwoButton, () => StartLevel(2));
        AddListener(backButton, BackToTitle);
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

    private static void StartLevel(int level)
    {
        GameSession.StartLevel(level);
        SceneManager.LoadScene(GameSession.GameSceneName);
    }

    private static void BackToTitle()
    {
        SceneManager.LoadScene(GameSession.StartSceneName);
    }
}
