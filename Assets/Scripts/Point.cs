using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D 
{
    [System.Serializable]
    public class Point {

        public Vector2 position;
        public float angle;
        public List<Edge> edges;

        public float x { get { return position.x; } set { position.x = value; } }
        public float y { get { return position.y; } set { position.y = value; } }

        public Point() { edges = new List<Edge>(); }
        public Point(float x, float y) { position.x = x; position.y = y; edges = new List<Edge>(); }
        public Point(Vector2 position) { this.position = position; edges = new List<Edge>(); }

        //~Point() { Debug.Log("Point garbage collected"); }
    }

    public class PointPositionEqualityComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
        }

        public int GetHashCode(Point point)
        {
            return point.position.GetHashCode();
        }
    }

    public class PointAngleEqualityComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.angle, rhs.angle);
        }

        public int GetHashCode(Point point)
        {
            return point.angle.GetHashCode();
        }
    }

    public class PointAnglerComparer : Comparer<Point> {
        public override int Compare(Point x, Point y)
        {
            if (x.angle < y.angle)
                return -1;
            if (y.angle < x.angle)
                return 1;

            return 0;
        }
    }
}
