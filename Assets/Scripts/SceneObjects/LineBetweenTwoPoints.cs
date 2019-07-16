﻿using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// Represents a line between two graphpoints and moves the line accordingly when the graphpoints move.
    /// Either the line is a line from one graphpoint to another (having one mid point as the graphpoint in the graph between).
    /// Or it is clustered line. In this case it goes from a centroid of a cluster to another and has two anchorpoints more so 5 points in total.
    /// </summary>
    class LineBetweenTwoPoints : MonoBehaviour
    {
        public Transform t1, t2, t3;
        public Vector3 fromGraphCentroid;
        public Vector3 midGraphCentroid;
        public Vector3 toGraphCentroid;
        public bool centroids;

        public Graph.GraphPoint graphPoint1;
        public Graph.GraphPoint graphPoint2;
        public Graph.GraphPoint graphPoint3;
        public SelectionManager selectionManager;
        public Graph.OctreeNode fromClusterNode;
        public Graph.OctreeNode toClusterNode;

        private LineRenderer lineRenderer;
        private Vector3[] linePosistions;
        private Vector3 fromPos, toPos, midPos, firstAnchor, secondAnchor;
        private Vector3 middle;
        private Vector3 currentTarget;
        private Vector3 currentPos;
        private bool initAnimate;
        private float x;
        private AnimationCurve curve;


        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (centroids)
            {
                fromPos = t1.TransformPoint(fromGraphCentroid);
                toPos = t2.TransformPoint(toGraphCentroid);
                midPos = t3.TransformPoint(midGraphCentroid);
                firstAnchor = (fromPos + midPos) / 2f;
                secondAnchor = (midPos + toPos) / 2f;
                linePosistions = new Vector3[] { fromPos, firstAnchor, midPos, secondAnchor, toPos };
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, fromPos);
                currentPos = linePosistions[0];
                currentTarget = linePosistions[1];
                lineRenderer.startWidth = lineRenderer.endWidth += 0.10f;
                initAnimate = true;

            }
            else
            {
                fromPos = t1.TransformPoint(graphPoint1.Position);
                toPos = t2.TransformPoint(graphPoint2.Position);
                midPos = t3.TransformPoint(graphPoint3.Position);
                linePosistions = new Vector3[] { fromPos, midPos, toPos };
                lineRenderer.SetPosition(0, fromPos);
                currentPos = linePosistions[0];
                currentTarget = linePosistions[1];
                initAnimate = true;
            }
        }

        private void Update()
        {
            if (initAnimate)
            {
                InitLine();
            }
            else if (t1.hasChanged || t2.hasChanged)
            {
                if (centroids)
                {
                    fromPos = t1.TransformPoint(fromGraphCentroid);
                    toPos = t2.TransformPoint(toGraphCentroid);
                    midPos = t3.TransformPoint(midGraphCentroid);
                    firstAnchor = (fromPos + midPos) / 2f;
                    secondAnchor = (midPos + toPos) / 2f;
                    lineRenderer.SetPositions(new Vector3[] { fromPos, firstAnchor, midPos, secondAnchor, toPos });
                }
                else
                {
                    fromPos = t1.TransformPoint(graphPoint1.Position);
                    toPos = t2.TransformPoint(graphPoint2.Position);
                    midPos = t3.TransformPoint(graphPoint3.Position);
                    lineRenderer.SetPositions(new Vector3[] { fromPos, midPos, toPos });
                }
            }
        }
        /// <summary>
        /// Animation that shows line progressivly move towards anchor points and lastly enpoint.
        /// </summary>
        private void InitLine()
        {
            float dist = Vector3.Distance(currentPos, currentTarget);
            x += Time.deltaTime * 5f; ;
            float increment = Mathf.Lerp(0, dist, x);
            if (lineRenderer.positionCount == 1)
            {
                lineRenderer.positionCount++;
                Vector3 pointAlongLine = (2 * increment) * Vector3.Normalize(currentTarget - currentPos) + currentPos;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, pointAlongLine);
            }
            else if (dist > increment)
            {
                Vector3 pointAlongLine = increment * Vector3.Normalize(currentTarget - currentPos) + currentPos;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, pointAlongLine);
            }
            else if (dist <= increment)
            {
                if (lineRenderer.positionCount == linePosistions.Length)
                {
                    if (centroids)
                    {
                        curve = new AnimationCurve();
                        curve.AddKey(1.0f, 1.0f);
                        curve.AddKey(0.2f, 0.10f);
                        curve.AddKey(0.5f, 0.1f);
                        curve.AddKey(0.8f, 0.10f);
                        curve.AddKey(0.0f, 1.0f);
                        lineRenderer.widthMultiplier = 0.15f;
                        lineRenderer.widthCurve = curve;
                    }
                    initAnimate = false;
                    return;
                }
                lineRenderer.positionCount++;
                currentPos = linePosistions[lineRenderer.positionCount - 2];
                currentTarget = linePosistions[lineRenderer.positionCount - 1];
                x = 0f;
            }
        }
    }
}


