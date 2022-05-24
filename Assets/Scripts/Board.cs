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
	[Space]
	[SerializeField] [Range(10, 40)] public int BoardWidth = 20;
	[SerializeField] [Range(10, 40)] public int BoardHeight = 20;
	[SerializeField] [Range(2, 10)] private int topSpawnGap = 4;
	[SerializeField] [Range(2, 10)] private int bottomSpawnGap = 2;
	[SerializeField] [Range(3, 8)] private int wallHeight = 5;
	[SerializeField] [Range(0, 6)] private int cameraPadding = 3;

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
		spriteRenderer.size = new Vector2(BoardWidth, BoardHeight);
		float positionX = (BoardWidth / 2) - (BoardWidth % 2 == 0 ? 0.5f : 0f);
		float positionY = (BoardHeight / 2) - (BoardHeight % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);

		// Set the camera orthographic size and position so it fits the entire board
		Camera.main.orthographicSize = (BoardHeight + cameraPadding) / 2f;
		Camera.main.transform.position = new Vector3(positionX, positionY, Camera.main.transform.position.z);

		// TODO: Set the UI element sizes when the board is resized
	}

	private void Awake ( ) {
		board = new BlockTile[BoardWidth, BoardHeight];
	}

	private void Start ( ) {
		GenerateWall( );
		SpawnRandomBlock( );

		// TODO: Add game loop
	}

	public void SpawnRandomBlock ( ) {
		Vector3 spawnPosition = new Vector3((BoardWidth / 2) - 0.5f, BoardHeight - topSpawnGap - 0.5f);

		Instantiate(blocks[Random.Range(0, blocks.Length)], spawnPosition, Quaternion.identity);
	}

	private void GenerateWall ( ) {
		for (int i = 0; i < BoardWidth; i++) {
			for (int j = bottomSpawnGap; j < wallHeight + bottomSpawnGap; j++) {
				BlockTile blockTile = Instantiate(tilePrefab, new Vector3(i, j), Quaternion.identity).GetComponent<BlockTile>( );
				blockTile.SetTileColor(TileColor.COAL);

				board[i, j] = blockTile;
			}
		}
	}

	public bool IsInBounds (Vector2Int position) {
		return (position.x >= 0 && position.x < BoardWidth && position.y >= 0 && position.y < BoardHeight);
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
						for (int j = 0; j < BoardHeight; j++) {
							explodedBlockTiles.Add(new Vector2Int(blockTilePositions[i].x, j));
						}
					} else { // Horizontal
						for (int j = 0; j < BoardWidth; j++) {
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
