using System;
using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D 
{
    public class Visibility2D : MonoBehaviour {

        [SerializeField] Edge[] _edges;
        [SerializeField] Vector2 center;
        [SerializeField] float maxDist = 0.001f;

        private void Start()
        {
            //BuildMap();

            //Edge.TestEdgeMerge();
        }

        private void Update()
        {
            //Visibility();
        }

        void BuildMap()
        {
            //Generate edges for colliders
        }

        void Visibility(List<Edge> map)
        {
            // Get points
            Point[] points = GetUniquePoints(map); //This can be done once during map generation

            // Update angles for points
            UpdateAnglesForPoints(points, center);

            // Remove identical angles
            HashSet<Point> uniqueAngles = new HashSet<Point>(new PointAngleEqualityComparer());
            for (int i = 0; i < points.Length; i++)
                uniqueAngles.Add(points[i]);
            points = new Point[uniqueAngles.Count];
            uniqueAngles.CopyTo(points);

            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];
                Debug.DrawRay(center, new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist, Color.cyan);
            }

            //NOTE should use object pooling for intersections
            List<Point> intersections = new List<Point>();

            // For each point find closest intersection, if any
            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];
                Vector2 rayEnd = new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist + center;
                Point closestIntersection = new Point();
                closestIntersection.angle = point.angle;

                //Compare ray to all edges
                for (int u = 0; u < map.Count; u++)
                {
                    Edge edge = map[u];
                    //Handle corners by ignoring edges who's start or end points are at the intersection (ie, probably the target for this ray)
                    //but only if there isn't another edge sharing the point and on the other side of the ray. This skims corners but doesn't
                    //penetrate if two segments are connected
                    if (edge.start == point || edge.end == point)
                    {
                        //TODO Only skip if both edges are on the same side (ie we're skimming a corner)
                        continue;
                    }

                    Vector2 intersectionPosition;
                    if (FindIntersectionLineSegments(center, rayEnd, edge.start.position, edge.end.position, out intersectionPosition))
                    {
                        if (closestIntersection.edges.Count == 0 ||
                           (intersectionPosition - center).sqrMagnitude < (closestIntersection.position - center).sqrMagnitude)
                        {
                            closestIntersection.position = intersectionPosition;
                            closestIntersection.edges.Add(edge);
                        }
                    }
                }

                if (closestIntersection.edges.Count == 0)
                    closestIntersection.position = rayEnd;

                intersections.Add(closestIntersection);
            }

            for (int i = 0; i < intersections.Count; i++)
            {
                Point intersection = intersections[i];
                Debug.DrawRay(center, intersection.position, Color.magenta);
                DebugScript.DrawCross(intersection.position, 0.2f, Color.red);
            }


            // Sort intersects by angle
            Point[] intersectionsArray = intersections.ToArray();
            intersectionsArray.MergeSort(0, intersections.Count - 1, new PointAnglerComparer());

            // Polygon verts are intersects, in order of angle
            //TODO build mesh
        }

        //Gets the polar angle of each point (radians, man)
        void UpdateAnglesForPoints(Point[] points, Vector2 center)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];
                Vector2 centerToPosition = point.position - center;
                point.angle = Mathf.Atan2(centerToPosition.y, centerToPosition.x);
                if (point.angle < 0)
                    point.angle += 2 * Mathf.PI;
            }
        }



        /// <summary>
        /// Finds the intersection point between two line segments. Returns false if there is no intersection point.
        /// </summary>
        public static bool FindIntersectionLineSegments(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersectionPoint)
        {
            float denom = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);

            if (denom == 0)
            {
                intersectionPoint = new Vector2(float.NaN, float.NaN);
                return false;
            }
            else
            {
                float ua = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denom;

                float ub = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denom;

                if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                {
                    intersectionPoint = new Vector2(float.NaN, float.NaN);
                    return false;
                }

                intersectionPoint = a1 + ua * (a2 - a1);
                return true;
            }
        }

        private Point[] GetUniquePoints(List<Edge> edges)
        {
            HashSet<Point> points = new HashSet<Point>(new PointPositionEqualityComparer());
            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];
                points.Add(edge.start);
                points.Add(edge.end);
            }

            Point[] results = new Point[points.Count];
            points.CopyTo(results);

            return results;
        }

        private void OnDrawGizmosSelected()
        {
            DebugScript.DrawCross(center, 0.2f, Color.green);

            //Do once at scene start
            //Create map
            List<Edge> map = new List<Edge>(_edges);

            //Do once per frame
            //Find corners of screen
            Point upperLeft = new Point(Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)));
            Point lowerRight = new Point(Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)));
            Point upperRight = new Point(new Vector2(lowerRight.x, upperLeft.y));
            Point lowerLeft = new Point(new Vector2(upperLeft.x, lowerRight.y));

            //Ad edges of screen
            map.Add(new Edge(upperLeft, upperRight));
            map.Add(new Edge(upperRight, lowerRight));
            map.Add(new Edge(lowerRight, lowerLeft));
            map.Add(new Edge(lowerLeft, upperLeft));

            //Merge nonunique points
            //MergeVeryClosePoints(map, maxDist);

            //Cull edges wholly off screen
            for (int i = 0; i < map.Count; i++)
            {
                Edge edge = map[i];
                if ((edge.start.x < upperLeft.x && edge.end.x < upperLeft.x) ||  //Too far left
                    (edge.start.x > lowerRight.x && edge.end.x > lowerRight.x) || //Too far right
                    (edge.start.y > upperLeft.y && edge.end.y > upperLeft.y) || //Too far up
                    (edge.start.y < lowerRight.y && edge.end.y < lowerRight.y))   //Too far down
                {
                    map.RemoveAt(i--);
                }
            }

            Visibility(map);

            for (int i = 0; i < map.Count; i++)
                Debug.DrawLine(map[i].start.position, map[i].end.position, Color.yellow);

        }



        //Returns 0 if point is on on the line, and +1 on the left side, -1 on the right side.
        public static float PointSideOfLine(Vector2 point, Vector2 end, Vector2 start)
        {
            return (point.x - start.x) * (end.y - start.y) - (point.y - start.y) * (end.x - start.x);
        }
    }
}