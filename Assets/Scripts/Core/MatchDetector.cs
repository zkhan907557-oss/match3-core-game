using System.Collections.Generic;
using UnityEngine;

public class MatchDetector : MonoBehaviour
{
    public List<Tile> FindAllMatches(Tile[,] board)
    {
        HashSet<Tile> matches = new HashSet<Tile>();
        if (board == null)
        {
            return new List<Tile>();
        }

        int width = board.GetLength(0);
        int height = board.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            int runStart = 0;
            for (int x = 1; x <= width; x++)
            {
                bool continues = x < width && AreMatchable(board[runStart, y], board[x, y]);
                if (continues)
                {
                    continue;
                }

                AddRunIfMatch(board, matches, runStart, y, x - runStart, horizontal: true);
                runStart = x;
            }
        }

        for (int x = 0; x < width; x++)
        {
            int runStart = 0;
            for (int y = 1; y <= height; y++)
            {
                bool continues = y < height && AreMatchable(board[x, runStart], board[x, y]);
                if (continues)
                {
                    continue;
                }

                AddRunIfMatch(board, matches, x, runStart, y - runStart, horizontal: false);
                runStart = y;
            }
        }

        return new List<Tile>(matches);
    }

    public bool HasAnyMatch(Tile[,] board)
    {
        return FindAllMatches(board).Count > 0;
    }

    private bool AreMatchable(Tile first, Tile second)
    {
        return first != null && second != null && first.CanMatchWith(second);
    }

    private void AddRunIfMatch(Tile[,] board, HashSet<Tile> matches, int startX, int startY, int length, bool horizontal)
    {
        if (length < 3)
        {
            return;
        }

        for (int i = 0; i < length; i++)
        {
            int x = horizontal ? startX + i : startX;
            int y = horizontal ? startY : startY + i;
            Tile tile = board[x, y];

            if (tile != null && tile.Kind != TileKind.Rainbow)
            {
                matches.Add(tile);
            }
        }
    }
}
