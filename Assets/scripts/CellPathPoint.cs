/*
 * CellPathPoint.cs
 *
 * A point in a CellPath.
 */

using UnityEngine;

[System.Serializable]
public class CellPathPoint
{
	/*
	 * The world position of the point.
	 */
	public Vector2 PosWorld => Trans.position;

	/*
	 * The anchored position of the point.
	 */
	public Vector2 PosAnch => Trans.anchoredPosition;

	/*
	 * The RectTransform of this point.
	 */
	public RectTransform Trans { get; private set; }

	/*
	 * How far apart this point is from next.
	 */
	public float Distance;

	/*
	 * Construct new CellPathPoint.
	 *
	 * @param trans  The transform of the point.
	 */
	public CellPathPoint(RectTransform trans)
	{
		this.Trans    = trans;
		this.Distance = 0.0f;
	}
}
