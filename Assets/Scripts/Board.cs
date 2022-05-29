using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
	[SerializeField] private GameObject blockPrefab;
	[SerializeField] private GameObject blockGroupPrefab;
	[SerializeField] private GameObject[ ] minoPrefabs;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;

	private List<Block> boomBlocks;


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
		boomBlocks = new List<Block>( );
	}

	private void Start ( ) {
		GenerateWall( );
		SpawnMino( );

		// TODO: Add game loop
	}

	public void SpawnMino ( ) {
		// The spawn position is going to be near the top middle of the board
		Vector3 spawnPosition = new Vector3((Constants.BOARD_WIDTH / 2) - 0.5f, Constants.BOARD_HEIGHT - Constants.BOARD_TOP_PADDING - 0.5f);

		// Spawn a random type of mino
		Instantiate(minoPrefabs[Random.Range(0, minoPrefabs.Length)], spawnPosition, Quaternion.identity);
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

	public Block GetBlockAtPosition (Vector3 position) {
		RaycastHit2D hit = Physics2D.Raycast(position + Vector3.back, Vector3.forward);
		if (hit) {
			return hit.transform.GetComponent<Block>( );
		}

		return null;
	}

	public bool IsPositionValid (Vector3 position, Transform parent = null) {
		Block block = GetBlockAtPosition(position);

		bool isInBounds = (position.x >= 0 && position.x < Constants.BOARD_WIDTH && position.y >= 0 && position.y < Constants.BOARD_HEIGHT);
		bool isBlockAtPosition = (block != null);
		bool hasParentTransform = (parent != null && block != null && block.transform.parent == parent);

		return (isInBounds && (!isBlockAtPosition || hasParentTransform));
	}

	private void ExplodeBoomBlocks ( ) {
		List<Vector3> explodedBlocks = new List<Vector3>( );

		// Get the exploded blocks based on how the boom blocks explode
		foreach (Block boomBlock in boomBlocks) {
			// Add all exploded blocks to an array that will be cleared at the end of this for loop
			// This is so boom blocks to not get rid of other boom blocks before they have exploded
			switch (boomBlock.Type) {
				case BlockType.BOOM_DIRECTION:
					explodedBlocks.Add(boomBlock.Position);

					bool negative = (boomBlock.Direction == BlockDirection.LEFT || boomBlock.Direction == BlockDirection.DOWN);
					int l = (negative ? -Constants.BOOM_DIRECTION_SIZE : 1);
					int u = (negative ? -1 : Constants.BOOM_DIRECTION_SIZE);

					if ((int) boomBlock.Direction % 2 == 1) { // Vertical
						for (int j = l; j <= u; j++) {
							for (int k = -1; k <= 1; k++) {
								explodedBlocks.Add(boomBlock.Position + new Vector3(k, j));
							}
						}
					} else { // Horizontal
						for (int j = l; j <= u; j++) {
							for (int k = -1; k <= 1; k++) {
								explodedBlocks.Add(boomBlock.Position + new Vector3(j, k));
							}
						}
					}

					break;
				case BlockType.BOOM_SURROUND:
					for (int j = -Constants.BOOM_SURROUND_SIZE; j <= Constants.BOOM_SURROUND_SIZE; j++) {
						for (int k = -Constants.BOOM_SURROUND_SIZE; k <= Constants.BOOM_SURROUND_SIZE; k++) {
							explodedBlocks.Add(boomBlock.Position + new Vector3(j, k));
						}
					}

					break;
				case BlockType.BOOM_LINE:
					if ((int) boomBlock.Direction % 2 == 1) { // Vertical
						for (int j = 0; j < Constants.BOARD_HEIGHT; j++) {
							explodedBlocks.Add(new Vector3(boomBlock.Position.x, j));
						}
					} else { // Horizontal
						for (int j = 0; j < Constants.BOARD_WIDTH; j++) {
							explodedBlocks.Add(new Vector3(j, boomBlock.Position.y));
						}
					}

					break;
			}
		}
		boomBlocks.Clear( );

		// Remove all blocks that are to be exploded from the board
		List<Block> blocksToRemove = new List<Block>( );
		for (int i = 0; i < explodedBlocks.Count; i++) {
			Block block = GetBlockAtPosition(explodedBlocks[i]);

			if (block != null) {
				blocksToRemove.Add(block);
			}
		}
		RemoveBlocksFromBoard(blocksToRemove);
	}

	public void AddMinoToBoard (Mino mino) {
		// Add all blocks that are part of the mino to the board
		while (mino.transform.childCount > 0) {
			Block block = mino.transform.GetChild(0).GetComponent<Block>( );

			AddBlockToBoard(block);

			if (block.Type != BlockType.NONE) {
				boomBlocks.Add(block);
			}
		}
		Destroy(mino.gameObject);

		ExplodeBoomBlocks( );

		SpawnMino( );
	}

	private void AddBlockToBoard (Block block) {
		if (block == null) {
			return;
		}

		// Get the surrounding block groups of the block that was just added
		List<BlockGroup> blockGroups = GetSurroundingBlockGroups(block.Position);

		// If the block has no surrounding block groups, create a new one
		if (blockGroups.Count == 0) {
			BlockGroup blockGroup = Instantiate(blockGroupPrefab, Vector3.zero, Quaternion.identity).GetComponent<BlockGroup>( );
			blockGroup.transform.SetParent(transform, true);
			block.transform.SetParent(blockGroup.transform, true);
		} else {
			// If the block has one or more block groups surrounding it, merge those groups together and add it to the merged block group
			block.transform.SetParent(MergeBlockGroups(blockGroups).transform, true);
		}
	}

	private void RemoveBlocksFromBoard (List<Block> blocks) {
		List<BlockGroup> changedBlockGroups = new List<BlockGroup>( );

		// Destroy all of the blocks in the parameter array
		// Also keep track of all the block groups that may have been modified
		while (blocks.Count > 0) {
			if (blocks[0].BlockGroup != null && !changedBlockGroups.Contains(blocks[0].BlockGroup)) {
				changedBlockGroups.Add(blocks[0].BlockGroup);
			}

			// Destroy the block object
			DestroyImmediate(blocks[0].gameObject);
			blocks.RemoveAt(0);
		}

		// If there were block groups that may have been changed, they need to be updated
		if (changedBlockGroups.Count > 0) {
			// Merge all possibly changed block groups together
			BlockGroup blockGroup = MergeBlockGroups(changedBlockGroups);
			// Store the blocks that are in the block group in a temp array
			List<Block> tempBlocks = new List<Block>(blockGroup.transform.GetComponentsInChildren<Block>( ));

			// Remove all of the blocks from the block group
			foreach (Block tempBlock in tempBlocks) {
				tempBlock.transform.SetParent(transform, true);
			}

			// Re-add all of the blocks to the board, which will update their block groups
			foreach (Block tempBlock in tempBlocks) {
				AddBlockToBoard(tempBlock);
			}
		}
	}

	public List<BlockGroup> GetSurroundingBlockGroups (Vector3 position) {
		List<BlockGroup> surroundingGroups = new List<BlockGroup>( );

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				// Only get the four cardinal directions around the position
				if (Mathf.Abs(i) == Mathf.Abs(j)) {
					continue;
				}

				Block block = GetBlockAtPosition(position + new Vector3(i, j));

				// If there is a block at the neighboring position and it has a new block group, add it to the surrounding block group list
				if (block != null && block.BlockGroup != null && !surroundingGroups.Contains(block.BlockGroup)) {
					surroundingGroups.Add(block.BlockGroup);
				}
			}
		}

		return surroundingGroups;
	}

	private BlockGroup MergeBlockGroups (List<BlockGroup> blockGroups) {
		// Merge all block groups that are part of the parameter array into one block group
		while (blockGroups.Count > 1) {
			blockGroups[1].MergeToBlockGroup(blockGroups[0]);
			blockGroups.RemoveAt(1);
		}

		return blockGroups[0];
	}
}
