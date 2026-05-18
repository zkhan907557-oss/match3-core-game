using System.Collections.Generic;
using UnityEngine;

public class PowerUpHandler : MonoBehaviour
{
    public List<Tile> ResolvePowerUpSwap(Tile first, Tile second, Tile[,] board)
    {
        HashSet<Tile> affected = new HashSet<Tile>();

        AddSwapPowerUpEffect(first, second, board, affected);
        AddSwapPowerUpEffect(second, first, board, affected);

        return new List<Tile>(affected);
    }

    public void ExpandPowerUpEffects(HashSet<Tile> tilesToClear, Tile[,] board)
    {
        if (tilesToClear == null || board == null)
        {
            return;
        }

        Queue<Tile> pending = new Queue<Tile>(tilesToClear);

        while (pending.Count > 0)
        {
            Tile tile = pending.Dequeue();
            if (tile == null)
            {
                continue;
            }

            List<Tile> additions = GetPowerUpArea(tile, board, targetColor: null);
            foreach (Tile added in additions)
            {
                if (added != null && tilesToClear.Add(added))
                {
                    pending.Enqueue(added);
                }
            }
        }
    }

    private void AddSwapPowerUpEffect(Tile powerTile, Tile otherTile, Tile[,] board, HashSet<Tile> affected)
    {
        if (powerTile == null || board == null || powerTile.Kind == TileKind.Normal)
        {
            return;
        }

        TileColor? targetColor = powerTile.Kind == TileKind.Rainbow && otherTile != null ? otherTile.Color : null;
        foreach (Tile tile in GetPowerUpArea(powerTile, board, targetColor))
        {
            affected.Add(tile);
        }
    }

    private List<Tile> GetPowerUpArea(Tile tile, Tile[,] board, TileColor? targetColor)
    {
        List<Tile> affected = new List<Tile>();
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        switch (tile.Kind)
        {
            case TileKind.Bomb:
                AddArea(tile.X - 1, tile.Y - 1, 3, 3, board, affected);
                break;
            case TileKind.Dynamite:
                AddCross(tile.X, tile.Y, board, affected);
                break;
            case TileKind.Rainbow:
                TileColor color = targetColor ?? tile.Color;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Tile candidate = board[x, y];
                        if (candidate != null && candidate.Color == color)
                        {
                            affected.Add(candidate);
                        }
                    }
                }

                affected.Add(tile);
                break;
        }

        return affected;
    }

    private void AddArea(int startX, int startY, int areaWidth, int areaHeight, Tile[,] board, List<Tile> affected)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        for (int x = startX; x < startX + areaWidth; x++)
        {
            for (int y = startY; y < startY + areaHeight; y++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }

                Tile tile = board[x, y];
                if (tile != null)
                {
                    affected.Add(tile);
                }
            }
        }
    }

    private void AddCross(int centerX, int centerY, Tile[,] board, List<Tile> affected)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            Tile tile = board[x, centerY];
            if (tile != null)
            {
                affected.Add(tile);
            }
        }

        for (int y = 0; y < height; y++)
        {
            Tile tile = board[centerX, y];
            if (tile != null)
            {
                affected.Add(tile);
            }
        }
    }
}
