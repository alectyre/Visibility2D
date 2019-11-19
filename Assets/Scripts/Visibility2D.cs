using System;
using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D {
    public class Visibility2D : MonoBehaviour {

        [SerializeField] Vector2 center;
        [SerializeField] float maxDist = 0.001f;
        [SerializeField] bool debugVisibility;
        [SerializeField] bool debugEdgeCount;
        [SerializeField] Collider2D[] colliders;

        List<Edge> map;

        private void Start()
        {
            map = BuildMapFromColliders(colliders);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
                center = Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0, 0, Camera.main.transform.position.z));

            if (debugVisibility)
                DebugScript.DrawCross(center, 0.2f, Color.green);

            Visibility(center, map);
        }

        List<Edge> BuildMapFromColliders(Collider2D[] colliders)
        {
            List<Edge> map = new List<Edge>();

            foreach (Collider2D collider in colliders)
            {
                //BoxCollider2D
                if (collider is BoxCollider2D boxCollider)
                {
                    Point downLeft = new Point(boxCollider.transform.localToWorldMatrix.MultiplyPoint3x4(
                        new Vector2(-boxCollider.size.x, -boxCollider.size.y) * 0.5f + boxCollider.offset));
                    Point upRight = new Point(boxCollider.transform.localToWorldMatrix.MultiplyPoint3x4(
                        new Vector2(boxCollider.size.x, boxCollider.size.y) * 0.5f + boxCollider.offset));
                    Point upLeft = new Point(downLeft.x, upRight.y);
                    Point downRight = new Point(upRight.x, downLeft.y);

                    map.Add(new Edge(upLeft, upRight, boxCollider));
                    map.Add(new Edge(upRight, downRight, boxCollider));
                    map.Add(new Edge(downRight, downLeft, boxCollider));
                    map.Add(new Edge(downLeft, upLeft, boxCollider));

                    //DebugScript.DrawCross(upLeft.position, 0.1f, Color.red);
                    //DebugScript.DrawCross(upRight.position, 0.1f, Color.white);
                    //DebugScript.DrawCross(downRight.position, 0.1f, Color.blue);
                    //DebugScript.DrawCross(downLeft.position, 0.1f, Color.yellow);
                }
                //EdgeCollider2D
                else if (collider is EdgeCollider2D edgeCollider)
                {
                    Vector2[] points = edgeCollider.points;

                    for (int i = 0; i < points.Length; i++)
                        points[i] = edgeCollider.transform.localToWorldMatrix.MultiplyPoint3x4(points[i]);

                    for (int i = 1; i < points.Length; i++)
                        map.Add(new Edge(points[i - 1], points[i], edgeCollider));

                    //Color[] colors = { Color.red, Color.white, Color.blue, Color.yellow };
                    //for (int i = 0; i < points.Length; i++)
                    //DebugScript.DrawCross(points[i], 0.1f, colors[(int)Mathf.Repeat(i, colors.Length)]);
                }
                //PolygonCollider2D
                else if (collider is PolygonCollider2D polygonCollider)
                {
                    Vector2[] points = polygonCollider.points;

                    for (int i = 0; i < points.Length; i++)
                        points[i] = polygonCollider.transform.localToWorldMatrix.MultiplyPoint3x4(points[i]);

                    for (int i = 1; i < points.Length; i++)
                        map.Add(new Edge(points[i - 1], points[i], polygonCollider));

                    map.Add(new Edge(points[points.Length - 1], points[0], polygonCollider));

                    //Color[] colors = { Color.red, Color.white, Color.blue, Color.yellow };
                    //for (int i = 0; i < points.Length; i++)
                    //DebugScript.DrawCross(points[i], 0.1f, colors[(int)Mathf.Repeat(i, colors.Length)]);
                }
                //Unsupported type
                else
                {
                    Debug.LogError("Unsupported Collider2D type found during Visibility2D map generation");
                }
            }

            return map;
        }

        void Visibility(Vector2 center, List<Edge> map)
        {
            /*
             * Algorithm Explanation:
             * 
             * Inputs: A map of all vision blocking edges (as line segments)
             * 
             * 1) Create a list of all unique angles from point of vision to each point on map
             * 2) Starting from an arbitrary angle, iterate through each angle the list as a 
             *    ray to check for intersections with edges on map, storing the closest intersection
             *    for each angle
             * 3) Sort intersections by angle calculated in step 1
             * 4) The visibility polygon is constructd by iterating over intersections
             * 5) Build mesh by stitching each set of adjacent intersections
             *    and the origin into a triangle
             * 
             * Sources: 
             * https://www.redblobgames.com/articles/visibility/
             * https://github.com/ncase/sight-and-light/blob/gh-pages/draft8.html
             * 
             *  Note: Adding the edges of screen to the map is necessary so the extents of the screen 
             *        are inlucded in the visibility polygon
             */

            // 1) Create a list of all unique angles from point of vision to each point on map

            map = new List<Edge>(map);


            //Find corners of screen
            Point lowerLeft = new Point(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)));
            Point upperRight = new Point(Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)));
            Point upperLeft = new Point(new Vector2(lowerLeft.x, upperRight.y));
            Point lowerRight = new Point(new Vector2(upperRight.x, lowerLeft.y));

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

            Edge.MergePoints(map, 0.01f);

            //Get all points in map
            Point[] points = GetPointsInMaps(map);

            if (debugVisibility)
                for (int i = 0; i < map.Count; i++)
                    Debug.DrawLine(map[i].start.position, map[i].end.position, Color.yellow);

            //Debug points
            if (debugEdgeCount)
                Point.DebugPointCount(new List<Point>(points));

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
                if (debugVisibility)
                    Debug.DrawRay(center, new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist, Color.cyan);
            }

            // 2) Starting from an arbitrary angle, iterate through each angle in the list as a 
            //    ray to check for intersections with edges on map, storing the closest intersection
            //    for each angle

            //TODO should use object pooling for intersections or make an intersection struct
            List<Point> intersections = new List<Point>();

            // For each point find closest intersection, if any
            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];

                Vector2 rayEnd = new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist + center;
                Point closestIntersection = new Point();
                closestIntersection.angle = point.angle;

                bool edgeOnLeft = false;
                bool edgeOnRight = false;
                float epsilon = 0.0001f;

                if (point.edges.Count > 1)
                {
                    foreach (Edge edge in point.edges)
                    {
                        float sideOfLine = PointSideOfLine(edge.start.position, rayEnd, center);

                        if (sideOfLine < -epsilon)
                            edgeOnRight = true;
                        if (sideOfLine > epsilon)
                            edgeOnLeft = true;

                        sideOfLine = PointSideOfLine(edge.end.position, rayEnd, center);

                        if (sideOfLine < -epsilon)
                            edgeOnRight = true;
                        if (sideOfLine > epsilon)
                            edgeOnLeft = true;
                    }
                }

                bool ignoreAdjacent = !(edgeOnLeft && edgeOnRight);

                //Compare ray to all edges
                for (int u = 0; u < map.Count; u++)
                {
                    Edge edge = map[u];

                    //Handle corners by ignoring edges who's start or end points are at the intersection (ie, probably the target for this ray)
                    //but only if there isn't another edge sharing the point and on the other side of the ray. This skims corners but doesn't
                    //penetrate if two segments are connected
                    if (ignoreAdjacent && (edge.start == point || edge.end == point))
                    {
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

                if (ignoreAdjacent && (point.position - center).sqrMagnitude < (closestIntersection.position - center).sqrMagnitude)
                    intersections.Insert(intersections.Count - 1, point);
            }

            //Debug intersections
            for (int i = 0; i < intersections.Count; i++)
            {
                Point intersection = intersections[i];
                if (debugVisibility)
                    Debug.DrawLine(center, intersection.position, Color.magenta);
                if (debugVisibility)
                    DebugScript.DrawCross(intersection.position, 0.2f, Color.red);
            }

            // 3) Sort intersects by angle
            Point[] intersectionsArray = intersections.ToArray();
            intersectionsArray.MergeSort(0, intersections.Count - 1, new PointAnglerComparer());

            // 4) The visibility polygon is constructd by stitching each two intersection points
            //    together with the origin to create triangles

            // 5) Build mesh by stitching each set of adjacent intersections 
            //    and the origin into a triangle
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

        private Point[] GetPointsInMaps(params List<Edge>[] maps)
        {
            HashSet<Point> points = new HashSet<Point>(new PointPositionEqualityComparer());
            foreach (List<Edge> map in maps)
            {
                for (int i = 0; i < map.Count; i++)
                {
                    Edge edge = map[i];
                    points.Add(edge.start);
                    points.Add(edge.end);
                }
            }

            Point[] results = new Point[points.Count];
            points.CopyTo(results);
            return results;
        }

        //Returns 0 if point is on on the line, and +1 on the left side, -1 on the right side.
        public static float PointSideOfLine(Vector2 point, Vector2 end, Vector2 start)
        {
            return (point.x - start.x) * (end.y - start.y) - (point.y - start.y) * (end.x - start.x);
        }
    }
}