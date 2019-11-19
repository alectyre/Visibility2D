using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D 
{
    [System.Serializable]
    public class Point {

        public Vector2 position;
        public float angle;
        public HashSet<Edge> edges;

        public float x { get { return position.x; } set { position.x = value; } }
        public float y { get { return position.y; } set { position.y = value; } }

        public Point() { edges = new HashSet<Edge>(); }
        public Point(float x, float y) { position.x = x; position.y = y; edges = new HashSet<Edge>(); }
        public Point(Vector2 position) { this.position = position; edges = new HashSet<Edge>(); }

        //~Point() { Debug.Log("Point garbage collected"); }

        public static void DebugPointCount(List<Point> points)
        {
            foreach (Point point in points)
            {
                switch (point.edges.Count)
                {
                    case 0:
                        DebugScript.DrawCross(point.position, 0.1f, Color.gray);
                        break;
                    case 1:
                        DebugScript.DrawCross(point.position, 0.2f, Color.green);
                        break;
                    case 2:
                        DebugScript.DrawCross(point.position, 0.2f, Color.yellow);
                        break;
                    case 3:
                        DebugScript.DrawCross(point.position, 0.2f, Color.red);
                        break;
                    default:
                        DebugScript.DrawCross(point.position, 0.2f, Color.magenta);
                        break;
                }
            }
        }
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
