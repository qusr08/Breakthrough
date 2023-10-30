using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum BoardState {
	UPDATING_MINO, MERGING_BLOCKGROUPS, UPDATING_BLOCKGROUPS, UPDATING_BOOMBLOCKS, BREAKTHROUGH, GENERATING_WALL
}

public class Board : MonoBehaviour {
	[SerializeField, Tooltip("The prefab object for wall blocks.")] private GameObject wallBlockPrefab;
	[SerializeField, Tooltip("The prefab object for mino blocks.")] private GameObject minoBlockPrefab;
	[SerializeField, Tooltip("The prefab object for boom blocks.")] private GameObject boomBlockPrefab;
	[SerializeField, Tooltip("The prefab object for block groups.")] private GameObject blockGroupPrefab;
	[SerializeField, Tooltip("A reference to the game manager.")] private GameManager gameManager;
	[SerializeField, Tooltip("A reference to the breakthrough board area.")] private BreakthroughBoardArea _breakthroughBoardArea;
	[SerializeField, Tooltip("A reference to the hazard board area.")] private HazardBoardArea _hazardBoardArea;
	[SerializeField, Tooltip("The current state of the board.")] private BoardState _boardState;
	[SerializeField, Tooltip("A list of all the block groups on the board.")] private List<BlockGroup> _blockGroups;

	private Block[ , ] grid;

	private Vector2 minoSpawnPosition;
	private WeightedList<MinoType> weightedMinoList;

	private bool needToUpdateBlockGroups;

	#region Properties
	/// <summary>
	///		A reference to the breakthrough board area
	/// </summary>
	public BreakthroughBoardArea BreakthroughBoardArea => _breakthroughBoardArea;

	/// <summary>
	///		A reference to the hazard board area
	/// </summary>
	public HazardBoardArea HazardBoardArea => _hazardBoardArea;

	/// <summary>
	///		A list of all the block groups that are currently on the board
	/// </summary>
	public List<BlockGroup> BlockGroups { get => _blockGroups; private set => _blockGroups = value; }

	/// <summary>
	///		The current state of the board processes
	/// </summary>
	public BoardState BoardState {
		get => _boardState;
		set {
			_boardState = value;

			Debug.Log($"BOARD STATE = {value}");

			switch (_boardState) {
				case BoardState.UPDATING_MINO:
					gameManager.ActiveMino = SpawnRandomMino( );
					break;
				case BoardState.MERGING_BLOCKGROUPS:
					MergeBlockGroups( );
					break;
				case BoardState.UPDATING_BLOCKGROUPS:
					needToUpdateBlockGroups = false;
					break;
				case BoardState.UPDATING_BOOMBLOCKS:
					needToUpdateBlockGroups = true;
					break;
				case BoardState.BREAKTHROUGH:
					StartCoroutine(BreakthroughProcess( ));
					break;
				case BoardState.GENERATING_WALL:
					needToUpdateBlockGroups = true;
					StartCoroutine(GenerateWallProcess( ));
					break;
			}
		}
	}
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		gameManager = FindObjectOfType<GameManager>( );
	}

	private void Awake ( ) {
		OnValidate( );

		// Calculate the spawn position of Minos
		float offsetX = GameSettingsManager.Instance.BoardWidth % 2 == 0 ? 0.5f : 0.0f;
		minoSpawnPosition = new Vector2((GameSettingsManager.Instance.BoardWidth / 2f) - offsetX, GameSettingsManager.Instance.BoardHeight - 2.5f);

		// Initialize the arrays
		grid = new Block[GameSettingsManager.Instance.BoardWidth, GameSettingsManager.Instance.BoardHeight];
		BlockGroups = new List<BlockGroup>( );

		// Convert the enum types to a list
		List<MinoType> minoTypeList = Enum.GetValues(typeof(MinoType)).Cast<MinoType>( ).ToList( );

		// Convert the allowed minos integer to a list of booleans
		List<bool> enabledMinoTypes = new List<bool>( );
		for (int i = 0; i < minoTypeList.Count; i++) {
			enabledMinoTypes.Add(((GameSettingsManager.Instance.AllowedMinos >> i) & 1) == 1);
		}

		// Initialize the mino weighted list array
		weightedMinoList = new WeightedList<MinoType>(minoTypeList, enabledMinoTypes, gameManager.MinoWeightPercentage);
	}

	private void Start ( ) {
		BoardState = BoardState.BREAKTHROUGH;
	}

	private void Update ( ) {
		switch (BoardState) {
			case BoardState.UPDATING_MINO:
				break;
			case BoardState.MERGING_BLOCKGROUPS:
				break;
			case BoardState.UPDATING_BLOCKGROUPS:
				break;
			case BoardState.UPDATING_BOOMBLOCKS:
				break;
		}
	}
	#endregion

	/// <summary>
	///		Check to see if the specified position is on the board
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		<strong>true</strong> if the specified position is on the board<br/>
	///		<strong>false</strong> if the specified position is not on the board
	/// </returns>
	public bool IsPositionOnBoard (Vector2Int position) {
		// See if the position is outside of the x axis bounds
		if (position.x < 0 || position.x >= GameSettingsManager.Instance.BoardWidth) {
			return false;
		}

		// See if the position is outside of the y axis bounds
		if (position.y < 0 || position.y >= GameSettingsManager.Instance.BoardHeight) {
			return false;
		}

		return true;
	}

	/// <summary>
	///		Get a reference to a block at the specified position
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		<strong>Block</strong> if the position is on the board and there is a block at the position<br/>
	///		<strong>null</strong> if the position is not on the board<br/>
	///		<strong>null</strong> if there is no block at the position
	/// </returns>
	public Block GetBlockAt (Vector2Int position) {
		// Make sure the position is on the board
		if (!IsPositionOnBoard(position)) {
			return null;
		}

		return grid[position.x, position.y];
	}

	/// <summary>
	///		Get a reference to a block group at the specified position
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		<strong>BlockGroup</strong> if the position is on the board and there is a block at the position<br/>
	///		<strong>null</strong> if the position is not on the board<br/>
	///		<strong>null</strong> if there is no block at the position
	/// </returns>
	public BlockGroup GetBlockGroupAt (Vector2Int position) {
		// Get the block at the specified position
		Block block = GetBlockAt(position);

		// If the block is null, then the block group will also be null
		if (block == null) {
			return null;
		}

		return block.BlockGroup;
	}

	/// <summary>
	///		Damage a block by a specified amount
	/// </summary>
	/// <param name="block">The block to damage</param>
	/// <param name="damage">The amount of damage to inflict</param>
	public void DamageBlock (Block block, int damage) {
		// If the block is null, then do not try to damage it
		if (block == null) {
			return;
		}

		// Remove health from the block
		block.Health -= damage;
	}

	/// <summary>
	///		Damage a block at the specified position by a specified amount
	/// </summary>
	/// <param name="position">The position of the block to damage</param>
	/// <param name="damage">The amount of damage to inflict</param>
	public void DamageBlockAt (Vector2Int position, int damage) => DamageBlock(GetBlockAt(position), damage);

	/// <summary>
	///		Destroy a block at a specified position
	/// </summary>
	/// <param name="position">The position of the block to destroy</param>
	public void DestroyBlockAt (Vector2Int position) => DamageBlock(GetBlockAt(position), 999);

	/// <summary>
	///		Move a block from the position it is currently in to the specified position
	/// </summary>
	/// <param name="block">The block to move</param>
	/// <param name="position">The position to move the block to</param>
	public void MoveBlockTo (Block block, Vector2Int position) {
		// If the block is equal to null, then do not try and change its position
		if (block == null) {
			return;
		}

		// Set the current location of the block in the grid array to null
		// Only do this if the block has a valid position
		if (IsPositionOnBoard(block.Position)) {
			grid[block.Position.x, block.Position.y] = null;
		}

		// Set the new position of the block
		// Only do this if the inputted position is valid
		if (IsPositionOnBoard(position)) {
			block.Position = position;
			grid[position.x, position.y] = block;
		}
	}

	/// <summary>
	///		Create a new wall block on the board
	/// </summary>
	/// <param name="position">The position of the wall block</param>
	/// <param name="health">The starting health of the wall block</param>
	/// <param name="blockGroup">The starting block group of the wall block</param>
	/// <returns>
	///		<strong>Block</strong> that is the created wall block
	/// </returns>
	public Block CreateWallBlockAt (Vector2Int position, int health = 1, BlockGroup blockGroup = null) {
		// Create a wall block object
		WallBlock wallBlock = Instantiate(wallBlockPrefab, transform).GetComponent<WallBlock>( );

		return InitializeBlock(wallBlock, position, health: health, blockGroup: blockGroup);
	}

	/// <summary>
	///		Create a new mino block on the board
	/// </summary>
	/// <param name="position">The position of the mino block</param>
	/// <param name="minoType">The type (or color) of mino that the block is a part of</param>
	/// <param name="health">The starting health of the mino block</param>
	/// <param name="blockGroup">The starting block group of the mino block</param>
	/// <returns>
	///		<strong>Block</strong> that is the created mino block
	/// </returns>
	public Block CreateMinoBlockAt (Vector2Int position, MinoType minoType, BlockGroup blockGroup = null) {
		// Create a mino block object
		MinoBlock minoBlock = Instantiate(minoBlockPrefab, transform).GetComponent<MinoBlock>( );
		minoBlock.MinoType = minoType;

		return InitializeBlock(minoBlock, position, blockGroup: blockGroup);
	}

	/// <summary>
	///		Create a new boom block on the board
	/// </summary>
	/// <param name="position">The position of the boom block</param>
	/// <param name="boomBlockType">The type of boom block to create</param>
	/// <param name="minoType">The type (or color) of mino that the block is a part of</param>
	/// <param name="blockGroup">The starting block group of the boom block</param>
	/// <returns>
	///		<strong>Block</strong> that is the created boom block
	/// </returns>
	public Block CreateBoomBlockAt (Vector2Int position, BoomBlockType boomBlockType, MinoType minoType, BlockGroup blockGroup = null) {
		// Create a boom block object
		BoomBlock boomBlock = Instantiate(boomBlockPrefab, transform).GetComponent<BoomBlock>( );
		boomBlock.BoomBlockType = boomBlockType;
		boomBlock.MinoType = minoType;

		return InitializeBlock(boomBlock, position, blockGroup: blockGroup);
	}

	/// <summary>
	///		Initialize the variables of a newly created block
	/// </summary>
	/// <param name="block">The block to initialize</param>
	/// <param name="position">The position of the block</param>
	/// <param name="health">The starting health of the block</param>
	/// <param name="blockGroup">The starting block group of the block</param>
	/// <returns>
	///		<strong>Block</strong> that is the initialize block
	/// </returns>
	private Block InitializeBlock (Block block, Vector2Int position, int health = 1, BlockGroup blockGroup = null) {
		// Set the position temporarily to be off the board
		// Needed to properly set the position of this block in the grid array
		block.Position = -Vector2Int.one;

		// Initialize all general block variables
		block.SetLocation(position);
		block.Health = health;

		// Add the block to the block group
		if (blockGroup == null) {
			blockGroup = CreateBlockGroup( );
		}
		block.BlockGroup = blockGroup;

		return block;
	}

	/// <summary>
	///		Spawn a random mino on the board
	/// </summary>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the spawned mino block group
	/// </returns>
	public BlockGroup SpawnRandomMino ( ) {
		// Create a Mino block group
		BlockGroup minoBlockGroup = CreateBlockGroup(position: minoSpawnPosition, isPlayerControlled: true);

		// Get a random weighted Mino type from the weighted array
		// The array will internally update its percentage values
		MinoType selectedMinoType = weightedMinoList.GetWeightedValue( );

		// Get block data for the selected Mino type
		List<Vector2> blockData = MinoData.MinoBlockData[selectedMinoType];

		// Check to see if a boom block will spawn on this Mino
		int boomBlockIndex = -1;
		if (UnityEngine.Random.Range(0f, 1f) <= gameManager.BoomBlockSpawnChance) {
			// If a boom block will spawn, choose a random index for it to spawn on
			boomBlockIndex = UnityEngine.Random.Range(0, blockData.Count);
			gameManager.BoomBlockDrought = 0;
		} else {
			gameManager.BoomBlockDrought++;
		}

		// Loop through all of the selected Mino's block positions and spawn a block in
		for (int i = 0; i < blockData.Count; i++) {
			// Calculate the board position of the block
			Vector2Int blockPosition = Utils.Vect2Round(blockData[i] + (Vector2) minoBlockGroup.transform.position);

			// If the current index is the index that was chosen to be a Boom Block, then spawn a Boom Block instead of a Mino Block
			if (i == boomBlockIndex) {
				BoomBlockType boomBlockType = Utils.GetRandomEnumValue<BoomBlockType>( );
				CreateBoomBlockAt(blockPosition, boomBlockType, selectedMinoType, blockGroup: minoBlockGroup);
			} else {
				CreateMinoBlockAt(blockPosition, selectedMinoType, blockGroup: minoBlockGroup);
			}
		}

		return minoBlockGroup;
	}



	/// <summary>
	///		Create a new block group
	/// </summary>
	/// <param name="position">
	///		The position of the block group on the board<br/>
	///		This should only be used if the block group will be player controlled and needs a rotational pivot point
	///	</param>
	/// <param name="isPlayerControlled">Whether or not the block group can be controlled by player inputs</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the created block group
	///	</returns>
	public BlockGroup CreateBlockGroup (Vector2 position = default, bool isPlayerControlled = false) {
		BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform).GetComponent<BlockGroup>( );
		blockGroup.transform.position = position;
		blockGroup.IsPlayerControlled = isPlayerControlled;
		blockGroup.IsModified = false;
		BlockGroups.Add(blockGroup);

		return blockGroup;
	}

	/// <summary>
	///		Merge all block groups currently on the board together
	/// </summary>
	private void MergeBlockGroups ( ) {
		// Loop through every board position on the board
		for (int y = 0; y < GameSettingsManager.Instance.BoardHeight; y++) {
			for (int x = 0; x < GameSettingsManager.Instance.BoardWidth; x++) {
				// Get a reference to the block at the current position
				Vector2Int position = new Vector2Int(x, y);
				Block block = GetBlockAt(position);

				// If there is no block at the position, move to the next position
				if (block == null) {
					continue;
				}

				// Get the surrounding block groups to the current block
				List<BlockGroup> surroundingBlockGroups = GetSurroundingBlockGroups(block);

				if (surroundingBlockGroups.Count > 0) {
					// Make the first block group in the merge list the block group that all blocks will merge into
					BlockGroup mergedBlockGroup = surroundingBlockGroups[0];
					surroundingBlockGroups.RemoveAt(0);

					// While there are still block groups to merge, merge them together
					while (surroundingBlockGroups.Count > 0) {
						mergedBlockGroup = mergedBlockGroup.MergeBlockGroup(surroundingBlockGroups[0]);
						surroundingBlockGroups.RemoveAt(0);
					}

					// Transfer the block to the final merged block group
					block.BlockGroup = mergedBlockGroup;
				} else {
					// Create a new block group and transfer the block to it
					block.BlockGroup = CreateBlockGroup( );
				}
			}
		}
	}

	/// <summary>
	///		Get a list of the surround block groups to the specified block
	/// </summary>
	/// <param name="block">The block to check the surrounding block groups of</param>
	/// <param name="countCurrentBlockGroup">Whether or not to count the block group that the specified block is currently part of as a surrounding block group</param>
	/// <returns>
	///		<strong>BlockGroup List</strong> that contains all of the surrounding block groups to the specified block
	/// </returns>
	private List<BlockGroup> GetSurroundingBlockGroups (Block block, bool countCurrentBlockGroup = true) {
		// Create a list to store the block groups
		List<BlockGroup> surroundingBlockGroups = new List<BlockGroup>( );

		// Make sure the block is not null before proceeding
		if (block == null) {
			return surroundingBlockGroups;
		}

		// Get all of the block groups that surround the inputted block
		List<Vector2Int> cardinalPositions = Utils.GetCardinalPositions(block.Position);

		// Include the block's current block group if it is specified
		if (countCurrentBlockGroup) {
			cardinalPositions.Add(block.Position);
		}

		foreach (Vector2Int cardinalPosition in cardinalPositions) {
			// Get the cardinal block group
			BlockGroup cardinalBlockGroup = GetBlockGroupAt(cardinalPosition);

			// If the cardinal block group is null, then continue to the next cardinal position
			if (cardinalBlockGroup == null) {
				continue;
			}

			// If the cardinal block group is modified, then continue to the next cardinal position
			// Since the block group was modified, it needs to be regenerated into new block groups to prevent floating blocks
			if (cardinalBlockGroup.IsModified) {
				continue;
			}

			// If the block group passes all the above checks, then add it as a surrounding block group
			surroundingBlockGroups.Add(cardinalBlockGroup);
		}

		// Make sure all elements in the surrounding block groups list are different
		return surroundingBlockGroups.Distinct( ).ToList( );
	}

	/// <summary>
	///		Generate the wall on the board
	/// </summary>
	/// <returns></returns>
	public IEnumerator GenerateWallProcess ( ) {
		// Generate a random noise grid
		float[ , ] wallValues = Utils.GenerateRandomNoiseGrid(GameSettingsManager.Instance.BoardWidth, gameManager.WallHeight, gameManager.WallStrength - 1, gameManager.WallStrength + 1);
		for (int j = 0; j < gameManager.WallHeight; j++) {
			for (int i = 0; i < GameSettingsManager.Instance.BoardWidth; i++) {
				// Round the random noise grid value to an integer
				int randomValue = Mathf.RoundToInt(wallValues[i, j]);

				// Randomly increase the health of the block
				if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
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
					CreateWallBlockAt(new Vector2Int(i, j + BreakthroughBoardArea.DefaultHeight), health: randomValue);
				}
			}

			// Have each row of the wall generate slightly delayed of one another
			yield return new WaitForSeconds(gameManager.FastMinoMoveTime);
		}

		BoardState = BoardState.MERGING_BLOCKGROUPS;
	}

	/// <summary>
	///		Reset the board once the player gets a breakthrough
	/// </summary>
	/// <returns></returns>
	public IEnumerator BreakthroughProcess ( ) {
		// Clear the board
		yield return new WaitForEndOfFrame( );

		// Reset board area heights
		BreakthroughBoardArea.Height = BreakthroughBoardArea.DefaultHeight;
		HazardBoardArea.Height = HazardBoardArea.DefaultHeight;

		// Reset game point totals
		gameManager.TotalPoints += Mathf.RoundToInt(gameManager.BoardPoints * (gameManager.PercentageCleared / 100f));

		// Update the difficulty of the game
		gameManager.UpdateDifficulty( );

		// Generate a new wall
		BoardState = BoardState.GENERATING_WALL;
	}
}
