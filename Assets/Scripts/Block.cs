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
	[SerializeField] private GameObject blockParticlesPrefab;
	[Header("Components")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Header("Properties")]
	[SerializeField] public int SurroundBoomBlockSize = 2;
	[SerializeField] public int DirectionalBoomBlockSize = 3;
	[Space]
	[SerializeField] private string[ ] colors;
	[SerializeField] private Sprite[ ] icons;
	[SerializeField] private BlockColor _blockColor = BlockColor.WALL_3;
	[SerializeField] private BlockType _blockType = BlockType.NORMAL;
	[SerializeField] private BlockDirection _blockDirection = BlockDirection.RIGHT;
	[SerializeField] private int _health = 1;
	[SerializeField] private int minoIndex = -1;

	private BlockColor[ ] wallColorStages = new BlockColor[ ] { BlockColor.WALL_1, BlockColor.WALL_2, BlockColor.WALL_3 };

	public BlockColor BlockColor {
		get {
			return _blockColor;
		}

		set {
			_blockColor = value;

			spriteRenderer.color = Utils.GetColorFromHex(colors[(int) _blockColor]);

			int textureX = (int) spriteRenderer.sprite.rect.x;
			int textureY = (int) spriteRenderer.sprite.rect.y;
			int textureWidth = (int) spriteRenderer.sprite.rect.width;
			int textureHeight = (int) spriteRenderer.sprite.rect.height;
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
		get {
			return _blockDirection;
		}

		set {
			_blockDirection = value;

			// transform.eulerAngles = new Vector3(0, 0, (int) _blockDirection * MINO_ROTATE_DIRECTION * 90);
		}
	}
	public int Health {
		get {
			return _health;
		}

		set {
			if (value < _health) {
				SpawnBlockParticles( );
			}

			_health = value;

			if (_health > 0) {
				BlockColor = wallColorStages[_health - 1];
			}
		}
	}
	public Vector3 Position {
		get {
			return transform.position;
		}

		set {
			transform.position = value;
		}
	}
	public BlockGroup BlockGroup {
		get {
			return transform.parent.GetComponent<BlockGroup>( );
		}
	}
	public bool IsBoomBlock {
		get {
			return (BlockType != BlockType.NORMAL);
		}
	}
	public Sprite Sprite {
		get {
			return spriteRenderer.sprite;
		}
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

		transform.localScale = new Vector3(Mino.TILE_SCALE, Mino.TILE_SCALE, 1);
		transform.eulerAngles = new Vector3(0, 0, (int) BlockDirection * Mino.ROTATE_DIRECTION * 90);
	}

	public bool IsWithinRange (Vector3 position) {
		bool negative = (BlockDirection == BlockDirection.LEFT || BlockDirection == BlockDirection.DOWN);
		// Calculate the bounds of this block
		float minX = -1, maxX = -1, minY = -1, maxY = -1;

		// Based on this type of block, determine where the boom block would explode and if the block parameter is within range of it
		switch (BlockType) {
			case BlockType.BOOM_DIRECTION:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					int layerSize = (int) Mathf.Abs(Position.x - position.x);
					minX = Position.x + (negative ? -DirectionalBoomBlockSize : 1);
					maxX = Position.x + (negative ? -1 : DirectionalBoomBlockSize);
					minY = Position.y - layerSize;
					maxY = Position.y + layerSize;
				} else { // Vertical
					int layerSize = (int) Mathf.Abs(Position.y - position.y);
					minX = Position.x - layerSize;
					maxX = Position.x + layerSize;
					minY = Position.y + (negative ? -DirectionalBoomBlockSize : 1);
					maxY = Position.y + (negative ? -1 : DirectionalBoomBlockSize);
				}

				break;
			case BlockType.BOOM_LINE:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					minX = 0;
					maxX = board.Width;
					minY = Position.y;
					maxY = Position.y;
				} else { // Vertical
					minX = Position.x;
					maxX = Position.x;
					minY = 0;
					maxY = board.Height;
				}

				break;
			case BlockType.BOOM_SURROUND:
				minX = Position.x - SurroundBoomBlockSize;
				maxX = Position.x + SurroundBoomBlockSize;
				minY = Position.y - SurroundBoomBlockSize;
				maxY = Position.y + SurroundBoomBlockSize;

				break;
		}

		// Check to see if the block position is within the bounds of the block
		bool inX = (position.x >= minX && position.x <= maxX);
		bool inY = (position.y >= minY && position.y <= maxY);

		return (inX && inY);
	}

	private void SpawnBlockParticles ( ) {
		ParticleSystem blockParticles = Instantiate(blockParticlesPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>( );
		ParticleSystem.MainModule blockParticlesMainModule = blockParticles.main;
		
		blockParticlesMainModule.startColor = spriteRenderer.color;
		
		blockParticles.Play( );
	}
}
