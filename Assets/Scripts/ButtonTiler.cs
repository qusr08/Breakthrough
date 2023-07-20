using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTiler : MonoBehaviour {
	[Header("Properties")]
	[SerializeField, Range(0f, 10f)] private float gap;

	#region Unity Functions
	private void OnValidate ( ) {
		foreach (RectTransform rectTransform in GetComponentsInChildren<RectTransform>()) {
			Vector2 size = rectTransform.sizeDelta;
			rectTransform.sizeDelta = new Vector2(size.x - (gap / 2f), size.y - (gap / 2f));
		}
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion
}
