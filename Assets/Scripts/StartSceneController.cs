using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        AddListener(playButton, OpenLevelSelection);
        AddListener(quitButton, Application.Quit);
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

    private static void OpenLevelSelection()
    {
        SceneManager.LoadScene(GameSession.LevelSelectionSceneName);
    }
}
