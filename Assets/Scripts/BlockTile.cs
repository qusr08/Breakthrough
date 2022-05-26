using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum TileColor {
	YELLOW, DARK_GREEN, DARK_BLUE, TEAL, DARK_PINK, ORANGE, LIGHT_GREEN, LIGHT_BLUE, PURPLE, LIGHT_PINK, COAL
}

public enum TileType {
	NONE, BOMB_DIRECTION, BOMB_SURROUND, BOMB_LINE
}

public enum TileDirection {
	RIGHT, DOWN, LEFT, UP
}

public class BlockTile : MonoBehaviour {
	[SerializeField] private Sprite[ ] colors;
	[SerializeField] private Sprite[ ] icons;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Space]
	[SerializeField] public TileColor Color = TileColor.COAL;
	[SerializeField] public TileType Type = TileType.NONE;
	[SerializeField] public TileDirection Direction = TileDirection.RIGHT;

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

		SetTileColor(Color);
		SetTileType(Type);
	}

	private void Start ( ) {
		SetTileDirection((TileDirection) Random.Range(0, 4));

		transform.localScale = new Vector3(Constants.BLOCK_TILE_SCALE, Constants.BLOCK_TILE_SCALE, 1);
		transform.eulerAngles = new Vector3(0, 0, (int) Direction * Constants.BLOCK_ROTATE_DIRECTION * 90);
	}

	public void SetTileColor (TileColor color) {
		Color = color;
		spriteRenderer.sprite = colors[(int) Color];
	}

	public void SetTileType (TileType type) {
		Type = type;
		iconSpriteRenderer.sprite = icons[(int) Type];
	}

	public void SetTileDirection (TileDirection direction) {
		Direction = direction;
		// transform.eulerAngles = new Vector3(0, 0, (int) Direction * Constants.BLOCK_ROTATE_DIRECTION * 90);
	}
}
