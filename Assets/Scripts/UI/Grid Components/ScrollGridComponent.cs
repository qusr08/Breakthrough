using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollGridComponent : GridComponent {
	[Space]
	[SerializeField] private Image scrollbarHandleImage;
	[SerializeField] private Image scrollbarImage;

	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		scrollbarHandleImage.color = themeManager.GetRandomBackgrounDetailColor( );
		scrollbarImage.color = Color.white;
	}
	#endregion
}
