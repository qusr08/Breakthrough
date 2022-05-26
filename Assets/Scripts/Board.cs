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

				board[i, j] = block;
			}
		}
	}

	public bool IsInBounds (Vector2Int position) {
		return (position.x >= 0 && position.x < Constants.BOARD_WIDTH && position.y >= 0 && position.y < Constants.BOARD_HEIGHT);
	}

	public bool IsBoardSpaceFree (Vector2Int position) {
		return (board[position.x, position.y] == null);
	}

	public void AddMinoToBoard (Mino mino) {
		List<Vector2Int> blockPositions = new List<Vector2Int>( );
		List<Vector2Int> explodedBlocks = new List<Vector2Int>( );

		// Add all blocks that are part of the mino to the board
		foreach (Block block in mino.GetComponentsInChildren<Block>( )) {
			int x = Mathf.RoundToInt(block.transform.position.x);
			int y = Mathf.RoundToInt(block.transform.position.y);
			blockPositions.Add(new Vector2Int(x, y));

			board[x, y] = block;
		}

		// TODO: Make this code for determining how bombs explode a little cleaner
		// TODO: Make animations for the bombs exploding
		//		     Probably going to need to use coroutines for each bomb or something
		// TODO: Further test bombs to see if one needs to be removed or a different bomb needs to be added

		// Loop through all added blocks to see if one of them was a boom block
		for (int i = 0; i < blockPositions.Count; i++) {
			Block currentTile = board[blockPositions[i].x, blockPositions[i].y];

			// Add all exploded blocks to an array that will be cleared at the end of this for loop
			// This is so boom blocks to not get rid of other boom blocks before they have exploded
			switch (currentTile.Type) {
				case BlockType.BOOM_DIRECTION:
					explodedBlocks.Add(blockPositions[i]);

					switch (currentTile.Direction) {
						case BlockDirection.RIGHT:
							for (int j = 1; j <= 2; j++) {
								for (int k = -1; k <= 1; k++) {
									explodedBlocks.Add(blockPositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case BlockDirection.DOWN:
							for (int j = -1; j <= 1; j++) {
								for (int k = -1; k >= -2; k--) {
									explodedBlocks.Add(blockPositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case BlockDirection.LEFT:
							for (int j = -1; j >= -2; j--) {
								for (int k = -1; k <= 1; k++) {
									explodedBlocks.Add(blockPositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case BlockDirection.UP:
							for (int j = -1; j <= 1; j++) {
								for (int k = 1; k <= 2; k++) {
									explodedBlocks.Add(blockPositions[i] + new Vector2Int(j, k));
								}
							}

							break;
					}
					break;
				case BlockType.BOOM_SURROUND:
					for (int j = -1; j <= 1; j++) {
						for (int k = -1; k <= 1; k++) {
							explodedBlocks.Add(blockPositions[i] + new Vector2Int(j, k));
						}
					}

					break;
				case BlockType.BOOM_LINE:
					if ((int) currentTile.Direction % 2 == 1) { // Vertical
						for (int j = 0; j < Constants.BOARD_HEIGHT; j++) {
							explodedBlocks.Add(new Vector2Int(blockPositions[i].x, j));
						}
					} else { // Horizontal
						for (int j = 0; j < Constants.BOARD_WIDTH; j++) {
							explodedBlocks.Add(new Vector2Int(j, blockPositions[i].y));
						}
					}

					break;
			}
		}

		// Remove all blocks that are to be exploded from the board
		for (int i = 0; i < explodedBlocks.Count; i++) {
			RemoveBlockFromBoard(explodedBlocks[i]);
		}
	}

	private void RemoveBlockFromBoard (Vector2Int position) {
		if (IsInBounds(position) && !IsBoardSpaceFree(position)) {
			Destroy(board[position.x, position.y].gameObject);
			board[position.x, position.y] = null;
		}
	}
}
