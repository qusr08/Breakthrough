using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BlockColor {
	DARK_PURPLE, DARK_BLUE, DARK_PINK, TEAL, ORANGE, GREEN, LIGHT_BLUE, LIGHT_PURPLE, LIGHT_PINK, WALL_1, WALL_2, WALL_3
}

public enum BlockType {
	NORMAL, BOOM_DIRECTION, BOOM_LINE, BOOM_SURROUND
}

public enum BlockDirection {
	RIGHT, DOWN, LEFT, UP
}

public class Block : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[Header("Prefabs")]
	[SerializeField] private GameObject prefabBlockDebris;
	[SerializeField] private GameObject prefabBlockParticle;
	[Header("Components")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[SerializeField] private Rigidbody2D blockRigidbody2D;
	[Header("Properties")]
	[SerializeField, Tooltip("The range of the surround boom block when it explodes.")] public int SurroundBoomBlockSize = 2;
	[SerializeField, Tooltip("The range of the directional boom block when it explodes.")] public int DirectionalBoomBlockSize = 3;
	[Space]
	[SerializeField, Tooltip("A list of all the colors that the blocks can be. These colors should line up with the BlockColor enum in Block.cs.")] private string[ ] colors;
	[SerializeField, Tooltip("A list of all the icons that can overlay on top of the block. These can represent boom block types for example.")] private Sprite[ ] icons;
	[SerializeField, Tooltip("The current color of the block.")] private BlockColor _blockColor = BlockColor.WALL_3;
	[SerializeField, Tooltip("The current type of the block.")] private BlockType _blockType = BlockType.NORMAL;
	[SerializeField, Tooltip("The current direction that the block is 'facing'.")] private BlockDirection _blockDirection = BlockDirection.RIGHT;
	[SerializeField, Tooltip("The current health of the block.")] private int _health = 1;
	[SerializeField, Tooltip("The index of the mino that this block applies to. This value is used to see if a full mino was destroyed in Board.cs.")] private int minoIndex = -1;
	[SerializeField, Min(0f), Tooltip("The minimum velocity at which the blocks explode off the board at.")] private float minBlockVelocity;
	[SerializeField, Min(0f), Tooltip("The maximum velocity at which the blocks explode off the board at.")] private float maxBlockVelocity;
	[SerializeField, Min(0f), Tooltip("The minimum angular velocity at which the blocks explode off the board at.")] private float minBlockAngularVelocity;
	[SerializeField, Min(0f), Tooltip("The maximum angular velocity at which the blocks explode off the board at.")] private float maxBlockAngularVelocity;

	private readonly BlockColor[ ] wallColorStages = new BlockColor[ ] { BlockColor.WALL_1, BlockColor.WALL_2, BlockColor.WALL_3 };

	public BlockColor BlockColor {
		get {
			return _blockColor;
		}

		set {
			_blockColor = value;
			spriteRenderer.color = Utils.GetColorFromHex(colors[(int) _blockColor]);
		}
	}
	public BlockType BlockType {
		get {
			return _blockType;
		}

		set {
			_blockType = value;
			iconSpriteRenderer.sprite = icons[(int) _blockType];
		}
	}
	public BlockDirection BlockDirection {
		get => _blockDirection; set => _blockDirection = value;
	}
	public int Health {
		get {
			return _health;
		}

		set {
			// If the health has been decreased, spawn some particles on the block
			if (value < _health) {
				SpawnBlockDebris( );
			}

			_health = value;

			// Only wall blocks should have health above 1, so if the health is not 0, update the wall sprite
			// If the health is 0, though, the block will be destroyed and so a block particle should be spawned
			if (_health > 0) {
				BlockColor = wallColorStages[_health - 1];
			} else {
				SpawnBlockParticle( );
			}
		}
	}
	public Vector3 Position {
		get => transform.position; set => transform.position = value;
	}
	public BlockGroup BlockGroup {
		get => transform.parent.GetComponent<BlockGroup>( );
	}
	public bool IsBoomBlock {
		get => BlockType != BlockType.NORMAL;
	}
	public Sprite Sprite {
		get => spriteRenderer.sprite;
	}
	public int MinoIndex {
		get => minoIndex; set => minoIndex = value;
	}

#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );

		BlockColor = _blockColor;
		BlockType = _blockType;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private void Start ( ) {
		BlockDirection = (BlockDirection) Random.Range(0, 4);

		transform.localScale = new Vector3(gameManager.BlockScale, gameManager.BlockScale, 1);
		transform.eulerAngles = new Vector3(0, 0, (int) BlockDirection * gameManager.RotateDirection * 90);
	}

	/// <summary>
	/// Check to see if a block is within the range of a block (if it is a boom block)
	/// </summary>
	/// <param name="position">The position to check.</param>
	/// <returns>Whether or not the position is within the range of the block</returns>
	public bool IsWithinRange (Vector3Int position) {
		bool negative = (BlockDirection == BlockDirection.LEFT || BlockDirection == BlockDirection.DOWN);
		// Calculate the bounds of this block
		int minX = -1, maxX = -1, minY = -1, maxY = -1;
		Vector3Int truePosition = Utils.Vect3Round(Position);

        // Based on this type of block, determine where the boom block would explode and if the block parameter is within range of it
        switch (BlockType) {
			case BlockType.BOOM_DIRECTION:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					int layerSize = Mathf.Abs(truePosition.x - position.x);
					minX = truePosition.x + (negative ? -DirectionalBoomBlockSize : 1);
					maxX = truePosition.x + (negative ? -1 : DirectionalBoomBlockSize);
					minY = truePosition.y - layerSize;
					maxY = truePosition.y + layerSize;
				} else { // Vertical
					int layerSize = Mathf.Abs(truePosition.y - position.y);
					minX = truePosition.x - layerSize;
					maxX = truePosition.x + layerSize;
					minY = truePosition.y + (negative ? -DirectionalBoomBlockSize : 1);
					maxY = truePosition.y + (negative ? -1 : DirectionalBoomBlockSize);
				}

				break;
			case BlockType.BOOM_LINE:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					minX = 0;
					maxX = board.Width;
					minY = truePosition.y;
					maxY = truePosition.y;
				} else { // Vertical
					minX = truePosition.x;
					maxX = truePosition.x;
					minY = 0;
					maxY = board.Height;
				}

				break;
			case BlockType.BOOM_SURROUND:
				minX = truePosition.x - SurroundBoomBlockSize;
				maxX = truePosition.x + SurroundBoomBlockSize;
				minY = truePosition.y - SurroundBoomBlockSize;
				maxY = truePosition.y + SurroundBoomBlockSize;

				break;
		}

		// Check to see if the block position is within the bounds of the block
		Debug.Log($"({position.x}, {position.y}) => min({minX}, {minY}) max({maxX}, {maxY})");
		bool inX = (position.x >= minX && position.x <= maxX);
		bool inY = (position.y >= minY && position.y <= maxY);

		return (inX && inY);
	}

	/// <summary>
	/// Spawn this blocks particle effect
	/// </summary>
	private void SpawnBlockDebris ( ) {
		ParticleSystem blockDebris = Instantiate(prefabBlockDebris, transform.position, Quaternion.identity).GetComponent<ParticleSystem>( );
		ParticleSystem.MainModule blockDebrisMainModule = blockDebris.main;
		blockDebrisMainModule.startColor = spriteRenderer.color;
		blockDebris.Play( );
	}

	/// <summary>
	/// Spawn this block's particle
	/// </summary>
	private void SpawnBlockParticle ( ) {
		SpriteRenderer blockParticleSpriteRenderer = Instantiate(prefabBlockParticle, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>( );
		blockParticleSpriteRenderer.color = spriteRenderer.color;
	}
}
