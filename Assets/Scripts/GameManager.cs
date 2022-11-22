using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour {
	// [SerializeField] private GameObject pauseMenu;
	// [SerializeField] private GameObject loseMenu;
	[Header("Prefabs")]
	[SerializeField] private GameObject pointsPopupPrefab;
	[Header("Properties")]
	[SerializeField] public int BoardPoints;
	[SerializeField] public int TotalPoints;
	[Space]
	[SerializeField] public int PointsPerDestroyedBlock = 6;

	public PointsPopup AddPoints (Vector2 position, int points) {
		BoardPoints += points;

		PointsPopup pointsPopup = Instantiate(pointsPopupPrefab, position, Quaternion.identity).GetComponent<PointsPopup>( );
		pointsPopup.Points = points;

		return pointsPopup;
	}
}
