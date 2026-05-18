using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private float tileSpacing = 1f;
    [SerializeField] private Transform tileParent;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private bool createPrefabIfMissing = true;

    [Header("Sprites")]
    [SerializeField] private Sprite redNormal;
    [SerializeField] private Sprite blueNormal;
    [SerializeField] private Sprite greenNormal;
    [SerializeField] private Sprite yellowNormal;
    [SerializeField] private Sprite purpleNormal;
    [SerializeField] private Sprite orangeNormal;
    [SerializeField] private Sprite bombSprite;
    [SerializeField] private Sprite rainbowSprite;
    [SerializeField] private Sprite dynamiteSprite;

    [Header("Level")]
    [SerializeField] private int moveLimit = 25;
    [SerializeField] private int targetScore = 10000;
    [SerializeField] private float swapDuration = 0.15f;
    [SerializeField] private float fallDurationPerCell = 0.07f;

    [Header("Power Up Creation")]
    [Range(0f, 1f)]
    [SerializeField] private float randomPowerUpSpawnChance = 0.02f;

    public int Width => width;
    public int Height => height;
    public int MovesRemaining { get; private set; }
    public bool IsBusy { get; private set; }
    public bool IsGameOver { get; private set; }
    public Tile[,] Tiles { get; private set; }

    public event System.Action<int> MovesChanged;
    public event System.Action GameWon;
    public event System.Action GameLost;

    private MatchDetector matchDetector;
    private PowerUpHandler powerUpHandler;
    private ScoreManager scoreManager;
    private Tile runtimeTilePrefab;
    private Sprite runtimeFallbackSprite;

    private readonly TileColor[] colors = (TileColor[])System.Enum.GetValues(typeof(TileColor));

    private void Awake()
    {
        matchDetector = GetComponent<MatchDetector>();
        powerUpHandler = GetComponent<PowerUpHandler>();
        scoreManager = GetComponent<ScoreManager>();

        if (tileParent == null)
        {
            tileParent = transform;
        }

        if (tilePrefab == null && createPrefabIfMissing)
        {
            runtimeTilePrefab = CreateRuntimeTilePrefab();
            tilePrefab = runtimeTilePrefab;
        }
    }

    private void Start()
    {
        BuildBoard();
    }

    public void BuildBoard()
    {
        StopAllCoroutines();
        ClearExistingTiles();

        IsBusy = false;
        IsGameOver = false;
        MovesRemaining = moveLimit;
        Tiles = new Tile[width, height];
        scoreManager?.ResetScore();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnTile(x, y, pickColorAvoidingInitialMatches: true);
            }
        }

        MovesChanged?.Invoke(MovesRemaining);
    }

    public void TrySwap(Tile first, Tile second)
    {
        if (IsBusy || IsGameOver || first == null || second == null || !AreAdjacent(first, second))
        {
            return;
        }

        StartCoroutine(SwapAndResolve(first, second));
    }

    public Sprite GetSprite(TileColor color, TileKind kind)
    {
        if (kind == TileKind.Bomb && bombSprite != null)
        {
            return bombSprite;
        }

        if (kind == TileKind.Rainbow && rainbowSprite != null)
        {
            return rainbowSprite;
        }

        if (kind == TileKind.Dynamite && dynamiteSprite != null)
        {
            return dynamiteSprite;
        }

        switch (color)
        {
            case TileColor.Red: return redNormal != null ? redNormal : GetRuntimeFallbackSprite();
            case TileColor.Blue: return blueNormal != null ? blueNormal : GetRuntimeFallbackSprite();
            case TileColor.Green: return greenNormal != null ? greenNormal : GetRuntimeFallbackSprite();
            case TileColor.Yellow: return yellowNormal != null ? yellowNormal : GetRuntimeFallbackSprite();
            case TileColor.Purple: return purpleNormal != null ? purpleNormal : GetRuntimeFallbackSprite();
            case TileColor.Orange: return orangeNormal != null ? orangeNormal : GetRuntimeFallbackSprite();
            default: return GetRuntimeFallbackSprite();
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return transform.position + new Vector3(x * tileSpacing, y * tileSpacing, 0f);
    }

    public Tile GetTile(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return null;
        }

        return Tiles[x, y];
    }

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void ReplaceWithPowerUp(Tile tile, TileKind kind)
    {
        if (tile == null || kind == TileKind.Normal)
        {
            return;
        }

        tile.SetType(tile.Color, kind);
    }

    private IEnumerator SwapAndResolve(Tile first, Tile second)
    {
        IsBusy = true;

        SwapTilesInArray(first, second);
        yield return AnimateTilesToGrid(first, second);

        bool powerUpSwap = first.Kind != TileKind.Normal || second.Kind != TileKind.Normal;
        List<Tile> matchedTiles = matchDetector != null ? matchDetector.FindAllMatches(Tiles) : new List<Tile>();

        if (!powerUpSwap && matchedTiles.Count == 0)
        {
            SwapTilesInArray(first, second);
            yield return AnimateTilesToGrid(first, second);
            IsBusy = false;
            yield break;
        }

        ConsumeMove();

        if (powerUpSwap)
        {
            matchedTiles.AddRange(powerUpHandler != null ? powerUpHandler.ResolvePowerUpSwap(first, second, Tiles) : new List<Tile>());
        }

        yield return ResolveBoard(matchedTiles);
        CheckEndConditions();

        IsBusy = false;
    }

    private IEnumerator ResolveBoard(List<Tile> initialMatches)
    {
        int chainIndex = 0;
        List<Tile> currentMatches = initialMatches;

        while (currentMatches.Count > 0)
        {
            chainIndex++;

            HashSet<Tile> tilesToClear = new HashSet<Tile>(currentMatches);
            if (powerUpHandler != null)
            {
                powerUpHandler.ExpandPowerUpEffects(tilesToClear, Tiles);
            }

            ClearTiles(tilesToClear, chainIndex);
            yield return CollapseColumns();
            yield return FillEmptyCells();

            currentMatches = matchDetector != null ? matchDetector.FindAllMatches(Tiles) : new List<Tile>();
        }
    }

    private void ClearTiles(HashSet<Tile> tilesToClear, int chainIndex)
    {
        foreach (Tile tile in tilesToClear)
        {
            if (tile == null || !IsInsideBoard(tile.X, tile.Y) || Tiles[tile.X, tile.Y] != tile)
            {
                continue;
            }

            Tiles[tile.X, tile.Y] = null;
            Destroy(tile.gameObject);
        }

        if (scoreManager != null)
        {
            scoreManager.AddScore(tilesToClear.Count, chainIndex);
        }
    }

    private IEnumerator CollapseColumns()
    {
        List<Tile> movedTiles = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            int writeY = 0;

            for (int readY = 0; readY < height; readY++)
            {
                Tile tile = Tiles[x, readY];
                if (tile == null)
                {
                    continue;
                }

                if (readY != writeY)
                {
                    Tiles[x, writeY] = tile;
                    Tiles[x, readY] = null;
                    tile.SetGridPosition(x, writeY);
                    movedTiles.Add(tile);
                }

                writeY++;
            }
        }

        if (movedTiles.Count > 0)
        {
            yield return AnimateTilesToGrid(movedTiles);
        }
    }

    private IEnumerator FillEmptyCells()
    {
        List<Tile> spawnedTiles = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Tiles[x, y] != null)
                {
                    continue;
                }

                Tile tile = SpawnTile(x, y, pickColorAvoidingInitialMatches: false);
                tile.transform.position = GridToWorld(x, height + y);
                spawnedTiles.Add(tile);
            }
        }

        if (spawnedTiles.Count > 0)
        {
            yield return AnimateTilesToGrid(spawnedTiles);
        }
    }

    private Tile SpawnTile(int x, int y, bool pickColorAvoidingInitialMatches)
    {
        if (tilePrefab == null)
        {
            Debug.LogError("GridManager needs a Tile prefab with Tile, SpriteRenderer, and Collider2D components.");
            return null;
        }

        TileColor color = pickColorAvoidingInitialMatches ? GetRandomColorWithoutInitialMatch(x, y) : colors[Random.Range(0, colors.Length)];
        TileKind kind = Random.value < randomPowerUpSpawnChance ? GetRandomPowerUpKind() : TileKind.Normal;

        Tile tile = Instantiate(tilePrefab, GridToWorld(x, y), Quaternion.identity, tileParent);
        tile.gameObject.SetActive(true);
        tile.Initialize(this, x, y, color, kind);
        Tiles[x, y] = tile;
        return tile;
    }

    private Tile CreateRuntimeTilePrefab()
    {
        GameObject prefab = new GameObject("RuntimeTilePrefab");
        prefab.SetActive(false);
        SpriteRenderer renderer = prefab.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRuntimeFallbackSprite();
        BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one * tileSpacing * 0.9f;
        Tile tile = prefab.AddComponent<Tile>();
        return tile;
    }

    private Sprite GetRuntimeFallbackSprite()
    {
        if (runtimeFallbackSprite != null)
        {
            return runtimeFallbackSprite;
        }

        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        runtimeFallbackSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        return runtimeFallbackSprite;
    }

    private TileKind GetRandomPowerUpKind()
    {
        int value = Random.Range(0, 3);
        return value == 0 ? TileKind.Bomb : value == 1 ? TileKind.Rainbow : TileKind.Dynamite;
    }

    private TileColor GetRandomColorWithoutInitialMatch(int x, int y)
    {
        List<TileColor> allowedColors = new List<TileColor>(colors);

        if (x >= 2 && Tiles[x - 1, y] != null && Tiles[x - 2, y] != null && Tiles[x - 1, y].Color == Tiles[x - 2, y].Color)
        {
            allowedColors.Remove(Tiles[x - 1, y].Color);
        }

        if (y >= 2 && Tiles[x, y - 1] != null && Tiles[x, y - 2] != null && Tiles[x, y - 1].Color == Tiles[x, y - 2].Color)
        {
            allowedColors.Remove(Tiles[x, y - 1].Color);
        }

        return allowedColors[Random.Range(0, allowedColors.Count)];
    }

    private void SwapTilesInArray(Tile first, Tile second)
    {
        int firstX = first.X;
        int firstY = first.Y;
        int secondX = second.X;
        int secondY = second.Y;

        Tiles[firstX, firstY] = second;
        Tiles[secondX, secondY] = first;

        first.SetGridPosition(secondX, secondY);
        second.SetGridPosition(firstX, firstY);
    }

    private IEnumerator AnimateTilesToGrid(params Tile[] tiles)
    {
        yield return AnimateTilesToGrid((IList<Tile>)tiles);
    }

    private IEnumerator AnimateTilesToGrid(IList<Tile> tiles)
    {
        float elapsed = 0f;
        Dictionary<Tile, Vector3> starts = new Dictionary<Tile, Vector3>();
        Dictionary<Tile, Vector3> ends = new Dictionary<Tile, Vector3>();
        float duration = swapDuration;

        foreach (Tile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            Vector3 start = tile.transform.position;
            Vector3 end = GridToWorld(tile.X, tile.Y);
            starts[tile] = start;
            ends[tile] = end;
            duration = Mathf.Max(duration, Mathf.Abs(start.y - end.y) / tileSpacing * fallDurationPerCell);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            foreach (Tile tile in starts.Keys)
            {
                tile.transform.position = Vector3.Lerp(starts[tile], ends[tile], eased);
            }

            yield return null;
        }

        foreach (Tile tile in starts.Keys)
        {
            tile.transform.position = ends[tile];
        }
    }

    private bool AreAdjacent(Tile first, Tile second)
    {
        int distance = Mathf.Abs(first.X - second.X) + Mathf.Abs(first.Y - second.Y);
        return distance == 1;
    }

    private void ConsumeMove()
    {
        MovesRemaining = Mathf.Max(0, MovesRemaining - 1);
        MovesChanged?.Invoke(MovesRemaining);
    }

    private void CheckEndConditions()
    {
        if (scoreManager != null && scoreManager.CurrentScore >= targetScore)
        {
            IsGameOver = true;
            GameWon?.Invoke();
            return;
        }

        if (MovesRemaining <= 0)
        {
            IsGameOver = true;
            GameLost?.Invoke();
        }
    }

    private void ClearExistingTiles()
    {
        if (tileParent == null)
        {
            return;
        }

        for (int i = tileParent.childCount - 1; i >= 0; i--)
        {
            Transform child = tileParent.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
