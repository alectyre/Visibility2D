using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D 
{
    [System.Serializable]
    public class Point {

        public Vector2 position;
        public float angle;

        public float x { get { return position.x; } set { position.x = value; } }
        public float y { get { return position.y; } set { position.y = value; } }

        public Point(Vector2 position) { this.position = position; angle = 0; }

        ~Point() { Debug.Log("Point garbage collected"); }
    }

    public class PointPositionComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
        }

        public int GetHashCode(Point point)
        {
            return point.position.GetHashCode();
        }
    }

    public class PointAngleComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.angle, rhs.angle);
        }

        public int GetHashCode(Point point)
        {
            return point.angle.GetHashCode();
        }
    }
}
