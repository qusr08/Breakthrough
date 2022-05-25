using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
	[SerializeField] private GameObject tilePrefab;
	[SerializeField] private GameObject[ ] blocks;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;

	private BlockTile[ , ] board;

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
		board = new BlockTile[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
	}

	private void Start ( ) {
		GenerateWall( );
		SpawnRandomBlock( );

		// TODO: Add game loop
	}

	public void SpawnRandomBlock ( ) {
		Vector3 spawnPosition = new Vector3((Constants.BOARD_WIDTH / 2) - 0.5f, Constants.BOARD_HEIGHT - Constants.BOARD_TOP_PADDING - 0.5f);

		Instantiate(blocks[Random.Range(0, blocks.Length)], spawnPosition, Quaternion.identity);
	}

	private void GenerateWall ( ) {
		for (int i = 0; i < Constants.BOARD_WIDTH; i++) {
			for (int j = Constants.BOARD_BOTTOM_PADDING; j < Constants.BOARD_WALL_HEIGHT + Constants.BOARD_BOTTOM_PADDING; j++) {
				BlockTile blockTile = Instantiate(tilePrefab, new Vector3(i, j), Quaternion.identity).GetComponent<BlockTile>( );
				blockTile.SetTileColor(TileColor.COAL);

				board[i, j] = blockTile;
			}
		}
	}

	public bool IsInBounds (Vector2Int position) {
		return (position.x >= 0 && position.x < Constants.BOARD_WIDTH && position.y >= 0 && position.y < Constants.BOARD_HEIGHT);
	}

	public bool IsBoardTileFree (Vector2Int position) {
		return (board[position.x, position.y] == null);
	}

	public void AddTilesToBoard (Block block) {
		List<Vector2Int> blockTilePositions = new List<Vector2Int>( );
		List<Vector2Int> explodedBlockTiles = new List<Vector2Int>( );

		foreach (BlockTile blockTile in block.GetComponentsInChildren<BlockTile>( )) {
			int x = Mathf.RoundToInt(blockTile.transform.position.x);
			int y = Mathf.RoundToInt(blockTile.transform.position.y);
			blockTilePositions.Add(new Vector2Int(x, y));

			board[x, y] = blockTile;
		}

		// TODO: Make this code for determining how bombs explode a little cleaner
		// TODO: Make animations for the bombs exploding
		//		     Probably going to need to use coroutines for each bomb or something
		// TODO: Further test bombs to see if one needs to be removed or a different bomb needs to be added

		for (int i = 0; i < blockTilePositions.Count; i++) {
			BlockTile currentTile = board[blockTilePositions[i].x, blockTilePositions[i].y];

			switch (currentTile.Type) {
				case TileType.BOMB_DIRECTION:
					explodedBlockTiles.Add(blockTilePositions[i]);

					switch (currentTile.Direction) {
						case TileDirection.RIGHT:
							for (int j = 1; j <= 2; j++) {
								for (int k = -1; k <= 1; k++) {
									explodedBlockTiles.Add(blockTilePositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case TileDirection.DOWN:
							for (int j = -1; j <= 1; j++) {
								for (int k = -1; k >= -2; k--) {
									explodedBlockTiles.Add(blockTilePositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case TileDirection.LEFT:
							for (int j = -1; j >= -2; j--) {
								for (int k = -1; k <= 1; k++) {
									explodedBlockTiles.Add(blockTilePositions[i] + new Vector2Int(j, k));
								}
							}

							break;
						case TileDirection.UP:
							for (int j = -1; j <= 1; j++) {
								for (int k = 1; k <= 2; k++) {
									explodedBlockTiles.Add(blockTilePositions[i] + new Vector2Int(j, k));
								}
							}

							break;
					}
					break;
				case TileType.BOMB_SURROUND:
					for (int j = -1; j <= 1; j++) {
						for (int k = -1; k <= 1; k++) {
							explodedBlockTiles.Add(blockTilePositions[i] + new Vector2Int(j, k));
						}
					}

					break;
				case TileType.BOMB_LINE:
					if ((int) currentTile.Direction % 2 == 1) { // Vertical
						for (int j = 0; j < Constants.BOARD_HEIGHT; j++) {
							explodedBlockTiles.Add(new Vector2Int(blockTilePositions[i].x, j));
						}
					} else { // Horizontal
						for (int j = 0; j < Constants.BOARD_WIDTH; j++) {
							explodedBlockTiles.Add(new Vector2Int(j, blockTilePositions[i].y));
						}
					}

					break;
			}
		}

		for (int i = 0; i < explodedBlockTiles.Count; i++) {
			RemoveTileFromBoard(explodedBlockTiles[i]);
		}
	}

	private void RemoveTileFromBoard (Vector2Int position) {
		if (IsInBounds(position) && !IsBoardTileFree(position)) {
			Destroy(board[position.x, position.y].gameObject);
			board[position.x, position.y] = null;
		}
	}
}
