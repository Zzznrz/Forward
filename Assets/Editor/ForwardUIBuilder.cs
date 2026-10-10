using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Creates the real Forward uGUI hierarchy in the active scene.
/// The command is safe to run repeatedly: it rebuilds only ForwardCanvas.
/// </summary>
public static class ForwardUIBuilder
{
    private const string CanvasName = "ForwardCanvas";
    private const string TitleScreenName = "TitleScreen";
    private const string LevelScreenName = "LevelSelectionScreen";
    private const string HowToScreenName = "HowToPlayScreen";
    private const string PauseScreenName = "PauseScreen";
    private const string GameOverScreenName = "GameOverScreen";
    private const string VictoryScreenName = "VictoryScreen";

    private static readonly Color PanelColor = new Color(0.09f, 0.12f, 0.19f, 0.98f);
    private static readonly Color Red = new Color(1f, 0.2f, 0.31f, 1f);
    private static readonly Color Blue = new Color(0.32f, 0.39f, 0.48f, 1f);
    private static readonly Color TextColor = new Color(0.95f, 0.96f, 0.98f, 1f);
    private static readonly Color SecondaryText = new Color(0.65f, 0.71f, 0.80f, 1f);

    [MenuItem("Tools/Forward/Build Game UI")]
    public static void BuildGameUI()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog(
                "TextMeshPro resources missing",
                "Import TMP Essential Resources from Window > TextMeshPro > Import TMP Essential Resources, then run this command again.",
                "OK");
            return;
        }

        Canvas canvas = GetOrCreateCanvas();
        ClearGeneratedChildren(canvas.transform);

        UIManager uiManager = canvas.GetComponent<UIManager>();
        if (uiManager == null)
            uiManager = canvas.gameObject.AddComponent<UIManager>();

        Dictionary<string, Object> references = new Dictionary<string, Object>();
        BuildBackdrop(canvas.transform);
        BuildTitleScreen(canvas.transform, references);
        BuildLevelSelectionScreen(canvas.transform, references);
        BuildHowToPlayScreen(canvas.transform, references);
        BuildPauseScreen(canvas.transform, references);
        BuildGameOverScreen(canvas.transform, references);
        BuildVictoryScreen(canvas.transform, references);
        BuildHud(canvas.transform, references);
        AssignReferences(uiManager, references);
        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = canvas.gameObject;

        Debug.Log("Forward UI built successfully. Run the scene to preview it.");
    }

    private static Canvas GetOrCreateCanvas()
    {
        GameObject canvasObject = GameObject.Find(CanvasName);
        if (canvasObject == null)
        {
            canvasObject = new GameObject(CanvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        Canvas canvasComponent = canvasObject.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 100;
        canvasObject.transform.localPosition = Vector3.zero;
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvasComponent;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem");
            eventSystem = eventObject.AddComponent<EventSystem>();
        }

        StandaloneInputModule oldInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldInputModule != null)
            Object.DestroyImmediate(oldInputModule);

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
    }

    private static void ClearGeneratedChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static void BuildTitleScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, TitleScreenName, new Vector2(760f, 470f), Red, new Vector2(-390f, 0f));
        CreateText(screen.transform, "Title", "FORWARD", 74, TextColor, new Vector2(0f, 130f), new Vector2(680f, 90f));
        CreateText(screen.transform, "Subtitle", "KEEP MOVING", 24, SecondaryText, new Vector2(0f, 55f), new Vector2(680f, 38f));
        CreateText(screen.transform, "Rule", "--------------------", 20, Red, new Vector2(0f, 15f), new Vector2(680f, 32f));

        references["playButton"] = CreateButton(screen.transform, "PlayButton", "PLAY", true, new Vector2(0f, -80f), new Vector2(560f, 74f));
        references["howToPlayButton"] = CreateButton(screen.transform, "HowToPlayButton", "HOW TO PLAY", false, new Vector2(0f, -170f), new Vector2(560f, 58f));
        references["quitButton"] = CreateButton(screen.transform, "QuitButton", "QUIT", false, new Vector2(0f, -240f), new Vector2(560f, 58f));
        CreateText(screen.transform, "Controls", "SPACE / UP  JUMP       S / DOWN  SLIDE       ESC  PAUSE", 17, SecondaryText, new Vector2(0f, -310f), new Vector2(680f, 32f));
        references["titleScreen"] = screen.GetComponent<CanvasGroup>();

        GameObject preview = CreateImageChild(screen.transform, "PreviewCard", new Color(0.04f, 0.08f, 0.14f, 0.92f));
        RectTransform previewRect = preview.GetComponent<RectTransform>();
        SetRect(previewRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 10f), new Vector2(620f, 350f));
        Outline previewOutline = preview.AddComponent<Outline>();
        previewOutline.effectColor = new Color(Red.r, Red.g, Red.b, 0.65f);
        previewOutline.effectDistance = new Vector2(2f, -2f);
        CreateText(preview.transform, "PreviewTitle", "RUN // SECTOR 01", 20, SecondaryText, new Vector2(0f, 125f), new Vector2(520f, 32f));
        CreateText(preview.transform, "PreviewDistance", "DISTANCE     128 M", 25, TextColor, new Vector2(0f, 78f), new Vector2(520f, 38f));
        CreateText(preview.transform, "PreviewHint", "JUMP     SLIDE     SURVIVE", 17, Red, new Vector2(0f, -140f), new Vector2(520f, 30f));
    }

    private static void BuildBackdrop(Transform canvas)
    {
        GameObject backdrop = new GameObject("SciFiBackdrop");
        backdrop.transform.SetParent(canvas, false);
        RectTransform backdropRect = backdrop.AddComponent<RectTransform>();
        SetRect(backdropRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);

        GameObject background = CreateImageChild(backdrop.transform, "Background", new Color(0.043f, 0.059f, 0.098f, 1f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        SetRect(backgroundRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
        background.GetComponent<Image>().raycastTarget = false;

        for (int index = 0; index < 12; index++)
        {
            float x = -900f + index * 165f;
            GameObject skyline = CreateImageChild(backdrop.transform, $"Skyline{index:00}", new Color(0.07f, 0.11f, 0.18f, 1f));
            RectTransform skylineRect = skyline.GetComponent<RectTransform>();
            float height = 140f + (index % 4) * 75f;
            SetRect(skylineRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(x, -410f), new Vector2(125f, height));
            skyline.GetComponent<Image>().raycastTarget = false;
        }

        GameObject horizon = CreateImageChild(backdrop.transform, "RedHorizon", new Color(Red.r, Red.g, Red.b, 0.16f));
        SetRect(horizon.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, -205f), new Vector2(0f, 3f));
        horizon.GetComponent<Image>().raycastTarget = false;

        for (int index = 0; index < 8; index++)
        {
            GameObject gridLine = CreateImageChild(backdrop.transform, $"GridLine{index:00}", new Color(0.32f, 0.39f, 0.48f, 0.12f));
            SetRect(gridLine.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, -170f + index * 48f), new Vector2(0f, 1f));
            gridLine.GetComponent<Image>().raycastTarget = false;
        }
    }

    private static void BuildLevelSelectionScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, LevelScreenName, new Vector2(860f, 460f), Blue);
        CreateText(screen.transform, "Title", "SELECT LEVEL", 52, TextColor, new Vector2(0f, 140f), new Vector2(780f, 70f));
        CreateText(screen.transform, "Subtitle", "CHOOSE YOUR ROUTE", 21, SecondaryText, new Vector2(0f, 82f), new Vector2(780f, 34f));
        references["levelOneButton"] = CreateButton(screen.transform, "LevelOneButton", "LEVEL 1\n\nEASY", true, new Vector2(-190f, -45f), new Vector2(330f, 145f));
        references["levelTwoButton"] = CreateButton(screen.transform, "LevelTwoButton", "LEVEL 2\n\nHARDER", false, new Vector2(190f, -45f), new Vector2(330f, 145f));
        references["levelBackButton"] = CreateButton(screen.transform, "LevelBackButton", "BACK", false, new Vector2(0f, -180f), new Vector2(260f, 54f));
        references["levelSelectionScreen"] = screen.GetComponent<CanvasGroup>();
    }

    private static void BuildHowToPlayScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, HowToScreenName, new Vector2(820f, 520f), Red);
        CreateText(screen.transform, "Title", "HOW TO PLAY", 52, TextColor, new Vector2(0f, 190f), new Vector2(760f, 70f));
        CreateText(screen.transform, "Controls", "AUTO RUN\nThe character moves automatically.\n\nSPACE / UP\nJump over gaps and platforms.\n\nS / DOWN\nSlide under low obstacles.\n\nA / D\nAdjust your landing point in the air.\n\nESC\nPause the game.", 22, SecondaryText, new Vector2(0f, -5f), new Vector2(650f, 360f));
        references["howToPlayBackButton"] = CreateButton(screen.transform, "HowToPlayBackButton", "BACK", false, new Vector2(0f, -205f), new Vector2(260f, 54f));
        references["howToPlayScreen"] = screen.GetComponent<CanvasGroup>();
    }

    private static void BuildPauseScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, PauseScreenName, new Vector2(600f, 440f), Blue);
        CreateText(screen.transform, "Title", "PAUSED", 48, TextColor, new Vector2(0f, 145f), new Vector2(540f, 65f));
        references["resumeButton"] = CreateButton(screen.transform, "ResumeButton", "RESUME", true, new Vector2(0f, 55f), new Vector2(360f, 62f));
        references["restartButton"] = CreateButton(screen.transform, "RestartButton", "RESTART", false, new Vector2(0f, -20f), new Vector2(360f, 56f));
        references["pauseMainMenuButton"] = CreateButton(screen.transform, "PauseMainMenuButton", "MAIN MENU", false, new Vector2(0f, -90f), new Vector2(360f, 56f));
        references["pauseScreen"] = screen.GetComponent<CanvasGroup>();
    }

    private static void BuildGameOverScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, GameOverScreenName, new Vector2(700f, 430f), Red);
        CreateText(screen.transform, "Title", "GAME OVER", 48, Red, new Vector2(0f, 140f), new Vector2(640f, 65f));
        CreateText(screen.transform, "Subtitle", "KEEP GOING. YOU CAN DO IT!", 20, SecondaryText, new Vector2(0f, 78f), new Vector2(640f, 34f));
        references["gameOverRetryButton"] = CreateButton(screen.transform, "GameOverRetryButton", "RETRY", true, new Vector2(-120f, -75f), new Vector2(250f, 58f));
        references["gameOverMainMenuButton"] = CreateButton(screen.transform, "GameOverMainMenuButton", "MAIN MENU", false, new Vector2(150f, -75f), new Vector2(250f, 58f));
        references["gameOverScreen"] = screen.GetComponent<CanvasGroup>();
    }

    private static void BuildVictoryScreen(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject screen = CreateScreen(canvas, "VictoryScreen", new Vector2(760f, 450f), Red);
        CreateText(screen.transform, "Title", "LEVEL COMPLETE", 46, TextColor, new Vector2(0f, 145f), new Vector2(700f, 65f));
        references["victoryTimeText"] = CreateText(screen.transform, "VictoryTime", "TIME  00:00.0", 23, SecondaryText, new Vector2(-170f, 55f), new Vector2(280f, 36f));
        references["victoryDistanceText"] = CreateText(screen.transform, "VictoryDistance", "DISTANCE  000 M", 23, SecondaryText, new Vector2(170f, 55f), new Vector2(280f, 36f));
        references["victoryPlayAgainButton"] = CreateButton(screen.transform, "VictoryPlayAgainButton", "PLAY AGAIN", true, new Vector2(-150f, -85f), new Vector2(280f, 60f));
        references["victoryMainMenuButton"] = CreateButton(screen.transform, "VictoryMainMenuButton", "MAIN MENU", false, new Vector2(150f, -85f), new Vector2(280f, 60f));
        references["victoryScreen"] = screen.GetComponent<CanvasGroup>();
    }

    private static void BuildHud(Transform canvas, Dictionary<string, Object> references)
    {
        GameObject hud = new GameObject("GameplayHUD");
        hud.transform.SetParent(canvas, false);
        RectTransform rect = hud.AddComponent<RectTransform>();
        SetRect(rect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, -42f), new Vector2(-48f, -24f));
        CanvasGroup group = hud.AddComponent<CanvasGroup>();
        references["hud"] = group;

        references["timeText"] = CreateText(hud.transform, "TimeText", "TIME  00:00.0", 22, TextColor, new Vector2(170f, -35f), new Vector2(300f, 44f));
        references["distanceText"] = CreateText(hud.transform, "DistanceText", "DISTANCE  000 M", 22, TextColor, new Vector2(500f, -35f), new Vector2(300f, 44f));
        references["pauseButton"] = CreateButton(hud.transform, "PauseButton", "PAUSE", false, new Vector2(-130f, -35f), new Vector2(120f, 52f));

        GameObject progressObject = new GameObject("ProgressBar");
        progressObject.transform.SetParent(hud.transform, false);
        RectTransform progressRect = progressObject.AddComponent<RectTransform>();
        SetRect(progressRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, -76f), new Vector2(520f, 10f));
        Slider progress = progressObject.AddComponent<Slider>();
        progress.minValue = 0f;
        progress.maxValue = 1f;
        progress.value = 0f;
        GameObject background = CreateImageChild(progressObject.transform, "Background", new Color(0.08f, 0.12f, 0.20f, 0.95f));
        SetRect(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
        GameObject fill = CreateImageChild(progressObject.transform, "Fill", Red);
        SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
        progress.fillRect = fill.GetComponent<RectTransform>();
        references["progressBar"] = progress;
    }

    private static GameObject CreateScreen(Transform canvas, string name, Vector2 size, Color outlineColor, Vector2 panelPosition = default)
    {
        GameObject screen = new GameObject(name);
        screen.transform.SetParent(canvas, false);
        RectTransform screenRect = screen.AddComponent<RectTransform>();
        SetRect(screenRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
        CanvasGroup group = screen.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        GameObject panel = CreateImageChild(screen.transform, "Panel", PanelColor);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, panelPosition, size);
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(2f, -2f);
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(0f, -8f);
        return screen;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, int fontSize, Color color, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, position, size);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.fontStyle = FontStyles.Bold;
        return label;
    }

    private static Button CreateButton(Transform parent, string name, string text, bool primary, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = CreateImageChild(parent, name, primary ? Red : new Color(0.05f, 0.10f, 0.18f, 0.98f));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, position, size);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        CreateText(buttonObject.transform, "Label", text, primary ? 25 : 20, TextColor, Vector2.zero, Vector2.zero);
        RectTransform labelRect = buttonObject.transform.Find("Label").GetComponent<RectTransform>();
        SetRect(labelRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Vector2.zero);
        return button;
    }

    private static GameObject CreateImageChild(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.offsetMin = offsetMin;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void AssignReferences(UIManager manager, Dictionary<string, Object> references)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        foreach (KeyValuePair<string, Object> reference in references)
        {
            SerializedProperty property = serializedManager.FindProperty(reference.Key);
            if (property != null)
                property.objectReferenceValue = reference.Value;
        }
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);
    }
}
