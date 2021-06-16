using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JPBotelho
{
    /*  
        Catmull-Rom splines are Hermite curves with special tangent values.
        Hermite curve formula:
        (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
        Tangent M[k] = (P[k+1] - P[k-1]) / 2
    */
    public class CatmullRom
    {
        //Struct to keep position, normal and tangent of a spline point
        [System.Serializable]
        public struct CatmullRomPoint
        {
            public Vector3 position;
            public Vector3 tangent;
            public Vector3 normal;

            public CatmullRomPoint(Vector3 position, Vector3 tangent, Vector3 normal)
            {
                this.position = position;
                this.tangent = tangent;
                this.normal = normal;
            }
        }

        //Evaluates curve at t[0, 1]. Returns point/normal/tan struct. [0, 1] means clamped between 0 and 1.
        public static CatmullRomPoint Evaluate(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
        {
            Vector3 position = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
            Vector3 tangent = CalculateTangent(start, tanPoint1, tanPoint2, end, t);            
            Vector3 normal = NormalFromTangent(tangent);

            return new CatmullRomPoint(position, tangent, normal);
        }

        //Calculates curve position at t[0, 1]
        public static Vector3 CalculatePosition(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
        {
            // Hermite curve formula:
            // (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
            Vector3 position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
                + (t * t * t - 2.0f * t * t + t) * tanPoint1
                + (-2.0f * t * t * t + 3.0f * t * t) * end
                + (t * t * t - t * t) * tanPoint2;

            return position;
        }

        //Calculates tangent at t[0, 1]
        public static Vector3 CalculateTangent(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
        {
            // Calculate tangents
            // p'(t) = (6t² - 6t)p0 + (3t² - 4t + 1)m0 + (-6t² + 6t)p1 + (3t² - 2t)m1
            Vector3 tangent = (6 * t * t - 6 * t) * start
                + (3 * t * t - 4 * t + 1) * tanPoint1
                + (-6 * t * t + 6 * t) * end
                + (3 * t * t - 2 * t) * tanPoint2;

            return tangent.normalized;
        }
        public static float ApproximateLength(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, int subdiv) {
            Vector3 lastPoint = CalculatePosition(start, tanPoint1, tanPoint2, end, 0f);
            float length = 0f;
            for(int i=1;i<subdiv;i++){
                float t = (float)i/(float)subdiv;
                Vector3 nextPoint = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
                length += Vector3.Distance(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }
            return length;
        }
        
        //Calculates normal vector from tangent
        public static Vector3 NormalFromTangent(Vector3 tangent)
        {
            return Vector3.Cross(tangent, Vector3.up).normalized / 2;
        }    
        public static Vector3 ClosestPointOnCurve(Vector3 pt, Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, out float bestT, int ndivs) {
            Vector3 result = new Vector3();
            float bestDistance = 0;
            bestT = 0;
            for (int i=0; i<=ndivs; i++) {
                float t = (float)(i) / (float)(ndivs);
                Vector3 p = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
                float dissq = Vector3.Distance(p, pt);
                if (i==0 || dissq < bestDistance) {
                    bestDistance = dissq;
                    bestT = t;
                    result = p;
                }
            }
            return result;
        }
    }
}