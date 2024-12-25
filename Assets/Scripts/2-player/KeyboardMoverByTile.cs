using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * This component allows the player to move by clicking the arrow keys,
 * but only if the new position is on an allowed tile.
 */
public class KeyboardMoverByTile: KeyboardMover {
    [SerializeField] Tilemap tilemap = null;
//    [SerializeField] TileBase[] allowedTiles = null;
    
    // These are the allowed tiles that the player can move to without
    // using the goat help or the boat help
    [SerializeField] AllowedTiles allowedTilesWithoutHelp = null;

    [SerializeField] TileBase[] seaTiles = null; // For the boat

    [SerializeField] TileBase[] mountainTiles = null;  //For the goat

    [SerializeField] TileBase grassTile = null;  //For the axe to replace mountain tile with grass tile

    [SerializeField] private bool isPlayerInBoat = false;

    [SerializeField] private bool isPlayerRidingGoat = false;

    [SerializeField] private bool isPlayerHoldingAxe = false;

    [SerializeField] private Transform boatTransform = null;
    [SerializeField] private Transform goatTransform = null;
    [SerializeField] private Transform axeTransform = null;

    [SerializeField] private bool doneOnceForBoat = false;
    

    private TileBase TileOnPosition(Vector3 worldPosition) {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition);
    }

    Vector3Int FindTilePosition(TileBase tile)
    {
        BoundsInt bounds = tilemap.cellBounds;
        foreach(var position in bounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) == tile)
            {
                return position;
            }
        }

        return Vector3Int.zero;
    }

    Vector3Int ConvertToTilePosition(Vector3 position)
    {
        Vector3 vector3Position = new Vector3((float)position.x, (float)position.y, 0f);

        Vector3Int tilePosition = Vector3Int.FloorToInt(vector3Position);

        return tilePosition;
    }


    void Update()  {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isPlayerRidingGoat = false;   
            isPlayerHoldingAxe = false;         
        }

        Vector3 newPosition = NewPosition();
        TileBase tileOnNewPosition = TileOnPosition(newPosition);

        if (!isPlayerInBoat && !isPlayerRidingGoat && !isPlayerHoldingAxe)
        {
            if (allowedTilesWithoutHelp.Contains(tileOnNewPosition)) {
                transform.position = newPosition;
            } else {
                Debug.Log("You cannot walk on " + tileOnNewPosition + "!");
            }
        }
        
        if (isPlayerInBoat)  // Player is in the boat
        {
            Vector3 playerMoveDelta = newPosition - transform.position;

            if (seaTiles.Contains(tileOnNewPosition)) 
            { // Player moves to a sea tile
                doneOnceForBoat = false;
                transform.position = newPosition;
                boatTransform.position += playerMoveDelta;
            }
            else if (!doneOnceForBoat)
            {
                boatTransform.position += playerMoveDelta;
                doneOnceForBoat = true;    
            }
            
            if (allowedTilesWithoutHelp.Contains(tileOnNewPosition))
            {
                transform.position = newPosition;
            }
        }

        if (isPlayerRidingGoat)  // Player is RIDING THE goat
        {
            Vector3 playerMoveDelta = newPosition - transform.position;

            if (mountainTiles.Contains(tileOnNewPosition) ||
                allowedTilesWithoutHelp.Contains(tileOnNewPosition)) 
            { // Player moves to a mountain tile
                transform.position = newPosition;
                goatTransform.position += playerMoveDelta;
            }
            else if (allowedTilesWithoutHelp.Contains(tileOnNewPosition))
            {
                transform.position = newPosition;
            }
        }


        if (isPlayerHoldingAxe)  // Player is holding axe
        {
            //TileBase targetTile = TileOnPosition(newPosition);
            //Vector3Int targetTilePos = FindTilePosition(targetTile);
            Vector3Int targetTilePos = ConvertToTilePosition (newPosition);

            if (mountainTiles.Contains(tileOnNewPosition))
            { // Player tries moving to a mountain tile
                tilemap.SetTile(targetTilePos, grassTile);
            }

            if (allowedTilesWithoutHelp.Contains(tileOnNewPosition))
            {
                Vector3 playerMoveDelta = newPosition - transform.position;

                transform.position = newPosition;
                axeTransform.position += playerMoveDelta;
            }
        }


    }

    private void OnTriggerEnter2D(Collider2D other)
    {
            if (other.CompareTag("Boat"))
            {
                Debug.Log("Player enters boat");
                isPlayerInBoat = true;
                boatTransform = other.transform;
                doneOnceForBoat = false;
            }

            if (other.CompareTag("Goat"))
            {
                Debug.Log("Player rides goat");
                isPlayerRidingGoat = true;
                goatTransform = other.transform;
            }
            
            if (other.CompareTag("Axe"))
            {
                Debug.Log("Player takes axe");
                isPlayerHoldingAxe = true;
                axeTransform = other.transform;
            }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
            if (other.CompareTag("Boat"))
            {
                Debug.Log("Player leaves the boat");
                isPlayerInBoat = false;
            }
            
            if (other.CompareTag("Goat"))
            {
                Debug.Log("Player leaves the goat");
                isPlayerRidingGoat = false;
            }

             if (other.CompareTag("Axe"))
            {
                Debug.Log("Player drops the axe");
                isPlayerHoldingAxe = false;
            }
    }



}
