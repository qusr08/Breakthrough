using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
	[SerializeField] private GameObject blockPrefab;
	[SerializeField] private GameObject[ ] blocks;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;

	private Block[ , ] board;
	private List<Block> boomBlocks;
	private List<List<Block>> blockGroups;

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
		spriteRenderer.size = new Vector2(Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
		float positionX = (Constants.BOARD_WIDTH / 2) - (Constants.BOARD_WIDTH % 2 == 0 ? 0.5f : 0f);
		float positionY = (Constants.BOARD_HEIGHT / 2) - (Constants.BOARD_HEIGHT % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);

		// Set the camera orthographic size and position so it fits the entire board
		Camera.main.orthographicSize = (Constants.BOARD_HEIGHT + Constants.BOARD_CAMERA_PADDING) / 2f;
		Camera.main.transform.position = new Vector3(positionX, positionY, Camera.main.transform.position.z);

		// TODO: Set the UI element sizes when the board is resized
	}

	private void Awake ( ) {
		board = new Block[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
		boomBlocks = new List<Block>( );
		blockGroups = new List<List<Block>>( );
	}

	private void Start ( ) {
		GenerateWall( );
		SpawnMino( );

		// TODO: Add game loop
	}

	public void SpawnMino ( ) {
		// The spawn position is going to be near the top middle of the board
		Vector3 spawnPosition = new Vector3((Constants.BOARD_WIDTH / 2) - 0.5f, Constants.BOARD_HEIGHT - Constants.BOARD_TOP_PADDING - 0.5f);

		// Spawn a random type of block
		Instantiate(blocks[Random.Range(0, blocks.Length)], spawnPosition, Quaternion.identity);
	}

	private void GenerateWall ( ) {
		for (int i = 0; i < Constants.BOARD_WIDTH; i++) {
			for (int j = Constants.BOARD_BOTTOM_PADDING; j < Constants.BOARD_WALL_HEIGHT + Constants.BOARD_BOTTOM_PADDING; j++) {
				Block block = Instantiate(blockPrefab, new Vector3(i, j), Quaternion.identity).GetComponent<Block>( );
				block.SetColor(BlockColor.COAL);

				AddBlockToBoard(block);
			}
		}
	}

	public bool IsInBounds (Vector2Int position) {
		return (position.x >= 0 && position.x < Constants.BOARD_WIDTH && position.y >= 0 && position.y < Constants.BOARD_HEIGHT);
	}

	public bool IsBoardSpaceFree (Vector2Int position) {
		return (board[position.x, position.y] == null);
	}

	public Block GetBlockAtPosition (Vector2Int position) {
		if (IsInBounds(position) && !IsBoardSpaceFree(position)) {
			return board[position.x, position.y];
		}

		return null;
	}

	private void ExplodeBoomBlocks ( ) {
		List<Vector2Int> explodedBlocks = new List<Vector2Int>( );

		// Get the exploded blocks based on how the boom blocks explode
		for (int i = boomBlocks.Count - 1; i >= 0; i--) {
			Vector2Int blockPosition = Utils.Vect2Round(boomBlocks[i].transform.position);

			// Add all exploded blocks to an array that will be cleared at the end of this for loop
			// This is so boom blocks to not get rid of other boom blocks before they have exploded
			switch (boomBlocks[i].Type) {
				case BlockType.BOOM_DIRECTION:
					explodedBlocks.Add(blockPosition);

					bool negative = (boomBlocks[i].Direction == BlockDirection.LEFT || boomBlocks[i].Direction == BlockDirection.DOWN);
					int l = (negative ? -Constants.BOOM_DIRECTION_SIZE : 1);
					int u = (negative ? -1 : Constants.BOOM_DIRECTION_SIZE);

					if ((int) boomBlocks[i].Direction % 2 == 1) { // Vertical
						for (int j = l; j <= u; j++) {
							for (int k = -1; k <= 1; k++) {
								explodedBlocks.Add(blockPosition + new Vector2Int(k, j));
							}
						}
					} else { // Horizontal
						for (int j = l; j <= u; j++) {
							for (int k = -1; k <= 1; k++) {
								explodedBlocks.Add(blockPosition + new Vector2Int(j, k));
							}
						}
					}

					break;
				case BlockType.BOOM_SURROUND:
					for (int j = -Constants.BOOM_SURROUND_SIZE; j <= Constants.BOOM_SURROUND_SIZE; j++) {
						for (int k = -Constants.BOOM_SURROUND_SIZE; k <= Constants.BOOM_SURROUND_SIZE; k++) {
							explodedBlocks.Add(blockPosition + new Vector2Int(j, k));
						}
					}

					break;
				case BlockType.BOOM_LINE:
					if ((int) boomBlocks[i].Direction % 2 == 1) { // Vertical
						for (int j = 0; j < Constants.BOARD_HEIGHT; j++) {
							explodedBlocks.Add(new Vector2Int(blockPosition.x, j));
						}
					} else { // Horizontal
						for (int j = 0; j < Constants.BOARD_WIDTH; j++) {
							explodedBlocks.Add(new Vector2Int(j, blockPosition.y));
						}
					}

					break;
			}

			boomBlocks.RemoveAt(i);
		}

		// Remove all blocks that are to be exploded from the board
		for (int i = 0; i < explodedBlocks.Count; i++) {
			RemoveBlockFromBoard(explodedBlocks[i]);
		}
	}

	private void UpdateBlockGroups ( ) {
		// TODO: Make updating block groups more efficient
		//			Could make blocks hold the values of their surrounding blocks
		//			Could have an array that checks to see if a block group was modified instead of resetting all of the block groups
		//			Could make a block group class that handles all block group functions

		// Clear all the block groups
		for (int i = 0; i < blockGroups.Count; i++) {
			for (int j = 0; j < blockGroups[i].Count; j++) {
				blockGroups[i][j].BlockGroup = -1;
			}
		}
		blockGroups.Clear( );

		// Update all blocks on the board
		for (int i = 0; i < Constants.BOARD_WIDTH; i++) {
			for (int j = 0; j < Constants.BOARD_HEIGHT; j++) {
				Block block = GetBlockAtPosition(new Vector2Int(i, j));

				if (block != null) {
					UpdateBlockGroup(block);
				}
			}
		}
	}

	public void AddMinoToBoard (Mino mino) {
		// Add all blocks that are part of the mino to the board
		foreach (Block block in mino.GetComponentsInChildren<Block>( )) {
			AddBlockToBoard(block);

			if (block.Type != BlockType.NONE) {
				boomBlocks.Add(block);
			}
		}
		Destroy(mino.gameObject);

		ExplodeBoomBlocks( );
		UpdateBlockGroups( );

		SpawnMino( );
	}

	private void AddBlockToBoard (Block block) {
		Vector2Int blockPosition = Utils.Vect2Round(block.transform.position);

		board[blockPosition.x, blockPosition.y] = block;
		block.transform.SetParent(transform, true);
		UpdateBlockGroup(block);
	}

	private void RemoveBlockFromBoard (Vector2Int position) {
		Block block = GetBlockAtPosition(position);

		if (block != null) {
			Destroy(block.gameObject);
			board[position.x, position.y] = null;

			if (block.BlockGroup >= 0) {
				blockGroups[block.BlockGroup].Remove(block);
			}
		}
	}

	private void UpdateBlockGroup (Block block) {
		Vector2Int blockPosition = Utils.Vect2Round(block.transform.position);
		List<int> neighborBlockGroups = new List<int>( );

		// Get all surrounding block groups to the current block
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				// Only get the 4 cardinal directions
				if (Mathf.Abs(i) == Mathf.Abs(j)) {
					continue;
				}

				// Get the block at the adjacent position
				Block neighborBlock = GetBlockAtPosition(blockPosition + new Vector2Int(i, j));

				// If the neighbor exists, then add it has a block group ID
				if (neighborBlock != null) {
					// Make sure neighbor index has not already been added, and make sure that the block has had one set already
					if (neighborBlock.BlockGroup >= 0 && !neighborBlockGroups.Contains(neighborBlock.BlockGroup)) {
						neighborBlockGroups.Add(neighborBlock.BlockGroup);
					}
				}
			}
		}

		neighborBlockGroups.Sort( );

		// If there are no surrounding groups, create a new one
		if (neighborBlockGroups.Count == 0) {
			block.BlockGroup = GetAvailableBlockGroup( );
			blockGroups[block.BlockGroup].Add(block);
		} else {
			int fromBlockGroup, toBlockGroup = neighborBlockGroups[0];

			// Merge all connected groups together to make one group
			while (neighborBlockGroups.Count > 1) {
				fromBlockGroup = neighborBlockGroups[neighborBlockGroups.Count - 1];

				while (blockGroups[fromBlockGroup].Count > 0) {
					// Change the block group ID to the ID that the block has moved to
					blockGroups[fromBlockGroup][0].BlockGroup = toBlockGroup;
					// Move the block from one block group to another
					blockGroups[toBlockGroup].Add(blockGroups[fromBlockGroup][0]);
					// Remove the block object from the list it was moved from
					blockGroups[fromBlockGroup].RemoveAt(0);
				}

				neighborBlockGroups.RemoveAt(neighborBlockGroups.Count - 1);
			}

			// Set the block group ID to the one found
			block.BlockGroup = toBlockGroup;
			blockGroups[toBlockGroup].Add(block);
		}
	}

	private int GetAvailableBlockGroup ( ) {
		// Check to see if a block group has no blocks in it
		// As in, it is an unused group
		for (int i = 0; i < blockGroups.Count; i++) {
			if (blockGroups[i].Count == 0) {
				return i;
			}
		}

		// If all current block groups are filled, make a new one
		blockGroups.Add(new List<Block>( ));
		return blockGroups.Count - 1;
	}
}
