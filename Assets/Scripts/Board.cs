using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BoardUpdateState {
	PLACING_MINO, UPDATING_BOOM_BLOCKS, UPDATING_BLOCK_GROUPS
}

public class Board : MonoBehaviour {
	[SerializeField] private GameObject blockPrefab;
	[SerializeField] private GameObject blockGroupPrefab;
	[SerializeField] private GameObject[ ] minoPrefabs;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;

	// TODO: Change this to some sort of tree list to make it easier to handle
	private List<List<List<Block>>> explodingBlockFrames;
	private float prevExplodeTime;

	private BoardUpdateState _boardUpdateState;
	public BoardUpdateState BoardUpdateState {
		get {
			return _boardUpdateState;
		}

		set {
			switch (value) {
				case BoardUpdateState.PLACING_MINO:
					GenerateRandomMino( );

					break;
				case BoardUpdateState.UPDATING_BOOM_BLOCKS:
					UpdateExplodingBlockFrames( );

					break;
				case BoardUpdateState.UPDATING_BLOCK_GROUPS:
					UpdateBlockGroups( );

					explodingBlockFrames.Clear( );

					break;
			}

			_boardUpdateState = value;
		}
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
		explodingBlockFrames = new List<List<List<Block>>>( );
	}

	private void Start ( ) {
		GenerateWall( );

		BoardUpdateState = BoardUpdateState.PLACING_MINO;

		// TODO: Add game loop
	}

	private void Update ( ) {
		switch (BoardUpdateState) {
			case BoardUpdateState.PLACING_MINO:
				break;
			case BoardUpdateState.UPDATING_BOOM_BLOCKS:
				// If a certain amount of time has passed, destroy the next frame of blocks
				if (Time.time - prevExplodeTime > Constants.BOOM_ANIMATION_SPEED) {
					// Loop through each of the boom blocks explosion frames
					for (int i = explodingBlockFrames.Count - 1; i >= 0; i--) {
						// Remove each block in the frame
						while (explodingBlockFrames[i][0].Count > 0) {
							RemoveBlockFromBoard(explodingBlockFrames[i][0][0]);
							explodingBlockFrames[i][0].RemoveAt(0);
						}

						// Remove the current frame
						explodingBlockFrames[i].RemoveAt(0);

						// If there are no more frames in the current boom block frame list, remove it from the main list
						if (explodingBlockFrames[i].Count == 0) {
							explodingBlockFrames.RemoveAt(i);
						}
					}

					// If there are no more boom blocks to explode, switch the update state
					if (explodingBlockFrames.Count == 0) {
						BoardUpdateState = BoardUpdateState.UPDATING_BLOCK_GROUPS;
					}

					prevExplodeTime = Time.time;
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

	public void GenerateRandomMino ( ) {
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

	private void UpdateExplodingBlockFrames ( ) {
		// Whether or not a new block to be exploded was found
		bool foundExplodedBlock;
		// The index of the current frame within a block frame list
		int frameIndex = 0;

		// Construct the frames of each boom block animation
		do {
			foundExplodedBlock = false;

			// Loops through each boom block that has been added to the board
			for (int i = 0; i < explodingBlockFrames.Count; i++) {
				// Some boom blocks may have longer frame sequences
				if (explodingBlockFrames[i].Count <= frameIndex) {
					continue;
				}

				// Loops through each block that will explode on the current frame
				foreach (Block block in explodingBlockFrames[i][frameIndex]) {
					// Add all neighboring blocks to this current block to the next frame
					foreach (Block neighborBlock in GetSurroundingBlocks(block)) {
						// If a neighbor is found, then there will be a next frame of the explosion
						if (AddExplodingBlock(neighborBlock, i, frameIndex + 1)) {
							foundExplodedBlock = true;
						}
					}
				}
			}

			frameIndex++;
		} while (foundExplodedBlock);
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
					BlockGroup.MergeToBlockGroup(blockGroups[i], blockGroups[mergeToGroupIndex]);
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

	private bool AddExplodingBlock (Block block, int blockFramesIndex, int frameIndex) {
		// Make sure the block is within range of the boom block and is not anther boom block
		if (!explodingBlockFrames[blockFramesIndex][0][0].IsWithinRange(block) || block.IsBoomBlock) {
			return false;
		}

		// Check to see if the block has already been added to a frame 
		foreach (List<Block> blockFrame in explodingBlockFrames[blockFramesIndex]) {
			if (blockFrame.Contains(block)) {
				return false;
			}
		}

		// Make sure the block frame list is big enough for the frame index
		while (explodingBlockFrames[blockFramesIndex].Count <= frameIndex) {
			explodingBlockFrames[blockFramesIndex].Add(new List<Block>( ));
		}

		explodingBlockFrames[blockFramesIndex][frameIndex].Add(block);

		return true;
	}

	private void AddBoomBlock (Block block) {
		explodingBlockFrames.Add(new List<List<Block>>( ) { new List<Block>( ) { block } });
	}

	public void AddMinoToBoard (Mino mino) {
		// Add all blocks that are part of the mino to the board
		while (mino.transform.childCount > 0) {
			AddBlockToBoard(mino.transform.GetChild(0).GetComponent<Block>( ));
		}

		Destroy(mino.gameObject);

		BoardUpdateState = BoardUpdateState.UPDATING_BOOM_BLOCKS;
	}

	private void AddBlockToBoard (Block block, bool excludeCurrentBlockGroup = false) {
		// Make sure the block exists
		if (block == null) {
			return;
		}

		// Get the surrounding block groups of the block that was just added
		List<BlockGroup> blockGroups = GetSurroundingBlockGroups(block, excludeCurrentBlockGroup);

		// If the block has no surrounding block groups, create a new one
		if (blockGroups.Count == 0) {
			BlockGroup blockGroup = Instantiate(blockGroupPrefab, transform.position, Quaternion.identity).GetComponent<BlockGroup>( );
			blockGroup.transform.SetParent(transform, true);
			block.transform.SetParent(blockGroup.transform, true);
		} else {
			// If the block has one or more block groups surrounding it, merge those groups together and add it to the merged block group
			block.transform.SetParent(BlockGroup.MergeAllBlockGroups(blockGroups).transform, true);
		}

		if (block.IsBoomBlock) {
			AddBoomBlock(block);
		}
	}

	private void RemoveBlockFromBoard (Block block) {
		// Make sure the block exists
		if (block == null) {
			return;
		}

		// Make sure the block group that the block was a part of is marked as modified
		block.BlockGroup.IsModified = true;

		DestroyImmediate(block.gameObject);
	}

	public bool IsPositionValid (Vector3 position, Transform parent = null) {
		Block block = GetBlockAtPosition(position);

		bool isInBounds = (position.x >= 0 && position.x < Constants.BOARD_WIDTH && position.y >= 0 && position.y < Constants.BOARD_HEIGHT);
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

	public List<BlockGroup> GetSurroundingBlockGroups (Block block, bool excludeCurrentBlockGroup = false) {
		List<BlockGroup> surroundingGroups = new List<BlockGroup>( );

		foreach (Block neighborBlock in GetSurroundingBlocks(block)) {
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

	public List<Block> GetSurroundingBlocks (Block block, bool excludeNullBlocks = true) {
		List<Block> surroundingBlocks = new List<Block>( );

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				// Only get the four cardinal directions around the position
				if (Mathf.Abs(i) == Mathf.Abs(j)) {
					continue;
				}

				Block neighborBlock = GetBlockAtPosition(block.Position + new Vector3(i, j));

				// If there is a block at the neighboring position, add it to the list
				if (excludeNullBlocks || neighborBlock != null) {
					surroundingBlocks.Add(neighborBlock);
				}
			}
		}

		return surroundingBlocks;
	}

	// EXCLUDE NULL BLOCKS FROM BEING FILTERED OUT IN GETSURROUNDINGBLOCKS
	// CHANGED BLOCK PARAMETER BACK TO POSITION PARAMETER BECAUSE OF NULL BLOCKS
	// GETCARDINALPOSITIONS METHOD
}
