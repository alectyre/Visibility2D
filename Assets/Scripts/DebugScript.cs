using UnityEngine;
using System.Collections;


/// <summary>
/// A class to help easily draw common 2D shapes in 3D space for debugging.
/// </summary>
public class DebugScript : MonoBehaviour {

	private const float DEFAULT_DURATION = 0.001f;
	private const int DEFAULT_VERTICES = 36;


	public static void DrawRect(Rect rect, Color color)
	{
		DrawRect(rect, color, DEFAULT_DURATION);
	}

	public static void DrawRect(Rect rect, Color color, float duration)
	{
		DrawRect (rect, color, duration, 0);
	}

	public static void DrawRect(Rect rect, Color color, float duration, float zDepth)
	{
		Vector3 upperLeft = new Vector3(rect.x, rect.y, zDepth);
		Vector3 upperRight = new Vector3(rect.x + rect.width, rect.y, zDepth);
		Vector3 lowerRight = new Vector3(rect.x + rect.width, rect.y + rect.height, zDepth);
		Vector3 lowerLeft = new Vector3(rect.x, rect.y + rect.height, zDepth);
		
		Debug.DrawLine(upperLeft, upperRight, color, duration);
		Debug.DrawLine(upperRight, lowerRight, color, duration);
		Debug.DrawLine(lowerRight, lowerLeft, color, duration);
		Debug.DrawLine(lowerLeft, upperLeft, color, duration);
	}

	/// <summary>
	/// Draw a circle in 3D space.
	/// </summary>
	/// <param name="radius">Radius of the circle.</param>
	/// <param name="position">The position of the circle's center.</param>
	/// <param name="axis">The axis of the circle.</param>
	/// <param name="color">The color of the circle.</param>
	/// <param name="duration">The duration of the circle.</param>
	/// <param name="vertexCount">The number of vertices used to draw the circle.</param>
	public static void DrawCircle(float radius, Vector3 position, Vector3 axis, Color color, float duration = DEFAULT_DURATION, int vertexCount = DEFAULT_VERTICES)
	{
		DrawArc(radius, 360, 0, position, axis, color, duration, vertexCount);
	}

	/// <summary>
	/// Draw an arc in 3D space.
	/// </summary>
	/// <param name="radius">Radius of the arc's circle.</param>
	/// <param name="degrees">Degrees of the arc, or arc length.</param>
	/// <param name="arcRotation">The rotation of the starting point of the arc.</param>
	/// <param name="position">The position of the arc's circle's center.</param>
	/// <param name="axis">The axis of the arc.</param>
	/// <param name="color">The color of the arc.</param>
	/// <param name="duration">The duration of the arc.</param>
	/// <param name="vertexCount">The number of vertices used to draw the arc.</param>
	public static void DrawArc(float radius, float degrees, float arcRotation, Vector3 position, Vector3 axis, Color color, float duration = DEFAULT_DURATION, int vertexCount = DEFAULT_VERTICES)
	{
		Vector3[] points = new Vector3[vertexCount];
		
		float stepSize = (degrees/180)*Mathf.PI / (vertexCount - 1); //Minus one before first step is zero!
		float theta = 0f;
		
		for(int i = 0; i < vertexCount; i++ )
		{
			// Calculate position of point
			float x = (radius) * Mathf.Cos(theta);
			float y = (radius) * Mathf.Sin(theta);
			
			// Set the position of this point
			Vector3 pos = new Vector3(x, y, 0);
			points[i] = pos;
			theta += stepSize;
		}
		
		for(int i = 0; i < points.Length; i++)
		{
			//Rotate around origin by rotation
			points[i] = Quaternion.Euler(0,0,arcRotation) * points[i];

			//Rotate to axis
			points[i] = Quaternion.FromToRotation(Vector3.forward, axis) * points[i];
			
			//Offset by offset
			points[i] += position;
		}

		DrawBetweenPoints(points, color, false, duration);

	}

	/// <summary>
	/// Draw a cross at a point in 3D space.
	/// </summary>
	/// <param name="position">Position of the cross.</param>
	/// <param name="size">Size of the cross.</param>
	/// <param name="color">Color of the cross.</param>
	/// <param name="duration">Duration of the cross.</param>
	public static void DrawCross(Vector3 position, float size, Color color, float duration = DEFAULT_DURATION)
	{
		//Horizontal line
		Vector3 p1 = new Vector3(position.x - size/2, position.y, position.z);
		Vector3 p2 = new Vector3(position.x + size/2, position.y, position.z);
		Debug.DrawLine(p1, p2, color, duration);

		//Vertical line
		p1 = new Vector3(position.x, position.y - size/2, position.z);
		p2 = new Vector3(position.x, position.y + size/2, position.z);
		Debug.DrawLine(p1, p2, color, duration);

		//Whateve-the-z-axis-is line
		p1 = new Vector3(position.x, position.y, position.z - size/2);
		p2 = new Vector3(position.x, position.y, position.z + size/2);
		Debug.DrawLine(p1, p2, color, duration);
	}

	/// <summary>
	/// Draws a line sequentially from point to poit.
	/// </summary>
	/// <param name="points">The array of points to draw.</param>
	/// <param name="color">Color of the line.</param>
	/// <param name="loop">If set to <c>true</c>, draw a line between the last and first points.</param>
	/// <param name="duration">Duration of the line.</param>
	public static void DrawBetweenPoints(Vector3[] points, Color color, bool loop = true, float duration = DEFAULT_DURATION)
	{
		for(int i = 0; i < points.Length - 1; i++)
			Debug.DrawLine(points[i], points[i+1], color, duration);

		if(loop)
			Debug.DrawLine(points[points.Length-1], points[0], color, duration);
	}
}
