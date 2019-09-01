using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Visibility2D : MonoBehaviour {
    [SerializeField] Edge[] _edges;
    [SerializeField] Vector2 center;
    [SerializeField] float maxDist = 0.001f;

    [System.Serializable]
    class Point {
        public Vector2 position;
        public float angle;

        public Point(Vector2 position) { this.position = position; this.angle = 0; }
        public float x { get { return position.x; } set { position.x = value; } }
        public float y { get { return position.y; } set { position.y = value; } }

        ~Point() { Debug.Log("Point garbage collected"); }
    }

    class PointPositionComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
        }

        public int GetHashCode(Point point)
        {
            return point.position.GetHashCode();
        }
    }

    class PointAngleComparer : IEqualityComparer<Point> {
        public bool Equals(Point lhs, Point rhs)
        {
            return Mathf.Approximately(lhs.angle, rhs.angle);
        }

        public int GetHashCode(Point point)
        {
            return point.angle.GetHashCode();
        }
    }

    //class PointAngleComparer2 : IComparer<Point> {
    //    Vector2 origin;

    //    public PointAngleComparer2(Vector2 origin) { this.origin = origin; }

    //    public int Compare(Point lhs, Point rhs)
    //    {
    //        return 
    //    }
    //}

    [System.Serializable]
    class Edge {
        public Collider2D collider;
        public Point start;
        public Point end;
        public Edge(Point start, Point end, Collider2D collider)
        {
            this.start = start; this.end = end; this.collider = collider;
        }
        public Edge(Point start, Point end) : this(start, end, null) { }
    }

    [System.Serializable]
    class Intersection : IComparable<Intersection> {
        public Vector2 position;
        public Edge edge;
        public float angle;
        //public Intersection(Vector2 position, Edge edge) { this.position = position; this.edge = edge; }
        public int CompareTo(Intersection otherIntersection)
        {
            if (otherIntersection == null) return 1;
            return this.angle.CompareTo(otherIntersection.angle);
        }
    }

    private void Start()
    {
        //BuildMap();


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
        UpdateAnglesForPoints(ref points, center);

        // Remove identical angles
        HashSet<Point> uniqueAngles = new HashSet<Point>(new PointAngleComparer());
        for (int i = 0; i < points.Length; i++)
            uniqueAngles.Add(points[i]);
        points = new Point[uniqueAngles.Count];
        uniqueAngles.CopyTo(points);

        for (int i = 0; i < points.Length; i++)
        {
            Point point = points[i];
            Debug.DrawRay(center, new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist, Color.cyan);
        }

        List<Intersection> intersections = new List<Intersection>();

        // For each point find closest intersection, if any
        for (int i = 0; i < points.Length; i++)
        {
            Point point = points[i];
            Vector2 rayEnd = new Vector2(Mathf.Cos(point.angle), Mathf.Sin(point.angle)) * maxDist + center;
            Intersection closestIntersection = new Intersection();
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
                    if (closestIntersection.edge == null ||
                       (intersectionPosition - center).sqrMagnitude < (closestIntersection.position - center).sqrMagnitude)
                    {
                        closestIntersection.position = intersectionPosition;
                        closestIntersection.edge = edge;
                    }
                }
            }

            if (closestIntersection.edge == null)
                closestIntersection.position = rayEnd;

            intersections.Add(closestIntersection);
        }

        for (int i = 0; i < intersections.Count; i++)
        {
            Intersection intersection = intersections[i];
            Debug.DrawRay(center, intersection.position, Color.magenta);
            DebugScript.DrawCross(intersection.position, 0.2f, Color.cyan);
        }


        // Sort intersects by angle
        Intersection[] intersectionsArray = intersections.ToArray();
        MergeSort(ref intersectionsArray, 0, intersections.Count - 1);

        // Polygon verts are intersects, in order of angle
        //TODO build mesh
    }

    //Gets the polar angle of each point (radians, man)
    void UpdateAnglesForPoints(ref Point[] points, Vector2 center)
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

    // Merges two subarrays of arr[]. 
    // First subarray is arr[l..m] 
    // Second subarray is arr[m+1..r] 
    void Merge<T>(ref T[] arr, int l, int m, int r) where T : IComparable<T>
    {
        int i, j, k;
        int n1 = m - l + 1;
        int n2 = r - m;

        /* create temp arrays */
        T[] L = new T[n1];
        T[] R = new T[n2];

        /* Copy data to temp arrays L[] and R[] */
        for (i = 0; i < n1; i++)
            L[i] = arr[l + i];
        for (j = 0; j < n2; j++)
            R[j] = arr[m + 1 + j];

        /* Merge the temp arrays back into arr[l..r]*/
        i = 0; // Initial index of first subarray 
        j = 0; // Initial index of second subarray 
        k = l; // Initial index of merged subarray 
        while (i < n1 && j < n2)
        {
            if (L[i].CompareTo(R[j]) < 0)
            {
                arr[k] = L[i];
                i++;
            }
            else
            {
                arr[k] = R[j];
                j++;
            }
            k++;
        }

        /* Copy the remaining elements of L[], if there 
           are any */
        while (i < n1)
        {
            arr[k] = L[i];
            i++;
            k++;
        }

        /* Copy the remaining elements of R[], if there 
           are any */
        while (j < n2)
        {
            arr[k] = R[j];
            j++;
            k++;
        }
    }

    /* l is for left index and r is right index of the 
       sub-array of arr to be sorted */
    void MergeSort<T>(ref T[] arr, int l, int r) where T : IComparable<T>
    {
        if (l < r)
        {
            // Same as (l+r)/2, but avoids overflow for 
            // large l and h 
            int m = l + (r - l) / 2;

            // Sort first and second halves 
            MergeSort(ref arr, l, m);
            MergeSort(ref arr, m + 1, r);

            Merge(ref arr, l, m, r);
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
        HashSet<Point> points = new HashSet<Point>(new PointPositionComparer());
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

        for (int i = 0; i < map.Count; i++)
            Debug.DrawLine(map[i].start.position, map[i].end.position, Color.yellow);

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
    }

    void MergeVeryClosePoints(List<Edge> edges, float minDist)
    {
        Debug.Log("MergeVeryClosePoints START");

        //Check for invalid edges
        for (int i = 0; i < edges.Count; i++)
        {
            Edge edge = edges[i];
            if (edge.start == edge.end || (edge.end.position - edge.start.position).sqrMagnitude > minDist)
            {
                Debug.Log("Removing invalid edge at " + i);
                //The edge's start and end are too close or are the same point,
                //so we remove it entirely
                edges.RemoveAt(i--);
            }
        }

        Queue<Edge> open = new Queue<Edge>(edges);
        int edgeIndex = 0;
        int otherIndex = 0;

        //Check for edges with points close enough to be merged
        while (open.Count > 0)
        {
            Edge edge = open.Dequeue();

            Debug.Log("Checking edge " + edgeIndex + " start");
            otherIndex = open.Count - 1;

            foreach (Edge other in open.Reverse())
            {
                //Check for unmerged points that are very close together and merge them
                if (edge.start != other.start &&
                    (edge.start.position - other.start.position).sqrMagnitude > minDist)
                {
                    Debug.Log("Merging edge " + edgeIndex + " start with other " + otherIndex + " start");
                    edge.start = other.start;
                }
                else if (edge.start != other.end &&
                    (edge.start.position - other.end.position).sqrMagnitude > minDist)
                {
                    Debug.Log("Merging edge " + edgeIndex + " start with other " + otherIndex + " end");
                    edge.start = other.end;
                }

                otherIndex--;
            }

            Debug.Log("Checking edge " + edgeIndex + " end");
            otherIndex = open.Count - 1;

            //Check edge.end
            foreach (Edge other in open.Reverse())
            {
                //Check for unmerged points that are very close together and merge them
                if (edge.end != other.start &&
                    (edge.end.position - other.start.position).sqrMagnitude > minDist)
                {
                    Debug.Log("Merging edge " + edgeIndex + " end with other " + otherIndex + " start");
                    edge.end = other.start;
                }
                else if (edge.end != other.end &&
                    (edge.end.position - other.end.position).sqrMagnitude > minDist)
                {
                    Debug.Log("Merging edge " + edgeIndex + " end with other " + otherIndex + " end");
                    edge.end = other.end;
                }

                otherIndex--;
            }

            //Because dequeue from the front and iterate from the back, multiple points merging should
            //be collapsed into the last point without the need for rechecking edges
            //if (startWasMerged || endWasMerged)
            //open.Enqueue(edge);

            edgeIndex++;
        }

        Debug.Log("MergeVeryClosePoints END");
    }

    //Returns 0 if point is on on the line, and +1 on the left side, -1 on the right side.
    public static float PointSideOfLine(Vector2 point, Vector2 end, Vector2 start)
    {
        return (point.x - start.x) * (end.y - start.y) - (point.y - start.y) * (end.x - start.x);
    }
}
