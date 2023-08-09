using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum BoardState {
	PLACING_MINO, MERGING_BLOCKGROUPS, UPDATING_BOOMBLOCKS, UPDATING_BLOCKGROUPS, GENERATE_WALL, BREAKTHROUGH
}

public class Board : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private GameManager gameManager;
	[SerializeField] private ParticleManager particleManager;
	[SerializeField] private BoardArea breakthroughBoardArea;
	[SerializeField] private BoardArea hazardBoardArea;
	[SerializeField] private HazardBar hazardBar;
	[SerializeField] private Camera gameCamera;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer borderSpriteRenderer;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[Space]
	[SerializeField] private List<GameObject> minoPrefabs;
	[SerializeField] private GameObject blockGroupPrefab;
	[SerializeField] private GameObject blockPrefab;
	[Header("Properties")]
	[SerializeField] private BoardState _boardState;
	[SerializeField, Min(0f)] private float cameraPadding;
	[SerializeField, Min(0f)] private float _boardPadding;
	[SerializeField, Min(0f)] private float _borderThickness;
	[SerializeField, Min(0f)] private float _glowThickness;

	private readonly List<BoomBlockFrames> boomBlockFrames = new List<BoomBlockFrames>( );
	private float boomBlockFrameTimer;

	private readonly List<List<Block>> minos = new List<List<Block>>( );
	private WeightedList minoSpawnPercentages;
	private Vector3 minoSpawnPosition;

	private List<BlockGroup> blockGroups = new List<BlockGroup>( );
	private int blockGroupCount;
	private bool blockGroupsLocked = false;
	private float blockGroupsLockedStartTime = 0f;
	private bool needToUpdate = false;


	#region Properties
	public BoardArea BreakthroughBoardArea => breakthroughBoardArea;
	public BoardArea HazardBoardArea => hazardBoardArea;

	public float BoardPadding => _boardPadding;
	public float BorderThickness => _borderThickness;
	public float GlowThickness => _glowThickness;

	public BoardState BoardState {
		get => _boardState;
		set {
			_boardState = value;

			Debug.Log("Set Board State: " + value.ToString( ));

			switch (_boardState) {
				case BoardState.PLACING_MINO:
					needToUpdate = true;
					UpdatePercentageCleared( );
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
				case BoardState.BREAKTHROUGH:
					StartCoroutine(Breakthrough( ));
					break;
			}
		}
	}
	#endregion

	#region Unity
#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		themeManager = FindObjectOfType<ThemeManager>( );
		gameManager = FindObjectOfType<GameManager>( );
		particleManager = FindObjectOfType<ParticleManager>( );

		// Set the position of the mino spawn point
		float offsetX = gameManager.GameSettings.BoardWidth % 2 == 0 ? 0.5f : 0.0f;
		minoSpawnPosition = new Vector3((gameManager.GameSettings.BoardWidth / 2f) - offsetX, gameManager.GameSettings.BoardHeight - 2.5f);

		// Set the board size and position so the bottom left corner is at (0, 0)
		// This makes it easier when converting from piece transform position to a board array index
		float positionX = (gameManager.GameSettings.BoardWidth / 2) - (gameManager.GameSettings.BoardWidth % 2 == 0 ? 0.5f : 0f);
		float positionY = (gameManager.GameSettings.BoardHeight / 2) - (gameManager.GameSettings.BoardHeight % 2 == 0 ? 0.5f : 0f);
		spriteRenderer.size = new Vector2(gameManager.GameSettings.BoardWidth, gameManager.GameSettings.BoardHeight);
		spriteRenderer.color = themeManager.ActiveTheme.BackgroundColor;
		transform.position = new Vector3(positionX, positionY);

		// Set the size of the border
		float borderWidth = gameManager.GameSettings.BoardWidth + (BorderThickness * 2);
		float borderHeight = gameManager.GameSettings.BoardHeight + (BorderThickness * 2);
		borderSpriteRenderer.size = new Vector2(borderWidth, borderHeight);
		borderSpriteRenderer.color = themeManager.ActiveTheme.DetailColor;

		// Set the size of the glow around the board
		float glowWidth = gameManager.GameSettings.BoardWidth + (GlowThickness * 2);
		float glowHeight = gameManager.GameSettings.BoardHeight + (GlowThickness * 2);
		glowSpriteRenderer.size = new Vector2(glowWidth, glowHeight);
		glowSpriteRenderer.color = themeManager.ActiveTheme.GlowColor;

		// Set the camera orthographic size and position so it fits the entire board
		gameCamera.orthographicSize = (gameManager.GameSettings.BoardHeight + cameraPadding) / 2f;
		gameCamera.transform.position = new Vector3(positionX, positionY, gameCamera.transform.position.z);
		gameCamera.backgroundColor = themeManager.ActiveTheme.BackgroundColor;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif

		minoSpawnPercentages = new WeightedList(minoPrefabs.Count, gameManager.GameSettings.AllowedMinos);
	}

	private void Start ( ) {
		// Set game difficulty and start the game
		gameManager.Breakthroughs = gameManager.GameSettings.GameLevel * 6;
		BoardState = BoardState.BREAKTHROUGH;
	}

	private void Update ( ) {
		if (gameManager.GameState == GameState.GAMEOVER) {
			return;
		}

		switch (BoardState) {
			case BoardState.PLACING_MINO:
				gameManager.ActiveMino.UpdateBlockGroup( );
				break;
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
		bool inX = (position.x >= 0 && position.x < gameManager.GameSettings.BoardWidth);
		bool inY = (position.y >= 0 && position.y < gameManager.GameSettings.BoardHeight);
		return (inX && inY);
	}

	/// <summary>
	/// Check to see if there is a block at the input position. You should use this method over doing GetBlockAt(...) == null.
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <param name="block">An output parameter for the block at the position</param>
	/// <param name="blockGroupID">The block group ID to check</param>
	/// <param name="offBoardValue">The return value of the input position being off the board</param>
	/// <returns>true if there is a block at the input position, false otherwise. Also returns true if the position is out of bounds of the board. Also returns false if the block has the same specified block group ID</returns>
	public bool IsBlockAt (Vector2Int position, out Block block, int blockGroupID = -1, bool offBoardValue = true) {
		block = null;

		// Make sure the position is on the board before trying to get a block at the position
		if (!IsPositionOnBoard(position)) {
			return offBoardValue;
		}

		// Get a reference to the block at the input position
		block = GetBlockAt(position);

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
	/// Get the percentage of blocks cleared within a rectangle on the board
	/// </summary>
	/// <param name="x">The x value of the top left of the rectangular area</param>
	/// <param name="y">The y value of the top left of the rectangular area</param>
	/// <param name="width">The width of the rectangular area</param>
	/// <param name="height">The height of the rectangular area</param>
	/// <returns>The percentage value (0 - 1) of how cleared the rectangular area is</returns>
	public float GetPercentageClear (int x, int y, int width, int height) {
		// Count up all the blocks in the rectangular area
		int blockCount = 0;
		for (int i = x; i < x + width; i++) {
			for (int j = y; j > y - height; j--) {
				Block block = GetBlockAt(new Vector2Int(i, j));

				// If the block is null or it is part of a player controlled block group, then the space is considered empty
				if (block == null || (block.BlockGroupID != -1 && block.BlockGroup.IsPlayerControlled)) {
					blockCount++;
				}
			}
		}

		// Return the amount counted divided by the total blocks
		return (float) blockCount / (width * height);
	}

	/// <summary>
	/// Get all of the surround block groups to a block in all four cardinal directions
	/// </summary>
	/// <param name="block">The block to check</param>
	/// <returns>A list of all the surrounding block groups</returns>
	public List<BlockGroup> GetSurroundingBlockGroups (Block block) {
		// A list to store the surrounding block groups
		List<BlockGroup> surroundingBlockGroups = new List<BlockGroup>( );

		// Get all of the surrounding block groups
		foreach (Vector2Int neighborBlockPosition in Utils.GetCardinalPositions(block.Position)) {
			// If there is not a block at the neighboring position, then continue to the next position
			if (!IsBlockAt(neighborBlockPosition, out Block neighborBlock, offBoardValue: false)) {
				continue;
			}

			// If the neighboring block group is modified, then ignore it because it will be removed later
			if (neighborBlock.BlockGroup.IsModified) {
				continue;
			}

			surroundingBlockGroups.Add(neighborBlock.BlockGroup);
		}

		// Make the surrounding block groups list distint, meaning all duplicate values will be removed
		return surroundingBlockGroups.Distinct( ).ToList( );
	}

	/// <summary>
	/// Damage the specified block
	/// </summary>
	/// <param name="block">The block to damage</param>
	/// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
	/// <param name="dropped">Whether or not the block has been dropped below the bottom of the board (and to award different points to the player)</param>
	/// <returns>true if the block was completely destroyed, false otherwise</returns>
	public bool DamageBlock (Block block, bool destroy = false, bool dropped = false) {
		// If there is null, then do not try to destroy it
		if (block == null) {
			return false;
		}

		// Damage the block
		block.Health -= (destroy ? block.Health : 1);

		// If the block now has 0 health, as in the block has been completely destroyed
		if (block.Health == 0) {
			// If the block has a block group, make sure the block group knows that it was modified
			// The block group will be updated the next time merge block group is called
			if (block.BlockGroupID != -1) {
				block.BlockGroup.IsModified = true;
			}

			// If the block has a mino index, remove the reference to the block in the mino list
			if (block.MinoIndex != -1) {
				minos[block.MinoIndex].Remove(block);

				// If the mino index now has a size of 0, the entire mino has been destroyed
				if (minos[block.MinoIndex].Count == 0) {
					gameManager.BoardPoints = Constants.POINT_DSTRY_MINO;
				}
			}

			// Destroy the block game object
			gameManager.BoardPoints += (dropped ? Constants.POINT_DROP_BLOCK : Constants.POINT_DSTRY_BLOCK);
			Destroy(block.gameObject);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Damage the block at the specified position
	/// </summary>
	/// <param name="position">The position of the block to damage</param>
	/// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
	/// <param name="dropped">Whether or not the block has been dropped below the bottom of the board (and to award different points to the player)</param>
	/// <returns>true if the block at the specified position was completely destroyed, false otherwise</returns>
	public bool DamageBlockAt (Vector2Int position, bool destroy = false, bool dropped = false) {
		// If there is not a block at the specified position, then no block can be destroyed
		if (!IsBlockAt(position, out Block block, offBoardValue: false)) {
			return false;
		}

		return DamageBlock(block, destroy: destroy, dropped: dropped);
	}

	/// <summary>
	/// Create a new block
	/// </summary>
	/// <param name="position">The position to create the block at</param>
	/// <param name="health">The health of the block to create</param>
	public Block CreateBlock (Vector2Int position, int health = 1, BlockType blockType = BlockType.NORMAL) {
		// Create the block object
		Block block = Instantiate(blockPrefab, transform).GetComponent<Block>( );
		block.transform.position = (Vector3Int) position;
		block.Position = position;
		block.BlockType = blockType;
		block.Health = health;

		// Create a new block group object to manage the block object
		// No block should be outside of a block group
		block.BlockGroup = CreateBlockGroup( );

		return block;
	}

	/// <summary>
	/// Create a new block group
	/// </summary>
	/// <returns>The created block group object</returns>
	public BlockGroup CreateBlockGroup ( ) {
		BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform).GetComponent<BlockGroup>( );
		blockGroup.ID = blockGroupCount++;

		return blockGroup;
	}

	/// <summary>
	/// Create boom block frames from a boom block
	/// </summary>
	/// <param name="block">The block to create boom block frames from</param>
	private void GenerateBoomBlockFrames (Block block) {
		boomBlockFrames.Add(new BoomBlockFrames(this, particleManager, block));
	}

	/// <summary>
	/// Generate a random mino on the board
	/// </summary>
	private void GenerateMino ( ) {
		// Generate a random Mino from weighted percentages
		GameObject randomMinoPrefab = minoPrefabs[minoSpawnPercentages.GetWeightedValue( )];

		// Create the Mino gameobject
		gameManager.ActiveMino = Instantiate(randomMinoPrefab, minoSpawnPosition, Quaternion.identity).GetComponent<PlayerControlledBlockGroup>( );
		gameManager.ActiveMino.transform.SetParent(transform, true);
		gameManager.ActiveMino.ID = blockGroupCount++;
	}

	/// <summary>
	/// Place down the active mino onto the board
	/// </summary>
	public void PlaceActiveMino ( ) {
		for (int i = 0; i < gameManager.ActiveMino.Count; i++) {
			// If the block is a boom block, generate boom blocks frames for it
			if (gameManager.ActiveMino.GetBlock(i).IsBoomBlock) {
				GenerateBoomBlockFrames(gameManager.ActiveMino.GetBlock(i));
			}
		}

		gameManager.ActiveMino = null;
		BoardState = BoardState.MERGING_BLOCKGROUPS;
	}

	/// <summary>
	/// Update all of the block groups by merging them together or creating new ones
	/// </summary>
	private void MergeBlockGroups ( ) {
		// Get all of the blocks on the board
		Block[ ] blocks = GetComponentsInChildren<Block>( );

		for (int i = 0; i < blocks.Length; i++) {
			// Merge all of the block groups together and set it to be the block group of the block
			List<BlockGroup> surroundingBlockGroups = GetSurroundingBlockGroups(blocks[i]);

			// If there are more than one surrounding block groups, then merge them together and set that new block group to
			if (surroundingBlockGroups.Count > 0) {
				blocks[i].BlockGroup = BlockGroup.MergeAllBlockGroups(surroundingBlockGroups);
			} else {
				blocks[i].BlockGroup = CreateBlockGroup( );
			}
		}

		// Get all of the block groups on the board
		blockGroups = GetComponentsInChildren<BlockGroup>( ).ToList( );

		// Reset any variables in each of the block groups, and remove empty ones
		for (int i = blockGroups.Count - 1; i >= 0; i--) {
			blockGroups[i].CanFallBelow = false;
			blockGroups[i].IsModified = false;

			if (blockGroups[i].Count == 0) {
				Destroy(blockGroups[i].gameObject);
				blockGroups.RemoveAt(i);

				continue;
			}
		}

		// If there are more boom blocks to explode, then update them
		// If there are no more boom blocks to explode, then start to place another mino
		if (boomBlockFrames.Count > 0) {
			BoardState = BoardState.UPDATING_BOOMBLOCKS;
		} else if (needToUpdate) {
			BoardState = BoardState.UPDATING_BLOCKGROUPS;
		} else {
			BreakthroughBoardArea.OnMergeBlockGroups( );
			HazardBoardArea.OnMergeBlockGroups( );

			BoardState = BoardState.PLACING_MINO;
		}
	}

	/// <summary>
	/// Update the boom block frames
	/// </summary>
	private void UpdateBoomBlockFrames ( ) {
		// If a certain amount of time has passed, destroy the next frame of blocks
		if (Time.time - boomBlockFrameTimer >= gameManager.BoardAnimationSpeed) {
			// Loop through each of the boom blocks explosion frames
			for (int i = boomBlockFrames.Count - 1; i >= 0; i--) {
				boomBlockFrames[i].DestroyFirstFrame( );

				// If there are no more frames in the current boom block frame list, remove it from the main list
				if (boomBlockFrames[i].Count == 0) {
					boomBlockFrames.RemoveAt(i);
				}
			}

			// If there are no more boom blocks to explode, switch the update state
			if (boomBlockFrames.Count == 0) {
				BoardState = BoardState.MERGING_BLOCKGROUPS;
			}

			boomBlockFrameTimer = Time.time;
		}
	}

	/// <summary>
	/// Update the block groups by checking to see if they are still falling
	/// </summary>
	private void UpdateBlockGroups ( ) {
		bool canBlockGroupsFall = false;

		// Check to see if any of the block groups can fall (and are moving)
		for (int i = blockGroups.Count - 1; i >= 0; i--) {
			// If the current block group has a count of 0, then destroy it
			// This can happen as leftovers from merging the block groups or an entire block group can be destroyed by boom blocks
			if (blockGroups[i].Count == 0) {
				Destroy(blockGroups[i].gameObject);
				blockGroups.RemoveAt(i);

				continue;
			}

			// Update the block group
			blockGroups[i].UpdateBlockGroup( );

			// If the block group can fall, then at least one block group on the board is still updating
			if (blockGroups[i].CanFall || !blockGroups[i].IsDoneTweening) {
				canBlockGroupsFall = true;
			}
		}

		// If one of the block groups can fall, then update whether or not the block groups are locked
		// Block groups on the board are "locked" when all of them are finished moving
		if (!blockGroupsLocked && !canBlockGroupsFall) {
			blockGroupsLockedStartTime = Time.time;
			blockGroupsLocked = true;
		}

		if (blockGroupsLocked) {
			// If the block groups were locked but now one of the them can move again, then unlock them and restart the timer
			// If the timer ends then switch the board states as the block groups have finished updating
			if (canBlockGroupsFall) {
				blockGroupsLocked = false;
			} else if (Time.time - blockGroupsLockedStartTime >= gameManager.MinMinoFallTime * 2f) {
				blockGroupsLocked = false;
				BoardState = BoardState.MERGING_BLOCKGROUPS;
			}
		}
	}

	public void UpdatePercentageCleared ( ) {
		int x = 0;
		int y = gameManager.GameSettings.BoardHeight - HazardBoardArea.Height - 1;
		int width = gameManager.GameSettings.BoardWidth;
		int height = gameManager.GameSettings.BoardHeight - HazardBoardArea.Height - BreakthroughBoardArea.Height;
		gameManager.PercentCleared = GetPercentageClear(x, y, width, height) * 100f;
	}

	#region Sequences
	private IEnumerator GenerateWall ( ) {
		// Generate a random noise grid
		float[ , ] wallValues = Utils.GenerateRandomNoiseGrid(gameManager.GameSettings.BoardWidth, gameManager.WallHeight, gameManager.WallStrength - 1, gameManager.WallStrength + 1);
		for (int j = 0; j < gameManager.WallHeight; j++) {
			for (int i = 0; i < gameManager.GameSettings.BoardWidth; i++) {
				// Round the random noise grid value to an integer
				int randomValue = Mathf.RoundToInt(wallValues[i, j]);

				// Randomly increase the health of the block
				if (Random.Range(0f, 1f) < 0.1f) {
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
					CreateBlock(new Vector2Int(i, j + BreakthroughBoardArea.DefaultHeight), health: randomValue, blockType: BlockType.WALL);
				}
			}

			// Have each row of the wall generate slightly delayed of one another
			yield return new WaitForSeconds(gameManager.BoardAnimationSpeed);
		}

		BoardState = BoardState.MERGING_BLOCKGROUPS;
	}

	private IEnumerator ClearBoard (float speed) {
		// This sequence will destroy each row of the board one by one
		// Only keep looping if there is a block in the row
		bool hasBlockInRow = true;
		int y = BreakthroughBoardArea.Height;

		while (hasBlockInRow) {
			hasBlockInRow = false;

			// Loop through the entire row at a certain y value
			for (int x = 0; x < gameManager.GameSettings.BoardWidth; x++) {
				// If there is a block at the position, then remove it
				if (DamageBlockAt(new Vector2Int(x, y), true)) {
					hasBlockInRow = true;
				}
			}

			y++;

			// Wait a little bit before destroying the next row
			yield return new WaitForSeconds(speed);
		}

		// Clear and reset variables
		blockGroups.Clear( );
		blockGroupCount = 0;
	}

	private IEnumerator Breakthrough ( ) {
		// Clear the board
		yield return StartCoroutine(ClearBoard(0f));

		// Reset the board areas
		BreakthroughBoardArea.ResetHeight( );
		HazardBoardArea.ResetHeight( );
		hazardBar.ResetProgress( );

		// Reset the board points
		gameManager.TotalPoints += Mathf.RoundToInt(gameManager.BoardPoints * gameManager.PercentCleared / 100f);
		gameManager.BoardPoints = 0;

		// Update the difficulty of the game
		gameManager.UpdateDifficulty( );

		// Generate the wall
		BoardState = BoardState.GENERATE_WALL;
	}
	#endregion
}