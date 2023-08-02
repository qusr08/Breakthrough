using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageGridComponent : GridComponent {
	[Space]
	[SerializeField] protected RectTransform imageRectTransform;

	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		LeanTween.color(imageRectTransform, Color.white, 0f);
	}
	#endregion
}
