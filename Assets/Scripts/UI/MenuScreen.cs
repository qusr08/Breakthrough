using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuScreen : MonoBehaviour {
	[SerializeField] private MenuManager menuManager;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private int _menuLevel;
	[SerializeField] private List<GridComponent> _gridComponents;

	#region Properties
	public int MenuLevel => _menuLevel;
	public List<GridComponent> GridComponents { get => _gridComponents; private set => _gridComponents = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		menuManager = FindObjectOfType<MenuManager>( );
		rectTransform = GetComponent<RectTransform>( );

		GridComponents = GetComponentsInChildren<GridComponent>( ).ToList( );

		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(1, 1);
	}

	private IEnumerator Start ( ) {
		// https://www.reddit.com/r/Unity3D/comments/3b0861/getting_the_size_of_a_stretched_recttransform/
		yield return new WaitForEndOfFrame( );
		yield return new WaitForEndOfFrame( );

		OnValidate( );

		// Set the position of the rect transform
		rectTransform.anchoredPosition = new Vector2(0, -_menuLevel * rectTransform.rect.height);

		// Generate the grid of blocks in the background based on perlin noise
		for (int x = 0; x < menuManager.GridDimensions.x; x++) {
			for (int y = 0; y < menuManager.GridDimensions.y; y++) {
				// If the noise value is not equal to 0, then generate a grid box component
				if (menuManager.GridBoxNoise[x, y + (menuManager.GridDimensions.y * MenuLevel)] >= 0.25) {
					menuManager.CreateGridBoxComponent(this, new Vector2Int(x, y));
				}
			}
		}
	}
	#endregion
}
