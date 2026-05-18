using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int baseScorePerTile = 100;
    [SerializeField] private int chainMultiplierStep = 1;

    public int CurrentScore { get; private set; }
    public int LastChainIndex { get; private set; }

    public event System.Action<int> ScoreChanged;
    public event System.Action<int> ChainChanged;

    private void Start()
    {
        ResetScore();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        LastChainIndex = 0;
        ScoreChanged?.Invoke(CurrentScore);
        ChainChanged?.Invoke(LastChainIndex);
    }

    public void AddScore(int clearedTileCount, int chainIndex)
    {
        if (clearedTileCount <= 0)
        {
            return;
        }

        LastChainIndex = Mathf.Max(1, chainIndex);
        int multiplier = 1 + ((LastChainIndex - 1) * chainMultiplierStep);
        CurrentScore += clearedTileCount * baseScorePerTile * multiplier;

        ScoreChanged?.Invoke(CurrentScore);
        ChainChanged?.Invoke(LastChainIndex);
    }
}
