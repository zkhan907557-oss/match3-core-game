using UnityEngine;

public enum TileKind
{
    Normal,
    Bomb,
    Rainbow,
    Dynamite
}

public enum TileColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Tile : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public TileKind Kind { get; private set; }
    public TileColor Color { get; private set; }

    private GridManager gridManager;
    private SpriteRenderer spriteRenderer;

    public void Initialize(GridManager owner, int x, int y, TileColor color, TileKind kind)
    {
        gridManager = owner;
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetGridPosition(x, y);
        SetType(color, kind);
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        name = $"Tile_{x}_{y}_{Kind}_{Color}";
    }

    public void SetType(TileColor color, TileKind kind)
    {
        Color = color;
        Kind = kind;
        name = $"Tile_{X}_{Y}_{Kind}_{Color}";

        if (spriteRenderer != null && gridManager != null)
        {
            spriteRenderer.sprite = gridManager.GetSprite(color, kind);
            spriteRenderer.color = GetFallbackColor(color, kind);
        }
    }

    public bool CanMatchWith(Tile other)
    {
        return other != null && Kind != TileKind.Rainbow && other.Kind != TileKind.Rainbow && Color == other.Color;
    }

    private UnityEngine.Color GetFallbackColor(TileColor color, TileKind kind)
    {
        if (kind == TileKind.Bomb)
        {
            return UnityEngine.Color.black;
        }

        if (kind == TileKind.Rainbow)
        {
            return UnityEngine.Color.white;
        }

        if (kind == TileKind.Dynamite)
        {
            return new UnityEngine.Color(0.95f, 0.35f, 0.1f);
        }

        switch (color)
        {
            case TileColor.Red: return UnityEngine.Color.red;
            case TileColor.Blue: return UnityEngine.Color.blue;
            case TileColor.Green: return UnityEngine.Color.green;
            case TileColor.Yellow: return UnityEngine.Color.yellow;
            case TileColor.Purple: return new UnityEngine.Color(0.55f, 0.2f, 0.85f);
            case TileColor.Orange: return new UnityEngine.Color(1f, 0.55f, 0.05f);
            default: return UnityEngine.Color.gray;
        }
    }
}
