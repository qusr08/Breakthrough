using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour {
	[Header("Prefabs")]
	[SerializeField] private GameObject pointsPopupPrefab;
	[Header("Properties")]
	[SerializeField] public int BoardPoints;
	[SerializeField] public int TotalPoints;
	[Space]
	[SerializeField] public int PointsPerDestroyedBlock = 6;

	public PointsPopup CreatePointsPopup (Vector2 position, string title, int points) {
		PointsPopup pointsPopup = Instantiate(pointsPopupPrefab, position, Quaternion.identity).GetComponent<PointsPopup>( );
		pointsPopup.Title = title;
		pointsPopup.Points = points;

		return pointsPopup;
	}
}
