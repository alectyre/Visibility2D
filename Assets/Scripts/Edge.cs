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
        }

        public Edge(Point start, Point end) : this(start, end, null) { }

        static void MergeVeryClosePoints(List<Edge> edges, float minDist)
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
    }
}