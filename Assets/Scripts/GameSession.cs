/// <summary>Small static bridge for data shared between menu, game, and result scenes.</summary>
public static class GameSession
{
    public const string GameSceneName = "GameScene";
    public const string StartSceneName = "StartScene";
    public const string LevelSelectionSceneName = "LevelSelectionScene";
    public const string EndSceneName = "EndScene";

    public static int SelectedLevel { get; private set; } = 1;
    public static bool HasLaunchRequest { get; private set; }
    public static bool LevelCompleted { get; private set; }

    public static void StartLevel(int level)
    {
        SelectedLevel = level;
        HasLaunchRequest = true;
    }

    public static int ConsumeLaunchLevel()
    {
        HasLaunchRequest = false;
        return SelectedLevel;
    }

    public static void SetResult(bool completed)
    {
        LevelCompleted = completed;
    }
}
