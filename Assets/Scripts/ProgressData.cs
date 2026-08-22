using UnityEngine;

// Central owner of locally saved player progress. Shared by GameManager
// (which advances it) and MainMenuController (which reads and resets it),
// since MainMenu has no GameManager instance to delegate to.
public static class ProgressData
{
    private const string UnlockedLevelKey = "TidalRush.UnlockedLevel";

    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, 1);
    }

    public static void SetUnlockedLevelIfHigher(int newlyUnlockedLevel)
    {
        if (newlyUnlockedLevel <= GetUnlockedLevel())
        {
            return;
        }

        PlayerPrefs.SetInt(UnlockedLevelKey, newlyUnlockedLevel);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(UnlockedLevelKey, 1);
        PlayerPrefs.Save();
    }
}
