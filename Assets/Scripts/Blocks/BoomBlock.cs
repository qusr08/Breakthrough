using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoomBlockType {
	PYRA, AREA, LINE
}

public class BoomBlock : MinoBlock {
	[SerializeField, Tooltip("A reference to this blocks icon sprite renderer.")] protected SpriteRenderer iconSpriteRenderer;
	[SerializeField, Tooltip("This boom block's type. This will decide the pattern that it will explode in when it lands on the board.")] protected BoomBlockType _boomBlockType;
	[SerializeField, Tooltip("A dictionary that converts BoomBlockType to an icon sprite.")] private BoomBlockIconDictionary boomBlockIconDictionary;

	#region Properties
	public BoomBlockType BoomBlockType {
		get => _boomBlockType;
		set {
			_boomBlockType = value;
			iconSpriteRenderer.sprite = boomBlockIconDictionary[_boomBlockType];
		}
	}
	#endregion


}
