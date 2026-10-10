using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the generated uGUI screens and keeps them synchronized with LevelBuilder.
/// The editor builder assigns all references; no manual Inspector wiring is required.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private CanvasGroup titleScreen;
    [SerializeField] private CanvasGroup levelSelectionScreen;
    [SerializeField] private CanvasGroup howToPlayScreen;
    [SerializeField] private CanvasGroup pauseScreen;
    [SerializeField] private CanvasGroup gameOverScreen;
    [SerializeField] private CanvasGroup victoryScreen;

    [Header("HUD")]
    [SerializeField] private CanvasGroup hud;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text victoryTimeText;
    [SerializeField] private TMP_Text victoryDistanceText;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button levelOneButton;
    [SerializeField] private Button levelTwoButton;
    [SerializeField] private Button levelBackButton;
    [SerializeField] private Button howToPlayBackButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button gameOverRetryButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private Button victoryPlayAgainButton;
    [SerializeField] private Button victoryMainMenuButton;

    private LevelBuilder levelBuilder;
    private Player player;
    private bool showingHowToPlay;
    private bool showingGameOver;
    private float elapsedTime;
    private readonly List<GameObject> backdropObjects = new List<GameObject>();

    private void Awake()
    {
        AudioManager.GetOrCreate();
        AutoWireReferences();
        levelBuilder = FindAnyObjectByType<LevelBuilder>();
        player = FindAnyObjectByType<Player>();
        CacheBackdropObjects();
        if (player != null)
            player.Died += OnPlayerDied;
        WireButtons();
        Time.timeScale = 1f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoWireReferences();
    }
#endif

    private void AutoWireReferences()
    {
        titleScreen = FindChildComponent<CanvasGroup>("TitleScreen");
        levelSelectionScreen = FindChildComponent<CanvasGroup>("LevelSelectionScreen");
        howToPlayScreen = FindChildComponent<CanvasGroup>("HowToPlayScreen");
        pauseScreen = FindChildComponent<CanvasGroup>("PauseScreen");
        gameOverScreen = FindChildComponent<CanvasGroup>("GameOverScreen");
        victoryScreen = FindChildComponent<CanvasGroup>("VictoryScreen");

        hud = FindChildComponent<CanvasGroup>("GameplayHUD");
        timeText = FindChildComponent<TMP_Text>("GameplayHUD/TimeText");
        distanceText = FindChildComponent<TMP_Text>("GameplayHUD/DistanceText");
        progressBar = FindChildComponent<Slider>("GameplayHUD/ProgressBar");
        victoryTimeText = FindChildComponent<TMP_Text>("VictoryScreen/VictoryTime");
        victoryDistanceText = FindChildComponent<TMP_Text>("VictoryScreen/VictoryDistance");

        playButton = FindChildComponent<Button>("TitleScreen/PlayButton");
        howToPlayButton = FindChildComponent<Button>("TitleScreen/HowToPlayButton");
        quitButton = FindChildComponent<Button>("TitleScreen/QuitButton");
        levelOneButton = FindChildComponent<Button>("LevelSelectionScreen/LevelOneButton");
        levelTwoButton = FindChildComponent<Button>("LevelSelectionScreen/LevelTwoButton");
        levelBackButton = FindChildComponent<Button>("LevelSelectionScreen/LevelBackButton");
        howToPlayBackButton = FindChildComponent<Button>("HowToPlayScreen/HowToPlayBackButton");
        pauseButton = FindChildComponent<Button>("GameplayHUD/PauseButton");
        resumeButton = FindChildComponent<Button>("PauseScreen/ResumeButton");
        restartButton = FindChildComponent<Button>("PauseScreen/RestartButton");
        pauseMainMenuButton = FindChildComponent<Button>("PauseScreen/PauseMainMenuButton");
        gameOverRetryButton = FindChildComponent<Button>("GameOverScreen/GameOverRetryButton");
        gameOverMainMenuButton = FindChildComponent<Button>("GameOverScreen/GameOverMainMenuButton");
        victoryPlayAgainButton = FindChildComponent<Button>("VictoryScreen/VictoryPlayAgainButton");
        victoryMainMenuButton = FindChildComponent<Button>("VictoryScreen/VictoryMainMenuButton");
    }

    private T FindChildComponent<T>(string childPath) where T : Component
    {
        Transform child = transform.Find(childPath);
        return child != null ? child.GetComponent<T>() : null;
    }

    private void OnDestroy()
    {
        if (player != null)
            player.Died -= OnPlayerDied;
    }

    private void Start()
    {
        RefreshPanels();
    }

    private void Update()
    {
        if (levelBuilder == null)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Playing)
                levelBuilder.PauseFromUI();
            else if (levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Paused)
                levelBuilder.ResumeFromUI();
        }

        if (levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Playing)
        {
            elapsedTime += Time.deltaTime;
            UpdateHud();
        }
    }

    public void RefreshPanels()
    {
        if (levelBuilder == null)
            levelBuilder = FindAnyObjectByType<LevelBuilder>();

        if (levelBuilder == null)
            return;

        bool titleVisible = levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Title && !showingHowToPlay && !showingGameOver;
        bool levelVisible = levelBuilder.CurrentScreen == LevelBuilder.ScreenState.LevelSelection && !showingHowToPlay && !showingGameOver;
        bool pauseVisible = levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Paused && !showingHowToPlay && !showingGameOver;
        bool victoryVisible = levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Completed && !showingHowToPlay && !showingGameOver;
        bool hudVisible = (levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Playing || pauseVisible) && !showingGameOver;
        bool gameplayVisible = levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Playing ||
                               levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Paused ||
                               levelBuilder.CurrentScreen == LevelBuilder.ScreenState.Completed ||
                               showingGameOver;

        SetVisible(titleScreen, titleVisible);
        SetVisible(levelSelectionScreen, levelVisible);
        SetVisible(howToPlayScreen, showingHowToPlay);
        SetVisible(pauseScreen, pauseVisible);
        SetVisible(gameOverScreen, showingGameOver);
        SetVisible(victoryScreen, victoryVisible);
        SetVisible(hud, hudVisible);
        SetBackdropVisible(!gameplayVisible);
        UpdateHud();

        if (victoryVisible)
            Time.timeScale = 0f;
    }

    private void UpdateHud()
    {
        if (player == null)
            player = FindAnyObjectByType<Player>();

        if (player == null || levelBuilder == null)
            return;

        float distance = Mathf.Max(0f, player.transform.position.x - levelBuilder.StartPosition);
        float totalDistance = Mathf.Max(1f, levelBuilder.FinishPosition - levelBuilder.StartPosition);

        string formattedTime = FormatTime(elapsedTime);
        if (timeText != null)
            timeText.text = $"TIME  {formattedTime}";
        if (distanceText != null)
            distanceText.text = $"DISTANCE  {distance:000} M";
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(distance / totalDistance);
        if (victoryTimeText != null)
            victoryTimeText.text = $"TIME  {formattedTime}";
        if (victoryDistanceText != null)
            victoryDistanceText.text = $"DISTANCE  {distance:000} M";
    }

    private void SetVisible(CanvasGroup group, bool visible)
    {
        if (group == null)
            return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private void CacheBackdropObjects()
    {
        AddBackdropObject("SciFiBackdrop");

        // These names support scenes generated by both the old and new UI builder.
        AddBackdropObject("Background");
        AddBackdropObject("RedHorizon");
        for (int index = 0; index < 12; index++)
        {
            AddBackdropObject($"Skyline{index:00}");
            AddBackdropObject($"GridLine{index:00}");
        }
    }

    private void AddBackdropObject(string objectName)
    {
        GameObject backdropObject = GameObject.Find(objectName);
        if (backdropObject != null && !backdropObjects.Contains(backdropObject))
            backdropObjects.Add(backdropObject);
    }

    private void SetBackdropVisible(bool visible)
    {
        foreach (GameObject backdropObject in backdropObjects)
        {
            if (backdropObject != null)
                backdropObject.SetActive(visible);
        }
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds % 60f;
        return $"{minutes:00}:{remainingSeconds:00.0}";
    }

    private void WireButtons()
    {
        AddListener(playButton, () => levelBuilder.BeginLevelSelection());
        AddListener(howToPlayButton, ShowHowToPlay);
        AddListener(quitButton, QuitGame);
        AddListener(levelOneButton, () => levelBuilder.SelectLevelFromUI(1));
        AddListener(levelTwoButton, () => levelBuilder.SelectLevelFromUI(2));
        AddListener(levelBackButton, BackToTitle);
        AddListener(howToPlayBackButton, BackToTitle);
        AddListener(pauseButton, () => levelBuilder.PauseFromUI());
        AddListener(resumeButton, () => levelBuilder.ResumeFromUI());
        AddListener(restartButton, () => levelBuilder.RestartFromUI());
        AddListener(pauseMainMenuButton, BackToTitle);
        AddListener(gameOverRetryButton, () => levelBuilder.RestartFromUI());
        AddListener(gameOverMainMenuButton, BackToTitle);
        AddListener(victoryPlayAgainButton, () => levelBuilder.RestartFromUI());
        AddListener(victoryMainMenuButton, BackToTitle);
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            AudioManager.GetOrCreate().PlayClick();
            action();
        });
    }

    private void ShowHowToPlay()
    {
        showingHowToPlay = true;
        if (player != null)
            player.SetMenuPaused(true);
        RefreshPanels();
    }

    private void OnPlayerDied()
    {
        // Death is a checkpoint respawn, not a level-ending state.
        // Player handles the short death pause and returns to the latest checkpoint.
        showingGameOver = false;
        Time.timeScale = 1f;
        RefreshPanels();
    }

    private void BackToTitle()
    {
        showingHowToPlay = false;
        showingGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameSession.StartSceneName);
    }

    private static void QuitGame()
    {
        Application.Quit();
    }
}
