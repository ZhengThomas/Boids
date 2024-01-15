using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;
using System;

/// <summary>
/// A KDTree class which represents a single node of the KDTree
/// generally the generic data that we want the tree to contain, in this case Vector3
/// </summary>
public class KDTree{
    int axis; //x, y, or z
    int pivotIndex;
    Vector3 pivotValue;

    KDTree[] children;

    /// <summary>
    /// Constructor of the KDTree. It is always 3 dimensional
    /// </summary>
    public KDTree()
    {
        children = new KDTree[2];
    }

    /// <summary>
    /// function that starts bu8ilding the KDTree, given an array of Vector3 points.
    /// It is designed to be used outside of this class, and this does not actually build the tree, only initialize
    /// required information
    /// <param name="points">an array of points to build the tree from</param>
    /// </summary>
    public static KDTree buildTree(Vector3[] points)
    {

        //initializing values
        int[] inds = new int[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            inds[i] = i;
        }

        return buildFromPoint(0, 0, points.Length - 1, points, inds); // build from point is what really builds the tree
    }

    /// <summary>
    /// recursive function that builds a KDTree, from a targetted point
    /// <param name="depth">current depth while building the tree</param>
    /// <param name="startInd">the starting index of where the tree is building from</param>
    /// <param name="endInd">the end index of where the tree is building from</param>
    /// <param name="points">an array of points to build the tree from</param>
    /// <param name="inds">indexes of the points</param>
    /// </summary>
    static KDTree buildFromPoint(int depth, int startInd, int endInd, Vector3[] points, int[] inds)
    {
        KDTree root = new KDTree(); // create current point
        root.axis = depth % 3;
        int splitPoint = pivot(points, inds, startInd, endInd, root.axis); //where we splitting boys

        root.pivotIndex = inds[splitPoint];
        root.pivotValue = points[root.pivotIndex];
        root.axis = depth % 3; // 3d, so mod 3

        int leftSideInd = splitPoint - 1; //leftside of split
        int rightSideInd = splitPoint + 1; //right of split

        
        if(leftSideInd >= startInd)
        {
            //if we dont crash into another group of info
            //recurse
            root.children[0] = buildFromPoint(depth + 1, startInd, leftSideInd, points, inds);
        }
        if(rightSideInd <= endInd)
        {
            root.children[1] = buildFromPoint(depth + 1, rightSideInd, endInd, points, inds);
        }

        return root;

    }

    /// <summary>
    /// finds the median of a set of numbers, assuming the set is sorted.
    /// never used on a sorted list, and thus is closer to finding a random point
    /// <param name="points">an array of points to find the median from</param>
    /// <param name="inds">the indexes to look for</param>
    /// <param name="start">the start of the list of indexes</param>
    /// <param name="start">the end of the list of indexes</param>
    /// <param name="axis">the axis to be sorting from</param>
    /// </summary>
    static int findMedian(Vector3[] points, int[] inds, int start, int end, int axis)
    {

        //find index of end start and middle of end and start
        int mid = (start + end) / 2;
        float a = points[inds[start]][axis];
        float b = points[inds[end]][axis];
        float m = points[inds[mid]][axis];

        //choose median of the three
        if(a > b)
        {
            if(m > a)
            {
                return start;
            }
            else if(b > m)
            {
                return end;
            }
        }
        else
        {
            if(m > b)
            {
                return end;
            }
            else if(a > m)
            {
                return start;
            }
        }
        return mid;
    }

    /// <summary>
    /// a function that swaps to values in an array
    /// </summary>
    /// <param name="array">the reference to the array to swap with</param>
    /// <param name="a">index of the first item</param>
    /// <param name="start">index of the second item</param>
    static void swap(ref int[] array, int a, int b)
    {
        int temp = array[a];
        array[a] = array[b];
        array[b] = temp;
    }

    /// <summary>
    /// a helper function used while creating a new KDTree. It randomly finds a point, and
    /// shifts all points to the left or right of it depending on the index. It only shifts those
    /// that are within the specified start and end index
    /// <param name="points">the array of points</param>
    /// <param name="inds">indexes of the points</param>
    /// <param name="startInd">the starting index of where the tree is pivoting from</param>
    /// <param name="endInd">the end index of where the tree is pivoting from from</param>
    /// <param name="axis">the axis of the current node</param>
    /// </summary>
    static int pivot(Vector3[] points, int[] inds, int startInd, int endInd, int axis)
    {
        //this is about like twice as good as finding a random split point
        //in other words, this does not guarantee creating a perfectly balanced tree
        //its significantly faster this way though since finding median in a list is a pain
        int splitPoint = findMedian(points, inds, startInd, endInd, axis);

        //int splitPoint = Random.Range(startInd, endInd);

        //value of pivot point
        Vector3 pivot = points[inds[splitPoint]];
        //we want to start the pivot all the way at the start
        //this is so we can eventually move the pivot to the desired position
        swap(ref inds, startInd, splitPoint); 


        int currentPoint = splitPoint + 1;
        int endPoint = endInd;

        while(currentPoint <= endPoint)
        {
            Vector3 current = points[inds[currentPoint]];
            
            if(current[axis] > pivot[axis])
            {
                //if current is greater then pivot, move it to all the way to the right
                swap(ref inds, currentPoint, endPoint);
                endPoint--;
            }
            else
            {
                //otherwise move the pivot forward, moving the current guy to the left
                swap(ref inds, currentPoint - 1, currentPoint);
                currentPoint++;
            }
        }

        return currentPoint - 1; //return pivot
    }

    /// <summary>
    /// a function that finds K nearest points to a specified point, K being a number that is specified as a parameter
    /// returns a priority queue, which needs to be imported.
    /// designed to be used from outside of this class, and thus does not actually do the searching, but rather initizalizes data
    /// <param name="point">the point to fbe finding nearby neighbours from</param>
    /// <param name="howMany">how many of these neighbours to find</param>
    /// </summary>
    public SimplePriorityQueue<float[]> findNearest(Vector3 point, int howMany)
    {
        //keeps a priority queue
        //this way you can find K nearest neighbours rather then just the single nearest
        SimplePriorityQueue<float[]> nearest = new SimplePriorityQueue<float[]>();

        float[] toInsert = new float[2];
        toInsert[0] = -1;
        toInsert[1] = 100000000000;
        nearest.Enqueue(toInsert, -10000000000);

        search(point, ref nearest, howMany);

        return nearest;
    }

    /// <summary>
    /// a function that finds K nearest points to a specified point, K being a number that is specified as a parameter
    /// deoes not return anything, but rather directly effect the things that need to be changed
    /// </summary>
    /// <param name="point">the point to fbe finding nearby neighbours from</param>
    /// <param name="nearest">a refernce to a priority queue that stores nearby neighbours</param>
    /// <param name="howMany">how many of these neighbours to find</param>
    void search(Vector3 point, ref SimplePriorityQueue<float[]> nearest, int howMany)
    {   
        //distance from the plane the pivot point cuts
        //we need this to test if there is a chance for another boid on the other side of this plane
        float planeDist = (point[axis] - pivotValue[axis]); 
        
        int whichDir = planeDist <= 0 ? 0 : 1;

        //if there are points on the side of the plane where the point is
        if (children[whichDir] != null)
        {
            //recurse
            children[whichDir].search(point, ref nearest, howMany);
        }

        //all distances are squared to avoid using squareroot
        //does not change logic of code, just useful to know
        float myDist = Vector3.SqrMagnitude(pivotValue - point);

        if(point != pivotValue)
        {
            //if we havent found k nearest yet, then we will insert regardless
            if (nearest.Count < howMany)
            {
                float[] toInsert = new float[2];
                toInsert[0] = pivotIndex;
                toInsert[1] = myDist;
                //insert negative to make order in queue work, but make the head the worst one rather then the best
                nearest.Enqueue(toInsert, -myDist);
            }
            else
            {
                //otherwise, we will replace the worst one with the new one we found, if it makes sense
                //if the heads distance is worse then mine
                if (nearest.First[1] > myDist)
                {
                    nearest.Dequeue();

                    float[] toInsert = new float[2];
                    toInsert[0] = pivotIndex;
                    toInsert[1] = myDist;
                    //insert negative to make order in queue work, but make the head the worst one rather then the best
                    nearest.Enqueue(toInsert, -myDist);
                }
            }
        }

        planeDist *= planeDist; //squared
        planeDist = Mathf.Abs(planeDist);

        whichDir = (whichDir + 1) % 2;
        //if there are points on the other side, and there are potentially points taht are closer then what we have
        //we also check the other side no matter what if we havent found k nearest neighbours yet
        if ((planeDist <= nearest.First[1]) || (nearest.Count < howMany))
        {
            if(children[whichDir] != null)
            {
                //recurse
                children[whichDir].search(point, ref nearest, howMany);
            }
        }
    }
}
