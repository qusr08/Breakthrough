using MyBox;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum BoomBlockType {
	PYRA, AREA, LINE
}

public class BoomBlock : MinoBlock, IThemeElement {
	[Separator("Boom Block Subclass")]
	[Header("References")]
	[SerializeField] private SpriteRenderer iconSpriteRenderer;
	[Header("Properties")]
	[SerializeField] private BoomBlockType _boomBlockType;
	[SerializeField, Min(0.001f)] private float iconSpriteSize;

	/// <summary>
	///		The type of pattern this boom block has when it explodes
	/// </summary>
	public BoomBlockType BoomBlockType {
		get => _boomBlockType;
		set {
			_boomBlockType = value;

			iconSpriteRenderer.sprite = ThemeSettingsManager.Instance.ActiveThemeSettings.BoomBlockIcons[_boomBlockType];
			iconSpriteRenderer.size = new Vector2(iconSpriteSize, iconSpriteSize);
		}
	}

	public override void UpdateThemeElements ( ) {
		base.UpdateThemeElements( );

		iconSpriteRenderer.color = ThemeSettingsManager.Instance.ActiveThemeSettings.BoomBlockIconColor;
		iconSpriteRenderer.sprite = ThemeSettingsManager.Instance.ActiveThemeSettings.BoomBlockIcons[BoomBlockType];
		iconSpriteRenderer.size = new Vector2(iconSpriteSize, iconSpriteSize);
	}
}
