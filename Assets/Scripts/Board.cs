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
	public BreakthroughBoardArea BreakthroughBoardArea => _breakthroughBoardArea;
	public HazardBoardArea HazardBoardArea => _hazardBoardArea;
	public List<BlockGroup> BlockGroups { get => _blockGroups; private set => _blockGroups = value; }

	public BoardState BoardState {
		get => _boardState;
		set {
			_boardState = value;

			switch (_boardState) {
				case BoardState.UPDATING_MINO:
					needToUpdateBlockGroups = true;
					gameManager.ActiveMino = SpawnRandomMino( );
					break;
				case BoardState.MERGING_BLOCKGROUPS:
					needToUpdateBlockGroups = true;
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

	public bool IsPositionValid (Vector2Int position) {
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

	public Block GetBlockAt (Vector2Int position) {
		// Make sure the position is on the board
		if (!IsPositionValid(position)) {
			return null;
		}

		return grid[position.x, position.y];
	}

	public BlockGroup GetBlockGroupAt (Vector2Int position) {
		// Get the block at the specified position
		Block block = GetBlockAt(position);

		// If the block is null, then the block group will also be null
		if (block == null) {
			return null;
		}

		// Return the block's block group
		// This may turn out to be null as well
		return block.BlockGroup;
	}

	public void DamageBlock (Block block, int damage) {
		// If the block is null, then do not try to damage it
		if (block == null) {
			return;
		}

		// Remove health from the block
		block.Health -= damage;
	}

	public void DamageBlockAt (Vector2Int position, int damage) => DamageBlock(GetBlockAt(position), damage);

	public void DestroyBlockAt (Vector2Int position) => DamageBlock(GetBlockAt(position), 999);

	public void MoveBlockTo (Block block, Vector2Int position) {
		// If the block is equal to null, then do not try and change its position
		if (block == null) {
			return;
		}

		// Set the current location of the block in the grid array to null
		// Only do this if the block has a valid position
		if (IsPositionValid(block.Position)) {
			grid[block.Position.x, block.Position.y] = null;
		}

		// Set the new position of the block
		// Only do this if the inputted position is valid
		if (IsPositionValid(position)) {
			block.Position = position;
			grid[position.x, position.y] = block;
		}
	}

	public Block CreateWallBlockAt (Vector2Int position, int health = 1, BlockGroup blockGroup = null) {
		// Create a wall block object
		WallBlock wallBlock = Instantiate(wallBlockPrefab, transform).GetComponent<WallBlock>( );

		return InitializeBlock(wallBlock, position, health: health, blockGroup: blockGroup);
	}

	public Block CreateMinoBlockAt (Vector2Int position, MinoType minoType, int health = 1, BlockGroup blockGroup = null) {
		// Create a mino block object
		MinoBlock minoBlock = Instantiate(minoBlockPrefab, transform).GetComponent<MinoBlock>( );
		minoBlock.MinoType = minoType;

		return InitializeBlock(minoBlock, position, health: health, blockGroup: blockGroup);
	}

	public Block CreateBoomBlockAt (Vector2Int position, BoomBlockType boomBlockType, int health = 1, BlockGroup blockGroup = null) {
		// Create a boom block object
		BoomBlock boomBlock = Instantiate(boomBlockPrefab, transform).GetComponent<BoomBlock>( );
		boomBlock.BoomBlockType = boomBlockType;

		return InitializeBlock(boomBlock, position, health: health, blockGroup: blockGroup);
	}

	private Block InitializeBlock (Block block, Vector2Int position, int health = 1, BlockGroup blockGroup = null) {
		// Initialize all general block variables
		block.SetLocation(position);
		block.Health = health;

		// Add the block to the block group
		if (blockGroup == null) {
			blockGroup = CreateBlockGroup( );
		}
		blockGroup.TransferBlock(block);

		return block;
	}

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
				CreateBoomBlockAt(blockPosition, boomBlockType, blockGroup: minoBlockGroup);
			} else {
				CreateMinoBlockAt(blockPosition, selectedMinoType, blockGroup: minoBlockGroup);
			}
		}

		return minoBlockGroup;
	}

	public BlockGroup CreateBlockGroup (Vector2 position = default, bool isPlayerControlled = false) {
		BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform).GetComponent<BlockGroup>( );
		blockGroup.transform.position = position;
		blockGroup.IsPlayerControlled = isPlayerControlled;
		BlockGroups.Add(blockGroup);

		return blockGroup;
	}

	public void MergeBlockGroups ( ) {
		// Set all current block groups to be old
		foreach (BlockGroup blockGroup in BlockGroups) {
			blockGroup.IsOld = true;
		}

		// Loop through every board position on the board
		for (int x = 0; x < GameSettingsManager.Instance.BoardWidth; x++) {
			for (int y = 0; y < GameSettingsManager.Instance.BoardHeight; y++) {
				// Get a reference to the block at the current position
				Vector2Int position = new Vector2Int(x, y);
				Block block = GetBlockAt(position);

				// If there is no block at the position, move to the next position
				if (block == null) {
					continue;
				}

				// Get the surrounding block groups to the current block
				List<BlockGroup> surroundingBlockGroups = new List<BlockGroup>( );
				List<Vector2Int> cardinalPositions = Utils.GetCardinalDirections(position);
				foreach (Vector2Int cardinalPosition in cardinalPositions) {
					// Get the cardinal block group
					BlockGroup cardinalBlockGroup = GetBlockGroupAt(cardinalPosition);

					// If the cardinal block group is null, then continue to the next cardinal position
					if (cardinalBlockGroup == null) {
						continue;
					}

					// If the block group is old, then continue to the next cardinal position
					if (cardinalBlockGroup.IsOld) {
						continue;
					}

					// If the block group has already been added as a surrounding block group, continue
					if (surroundingBlockGroups.Contains(cardinalBlockGroup)) {
						continue;
					}

					// If the block group passes all the above checks, then add it as a surrounding block group
					surroundingBlockGroups.Add(cardinalBlockGroup);
				}

				// If there are multiple block groups surrounding the block, merge them all together
				// If there is no block groups surrounding the block, create a new block group and add the block to it
				if (surroundingBlockGroups.Count > 0) {
					BlockGroup.MergeAllBlockGroups(surroundingBlockGroups).TransferBlock(block);
				} else {
					BlockGroup blockGroup = CreateBlockGroup( );
					blockGroup.TransferBlock(block);
				}
			}
		}
	}

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
