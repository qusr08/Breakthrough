using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] [Min(0.001f)] private float fallTime;

	private float prevTime;

	private void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		foreach(Transform tile in transform) {
			tile.localScale = new Vector3(0.95f, 0.95f, 1);
		}
	}

	private void Update ( ) {
		// TODO: Make inputs more expandable and better functioning
		// TODO: Add the ability to hold a button down and have the piece move continuously

		if (Input.GetKeyDown(KeyCode.LeftArrow) && board.IsMoveInBounds(this, Vector3.left)) {
			transform.position += Vector3.left;
		}

		if (Input.GetKeyDown(KeyCode.RightArrow) && board.IsMoveInBounds(this, Vector3.right)) {
			transform.position += Vector3.right;
		}

		if (Input.GetKeyDown(KeyCode.UpArrow) && board.IsRotationInBounds(this, -90)) {
			transform.RotateAround(transform.position, new Vector3(0, 0, 1), -90);

			// TODO: Move piece to the left or right to satisfy a rotation
		}

		if (Time.time - prevTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) {
			if (board.IsMoveInBounds(this, Vector3.down)) {
				transform.position += Vector3.down;
				prevTime = Time.time;
			} else {
				board.AddPieceToBoard(this);
				enabled = false;
				board.SpawnRandomPiece( );
			}
		}
	}
}
