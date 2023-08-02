using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextGridComponent : GridComponent {
	[Space]
	[SerializeField] protected RectTransform textRectTransform;
	[SerializeField] protected TextMeshProUGUI textMeshProUGUI;

	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		LeanTween.color(textRectTransform, themeManager.ActiveTheme.TextColor, 0f);
	}
	#endregion
}
