using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the runtime platforms, checkpoints, finish line, and player visuals.
/// </summary>
[ExecuteAlways]
public class LevelBuilder : MonoBehaviour
{
    public enum ScreenState
    {
        Title,
        LevelSelection,
        Playing,
        Paused,
        Completed
    }

    private const float StartX = -7.3f;
    private const float FinishX = 149.5f;

    [Header("Level Visuals")]
    [SerializeField] private Color platformColor = new Color(0.56f, 0.56f, 0.56f);
    [SerializeField] private Color outlineColor = new Color(1f, 0.04f, 0.12f);
    [SerializeField] private Color faceColor = new Color(0.24f, 0.03f, 0.04f);

    private static Sprite squareSprite;
    private static PhysicsMaterial2D noFrictionMaterial;
    private bool applyingLevel;
    private ScreenState screenState;

    public ScreenState CurrentScreen => screenState;
    public float StartPosition => StartX;
    public float FinishPosition => FinishX;

    private void Awake()
    {
        UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.black;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
        }

        BuildPlatforms(1);
        StylePlayer();
        BuildCheckpoints(1);
        CreateFinishZone();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        Time.timeScale = 1f;
        screenState = ScreenState.Title;
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
            player.SetMenuPaused(true);
    }

    private void BuildPlatforms(int level)
    {
        if (level == 2)
        {
            BuildLevelTwoPlatforms();
            return;
        }

        // Roughly 150 world units at the default 5 u/s run speed: about 30 seconds.
        // Gaps are intentional and roofs mark the sections that require sliding.
        CreatePlatform("LeftBlock", -5.4f, -2.2f, 6.8f, 3.0f);
        CreatePlatform("HighBlock", -1.25f, -1.9f, 2.0f, 3.6f);
        CreatePlatform("MiddleBlock", 3.0f, -2.35f, 4.0f, 2.7f);
        CreatePlatform("SmallStep", 9.0f, -2.35f, 8.0f, 2.7f);
        CreatePlatform("FarBlock", 18.0f, -2.35f, 4.0f, 2.7f);
        CreatePlatform("FinalStep", 22.0f, -1.8f, 2.5f, 3.6f);
        CreatePlatform("FinishBlock", 27.0f, -2.35f, 5.0f, 2.7f);

        CreatePlatform("SlideFloor01", 35.0f, -2.35f, 9.0f, 2.7f);
        CreatePlatform("SlideRoof01", 35.0f, 1.0f, 7.0f, 1.7f);
        CreatePlatform("JumpFloor01", 46.0f, -2.35f, 6.0f, 2.7f);
        CreatePlatform("JumpStep01", 52.0f, -1.8f, 2.5f, 3.6f);
        CreatePlatform("SlideFloor02", 58.0f, -2.35f, 6.0f, 2.7f);
        CreatePlatform("SlideRoof02", 58.0f, 1.0f, 4.5f, 1.7f);

        CreatePlatform("GapFloor01", 68.0f, -2.35f, 9.0f, 2.7f);
        CreatePlatform("GapFloor02", 79.0f, -2.35f, 6.0f, 2.7f);
        CreatePlatform("JumpStep02", 86.0f, -1.8f, 2.5f, 3.6f);
        CreatePlatform("SlideFloor03", 93.0f, -2.35f, 6.0f, 2.7f);
        CreatePlatform("SlideRoof03", 93.0f, 1.0f, 4.5f, 1.7f);

        CreatePlatform("GapFloor03", 103.0f, -2.35f, 9.0f, 2.7f);
        CreatePlatform("GapFloor04", 115.0f, -2.35f, 8.0f, 2.7f);
        CreatePlatform("JumpStep03", 124.0f, -1.8f, 3.0f, 3.6f);
        CreatePlatform("GapFloor05", 132.0f, -2.35f, 7.0f, 2.7f);
        CreatePlatform("FinalRun", 143.0f, -2.35f, 15.0f, 2.7f);
    }

    private void BuildLevelTwoPlatforms()
    {
        // Level 2 keeps the same readable silhouette but uses shorter platforms,
        // tighter landings, more roofs, and longer rhythm changes.
        CreatePlatform("LeftBlock", -5.4f, -2.2f, 6.8f, 3.0f);
        CreatePlatform("HighBlock", -1.0f, -1.8f, 1.8f, 3.8f);
        CreatePlatform("MiddleBlock", 3.2f, -2.35f, 3.0f, 2.7f);
        CreatePlatform("SmallStep", 7.5f, -1.8f, 1.8f, 3.8f);
        CreatePlatform("FarBlock", 12.0f, -2.35f, 3.0f, 2.7f);
        CreatePlatform("SlideFloor01", 18.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideRoof01", 18.0f, 1.0f, 4.0f, 1.7f);
        CreatePlatform("JumpFloor01", 25.0f, -2.35f, 3.5f, 2.7f);
        CreatePlatform("JumpStep01", 30.0f, -1.75f, 2.0f, 3.9f);
        CreatePlatform("SlideFloor02", 36.0f, -2.35f, 4.5f, 2.7f);
        CreatePlatform("SlideRoof02", 36.0f, 1.0f, 3.5f, 1.7f);
        CreatePlatform("GapFloor01", 43.0f, -2.35f, 4.0f, 2.7f);
        CreatePlatform("JumpStep02", 49.0f, -1.75f, 2.0f, 3.9f);
        CreatePlatform("GapFloor02", 55.0f, -2.35f, 4.5f, 2.7f);
        CreatePlatform("SlideFloor03", 63.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideRoof03", 63.0f, 1.0f, 4.0f, 1.7f);
        CreatePlatform("GapFloor03", 71.0f, -2.35f, 4.0f, 2.7f);
        CreatePlatform("JumpStep03", 77.0f, -1.75f, 2.0f, 3.9f);
        CreatePlatform("GapFloor04", 82.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideFloor04", 89.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideRoof04", 89.0f, 1.0f, 4.0f, 1.7f);
        CreatePlatform("GapFloor05", 96.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("JumpStep04", 102.5f, -1.75f, 2.0f, 3.9f);
        CreatePlatform("GapFloor06", 108.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideFloor05", 116.0f, -2.35f, 5.0f, 2.7f);
        CreatePlatform("SlideRoof05", 116.0f, 1.0f, 4.0f, 1.7f);
        CreatePlatform("GapFloor07", 124.0f, -2.35f, 7.0f, 2.7f);
        CreatePlatform("FinalRun", 139.0f, -2.35f, 23.0f, 2.7f);
    }

    private void CreatePlatform(string platformName, float x, float y, float width, float height)
    {
        GameObject platform = transform.Find(platformName)?.gameObject;
        if (platform == null)
        {
            platform = new GameObject(platformName);
            platform.transform.SetParent(transform, true);
            platform.AddComponent<SpriteRenderer>();
            platform.AddComponent<BoxCollider2D>();
        }

        platform.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
        renderer.sprite = GetSquareSprite();
        renderer.color = platformColor;

        BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = GetNoFrictionMaterial();
        platform.transform.localScale = new Vector3(width, height, 1f);
    }

    private static PhysicsMaterial2D GetNoFrictionMaterial()
    {
        if (noFrictionMaterial != null)
            return noFrictionMaterial;

        noFrictionMaterial = new PhysicsMaterial2D("PlatformNoFriction")
        {
            friction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine2D.Minimum,
            bounceCombine = PhysicsMaterialCombine2D.Minimum
        };
        return noFrictionMaterial;
    }

    private void StylePlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
            return;

        player.transform.position = new Vector3(StartX, -0.2f, 0f);
        player.transform.localScale = Vector3.one;

        SpriteRenderer body = player.GetComponent<SpriteRenderer>();
        if (body != null)
            body.color = Color.white;

        SpriteRenderer visualBody = player.transform.Find("Visual")?.GetComponent<SpriteRenderer>();
        if (visualBody != null)
            visualBody.color = Color.white;

        Transform faceParent = player.transform.Find("Visual") ?? player.transform;
        if (faceParent.Find("RedOutline") == null)
        {
            CreateFacePart(faceParent, "RedOutline", Vector3.zero, new Vector2(1.16f, 1.16f), outlineColor, -1);
            CreateFacePart(faceParent, "LeftEye", new Vector3(-0.22f, 0.18f, -0.2f), new Vector2(0.11f, 0.23f), faceColor, 1);
            CreateFacePart(faceParent, "RightEye", new Vector3(0.22f, 0.18f, -0.2f), new Vector2(0.11f, 0.23f), faceColor, 1);
            CreateFacePart(faceParent, "Mouth", new Vector3(0f, -0.2f, -0.2f), new Vector2(0.28f, 0.13f), faceColor, 1);
        }
    }

    private void CreateFinishZone()
    {
        GameObject finish = transform.Find("FinishZone")?.gameObject;
        if (finish == null)
        {
            finish = new GameObject("FinishZone");
            finish.transform.SetParent(transform, true);
            finish.AddComponent<BoxCollider2D>();
            finish.AddComponent<FinishZone>();

            // A simple red finish marker keeps the goal readable without adding art assets.
            GameObject marker = new GameObject("FinishMarker");
            marker.transform.SetParent(finish.transform, false);
            marker.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            marker.transform.localScale = new Vector3(0.12f, 2.1f, 1f);
            SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();
            renderer.color = outlineColor;
            renderer.sortingOrder = 2;
        }

        BoxCollider2D trigger = finish.GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.8f, 2.2f);
        finish.transform.position = new Vector3(FinishX, -0.05f, 0f);
    }

    private void BuildCheckpoints(int level)
    {
        if (level == 2)
        {
            CreateCheckpoint("CheckpointStart", -6.5f, -0.45f);
            CreateCheckpoint("Checkpoint01", 12f, -0.45f);
            CreateCheckpoint("Checkpoint02", 25f, -0.5f);
            CreateCheckpoint("Checkpoint03", 43f, -0.5f);
            CreateCheckpoint("Checkpoint04", 71f, -0.5f);
            CreateCheckpoint("Checkpoint05", 94f, -0.5f);
            CreateCheckpoint("Checkpoint06", 131f, -0.5f);
            return;
        }

        CreateCheckpoint("CheckpointStart", -6.5f, -0.45f);
        CreateCheckpoint("Checkpoint01", 10f, -0.45f);
        CreateCheckpoint("Checkpoint02", 28.5f, -0.5f);
        CreateCheckpoint("Checkpoint03", 55f, -0.45f);
        CreateCheckpoint("Checkpoint04", 90f, -0.45f);
        CreateCheckpoint("Checkpoint05", 111f, -0.45f);
        CreateCheckpoint("Checkpoint06", 135f, -0.45f);
    }

    private void CreateCheckpoint(string checkpointName, float x, float y)
    {
        GameObject checkpoint = transform.Find(checkpointName)?.gameObject;
        if (checkpoint == null)
        {
            checkpoint = new GameObject(checkpointName);
            checkpoint.transform.SetParent(transform, true);
            checkpoint.AddComponent<BoxCollider2D>();
            checkpoint.AddComponent<CheckpointZone>();
        }

        checkpoint.transform.position = new Vector3(x, y, 0f);
        BoxCollider2D trigger = checkpoint.GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.8f, 1.8f);
    }

    private void SelectLevel(int level)
    {
        if (applyingLevel)
            return;

        applyingLevel = true;
        screenState = ScreenState.LevelSelection;
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
            player.SetMenuPaused(true);

        StartCoroutine(ApplyLevelNextFrame(level));
    }

    private IEnumerator ApplyLevelNextFrame(int level)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        yield return null;

        BuildPlatforms(level);
        StylePlayer();
        BuildCheckpoints(level);
        CreateFinishZone();

        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            player.ResetForLevel(new Vector3(StartX, -0.2f, 0f));
            player.SetMenuPaused(false);
        }

        screenState = ScreenState.Playing;
        applyingLevel = false;
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    public void ShowCompletionScreen()
    {
        screenState = ScreenState.Completed;
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    public void BeginLevelSelection()
    {
        StartFromTitle();
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    public void SelectLevelFromUI(int level)
    {
        SelectLevel(level);
    }

    public void PauseFromUI()
    {
        PauseGame();
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    public void ResumeFromUI()
    {
        ResumeGame();
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    public void RestartFromUI()
    {
        RestartCurrentLevel();
    }

    public void ReturnToTitleFromUI()
    {
        Time.timeScale = 1f;
        screenState = ScreenState.Title;
        FindAnyObjectByType<Player>()?.SetMenuPaused(true);
        FindAnyObjectByType<UIManager>()?.RefreshPanels();
    }

    private void StartFromTitle()
    {
        screenState = ScreenState.LevelSelection;
    }

    private void PauseGame()
    {
        screenState = ScreenState.Paused;
        Time.timeScale = 0f;
        FindAnyObjectByType<Player>()?.SetMenuPaused(true);
    }

    private void ResumeGame()
    {
        screenState = ScreenState.Playing;
        Time.timeScale = 1f;
        FindAnyObjectByType<Player>()?.SetMenuPaused(false);
    }

    private void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CreateFacePart(Transform parent, string partName, Vector3 localPosition, Vector2 size, Color color, int sortingOffset)
    {
        GameObject part = new GameObject(partName);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOffset;
    }

    private static Sprite GetSquareSprite()
    {
        if (squareSprite != null)
            return squareSprite;

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "RuntimeSquare";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        squareSprite.name = "RuntimeSquareSprite";
        return squareSprite;
    }
}
