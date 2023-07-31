using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BlockColor {
	DARK_PURPLE, DARK_BLUE, DARK_PINK, TEAL, ORANGE, GREEN, LIGHT_BLUE, LIGHT_PURPLE, LIGHT_PINK, WALL_1, WALL_2, WALL_3
}

public enum BlockType {
	BOOM_PYRA, BOOM_LINE, BOOM_AREA, NORMAL, WALL
}

public enum BlockDirection {
	RIGHT, DOWN, LEFT, UP
}

public class Block : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private Board board;
	[SerializeField] private ParticleManager particleManager;
	[SerializeField] private GameManager gameManager;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Header("Properties")]
	[SerializeField] private Vector2Int _position = Vector2Int.zero;
	[SerializeField] private BlockColor _blockColor = BlockColor.WALL_1;
	[SerializeField] private BlockDirection _blockDirection = BlockDirection.UP;
	[SerializeField] private BlockType _blockType = BlockType.NORMAL;
	[SerializeField] private BlockGroup _blockGroup = null;
	[SerializeField] private int _health = 1;
	[SerializeField] private int _minoIndex = -1;
	[Space]
	[SerializeField] private BlockColorStringDictionary blockColors;
	[SerializeField] private BlockTypeSpriteDictionary blockIcons;
	[SerializeField] private int areaBoomBlockSize = 2;
	[SerializeField] private int pyraBoomBlockSize = 3;

	#region Properties
	public Color Color => spriteRenderer.color;
	public BlockDirection BlockDirection {
		get => _blockDirection;
		set => _blockDirection = value;
	}
	public int MinoIndex => _minoIndex;
	public int BlockGroupID => (BlockGroup == null ? -1 : BlockGroup.ID);
	public bool IsBoomBlock => (BlockType == BlockType.BOOM_PYRA || BlockType == BlockType.BOOM_LINE || BlockType == BlockType.BOOM_AREA);
	public Vector2Int Position {
		get {
			_position = Utils.Vect2Round(transform.position);
			return _position;
		}
		set {
			_position = value;
			transform.position = new Vector2(_position.x, _position.y);
		}
	}
	public BlockColor BlockColor {
		get => _blockColor;
		set {
			_blockColor = value;
			spriteRenderer.color = Utils.GetColorFromHex(blockColors[value]);
		}
	}
	public BlockType BlockType {
		get => _blockType;
		set {
			_blockType = value;
			iconSpriteRenderer.sprite = blockIcons[_blockType];
		}
	}
	public BlockGroup BlockGroup {
		get => _blockGroup;
		set {
			// If the block group is being set to the same thing, do nothing
			if (_blockGroup == value) {
				return;
			}

			_blockGroup = value;

			// Add this block to the new block group
			if (_blockGroup != null) {
				transform.SetParent(_blockGroup.transform, true);
			}
		}
	}
	public int Health {
		get => _health;
		set {
			// If the value is less than the current health, then the block was damaged
			if (value < _health) {
				particleManager.SpawnBlockDebris(Position, Color);
			}

			_health = value;

			// If the value is greater than 0, then the block was not completely destroyed
			// If the value is less than or equal to 0, then the block was completely destroyed
			if (value > 0 && BlockType == BlockType.WALL) {
				if (value == 3) {
					BlockColor = BlockColor.WALL_3;
				} else if (value == 2) {
					BlockColor = BlockColor.WALL_2;
				} else if (value == 1) {
					BlockColor = BlockColor.WALL_1;
				}
			} else {
				particleManager.SpawnBlockParticle(Position, Color);
			}
		}
	}
	#endregion

	#region Unity
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

		board = FindObjectOfType<Board>( );
		particleManager = FindObjectOfType<ParticleManager>( );
		gameManager = FindObjectOfType<GameManager>( );

		BlockGroup = GetComponentInParent<BlockGroup>( );
		transform.localScale = new Vector3(Constants.BLK_SCL, Constants.BLK_SCL, 1f);
	}

	protected void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
	#endregion

	/// <summary>
	/// Check to see if the input position is within range of this block. This block needs to be a boom block for this method to be effective.
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>true if the position is within range of the boom block, false otherwise. Also returns false if this block is not a boom block.</returns>
	public bool IsWithinRange (Vector2Int position) {
		// If the block is not a boom block, it has no range so return false
		if (!IsBoomBlock) {
			return false;
		}

		bool negative = (BlockDirection == BlockDirection.LEFT || BlockDirection == BlockDirection.DOWN);
		int minX = -1;
		int maxX = -1;
		int minY = -1;
		int maxY = -1;

		// Each boom block type is going to have a different range
		// Depending on which one it is, calculate the minimum and maximum x and y of the boom block range
		switch (BlockType) {
			case BlockType.BOOM_PYRA:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					int layerSize = Mathf.Abs(Position.x - position.x);
					minX = Position.x + (negative ? -pyraBoomBlockSize : 1);
					maxX = Position.x + (negative ? -1 : pyraBoomBlockSize);
					minY = Position.y - layerSize;
					maxY = Position.y + layerSize;
				} else { // Vertical
					int layerSize = Mathf.Abs(Position.y - position.y);
					minX = Position.x - layerSize;
					maxX = Position.x + layerSize;
					minY = Position.y + (negative ? -pyraBoomBlockSize : 1);
					maxY = Position.y + (negative ? -1 : pyraBoomBlockSize);
				}

				break;
			case BlockType.BOOM_LINE:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					minX = 0;
					maxX = gameManager.GameSettings.BoardWidth;
					minY = Position.y;
					maxY = Position.y;
				} else { // Vertical
					minX = Position.x;
					maxX = Position.x;
					minY = 0;
					maxY = gameManager.GameSettings.BoardHeight;
				}

				break;
			case BlockType.BOOM_AREA:
				minX = Position.x - areaBoomBlockSize;
				maxX = Position.x + areaBoomBlockSize;
				minY = Position.y - areaBoomBlockSize;
				maxY = Position.y + areaBoomBlockSize;

				break;
		}

		// Check to see if the input position is within the range of this block
		bool inX = (position.x >= minX && position.x <= maxX);
		bool inY = (position.y >= minY && position.y <= maxY);
		return (inX && inY);
	}
}