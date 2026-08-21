using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Tidal Rush/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber = 1;
    public float timeLimitSeconds = 60f;
    public int rows = 4;
    public int columns = 4;
}
