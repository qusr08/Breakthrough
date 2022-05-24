using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] [Min(0.001f)] private float fallTime;
	[SerializeField] [Range(0, 100)] public int PercentBomb = 60;

	private float prevTime;

	private void Awake ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Start ( ) {
		if (Random.Range(1, 101) <= PercentBomb) {
			BlockTile blockTile = GetComponentsInChildren<BlockTile>( )[Random.Range(0, transform.childCount)];

			switch (Random.Range(0, 3)) {
				case 0:
					blockTile.SetTileType(BlockTile.TileType.BOMB_DIRECTION);
					break;
				case 1:
					blockTile.SetTileType(BlockTile.TileType.BOMB_SURROUND);
					break;
				case 2:
					blockTile.SetTileType(BlockTile.TileType.BOMB_LINE);
					break;
			}
		}
	}

	private void Update ( ) {
		// TODO: Make inputs more expandable and better functioning
		// TODO: Add the ability to hold a button down and have the block move continuously

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			Move(Vector3.left);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			Move(Vector3.right);
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Rotate(-90);

			// TODO: Move block to satisfy a rotation
		}

		if (Time.time - prevTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) {
			if (Move(Vector3.down)) {
				prevTime = Time.time;
			} else {
				board.AddTilesToBoard(this);
				enabled = false;
				board.SpawnRandomBlock( );
			}
		}
	}

	public bool Move (Vector3 direction) {
		foreach (Transform child in transform) {
			Vector2Int position = Vector2Int.RoundToInt(child.position + direction);

			if (!board.IsInBounds(position) || !board.IsBoardTileFree(position)) {
				return false;
			}
		}

		transform.position += direction;

		return true;
	}

	public bool Rotate (float degRotation) {
		foreach (Transform child in transform) {
			Vector2Int position = Vector2Int.RoundToInt(Quaternion.Euler(0, 0, degRotation) * (child.position - transform.position) + transform.position);

			if (!board.IsInBounds(position) || !board.IsBoardTileFree(position)) {
				return false;
			}
		}

		transform.RotateAround(transform.position, new Vector3(0, 0, 1), -90);
		foreach (BlockTile blockTile in GetComponentsInChildren<BlockTile>( )) {
			blockTile.SetTileDirection((BlockTile.TileDirection) (((int) blockTile.Direction + 1) % 4));
		}

		return true;
	}
}
