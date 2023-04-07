using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum BoardState {
    PLACING_MINO, MERGING_BLOCKGROUPS, UPDATING_BOOMBLOCKS, UPDATING_BLOCKGROUPS, GENERATE_WALL
}

public class Board : MonoBehaviour {
    [Header("Components")]
    [SerializeField] private GameManager gameManager;
    [Space]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer borderSpriteRenderer;
    [SerializeField] private SpriteRenderer glowSpriteRenderer;
    [SerializeField] private Camera gameCamera;
    [Space]
    [SerializeField] private BoardArea breakthroughBoardArea;
    [SerializeField] private BoardArea hazardBoardArea;
    [Space]
    [SerializeField] private List<GameObject> minoPrefabs;
    [SerializeField] private GameObject blockGroupPrefab;
    [SerializeField] private GameObject blockPrefab;
    [Header("Properties")]
    [SerializeField] private BoardState _boardState;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private float cameraPadding;
    [SerializeField] private float borderThickness;
    [SerializeField] private float glowThickness;

    private List<List<Block>> minos = new List<List<Block>>( );
    private List<BoomBlockFrames> boomBlockFrames = new List<BoomBlockFrames>( );
    private int blockGroupCount;
    private List<Block> blocksToUpdate = new List<Block>( );

    private Vector3 minoSpawnPosition;
    private bool needToUpdate = false;

    #region Properties
    public BoardArea BreakthroughBoardArea => breakthroughBoardArea;
    public BoardArea HazardBoardArea => hazardBoardArea;

    public int Width => _width;
    public int Height => _height;
    public BoardState BoardState {
        get => _boardState;
        set {
            _boardState = value;

            switch (value) {
                case BoardState.PLACING_MINO:
                    needToUpdate = true;
                    GenerateMino( );
                    break;
                case BoardState.MERGING_BLOCKGROUPS:
                    MergeBlockGroups( );
                    break;
                case BoardState.UPDATING_BOOMBLOCKS:
                    needToUpdate = true;
                    break;
                case BoardState.UPDATING_BLOCKGROUPS:
                    needToUpdate = false;
                    break;
                case BoardState.GENERATE_WALL:
                    needToUpdate = true;
                    StartCoroutine(GenerateWall( ));
                    break;
            }
        }
    }
    #endregion

    #region Unity
#if UNITY_EDITOR
    protected void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
    protected void _OnValidate ( ) {
#if UNITY_EDITOR
        EditorApplication.delayCall -= _OnValidate;
        if (this == null) {
            return;
        }
#endif

        gameManager = FindObjectOfType<GameManager>( );

        minoSpawnPosition = new Vector3(Width / 2f, Height - 2f);

        // Set the board size and position so the bottom left corner is at (0, 0)
        // This makes it easier when converting from piece transform position to a board array index
        float positionX = (Width / 2) - (Width % 2 == 0 ? 0.5f : 0f);
        float positionY = (Height / 2) - (Height % 2 == 0 ? 0.5f : 0f);
        spriteRenderer.size = new Vector2(Width, Height);
        transform.position = new Vector3(positionX, positionY);

        // Set the size of the border
        borderSpriteRenderer.size = new Vector2(Width + (borderThickness * 2), Height + (borderThickness * 2));
        glowSpriteRenderer.size = new Vector2(Width + (glowThickness * 2), Height + (glowThickness * 2));

        // Set the camera orthographic size and position so it fits the entire board
        gameCamera.orthographicSize = (Height + cameraPadding) / 2f;
        gameCamera.transform.position = new Vector3(positionX, positionY, gameCamera.transform.position.z);
    }

    protected void Awake ( ) {
#if UNITY_EDITOR
        OnValidate( );
#else
		_OnValidate( );
#endif
    }

    private void Start ( ) {
        BoardState = BoardState.GENERATE_WALL;
    }

    private void Update ( ) {
        switch (BoardState) {
            case BoardState.UPDATING_BOOMBLOCKS:
                UpdateBoomBlockFrames( );
                break;
            case BoardState.UPDATING_BLOCKGROUPS:
                UpdateBlockGroups( );
                break;
        }
    }
    #endregion

    /// <summary>
    /// Whether or not the input position is within the bounds of the board
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns>true if position is within the bounds of the board, false otherwise</returns>
    public bool IsPositionOnBoard (Vector2Int position) {
        bool inX = (position.x >= 0 && position.x < Width);
        bool inY = (position.y >= 0 && position.y < Height);
        return (inX && inY);
    }

    /// <summary>
    /// Check to see if there is a block at the input position
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <param name="blockGroupID">The block group ID to check</param>
    /// <returns>true if there is a block at the input position, false otherwise. Also returns true if the position is out of bounds of the board. Also returns false if the block has the same specified block group ID</returns>
    public bool IsBlockAt (Vector2Int position, int blockGroupID = -1) {
        // If the position is out of bounds, then return true because we want to trick the program into thinking there is a block there
        // The boundaries of the board are solid
        if (!IsPositionOnBoard(position)) {
            return true;
        }

        // Get a reference to the block at the input position
        Block block = GetBlockAt(position);

        // If the reference to the block is null, then there is not a block at the input position
        if (block == null) {
            return false;
        }

        // If a block group ID has been specified and the block at the input position has the same block group ID, then the block at the position should be ignored and treated as no block being there
        // This makes things easier with determining valid moves for block groups
        if (blockGroupID != -1 && block.BlockGroupID == blockGroupID) {
            return false;
        }

        // If all above checks pass, then return that there is a valid block at the input position
        return true;
    }

    /// <summary>
    /// Get a block at a specific position
    /// </summary>
    /// <param name="position">The position to get the block at</param>
    /// <returns>Returns the reference to the block if there is on at that position, null otherwise</returns>
    public Block GetBlockAt (Vector2Int position) {
        RaycastHit2D hit = Physics2D.Raycast((Vector3Int) position + Vector3.back, Vector3.forward);
        if (hit) {
            return hit.transform.GetComponent<Block>( );
        }

        return null;
    }

    /// <summary>
    /// Damage the specified block
    /// </summary>
    /// <param name="block">The block to damage</param>
    /// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
    /// <returns>true if the block was completely destroyed, false otherwise</returns>
    public bool DamageBlock (Block block, bool destroy = false) {
        return DamageBlockAt(block.Position, destroy);
    }

    /// <summary>
    /// Damage the block at the specified position
    /// </summary>
    /// <param name="position">The position of the block to damage</param>
    /// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
    /// <returns>true if the block at the specified position was completely destroyed, false otherwise</returns>
    public bool DamageBlockAt (Vector2Int position, bool destroy = false) {
        // If there is not a block at the specified position, then no block can be destroyed
        if (!IsBlockAt(position)) {
            return false;
        }

        // Get a reference to the block and damage it
        Block block = GetBlockAt(position);
        block.Health -= (destroy ? block.Health : 1);

        // If the block now has 0 health, as in the block has been completely destroyed
        if (block.Health == 0) {
            // If the block has a block group, make sure the block group knows that it was modified
            if (block.BlockGroupID != -1) {
                block.BlockGroup.IsModified = true;
            }

            // If the block has a mino index, remove the reference to the block in the mino list
            if (block.MinoIndex != -1) {
                minos[block.MinoIndex].Remove(block);

                // If the mino index now has a size of 0, the entire mino has been destroyed
                if (minos[block.MinoIndex].Count == 0) {
                    gameManager.AddBoardPoints(PointsType.DESTROYED_MINO);
                }
            }

            Destroy(block);

            return true;
        }

        return false;
    }

    public void CreateBlock (Vector2Int position) {
        Block block = Instantiate(blockPrefab, transform).GetComponent<Block>( );
        block.Position = position;
        blocksToUpdate.Add(block);
    }

    /// <summary>
    /// Create boom block frames from a boom block
    /// </summary>
    /// <param name="block">The block to create boom block frames from</param>
    private void GenerateBoomBlockFrames (Block block) {
        if (!block.IsBoomBlock) {
            return;
        }

        boomBlockFrames.Add(new BoomBlockFrames(this, gameManager, block));
    }

    /// <summary>
    /// Generate a random mino on the board
    /// </summary>
    private void GenerateMino ( ) {
        gameManager.ActiveMino = Instantiate(minoPrefabs[Random.Range(0, minoPrefabs.Count)], minoSpawnPosition, Quaternion.identity).GetComponent<PlayerControlledBlockGroup>( );

        // If the mino was generated with a boom block, then the boom block drought is over
        // If the mino was not generated with a boom block, then the boom block drought continues and increases the chance of a boom block to spawn for the next mino
        if (gameManager.ActiveMino.HasBoomBlock) {
            gameManager.BoomBlockDrought = 0;
        } else {
            gameManager.BoomBlockDrought++;
        }
    }

    private void OnPlaceActiveMino ( ) {
        for (int i = 0; i < gameManager.ActiveMino.Count; i++) {
            // Add all of the blocks that make up the mino to be updated and merged into other block groups
            blocksToUpdate.Add(gameManager.ActiveMino[i]);

            // If the block is a boom block, generate boom blocks frames for it
            if (gameManager.ActiveMino[i].IsBoomBlock) {
                GenerateBoomBlockFrames(gameManager.ActiveMino[i]);
            }
        }

        gameManager.ActiveMino = null;
        BoardState = BoardState.MERGING_BLOCKGROUPS;
    }

    private void MergeBlockGroups ( ) {
        while (blocksToUpdate.Count > 0) {
            // Get the surrounding block groups to the current block
            List<BlockGroup> surroundingBlockGroups = new List<BlockGroup>( );

            foreach (Vector2Int neighborBlockPosition in Utils.GetCardinalPositions(blocksToUpdate[0].Position)) {
                // If there is not a block at the neighboring position, then continue to the next position
                if (!IsBlockAt(neighborBlockPosition)) {
                    continue;
                }

                // If the block at the neighboring position has the same block group as the current block, continue to the next position
                // We don't want block groups to merge with themselves
                if (GetBlockAt(neighborBlockPosition).BlockGroupID == blocksToUpdate[0].BlockGroupID) {
                    continue;
                }

                surroundingBlockGroups.Add(GetBlockAt(neighborBlockPosition).BlockGroup);
            }

            // If there are no surrounding block groups, create a new block group
            // If there are surrounding block groups, merge all of them together
            if (surroundingBlockGroups.Count == 0) {
                BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform).GetComponent<BlockGroup>( );
                blockGroup.ID = ++blockGroupCount;
                blocksToUpdate[0].BlockGroup = blockGroup;
            } else {
                blocksToUpdate[0].BlockGroup = BlockGroup.MergeAllBlockGroups(surroundingBlockGroups);
            }

            blocksToUpdate.RemoveAt(0);
        }

        blocksToUpdate.Clear( );

        // If there are more boom blocks to explode, then update them
        // If there are no more boom blocks to explode, then start to place another mino
        if (boomBlockFrames.Count > 0) {
            BoardState = BoardState.UPDATING_BOOMBLOCKS;
        } else if (needToUpdate) {
            BoardState = BoardState.UPDATING_BLOCKGROUPS;
        } else {
            BoardState = BoardState.PLACING_MINO;
        }
    }

    private void UpdateBoomBlockFrames ( ) {

    }

    private void UpdateBlockGroups ( ) {

    }

    #region Sequences
    private IEnumerator GenerateWall ( ) {
        // Generate a random noise grid
        // float[ , ] wallValues = Utils.GenerateRandomNoiseGrid(Width, gameManager.WallHeight, gameManager.WallHealthRange.x, gameManager.WallHealthRange.y);
        float[ , ] wallValues = Utils.GenerateRandomNoiseGrid(Width, 5, 0, 3);
        // for (int j = 0; j < gameManager.WallHeight; j++) {
        for (int j = 0; j < 5; j++) {
            for (int i = 0; i < Width; i++) {
                // Round the random noise grid value to an integer
                int randomValue = Mathf.RoundToInt(wallValues[i, j]);

                // Randomly increase the health of the block
                if (Random.Range(0f, 1f) < 0.5f) {
                    randomValue++;
                }

                // Make sure the bottom of the wall never has any holes in it
                if (randomValue == 0f && j == 0) {
                    randomValue = 1;
                }

                // Make sure the random value keeps within the range of the wall block health
                randomValue = Mathf.Clamp(randomValue, 0, 3);

                // If the random value is greater than 0, then generate a block at the current position with the random value as its health
                // A value of 0 means the block would have 0 health, so no block should be created there
                if (randomValue > 0) {
                    // Block block = Instantiate(blockPrefab, new Vector3(i, j + breakthroughBoardArea.Height), Quaternion.identity).GetComponent<Block>( );
                    Block block = Instantiate(blockPrefab, new Vector3(i, j + 2), Quaternion.identity).GetComponent<Block>( );
                    block.Health = randomValue;
                }
            }

            // Have each row of the wall generate slightly delayed of one another
            yield return new WaitForSeconds(0.25f);
        }

        BoardState = BoardState.MERGING_BLOCKGROUPS;
    }
    #endregion
}