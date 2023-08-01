using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : MonoBehaviour {
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private int menuLevel;

	#region Unity Functions
	private void OnValidate ( ) {
		rectTransform = GetComponent<RectTransform>( );

		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(1, 1);
		rectTransform.anchoredPosition = new Vector2(0, -menuLevel * rectTransform.rect.height);
	}
	#endregion
}
