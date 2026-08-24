using UnityEngine;

// Central owner of locally saved player progress. Shared by GameManager
// (which advances it) and MainMenuController (which reads and resets it),
// since MainMenu has no GameManager instance to delegate to.
public static class ProgressData
{
    private const string UnlockedLevelKey = "TidalRush.UnlockedLevel";
    private const string BestStarsKeyPrefix = "TidalRush.BestStars.";

    // Mirrors GameManager's own MaxLevel constant (Level_15.asset is the
    // highest currently authored level). Duplicated rather than shared,
    // following this project's existing per-class constant convention
    // (see GameManager's HexColor duplication note).
    private const int MaxLevel = 15;

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

    public static int GetBestStars(int level)
    {
        return PlayerPrefs.GetInt(BestStarsKeyPrefix + level, 0);
    }

    public static void SetBestStarsIfHigher(int level, int stars)
    {
        if (stars <= GetBestStars(level))
        {
            return;
        }

        PlayerPrefs.SetInt(BestStarsKeyPrefix + level, stars);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(UnlockedLevelKey, 1);

        for (int level = 1; level <= MaxLevel; level++)
        {
            PlayerPrefs.DeleteKey(BestStarsKeyPrefix + level);
        }

        PlayerPrefs.Save();
    }
}
