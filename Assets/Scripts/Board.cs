using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public enum BoardUpdateState {
	PLACING_MINO, UPDATING_BOOM_BLOCKS, UPDATING_BLOCK_GROUPS, PAUSED, BREAKTHROUGH, GAME_OVER
}

public class Board : MonoBehaviour {
	[Header("Scene GameObject")]
	[SerializeField] private GameManager gameManager;
	[Space]
	[SerializeField] private BoardArea gameOverArea;
	[SerializeField] private BoardArea breakthroughArea;
	[Header("Prefabs")]
	[SerializeField] private GameObject blockPrefab;
	[SerializeField] private GameObject blockGroupPrefab;
	[SerializeField] private GameObject[ ] minoPrefabs;
	[Header("Components")]
	[SerializeField] private SpriteRenderer boardSpriteRenderer;
	[SerializeField] private SpriteRenderer borderSpriteRenderer;
	[SerializeField] private RectTransform gameCanvasRectTransform;
	[SerializeField] private RectTransform leftListRectTransform;
	[SerializeField] private RectTransform rightListRectTransform;
	[Header("Properties")]
	[SerializeField, Range(4f, 32f)] public int Width = 16;
	[SerializeField, Range(20f, 40f)] public int Height = 28;
	[SerializeField, Range(0f, 20f)] public float CameraPadding = 3;
	[SerializeField, Min(0f)] public float UIPadding = 0f;
	[SerializeField, Range(0f, 5f)] private float borderThickness = 0.75f;
	[SerializeField, Min(0f)] private float gameCanvasScale = 0.028703f; /// TODO: Figure out how this number is achieved, I got no clue
	[SerializeField, Range(0.001f, 1f)] public float BoomBlockAnimationSpeed = 0.05f;
	[Space]
	[SerializeField] private BoardUpdateState _boardUpdateState;
	[SerializeField] public Mino ActiveMino = null;
	[Space]
	[SerializeField] private float level = 0f;
	[SerializeField] private int wallHeight = 0;
	[SerializeField] private float wallRoughness = 0f;
	[SerializeField] private float wallElevation = 0f;
	[SerializeField, Min(0)] private int wallMaxHeight = 11;
	[SerializeField, Min(0)] private int wallMinHeight = 2;
	[SerializeField, Range(0f, 1f)] private float wallMaxRoughness = 2f;
	[SerializeField, Range(0f, 1f)] private float wallMinRoughness = 0.1f;
	[SerializeField, Range(0f, 1f)] private float wallMaxElevation = 1f;
	[SerializeField, Range(0f, 1f)] private float wallMinElevation = 0.1f;
	[Space]
	[SerializeField, Range(0f, 1f)] private float boomBlockSpawnChance = 0.4f;
	[SerializeField, Min(0f)] private int boomBlockGuarantee = 5;
	[HideInInspector] public int BoomBlockDrought = 0;

	// Used for tracking the boom block explosions
	private List<BoomBlockFrames> boomBlockFrames;
	private float frameTimer = 0;

	// Used for tracking what minos are left on the board
	private List<List<Block>> minoBlocks;

	public float CurrentBoomBlockSpawnPercentage {
		get {
			return ((float) BoomBlockDrought / boomBlockGuarantee) * (1 - boomBlockSpawnChance) + boomBlockSpawnChance;
		}
	}

	public BoardUpdateState BoardUpdateState {
		get {
			return _boardUpdateState;
		}

		set {
			_boardUpdateState = value;

			switch (value) {
				case BoardUpdateState.BREAKTHROUGH:
					Generate( );

					break;
				case BoardUpdateState.PLACING_MINO:
					GenerateRandomMino( );

					break;
				case BoardUpdateState.UPDATING_BOOM_BLOCKS:
					break;
				case BoardUpdateState.UPDATING_BLOCK_GROUPS:
					UpdateBlockGroups( );

					break;
			}
		}
	}

	public BoardArea GameOverBoardArea {
		get => gameOverArea;
	}
	public BoardArea BreakthroughBoardArea {
		get => breakthroughArea;
	}

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

		// Set the board size and position so the bottom left corner is at (0, 0)
		// This makes it easier when converting from piece transform position to a board array index
		boardSpriteRenderer.size = new Vector2(Width, Height);
		float positionX = (Width / 2) - (Width % 2 == 0 ? 0.5f : 0f);
		float positionY = (Height / 2) - (Height % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);

		// Set the camera orthographic size and position so it fits the entire board
		Camera.main.orthographicSize = (Height + CameraPadding) / 2f;
		Camera.main.transform.position = new Vector3(positionX, positionY, Camera.main.transform.position.z);

		// Set the size of the border
		borderSpriteRenderer.size = new Vector2(Width + (borderThickness * 2), Height + (borderThickness * 2));

		// Set game canvas dimensions
		gameCanvasRectTransform.localPosition = Vector3.zero;
		gameCanvasRectTransform.localScale = new Vector3(gameCanvasScale, gameCanvasScale, 1);
		gameCanvasRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Width + (borderThickness * 2) + UIPadding) / gameCanvasScale);
		gameCanvasRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (Height + (borderThickness * 2)) / gameCanvasScale);
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif

		boomBlockFrames = new List<BoomBlockFrames>( );
		minoBlocks = new List<List<Block>>( );

		// Set board area delegate methods
		breakthroughArea.OnMinoEnter += ( ) => {
			// Update points
			gameManager.BoardPoints += gameManager.PointsPerBreakthrough;
			Debug.Log("Points: Breakthrough");
			gameManager.TotalPoints += gameManager.BoardPoints;
			gameManager.BoardPoints = 0;

			BoardUpdateState = BoardUpdateState.BREAKTHROUGH;
		};
		gameOverArea.OnMinoLand += ( ) => {
			BoardUpdateState = BoardUpdateState.GAME_OVER;
		};
	}

	private void Start ( ) {
		wallHeight = wallMinHeight;
		wallRoughness = wallMinRoughness;
		wallElevation = wallMinElevation;

		BoardUpdateState = BoardUpdateState.BREAKTHROUGH;
	}

	private void Update ( ) {
		switch (BoardUpdateState) {
			case BoardUpdateState.UPDATING_BOOM_BLOCKS:
				frameTimer -= Time.deltaTime;
				// If a certain amount of time has passed, destroy the next frame of blocks
				if (frameTimer < 0) {
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
						BoardUpdateState = BoardUpdateState.UPDATING_BLOCK_GROUPS;
					}

					frameTimer = BoomBlockAnimationSpeed;
				}

				break;
			case BoardUpdateState.UPDATING_BLOCK_GROUPS:
				// Wait until all block groups have finished moving
				bool blockGroupsCanMove = false;
				foreach (BlockGroup blockGroup in GetComponentsInChildren<BlockGroup>( )) {
					if (blockGroup.CanMove) {
						blockGroupsCanMove = true;

						break;
					}
				}

				// Once all of the block groups cannot move anymore, spawn another mino for the player
				if (!blockGroupsCanMove) {
					BoardUpdateState = BoardUpdateState.PLACING_MINO;
				}

				break;
		}
	}

	/// <summary>
	/// Generate the wall that appears in the level
	/// </summary>
	private void Generate ( ) {
		// Remove all previous blocks from the board
		RemoveAllBlocksFromBoard( );

		// Generate the wall
		float[ , ] wallValues = Utils.GeneratePerlinNoiseGrid(Width, wallHeight, wallRoughness, 4, wallElevation);
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < wallHeight; j++) {
				// Make sure the perlin noise value can be converted to a wall block
				int perlinValue = (int) Mathf.Clamp(Mathf.Round(wallValues[i, j]), 0, 3);

				// If the perlin noise value is greater than 0, a wall block will spawn
				// If it is less than or equal to 0, there will be a gap in the wall at that point
				if (perlinValue > 0) {
					Block block = Instantiate(blockPrefab, new Vector3(i, j + breakthroughArea.Height), Quaternion.identity).GetComponent<Block>( );
					block.Health = perlinValue;

					AddBlockToBoard(block);
				}
			}
		}

		// Increase the difficulty of the game
		UpdateDifficulty( );

		// Update the state to start placing minos
		BoardUpdateState = BoardUpdateState.PLACING_MINO;
	}

	/// <summary>
	/// Generate a random Mino at the top of the game board
	/// </summary>
	private void GenerateRandomMino ( ) {
		/// TODO: Make getting specific transform positions on the board cleaner in code

		// The spawn position is going to be near the top middle of the board
		Vector3 spawnPosition = new Vector3((Width / 2) - 0.5f, Height - (Height - gameOverArea.Height) * 0.25f - 0.5f);

		// Spawn a random type of mino
		ActiveMino = Instantiate(minoPrefabs[Random.Range(0, minoPrefabs.Length)], spawnPosition, Quaternion.identity).GetComponent<Mino>( );
	}

	/// <summary>
	/// Add a Mino object to the game board
	/// </summary>
	/// <param name="mino">The Mino to add</param>
	public void AddLandedMinoToBoard (Mino mino) {
		// Get an empty mino index to add this mino's blocks to
		int emptyMinoIndex = GetFirstEmptyMinoIndex( );

		// Add all blocks that are part of the mino to the board
		while (mino.transform.childCount > 0) {
			// Get a block from the mino
			Block block = mino.transform.GetChild(0).GetComponent<Block>( );

			// Add the block to the board
			minoBlocks[emptyMinoIndex].Add(block);
			AddBlockToBoard(block);
			block.MinoIndex = emptyMinoIndex;
		}

		// If the mino that was added is the active mino (meaning it was being dropped by the player) then set the active mino to null and wait for a new mino to spawn
		if (mino == ActiveMino) {
			ActiveMino = null;
		}

		Destroy(mino.gameObject);

		// Only start to update the boom blocks if the current state is not a breakthrough
		// When the code is in a state of a breakthrough, all of the wall is being regenerated and it makes no sense to update boom blocks
		if (BoardUpdateState != BoardUpdateState.BREAKTHROUGH) {
			BoardUpdateState = BoardUpdateState.UPDATING_BOOM_BLOCKS;
		}
	}

	/// <summary>
	/// Add a boom block that will be exploded.
	/// </summary>
	/// <param name="boomBlock">The boom block to add</param>
	private void AddBoomBlock (Block boomBlock) {
		boomBlockFrames.Add(new BoomBlockFrames(this, gameManager, boomBlock));
	}

	private void UpdateBlockGroups ( ) {
		// Get the current block groups on the board
		BlockGroup[ ] blockGroups = GetComponentsInChildren<BlockGroup>( );
		int mergeToGroupIndex = -1;

		// Merge all of the modified block groups into one block group
		for (int i = blockGroups.Length - 1; i >= 0; i--) {
			if (blockGroups[i].IsModified) {
				if (mergeToGroupIndex < 0) {
					mergeToGroupIndex = i;
				} else {
					blockGroups[i].MergeToBlockGroup(blockGroups[mergeToGroupIndex]);
				}
			}
		}

		// If there were no modifed block groups, then return and do no moving of blocks
		if (mergeToGroupIndex == -1) {
			return;
		}

		// For each of the blocks contained within modified block groups, add them back to the board in new block groups
		// The "true" will exclude the block group this block is currently in when checking for surrounding groups, so no blocks will be re-added to this group
		foreach (Block block in blockGroups[mergeToGroupIndex].GetComponentsInChildren<Block>( )) {
			AddBlockToBoard(block, true);
		}

		// Destroy this group once all blocks have been moved
		Destroy(blockGroups[mergeToGroupIndex].gameObject);
	}

	/// <summary>
	/// Update all board areas.
	/// </summary>
	public void UpdateBoardAreas ( ) {
		breakthroughArea.UpdateDelegates( );
		gameOverArea.UpdateDelegates( );
	}

	private void UpdateDifficulty ( ) {
		// Increase the difficulty of the game
		level += 0.16666666666f;
		wallHeight = Mathf.Min(Mathf.RoundToInt(Mathf.Sqrt(4.5f * level) + wallMinHeight), wallMaxHeight);
		wallRoughness = Mathf.Min(level * level * 0.006f + wallMinRoughness, wallMaxRoughness);
		wallElevation = Mathf.Min(level * level * 0.02f + wallMinElevation, wallMaxElevation);
	}

	/// <summary>
	/// Add a block to the game board
	/// </summary>
	/// <param name="block">The block to be added</param>
	/// <param name="excludeCurrentBlockGroup">Whether or not the exclude the block's current block group when checking for surrounding block groups</param>
	private void AddBlockToBoard (Block block, bool excludeCurrentBlockGroup = false) {
		// Make sure the block exists
		if (block == null) {
			return;
		}

		// Get the surrounding block groups of the block that was just added
		List<BlockGroup> blockGroups = GetSurroundingBlockGroups(block, excludeCurrentBlockGroup);

		// If the block has no surrounding block groups, create a new one
		if (blockGroups.Count == 0) {
			// Create a new blockgroup gameobject
			BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform.position, Quaternion.identity).GetComponent<BlockGroup>( );
			// Set the parent of the blockgroup gameobject to this board gameobject
			blockGroup.transform.SetParent(transform, true);
			// Set the block parent to the new blockgroup gameobject
			block.transform.SetParent(blockGroup.transform, true);
		} else {
			// If the block has one or more block groups surrounding it, merge those groups together and add it to the merged block group
			block.transform.SetParent(BlockGroup.MergeAllBlockGroups(blockGroups).transform, true);
		}

		if (block.IsBoomBlock) {
			AddBoomBlock(block);
		}
	}

	/// <summary>
	/// Remove a block from the board
	/// </summary>
	/// <param name="position">The position of the block to try and remove</param>
	/// <returns>Whether or not the block was removed</returns>
	public bool RemoveBlockFromBoard (Vector3 position) {
		return RemoveBlockFromBoard(GetBlockAtPosition(position));
	}

	/// <summary>
	/// Remove a block from the board
	/// </summary>
	/// <param name="block">The block to remove</param>
	/// <param name="ignoreHealth">Whether or not ignore the blocks health. If set to true, the block will definitely be destroyed no matter how damaged it is</param>
	/// <returns>Whether or not the block was destroyed</returns>
	public bool RemoveBlockFromBoard (Block block, bool ignoreHealth = false) {
		// Make sure the block exists
		if (block == null) {
			return false;
		}

		// Make sure the block group that the block was a part of is marked as modified
		block.BlockGroup.IsModified = true;

		int blockMinoIndex = block.MinoIndex;
		int blockIndex = minoBlocks[blockMinoIndex].IndexOf(block);

		block.Health -= (ignoreHealth ? block.Health : 1);

		// If the health has reached 0, then it has been destroyed
		// If the block is null here then the block was destroyed
		if (block == null) {
			// If the block has a mino index, it was once part of a mino
			// If it does not have a mino index, then it was originally part of the wall
			if (blockMinoIndex != -1 && blockIndex != -1) {
				// If the block gets destroyed, a full mino may have also been destroyed
				// Give the player some bonus points if this happens
				minoBlocks[blockMinoIndex].RemoveAt(blockIndex);
				if (minoBlocks[blockMinoIndex].Count == 0) {
					gameManager.BoardPoints += gameManager.PointsPerDestroyedMino;
					Debug.Log("Points: Full Mino");
				}
			}

			return true;
		}

		return false;
	}

	public void RemoveAllBlocksFromBoard ( ) {
		// Get all block groups and minos in the scene
		BlockGroup[ ] blockGroups = FindObjectsOfType<BlockGroup>( );
		Mino[ ] minos = FindObjectsOfType<Mino>( );

		// Destroy all of them
		for (int i = blockGroups.Length - 1; i >= 0; i--) {
			Destroy(blockGroups[i].gameObject);
		}
		for (int i = minos.Length - 1; i >= 0; i--) {
			Destroy(minos[i].gameObject);
		}
	}

	/// <summary>
	/// Check to see if a position:
	/// (1) is in the bounds of the board
	/// (2) is not already occupied by a block
	/// (3) if the block at the position has the specified parent transform
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <param name="parent">The parent transform to check</param>
	/// <returns>Returns 'true' if the position is valid</returns>
	public bool IsPositionValid (Vector3 position, Transform parent = null) {
		Block block = GetBlockAtPosition(position);

		bool isInBounds = (position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height);
		bool isBlockAtPosition = (block != null);
		bool hasParentTransform = (parent != null && block != null && block.transform.parent == parent);

		return (isInBounds && (!isBlockAtPosition || hasParentTransform));
	}

	public Block GetBlockAtPosition (Vector3 position) {
		RaycastHit2D hit = Physics2D.Raycast(position + Vector3.back, Vector3.forward);
		if (hit) {
			return hit.transform.GetComponent<Block>( );
		}

		return null;
	}

	/// <summary>
	/// Get the surrounding block groups to a current block
	/// </summary>
	/// <param name="block">The block to check around</param>
	/// <param name="excludeCurrentBlockGroup">Whether or not the exclude the block's current block group when checking for surrounding block groups</param>
	/// <returns>A list of all surrounding block groups</returns>
	private List<BlockGroup> GetSurroundingBlockGroups (Block block, bool excludeCurrentBlockGroup = false) {
		List<BlockGroup> surroundingGroups = new List<BlockGroup>( );

		foreach (Block neighborBlock in GetSurroundingBlocks(block.Position)) {
			// If there is a block at the neighboring position and it has a new block group, add it to the surrounding block group list
			// Also, make sure the neighbor block does or does not have the same group as the block parameter
			bool isNewBlockGroup = (neighborBlock.BlockGroup != null && !surroundingGroups.Contains(neighborBlock.BlockGroup));
			bool isExcludedBlockGroup = (excludeCurrentBlockGroup && neighborBlock.BlockGroup == block.BlockGroup);

			if (isNewBlockGroup && !isExcludedBlockGroup) {
				surroundingGroups.Add(neighborBlock.BlockGroup);
			}
		}

		return surroundingGroups;
	}

	/// <summary>
	/// Get all surrounding blocks to a position vector
	/// </summary>
	/// <param name="position">The position to check around</param>
	/// <param name="excludeNullBlocks">Whether or not to exclude null values from the returned list</param>
	/// <returns>A list of all surrounding blocks to the position vector</returns>
	private List<Block> GetSurroundingBlocks (Vector3 position, bool excludeNullBlocks = true) {
		List<Block> surroundingBlocks = new List<Block>( );

		foreach (Vector3 cardinalPosition in Utils.GetCardinalPositions(position)) {
			Block neighborBlock = GetBlockAtPosition(cardinalPosition);

			// If there is a block at the neighboring position, add it to the list
			if (!excludeNullBlocks || (excludeNullBlocks && neighborBlock != null)) {
				surroundingBlocks.Add(neighborBlock);
			}
		}

		return surroundingBlocks;
	}

	/// <summary>
	/// Get the first empty index in the minoBlocks array. If there are no empty indices, then a new one is created at the end.
	/// </summary>
	/// <returns>The first occurance of an empty index in the array</returns>
	private int GetFirstEmptyMinoIndex ( ) {
		// Create a new index for this mino to track which blocks of it are left on the board
		for (int i = 0; i < minoBlocks.Count; i++) {
			if (minoBlocks[i].Count == 0) {
				return i;
			}
		}

		minoBlocks.Add(new List<Block>( ));
		return minoBlocks.Count - 1;
	}
}