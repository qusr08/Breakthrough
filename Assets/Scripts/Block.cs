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
}
