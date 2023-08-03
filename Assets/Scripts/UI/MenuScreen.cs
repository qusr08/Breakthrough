using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : MonoBehaviour {
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private int _menuLevel;

	#region Properties
	public int MenuLevel => _menuLevel;
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		rectTransform = GetComponent<RectTransform>( );

		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(1, 1);
		rectTransform.anchoredPosition = new Vector2(0, -_menuLevel * rectTransform.rect.height);
	}

	private IEnumerator Start ( ) {
		// https://www.reddit.com/r/Unity3D/comments/3b0861/getting_the_size_of_a_stretched_recttransform/
		yield return new WaitForEndOfFrame( );

		OnValidate( );
	}
	#endregion
}
