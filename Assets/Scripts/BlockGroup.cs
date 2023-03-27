using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[Header("Components - Block Group")]
	[SerializeField] protected Board board;
	[SerializeField] protected GameManager gameManager;
	[Header("Properties - Block Group")]
	[SerializeField] private int _id = -1;
	[SerializeField] private bool _isModified = false;
	[Space]
	[SerializeField] private bool _canFall;
	[SerializeField] private bool _canFallBelow;

	protected List<Block> blocks = new List<Block>( );

	protected Vector2Int toPosition;
	protected Quaternion toRotation;
	protected float previousFallTime = 0;

	#region Properties
	public int ID => _id;
	public bool IsModified { get => _isModified; set => _isModified = value; }

	public bool CanFall { get => _canFall; private set => _canFall = value; }
	public bool CanFallBelow { get => _canFallBelow; private set => _canFallBelow = value; }

	public Block this[int i] {
		get => blocks[i];
		set => blocks[i] = value;
	}
	public int Count => blocks.Count;
	#endregion

	#region Unity 
	protected virtual void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}

	protected virtual void Update ( ) {
		/// TODO: Update transforms over time (tween)

		UpdateSelf( );
	}
	#endregion

	protected virtual void UpdateSelf ( ) {
		if (board.BoardState != BoardState.UPDATING_BLOCKGROUPS) {
			return;
		}

		if (Time.time - previousFallTime > gameManager.FallTimeAccelerated) {
			if (TryMove(Vector2Int.down)) {
				previousFallTime = Time.time;
			}
		}
	}

	protected bool TryMove (Vector2Int deltaPosition) {
		for (int i = Count - 1; i >= 0; i--) {
			if (!IsValidBlockPosition(blocks[i], blocks[i].Position + deltaPosition)) {
				return false;
			}
		}

		toPosition += deltaPosition;

		return true;
	}

	protected bool TryRotate ( ) {
		return true;
	}

	private bool IsValidBlockPosition (Block block, Vector2Int newPosition) {
		// If the block group cannot fall below the breakthrough line but the current block is trying to, return false
		if (!CanFallBelow && newPosition.y < board.BreakthroughBoardAreaHeight) {
			return false;
		}

		if (board.IsBlockAt(newPosition, ID)) {
			if (newPosition.y < 0) {
				board.DamageBlock(block, destroy: true);
			} else {
				return false;
			}
		}

		return true;
	}
}
