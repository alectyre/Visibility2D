using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D 
{
    [System.Serializable]
    public class Edge {

        public Collider2D collider;
        public Point start;
        public Point end;

        public Edge(Point start, Point end, Collider2D collider)
        {
            this.start = start;
            this.end = end;
            this.collider = collider;

            start.edges.Add(this);
            end.edges.Add(this);
        }

        public Edge(Point start, Point end) : this(start, end, null) { }



        //NOTE   Simple one at a time merging will behave a bit unintuitively as each merge
        //       effects whether successive merges occur as point positions are moved by merges
        //
        //NOTE   Could change merge strategy to average of point poisitions
        //
        //NOTE   Could merge in clusters?
        //       1 remove point from open list and add to merge list
        //       2 check for too close points and add to merge list
        //       3 check each point added to merge list for too close points not already in merge list and add to merge list
        //       4 if any points are added to merge list, go back to step 3
        //       5 if no points are added, merge all points in merge list to their average position
        public static void MergeVeryClosePoints(List<Edge> edges, float minDist)
        {
            //Debug.Log("MergeVeryClosePoints START");

            //Check for invalid edges
            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];
                if (edge.start == edge.end || (edge.end.position - edge.start.position).sqrMagnitude < minDist)
                {
                    //Debug.Log("Removing invalid edge at " + i);
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

                //Debug.Log("Checking edge " + edgeIndex + " start");
                otherIndex = edges.Count - 1;

                foreach (Edge other in open.Reverse())
                {
                    bool startWasMerged = false;
                    //Check for unmerged points that are very close together and merge them
                    if (edge.start != other.start &&
                        (edge.start.position - other.start.position).sqrMagnitude < minDist)
                    {
                        //Debug.Log("Merging edge " + edgeIndex + " start with other " + otherIndex + " start");
                        edge.start.edges.Remove(edge);
                        edge.start = other.start;
                        edge.start.edges.Add(edge);
                        startWasMerged = true;
                    }
                    else if (edge.start != other.end &&
                        (edge.start.position - other.end.position).sqrMagnitude < minDist)
                    {
                        //Debug.Log("Merging edge " + edgeIndex + " start with other " + otherIndex + " end");
                        edge.start.edges.Remove(edge);
                        edge.start = other.end;
                        edge.start.edges.Add(edge);
                        startWasMerged = true;
                    }

                    if (startWasMerged)
                        break;
                    otherIndex--;
                }

                //Debug.Log("Checking edge " + edgeIndex + " end");
                otherIndex = open.Count - 1;

                //Check edge.end
                foreach (Edge other in open.Reverse())
                {
                    bool endWasMerged = false;
                    //Check for unmerged points that are very close together and merge them
                    if (edge.end != other.start &&
                        (edge.end.position - other.start.position).sqrMagnitude < minDist)
                    {
                        //Debug.Log("Merging edge " + edgeIndex + " end with other " + otherIndex + " start");
                        edge.end.edges.Remove(edge);
                        edge.end = other.start;
                        edge.end.edges.Add(edge);
                        endWasMerged = true;
                    }
                    else if (edge.end != other.end &&
                        (edge.end.position - other.end.position).sqrMagnitude < minDist)
                    {
                        //Debug.Log("Merging edge " + edgeIndex + " end with other " + otherIndex + " end");
                        edge.end.edges.Remove(edge);
                        edge.end = other.end;
                        edge.end.edges.Add(edge);
                        endWasMerged = true;
                    }

                    if (endWasMerged)
                        break;
                    otherIndex--;
                }

                //Because dequeue from the front and iterate from the back, multiple points merging should
                //be collapsed into the last point without the need for rechecking edges
                //if (startWasMerged || endWasMerged)
                //open.Enqueue(edge);

                edgeIndex++;
            }

            //Debug.Log("MergeVeryClosePoints END");
        }

        #region Testing

        public static void TestEdgeMerge()
        {
            List<Edge> testMap = new List<Edge>();

            testMap.Add(new Edge(new Point(-1, 1), new Point(1, 1)));
            testMap.Add(new Edge(new Point(-1, 1), new Point(-1, 1)));
            testMap.Add(new Edge(new Point(1, 1), new Point(-1, -1)));
            testMap.Add(new Edge(new Point(-1, 1), new Point(-1, -1)));
            testMap.Add(new Edge(new Point(2, 2), new Point(-1, 1)));

            MergeVeryClosePoints(testMap, 1f);

            HashSet<Point> points = new HashSet<Point>();

            foreach (Edge edge in testMap)
            {
                Debug.DrawLine(edge.start.position, edge.end.position, Color.gray, 1000);
                points.Add(edge.start);
                points.Add(edge.end);
            }

            foreach (Point point in points)
            {
                Debug.Log("Drawing point");
                switch (point.edges.Count)
                {
                    case 0:
                        DebugScript.DrawCross(point.position, 0.1f, Color.gray, 1000);
                        break;
                    case 1:
                        DebugScript.DrawCross(point.position, 0.2f, Color.green, 1000);
                        break;
                    case 2:
                        DebugScript.DrawCross(point.position, 0.2f, Color.yellow, 1000);
                        break;
                    case 3:
                        DebugScript.DrawCross(point.position, 0.2f, Color.red, 1000);
                        break;
                    default:
                        DebugScript.DrawCross(point.position, 0.2f, Color.magenta, 1000);
                        break;
                }
            }
        }

#endregion
    }
}