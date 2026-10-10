using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates the scenes used by the finished game flow.
/// Run from Unity with: ForwardSceneBuilder.BuildSeparateScenes
/// </summary>
public static class ForwardSceneBuilder
{
    private const string SceneFolder = "Assets/Scenes";
    private const string OldGameScene = "Assets/Scenes/_Scene_0.unity";
    private const string StartScene = "Assets/Scenes/StartScene.unity";
    private const string LevelSelectionScene = "Assets/Scenes/LevelSelectionScene.unity";
    private const string GameScene = "Assets/Scenes/GameScene.unity";
    private const string EndScene = "Assets/Scenes/EndScene.unity";

    private static readonly Color Background = new Color(0.015f, 0.025f, 0.07f, 1f);
    private static readonly Color Panel = new Color(0.04f, 0.08f, 0.16f, 0.98f);
    private static readonly Color Accent = new Color(1f, 0.12f, 0.2f, 1f);
    private static readonly Color ButtonDark = new Color(0.07f, 0.13f, 0.24f, 1f);

    public static void BuildSeparateScenes()
    {
        EnsureFolder();
        CopyGameScene();
        CleanGameSceneMenus();
        CreateStartScene();
        CreateLevelSelectionScene();
        CreateEndScene();
        SetBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(StartScene, OpenSceneMode.Single);
        Debug.Log("Forward scenes built: StartScene, LevelSelectionScene, GameScene, EndScene.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(SceneFolder))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static void CopyGameScene()
    {
        if (!AssetDatabase.CopyAsset(OldGameScene, GameScene) && !System.IO.File.Exists(GameScene))
            Debug.LogError("Could not copy the existing game scene to GameScene.");
    }

    private static void CleanGameSceneMenus()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        GameObject canvas = GameObject.Find("ForwardCanvas");
        if (canvas == null)
            return;

        string[] menuObjects =
        {
            "TitleScreen",
            "LevelSelectionScreen",
            "HowToPlayScreen",
            "GameOverScreen",
            "VictoryScreen",
            "SciFiBackdrop"
        };

        foreach (string menuObject in menuObjects)
        {
            Transform child = canvas.transform.Find(menuObject);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScene);
    }

    private static void CreateStartScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateAudioCamera();
        Canvas canvas = CreateCanvas("StartCanvas");
        CreateBackground(canvas.transform, "StartBackground");
        GameObject panel = CreatePanel(canvas.transform, "StartPanel", new Vector2(720f, 700f));
        CreateText(panel.transform, "Title", "FORWARD", 86, new Vector2(0f, 220f), Accent);
        CreateText(panel.transform, "Subtitle", "KEEP MOVING", 25, new Vector2(0f, 145f), Color.white);

        Button play = CreateButton(panel.transform, "PLAY", new Vector2(0f, -5f), Accent);
        Button quit = CreateButton(panel.transform, "QUIT", new Vector2(0f, -115f), ButtonDark);

        GameObject controllerObject = new GameObject("StartSceneController");
        StartSceneController controller = controllerObject.AddComponent<StartSceneController>();
        controllerObject.AddComponent<AudioManager>();
        AssignStartReferences(controller, play, quit);
        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, StartScene);
    }

    private static void CreateLevelSelectionScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateAudioCamera();
        Canvas canvas = CreateCanvas("LevelSelectionCanvas");
        CreateBackground(canvas.transform, "LevelSelectionBackground");
        GameObject panel = CreatePanel(canvas.transform, "LevelSelectionPanel", new Vector2(760f, 720f));
        CreateText(panel.transform, "Title", "SELECT LEVEL", 64, new Vector2(0f, 245f), Accent);
        CreateText(panel.transform, "Subtitle", "CHOOSE YOUR RUN", 24, new Vector2(0f, 175f), Color.white);

        Button levelOne = CreateButton(panel.transform, "LEVEL 1   EASY", new Vector2(0f, 70f), Accent);
        Button levelTwo = CreateButton(panel.transform, "LEVEL 2   HARDER", new Vector2(0f, -40f), ButtonDark);
        Button back = CreateButton(panel.transform, "BACK", new Vector2(0f, -155f), ButtonDark);

        GameObject controllerObject = new GameObject("LevelSelectionSceneController");
        LevelSelectionSceneController controller = controllerObject.AddComponent<LevelSelectionSceneController>();
        controllerObject.AddComponent<AudioManager>();
        AssignLevelSelectionReferences(controller, levelOne, levelTwo, back);
        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, LevelSelectionScene);
    }

    private static void CreateEndScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateAudioCamera();
        Canvas canvas = CreateCanvas("EndCanvas");
        CreateBackground(canvas.transform, "EndBackground");
        GameObject panel = CreatePanel(canvas.transform, "EndPanel", new Vector2(760f, 620f));
        TMP_Text result = CreateText(panel.transform, "ResultText", "LEVEL COMPLETE", 62, new Vector2(0f, 160f), Accent);
        TMP_Text level = CreateText(panel.transform, "LevelText", "LEVEL 1", 28, new Vector2(0f, 85f), Color.white);
        Button retry = CreateButton(panel.transform, "RETRY", new Vector2(0f, -25f), Accent);
        Button mainMenu = CreateButton(panel.transform, "MAIN MENU", new Vector2(0f, -135f), ButtonDark);

        GameObject controllerObject = new GameObject("EndSceneController");
        EndSceneController controller = controllerObject.AddComponent<EndSceneController>();
        controllerObject.AddComponent<AudioManager>();
        AssignEndReferences(controller, result, level, retry, mainMenu);
        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, EndScene);
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateAudioCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.cullingMask = 0;
    }

    private static void CreateBackground(Transform parent, string name)
    {
        GameObject objectToCreate = new GameObject(name, typeof(RectTransform), typeof(Image));
        objectToCreate.transform.SetParent(parent, false);
        RectTransform rect = objectToCreate.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        objectToCreate.GetComponent<Image>().color = Background;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = Panel;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float size, Vector2 position, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(680f, 100f);
        rect.anchoredPosition = position;
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;
        return label;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = new GameObject(label.Replace(" ", ""), typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(520f, 86f);
        rect.anchoredPosition = position;
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(Mathf.Min(color.r + 0.12f, 1f), Mathf.Min(color.g + 0.12f, 1f), Mathf.Min(color.b + 0.12f, 1f), 1f);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f);
        button.colors = colors;
        TMP_Text buttonText = CreateText(buttonObject.transform, "Label", label, 28, Vector2.zero, Color.white);
        buttonText.rectTransform.sizeDelta = new Vector2(500f, 80f);
        return button;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static void AssignStartReferences(StartSceneController controller, Button play, Button quit)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("playButton").objectReferenceValue = play;
        serialized.FindProperty("quitButton").objectReferenceValue = quit;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignLevelSelectionReferences(LevelSelectionSceneController controller, Button levelOne, Button levelTwo, Button back)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("levelOneButton").objectReferenceValue = levelOne;
        serialized.FindProperty("levelTwoButton").objectReferenceValue = levelTwo;
        serialized.FindProperty("backButton").objectReferenceValue = back;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignEndReferences(EndSceneController controller, TMP_Text result, TMP_Text level, Button retry, Button mainMenu)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("resultText").objectReferenceValue = result;
        serialized.FindProperty("levelText").objectReferenceValue = level;
        serialized.FindProperty("retryButton").objectReferenceValue = retry;
        serialized.FindProperty("mainMenuButton").objectReferenceValue = mainMenu;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(StartScene, true),
            new EditorBuildSettingsScene(LevelSelectionScene, true),
            new EditorBuildSettingsScene(GameScene, true),
            new EditorBuildSettingsScene(EndScene, true)
        };
    }
}
