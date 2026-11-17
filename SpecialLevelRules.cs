using UnityEngine.SceneManagement;

public static class SpecialLevelRules
{
    public static bool IsSpecial(int level) => level == 10 || level == 20 || level == 30;
    public static bool IsSpecial(string sceneName)
    {
        return int.TryParse(sceneName.Replace("Level ", ""), out int level) && IsSpecial(level);
    }
    public static bool IsCurrentSpecial => IsSpecial(SceneManager.GetActiveScene().name);
}
