using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeightedList<T> : MonoBehaviour {
	private List<T> list;
	private List<bool> enabledIndices;
	private List<float> percentages;
	private float weightPercentage;
	private int enabledIndexCount;
	
	public T this[int i] { get => list[i]; set => list[i] = value; }

	/// <summary>
	///		Constructor for a weighted list
	/// </summary>
	/// <param name="list">A list of elements that will be effected by weighted percentages</param>
	/// <param name="enabledIndices">The indices in the inputted list that will be enabled. Should be the same size as the inputted list</param>
	/// <param name="weightPercentage">The percetange of a chosen list item that will be redistributed to the rest of the percetages</param>
	public WeightedList (List<T> list, List<bool> enabledIndices, float weightPercentage) {
		this.list = list;
		this.enabledIndices = enabledIndices;
		this.weightPercentage = weightPercentage;

		// Count the number of initially enabled indices
		enabledIndexCount = 0;
		for (int i = 0; i < list.Count; i++) {
			if (enabledIndices[i]) {
				enabledIndexCount++;
			}
		}

		// Set all percentages to a default value
		percentages = new List<float>( );
		for (int i = 0; i < list.Count; i++) {
			percentages[i] = enabledIndexCount / 100f;
		}
	}

	/// <summary>
	///		Get a weighted value from the list. Automatically redistributes percentage values to make the item that is chosen less likely to be chosen more than once in a row
	/// </summary>
	/// <returns>A reference to the item that was chosen from the list</returns>
	public T GetWeightedValue ( ) {
		// Generate a random number between 0 and 1
		float randomValue = Random.Range(0f, 1f);

		// Loop through all indices of the array
		float percentageSum = 0f;
		int chosenIndex = -1;
		for (int i = 0; i < list.Count; i++) {
			// Ignore percetange values that have not been set
			// Also ignore if the index is disabled
			if (percentages[i] == 0f || !enabledIndices[i]) {
				continue;
			}

			// Add up percentage values until it is over the random value
			percentageSum += percentages[i];

			// If the percetnage sum is less than the percetange sum, the chosen index is the current iterator value
			if (randomValue <= percentageSum) {
				chosenIndex = i;
				break;
			}
		}

		// Calculate the percetange that will be subtracted from the chosen index
		float percentageSubtracted = percentages[chosenIndex] * weightPercentage;
		percentages[chosenIndex] *= weightPercentage;
		float percentageIncrease = percentageSubtracted / (enabledIndexCount - 1);

		// Adjust the percetange values to weight the indices based on the one that has been chosen
		for (int i = 0; i < list.Count; i++) {
			// Ignore percetange values that have not been set
			// Also ignore the chosen index as nothing will be added to it
			// Also ingore if the index is disabled
			if (percentages[i] == 0f || i == chosenIndex || !enabledIndices[i]) {
				continue;
			}

			percentages[i] += percentageIncrease;
		}

		return list[chosenIndex];
	}

	/*
	public void EnableIndex (int index) {
		// If the index is already enabled, do nothing
		if (enabledIndices[index]) {
			return;
		}

		enabledIndices[index] = true;
		enabledIndexCount++;
	}
	*/

	/*
	public void DisableIndex (int index) {
		// If the index is already disabled, do nothing
		if (!enabledIndices[index]) {
			return;
		}

		enabledIndices[index] = false;
		enabledIndexCount--;
	}
	*/
}
