/*
 * CellPath.cs
 *
 * Defines a path in the scene.
 * This code is mostly the same as what I wrote for
 * LD44.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellPath : MonoBehaviour
{
	// Properties.
	public float TotalDistance { get; private set; }
	public float Size => m_Points.Count;

	// Privates.
	private List<CellPathPoint> m_Points;

	/*
	 * Called on initialisation.
	 */
	private void Start()
	{
		FindPoints();
		ComputeDistances();
	}

	/*
	 * Square bracket operator.
	 */
	public CellPathPoint this[int index]
	{
		get => m_Points[index];
		set => m_Points[index] = value;
	}

	/*
	 * Find the points in the path.
	 */
	private void FindPoints()
	{
		// Re-assign if null.
		if (m_Points == null)
		{
			m_Points = new List<CellPathPoint>();
		}
		m_Points.Clear();

		// Iterate over children and add to list.
		int i = 0;
		foreach(Transform child in transform)
		{
			child.gameObject.name = $"Pt{i}";
			m_Points.Add(new CellPathPoint(child.GetComponent<RectTransform>()));
			++i;
		}
	}

	/*
	 * Calculate the total length of the path.
	 */
	private void ComputeDistances()
	{
		// If we don't have enough points, just set to zero.
		TotalDistance = 0.0f;
		if (m_Points.Count < 2)
		{
			return;
		}

		// Iterate over list and add distances of all.
		CellPathPoint prev = m_Points[0], next;
		for (int i = 1; i < m_Points.Count; ++i, prev = next)
		{
			next = m_Points[i];
			float d = Vector2.Distance(prev.PosWorld, next.PosWorld);
			TotalDistance += d;
			prev.Distance = d;
		}
	}

	/*
	 * Debugging gizmos.
	 */
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		FindPoints();
		ComputeDistances();

		// Don't draw if we don't have more than 1 point.
		if (m_Points.Count < 2)
		{
			return;
		}

		Gizmos.color = Color.red;
		Vector2 prev = m_Points[0].PosWorld, next;
		for (int i = 1; i < m_Points.Count; ++i, prev = next)
		{
			next = m_Points[i].PosWorld;
			Gizmos.DrawLine(prev, next);
		}
	}
#endif
}
