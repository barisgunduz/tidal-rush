using UnityEngine;

// Central owner of the sound on/off preference, following the same
// PlayerPrefs-backed static class pattern as ProgressData. No audio clips
// exist yet; GameManager's PlaySound hooks check IsSoundEnabled() before
// doing anything so the toggle is functionally wired ahead of real audio.
public static class SoundSettings
{
    private const string SoundEnabledKey = "TidalRush.SoundEnabled";

    public static bool IsSoundEnabled()
    {
        return PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
    }

    public static void SetSoundEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SoundEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
