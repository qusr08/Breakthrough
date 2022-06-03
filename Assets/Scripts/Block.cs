using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BlockColor {
	YELLOW, COAL, DARK_BLUE, TEAL, DARK_PINK, ORANGE, GREEN, LIGHT_BLUE, PURPLE, LIGHT_PINK
}

public enum BlockType {
	NONE, BOOM_DIRECTION, BOOM_LINE, BOOM_SURROUND
}

public enum BlockDirection {
	RIGHT, DOWN, LEFT, UP
}

public class Block : MonoBehaviour {
	[SerializeField] private Sprite[ ] colors;
	[SerializeField] private Sprite[ ] icons;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Space]
	[SerializeField] public BlockColor Color = BlockColor.COAL;
	[SerializeField] public BlockType Type = BlockType.NONE;
	[SerializeField] public BlockDirection Direction = BlockDirection.RIGHT;

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
			return (Type != BlockType.NONE);
		}
	}

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

		SetColor(Color);
		SetType(Type);
	}

	private void Start ( ) {
		SetDirection((BlockDirection) Random.Range(0, 4));

		transform.localScale = new Vector3(Constants.MINO_TILE_SCALE, Constants.MINO_TILE_SCALE, 1);
		transform.eulerAngles = new Vector3(0, 0, (int) Direction * Constants.MINO_ROTATE_DIRECTION * 90);
	}

	public void SetColor (BlockColor color) {
		Color = color;
		spriteRenderer.sprite = colors[(int) Color];
	}

	public void SetType (BlockType type) {
		Type = type;
		iconSpriteRenderer.sprite = icons[(int) Type];
	}

	public void SetDirection (BlockDirection direction) {
		Direction = direction;
	}

	public bool IsWithinRange (Vector3 position) {
		bool negative = (Direction == BlockDirection.LEFT || Direction == BlockDirection.DOWN);
		// Calculate the bounds of this block
		float minX = -1, maxX = -1, minY = -1, maxY = -1;

		// Based on this type of block, determine where the boom block would explode and if the block parameter is within range of it
		switch (Type) {
			case BlockType.BOOM_DIRECTION:
				if (Utils.IsEven((int) Direction)) { // Horizontal
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
				if (Utils.IsEven((int) Direction)) { // Horizontal
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
}
