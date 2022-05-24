using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] [Min(0.001f)] private float fallTime;
	[SerializeField] [Min(0.001f)] private float moveTime;
	[SerializeField] [Range(0, 100)] public int PercentBomb = 60;

	private float prevFallTime;
	private float prevMoveTime;

	private Vector2Int moveTo;
	private Vector2Int fallTo;

	private void Awake ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Start ( ) {
		if (Random.Range(1, 101) <= PercentBomb) {
			BlockTile blockTile = GetComponentsInChildren<BlockTile>( )[Random.Range(0, transform.childCount)];

			switch (Random.Range(0, 3)) {
				case 0:
					blockTile.SetTileType(TileType.BOMB_DIRECTION);
					break;
				case 1:
					blockTile.SetTileType(TileType.BOMB_SURROUND);
					break;
				case 2:
					blockTile.SetTileType(TileType.BOMB_LINE);
					break;
			}
		}
	}

	private void Update ( ) {
		// TODO: Make inputs more expandable and better functioning
		// TODO: Add the ability to hold a button down and have the block move continuously
		// TODO: Make it so a block is not placed if it is moving/rotating
		//		     Probably have to alter prevTime or something to prevent that from happening

		if (Time.time - prevMoveTime > moveTime) {
			if (Input.GetKey(KeyCode.LeftArrow)) {
				Move(Vector3.left);
				prevMoveTime = Time.time;
			} else if (Input.GetKey(KeyCode.RightArrow)) {
				Move(Vector3.right);
				prevMoveTime = Time.time;
			} else {
				prevMoveTime = 0;
			}
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Rotate(-90);

			// TODO: Move block to satisfy a rotation
			//		     This could be used for something like t spins, but also just in general is a good thing to have to make the gameplay experience better
		}

		if (Time.time - prevFallTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) {
			if (Move(Vector3.down)) {
				prevFallTime = Time.time;
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
			blockTile.SetTileDirection((TileDirection) (((int) blockTile.Direction + 1) % 4));
		}

		return true;
	}
}
