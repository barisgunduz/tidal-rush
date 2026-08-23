using UnityEngine;

// Central owner of the audio preferences, following the same
// PlayerPrefs-backed static class pattern as ProgressData. Sfx and Music
// are independent toggles (BACKLOG.md item 29): no audio clips exist yet
// for either, GameManager's PlaySound hooks check IsSfxEnabled() before
// doing anything so sound effects stay functionally wired ahead of real
// audio, and IsMusicEnabled() is exposed the same way so a future music
// system can check it without any further plumbing.
public static class SoundSettings
{
    private const string SfxEnabledKey = "TidalRush.SfxEnabled";
    private const string MusicEnabledKey = "TidalRush.MusicEnabled";

    public static bool IsSfxEnabled()
    {
        return PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
    }

    public static void SetSfxEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsMusicEnabled()
    {
        return PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
    }

    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
