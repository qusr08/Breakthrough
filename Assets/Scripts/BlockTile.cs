using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BlockTile : MonoBehaviour {
	[SerializeField] private Sprite[ ] colors;
	[SerializeField] private Sprite[ ] icons;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Space]
	[SerializeField] public TileColor Color = TileColor.COAL;
	[SerializeField] public TileType Type = TileType.NONE;
	[SerializeField] [Range(0, 100)] public int PercentBomb = 10;

	public enum TileColor {
		YELLOW, DARK_GREEN, DARK_BLUE, TEAL, DARK_PINK, ORANGE, LIGHT_GREEN, LIGHT_BLUE, PURPLE, LIGHT_PINK, COAL
	}

	public enum TileType {
		NONE, BOMB_DIRECTION, BOMB_SURROUND, BOMB_LINE
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

		SetTileColor(Color);
		SetTileType(Type);
	}

	private void Start ( ) {
		transform.localScale = new Vector3(0.95f, 0.95f, 1);

		if (Random.Range(1, 101) <= PercentBomb) {
			switch (Random.Range(0, 3)) {
				case 0:
					SetTileType(TileType.BOMB_DIRECTION);

					break;
				case 1:
					SetTileType(TileType.BOMB_SURROUND);

					break;
				case 2:
					SetTileType(TileType.BOMB_LINE);

					break;
			}
		}
	}

	private void SetTileColor (TileColor color) {
		Color = color;
		spriteRenderer.sprite = colors[(int) Color];
	}

	private void SetTileType (TileType type) {
		Type = type;
		iconSpriteRenderer.sprite = icons[(int) Type];
	}
}
