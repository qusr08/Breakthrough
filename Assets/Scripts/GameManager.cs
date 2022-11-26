using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager: MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private TextMeshProUGUI totalPointsTextMesh;
	[SerializeField] private TextMeshProUGUI levelPointsTextMesh;
	[SerializeField] private TextMeshProUGUI breakthroughsTextMesh;
	[Space]
	[SerializeField] private Transform boardPointsEventParent;
	[Header("Prefabs")]
	[SerializeField] private GameObject boardPointsEventPrefab;
	[Header("Properties")]
	[SerializeField] public int BoardPoints;
	[SerializeField] public int TotalPoints;
	[Space]
	[SerializeField] public int PointsPerDestroyedBlock = 6;

	public BoardPointsEvent CreateBoardPointsEvent (string label, int points) {
		BoardPointsEvent boardPointsEvent = Instantiate(boardPointsEventPrefab, boardPointsEventParent).GetComponent<BoardPointsEvent>( );
		boardPointsEvent.Label = label;
		boardPointsEvent.Points = points;
		boardPointsEvent.Trigger( );

		return boardPointsEvent;
	}
}