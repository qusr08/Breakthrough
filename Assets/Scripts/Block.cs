using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BlockColor {
	DARK_PURPLE, DARK_COAL, DARK_BLUE, TEAL, DARK_PINK, ORANGE, GREEN, LIGHT_BLUE, LIGHT_PURPLE, LIGHT_PINK, MEDIUM_COAL, LIGHT_COAL
}

public enum BlockType {
	NORMAL, BOOM_DIRECTION, BOOM_LINE, BOOM_SURROUND, CRACK_STAGE_1, CRACK_STAGE_2
}

public enum BlockDirection {
	RIGHT, DOWN, LEFT, UP
}

public class Block : MonoBehaviour {
	[SerializeField] private GameObject blockParticlesPrefab;
	[SerializeField] private Sprite[ ] colors;
	[SerializeField] private Sprite[ ] icons;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Space]
	[SerializeField] private BlockColor _blockColor = BlockColor.DARK_COAL;
	[SerializeField] private BlockType _blockType = BlockType.NORMAL;
	[SerializeField] private BlockDirection _blockDirection = BlockDirection.RIGHT;
	[SerializeField] private int _health = 1;
	[SerializeField] private Texture2D colorTexture;

	private BlockColor[ ] wallColorStages = new BlockColor[ ] { BlockColor.LIGHT_COAL, BlockColor.MEDIUM_COAL, BlockColor.DARK_COAL };

	public BlockColor BlockColor {
		get {
			return _blockColor;
		}

		set {
			_blockColor = value;

			spriteRenderer.sprite = colors[(int) _blockColor];

			/// TODO: Make this less laggy
			int textureX = (int) spriteRenderer.sprite.rect.x;
			int textureY = (int) spriteRenderer.sprite.rect.y;
			int textureWidth = (int) spriteRenderer.sprite.rect.width;
			int textureHeight = (int) spriteRenderer.sprite.rect.height;
			colorTexture = new Texture2D(textureWidth, textureHeight);
			colorTexture.SetPixels(spriteRenderer.sprite.texture.GetPixels(textureX, textureY, textureWidth, textureHeight));
			colorTexture.Apply( );
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

			// transform.eulerAngles = new Vector3(0, 0, (int) _blockDirection * Constants.MINO_ROTATE_DIRECTION * 90);
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

			if (_health <= 0) {
				DestroyImmediate(gameObject);
			} else {
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

		BlockColor = _blockColor;
		BlockType = _blockType;
	}

	private void Start ( ) {
		BlockColor = _blockColor;
		BlockType = _blockType;
		BlockDirection = (BlockDirection) Random.Range(0, 4);

		transform.localScale = new Vector3(Constants.MINO_TILE_SCALE, Constants.MINO_TILE_SCALE, 1);
		transform.eulerAngles = new Vector3(0, 0, (int) _blockDirection * Constants.MINO_ROTATE_DIRECTION * 90);
	}

	public bool IsWithinRange (Vector3 position) {
		bool negative = (BlockDirection == BlockDirection.LEFT || BlockDirection == BlockDirection.DOWN);
		// Calculate the bounds of this block
		float minX = -1, maxX = -1, minY = -1, maxY = -1;

		// Based on this type of block, determine where the boom block would explode and if the block parameter is within range of it
		switch (BlockType) {
			case BlockType.BOOM_DIRECTION:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					minX = Position.x + (negative ? -Constants.BOOM_DIRECTION_SIZE : 1);
					maxX = Position.x + (negative ? -1 : Constants.BOOM_DIRECTION_SIZE);
					minY = Position.y - 1;
					maxY = Position.y + 1;
				} else { // Vertical
					minX = Position.x - 1;
					maxX = Position.x + 1;
					minY = Position.y + (negative ? -Constants.BOOM_DIRECTION_SIZE : 1);
					maxY = Position.y + (negative ? -1 : Constants.BOOM_DIRECTION_SIZE);
				}

				break;
			case BlockType.BOOM_LINE:
				if (Utils.IsEven((int) BlockDirection)) { // Horizontal
					minX = 0;
					maxX = Constants.BOARD_WIDTH;
					minY = Position.y;
					maxY = Position.y;
				} else { // Vertical
					minX = Position.x;
					maxX = Position.x;
					minY = 0;
					maxY = Constants.BOARD_HEIGHT;
				}

				break;
			case BlockType.BOOM_SURROUND:
				minX = Position.x - Constants.BOOM_SURROUND_SIZE;
				maxX = Position.x + Constants.BOOM_SURROUND_SIZE;
				minY = Position.y - Constants.BOOM_SURROUND_SIZE;
				maxY = Position.y + Constants.BOOM_SURROUND_SIZE;

				break;
		}

		// Check to see if the block position is within the bounds of the block
		bool inX = (position.x >= minX && position.x <= maxX);
		bool inY = (position.y >= minY && position.y <= maxY);

		return (inX && inY);
	}

	private void SpawnBlockParticles ( ) {
		ParticleSystem blockParticles = Instantiate(blockParticlesPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>( );
		blockParticles.GetComponent<ParticleSystemRenderer>( ).material.SetTexture("_MainTex", colorTexture);

		blockParticles.Play( );
	}
}
