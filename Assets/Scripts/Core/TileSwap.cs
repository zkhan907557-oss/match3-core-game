using UnityEngine;

public class TileSwap : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera boardCamera;
    [SerializeField] private LayerMask tileLayerMask = ~0;

    private Tile selectedTile;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (boardCamera == null)
        {
            boardCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (gridManager == null || boardCamera == null || gridManager.IsBusy || gridManager.IsGameOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerDown(Input.mousePosition);
        }
    }

    public void HandlePointerDown(Vector2 screenPosition)
    {
        Tile tile = RaycastTile(screenPosition);
        if (tile == null)
        {
            selectedTile = null;
            return;
        }

        if (selectedTile == null)
        {
            selectedTile = tile;
            return;
        }

        if (selectedTile == tile)
        {
            selectedTile = null;
            return;
        }

        gridManager.TrySwap(selectedTile, tile);
        selectedTile = null;
    }

    private Tile RaycastTile(Vector2 screenPosition)
    {
        Vector3 world = boardCamera.ScreenToWorldPoint(screenPosition);
        Vector2 point = new Vector2(world.x, world.y);
        Collider2D hit = Physics2D.OverlapPoint(point, tileLayerMask);
        return hit != null ? hit.GetComponent<Tile>() : null;
    }
}
