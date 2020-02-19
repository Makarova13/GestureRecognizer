using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Recognition
{
    class RecognizerWithFormCoeficient : IGestureRecognizer
    {
        private readonly List<Vector2> template;
        private float kTemplate;
        private const int maxPoints = 64;                  // max number of point in the gesture

        public List<Vector2> Template
        { 
            get 
            { 
                return template;
            } 
            set
            {
                List<Vector2> temp = new List<Vector2>();
                temp.AddRange(value);
                ToPolarCoordinates(temp);
                kTemplate = FormCoeficient(temp);
            }
        }

        public bool Recognize(List<Vector2> gesture)
        {
            gesture = OptimizeGesture(gesture);
            ToPolarCoordinates(gesture);
            float kGesture = FormCoeficient(gesture);

            return Compare(kGesture, kTemplate);
        }

        public void RecordTemplate(List<Vector2> points)
        {
            points = OptimizeGesture(points);
        }

        private void ToPolarCoordinates(List<Vector2> points)
        {
            Vector2 temp;

            for (int i = 0; i < points.Count; i++)
            {
                temp.x = (float)Math.Sqrt(points[i].x * points[i].x + points[i].y * points[i].y);
                temp.y = points[i].x / points[i].y; // ctg bc we will use it in the formula
                points[i] = temp;
            }
        }

        private float FormCoeficient(List<Vector2> points)
        {
            float Kfa = 0;

            for (int i = 1; i < points.Count; i++)
            {
                Kfa += (points[i].y + points[i - 1].y);
            }

            Debug.Log(Kfa);

            return Kfa;
        }

        private bool Compare(float k1, float k2)
        {
            if (k1 > k2)
            {
                Debug.Log(k2 / k1);

                return (k2 / k1) > 0.6f;
            }

            return (k1 / k2) > 0.6f;
        }

        private static List<Vector2> OptimizeGesture(List<Vector2> points)
        {
            // calc the interval relative the length of the gesture drawn by the user
            float interval = CalcTotalGestureLength(points) / (maxPoints / 1.5f);

            List<Vector2> optimizedPoints = new List<Vector2>
            {
                points[0]
            };

            float currentDistanceBetween2Points;
            float tempDistance = 0.0f;
            Vector2 newPoint;
            float newX;
            float newY;

            for (int i = 1; i < points.Count; ++i)
            {
                currentDistanceBetween2Points = CalcDistance(points[i - 1], points[i]);

                if ((tempDistance + currentDistanceBetween2Points) >= interval)
                {
                    Vector2 v1 = points[i - 1];
                    Vector2 v2 = points[i];

                    // the calc is: old pixel + the differens of old and new pixel multiply  
                    newX = v1.x + ((interval - tempDistance) / currentDistanceBetween2Points) * (v2.x - v1.x);
                    newY = v1.y + ((interval - tempDistance) / currentDistanceBetween2Points) * (v2.y - v1.y);

                    newPoint = new Vector2(newX, newY);
                    optimizedPoints.Add(newPoint);

                    tempDistance = 0.0f;
                }
                else
                {
                    tempDistance += currentDistanceBetween2Points;
                }
            }

            return optimizedPoints;
        }

        private static float CalcDistance(Vector2 point1, Vector2 point2)
        {
            // distance between two vector points.
            float dx = point2.x - point1.x;
            float dy = point2.y - point1.y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private static float CalcTotalGestureLength(List<Vector2> points)
        {
            // total length of gesture path
            float length = 0.0f;
            for (int i = 1; i < points.Count; ++i)
            {
                length += CalcDistance((Vector2)points[i - 1], (Vector2)points[i]);
            }

            return length;
        }
    }
}
