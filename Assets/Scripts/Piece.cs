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
		foreach (Transform tile in transform) {
			tile.localScale = new Vector3(0.95f, 0.95f, 1);
		}
	}

	private void Update ( ) {
		// TODO: Make inputs more expandable and better functioning
		// TODO: Add the ability to hold a button down and have the piece move continuously

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			Move(Vector3.left);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			Move(Vector3.right);
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Rotate(-90);

			// TODO: Move piece to the left or right to satisfy a rotation
			//		 Made tbe piece move away from walls
			// TODO: Move piece away from other pieces to be able to rotate
		}

		if (Time.time - prevTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) {
			if (Move(Vector3.down)) {
				prevTime = Time.time;
			} else {
				board.AddPieceToBoard(this);
				enabled = false;
				board.SpawnRandomPiece( );
			}
		}
	}

	public bool Move (Vector3 direction) {
		foreach (Transform child in transform) {
			if (!board.IsInBounds(Vector3Int.RoundToInt(child.transform.position + direction))) {
				return false;
			}
		}

		transform.position += direction;

		return true;
	}

	public bool Rotate (float degRotation) {
		bool canRotate = true;

		// Check to see if piece can rotate
		foreach (Transform child in transform) {
			Vector3 newPosition = Quaternion.Euler(0, 0, degRotation) * (child.position - transform.position) + transform.position;

			if (!board.IsInBounds(Vector3Int.RoundToInt(newPosition))) {
				canRotate = false;
			}
		}

		// If the piece cant rotate and is near a wall, move it away from the wall and then rotate
		if (!canRotate) {
			// Check to see which wall the piece is
			Vector3 direction = (Mathf.RoundToInt(transform.position.x) == 0 ? Vector3.right : Vector3.left);

			// If the piece can move away from the wall, try and rotate it again
			if (Move(direction)) {
				foreach (Transform child in transform) {
					Vector3 newPosition = Quaternion.Euler(0, 0, degRotation) * (child.position - transform.position) + transform.position;

					if (!board.IsInBounds(Vector3Int.RoundToInt(newPosition))) {
						continue;
					}
				}

				canRotate = true;
			}
		}

		// If the piece cant rotate at all, then return false
		if (!canRotate) {
			return false;
		}

		transform.RotateAround(transform.position, new Vector3(0, 0, 1), -90);

		return true;
	}
}
