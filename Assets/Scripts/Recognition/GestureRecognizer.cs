using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Recognition
{
	public class GestureRecognizer : IGestureRecognizer
	{
		// recognizer settings
		const int maxPoints = 64;                  // max number of point in the gesture
		const int sizeOfScaleRect = 500;           // the size of the bounding box

		public static ArrayList newTemplateArr;

		public List<Vector2> Template { get; set; }

		public bool Recognize(List<Vector2> gesture)
		{
			OptimizeGesture(gesture);

			//_ = CalcCenterOfGesture(gesture);
			//  Vector2 v = (Vector2)pointArray[0];

			//   float radians = Mathf.Atan2(center.y - v.y, center.x - v.x);
			//   pointArray = RotateGesture(pointArray, -radians, center);

			ScaleGesture(gesture, sizeOfScaleRect);
			TranslateGestureToOrigin(gesture);
			float score = GestureMatch(gesture, Template);

			Debug.Log(score * 100);
			return (score * 100 >= 88);
		}

		public void RecordTemplate(List<Vector2> points)
		{
			OptimizeGesture(points);
			ScaleGesture(points, sizeOfScaleRect);
			TranslateGestureToOrigin(points);

			/*given the rotation: 
			Vector2 center = CalcCenterOfGesture(points);
			Vector2 v = points[0];
			float radians = Mathf.Atan2(center.y - v.y, center.x - v.x);
			RotateGesture(points, -radians, center);
			*/
		}

		private static void OptimizeGesture(List<Vector2> points)
		{
			// takes all the points in the gesture and finds the correct points compared with distance and the maximun value of points
			// calc the interval relative the length of the gesture drawn by the user
			float interval = CalcTotalGestureLength(points) / (maxPoints - 1);

			Debug.Log("CalcTotalGestureLength done");
			// use the same starting point in the new array from the old one. 
			List <Vector2> optimizedPoints = new List<Vector2>();
			optimizedPoints.Add(points[0]);

			float currentDistanceBetween2Points;
			float tempDistance = 0.0f;
			Vector2 newPoint;
			float newX;
			float newY;
			List<Vector2> temp;
			Vector2 last;

			// run through the gesture array. Start at i = 1 because we compare point two with point one)
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

					// create new point
					newPoint = new Vector2(newX, newY);

					// set new point into array
					optimizedPoints.Add(newPoint);

					temp = points.GetRange(i, points.Count - i - 1);
					last = points[points.Count - 1];

					//points.AddRange(temp); //?
					points.Add(last);
					points.RemoveRange(i + 1, temp.Count);
					points.InsertRange(i + 1, temp);
					points.Insert(i, newPoint);

					tempDistance = 0.0f;
				}
				else
				{
					tempDistance += currentDistanceBetween2Points;

					// the point was too close to the last point compared with the interval.Therefore the distance 
					// will be stored for the next point to be compared.
				}
			}

			// Rounding-errors might happens. Just to check if all the points are in the new array
			if (optimizedPoints.Count == maxPoints - 1)
			{
				Vector2 v = points[points.Count - 1];
				optimizedPoints.Add(new Vector2(v.x, v.y));
			}
		}

		static List<Vector2> RotateGesture(List<Vector2> pointArray, float radians, Vector3 center)
		{
			// loop through original array, rotate each point and return the new array
			List<Vector2> newArray = new List<Vector2>();
			float cos = Mathf.Cos(radians);
			float sin = Mathf.Sin(radians);

			for (int i = 0; i < pointArray.Count; ++i)
			{
				Vector2 v = (Vector2)pointArray[i];
				float newX = (v.x - center.x) * cos - (v.y - center.y) * sin + center.x;
				float newY = (v.x - center.x) * sin + (v.y - center.y) * cos + center.y;
				newArray.Add(new Vector2(newX, newY));
			}
			return newArray;
		}

		static void ScaleGesture(List<Vector2> points, int size)
		{
			// equal min and max to the opposite infinity, such that every gesture size can fit the bounding box.
			float minX = Mathf.Infinity;
			float maxX = Mathf.NegativeInfinity;
			float minY = Mathf.Infinity;
			float maxY = Mathf.NegativeInfinity;

			// loop through array. Find the minimum and maximun values of x and y to be able to create the box
			foreach (Vector2 v in points)
			{
				if (v.x < minX) minX = v.x;
				if (v.x > maxX) maxX = v.x;
				if (v.y < minY) minY = v.y;
				if (v.y > maxY) maxY = v.y;
			}

			// create a rectangle surronding the gesture as a bounding box.
			Rect BoundingBox = new Rect(minX, minY, maxX - minX, maxY - minY);
			List<Vector2> newArray = new List<Vector2>();

			foreach (Vector2 v in points)
			{
				float newX = v.x * (size / BoundingBox.width);
				float newY = v.y * (size / BoundingBox.height);
				newArray.Add(new Vector2(newX, newY));
			}
		}

		static void TranslateGestureToOrigin(List<Vector2> points)
		{
			Vector2 origin = new Vector2(0, 0);
			Vector3 center = CalcCenterOfGesture(points);
			List<Vector2> translatedPoints = new List<Vector2>();

			foreach (Vector2 v in points)
			{
				float newX = v.x + origin.x - center.x;
				float newY = v.y + origin.y - center.y;
				translatedPoints.Add(new Vector2(newX, newY));
			}
		}


# region THE MATCHING PROCESS

        static float GestureMatch(List<Vector2> points, List<Vector2> Template)
		{
			float tempDistance = Mathf.Infinity;
			float distance = CalcGestureTemplateDistance(points, Template);

			if (distance < tempDistance)
			{
				tempDistance = distance;
			}

			float HalfDiagonal = 0.5f * Mathf.Sqrt(Mathf.Pow(sizeOfScaleRect, 2) + Mathf.Pow(sizeOfScaleRect, 2));
			float score = 1.0f - (tempDistance / HalfDiagonal);

			return score;
		}

		#endregion


# region THE HELP FUNCTIONS


		static Vector2 CalcCenterOfGesture(List<Vector2> points)
		{
			// finds the center of the drawn gesture

			float averageX = 0.0f;
			float averageY = 0.0f;

			foreach (Vector2 v in points)
			{
				averageX += v.x;
				averageY += v.y;
			}

			averageX /= points.Count;
			averageY /= points.Count;

			return new Vector2(averageX, averageY);
		}

		static float CalcDistance(Vector2 point1, Vector2 point2)
		{
			// distance between two vector points.
			float dx = point2.x - point1.x;
			float dy = point2.y - point1.y;

			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		static float CalcTotalGestureLength(List<Vector2> points)
		{
			// total length of gesture path
			float length = 0.0f;
			for (int i = 1; i < points.Count; ++i)
			{
				length += CalcDistance((Vector2)points[i - 1], (Vector2)points[i]);
			}

			return length;
		}

		static float CalcGestureTemplateDistance(List<Vector2> points, List<Vector2> TemplatePoints)
		{
			// calc the distance between gesture path from user and the Template gesture
			float distance = 0.0f;

			int count = points.Count >= TemplatePoints.Count ? TemplatePoints.Count : points.Count;

			// assumes points.length == TemplatePoints.length
			for (int i = 0; i < count; ++i)
			{
				distance += CalcDistance(points[i], TemplatePoints[i]);
			}

			return distance / points.Count;
		}

		/*
		static float CalcDistanceAtOptimalAngle(List<Vector2> points, List<Vector2> Template, float negativeAngle, float positiveAngle)
		{
			// Create two temporary distances. Compare while running through the angles. 
			// Each time a lower distace between points and Template points are foound store it in one of the temporary variables. 

			float radian1 = Mathf.PI * negativeAngle + (1.0f - Mathf.PI) * positiveAngle;
			float tempDistance1 = CalcDistanceAtAngle(points, Template, radian1);

			float radian2 = (1.0f - Mathf.PI) * negativeAngle + Mathf.PI * positiveAngle;
			float tempDistance2 = CalcDistanceAtAngle(points, Template, radian2);

			// the higher the number compareDetail is, the better recognition this system will perform. 
			for (int i = 0; i < compareDetail; ++i)
			{
				if (tempDistance1 < tempDistance2)
				{
					positiveAngle = radian2;
					radian2 = radian1;
					tempDistance2 = tempDistance1;
					radian1 = Mathf.PI * negativeAngle + (1.0f - Mathf.PI) * positiveAngle;
					tempDistance1 = CalcDistanceAtAngle(points, Template, radian1);
				}
				else
				{
					negativeAngle = radian1;
					radian1 = radian2;
					tempDistance1 = tempDistance2;
					radian2 = (1.0f - Mathf.PI) * negativeAngle + Mathf.PI * positiveAngle;
					tempDistance2 = CalcDistanceAtAngle(points, Template, radian2);
				}
			}

			return Mathf.Min(tempDistance1, tempDistance2);
		}

		static float CalcDistanceAtAngle(ArrayList pointArray, ArrayList T, float radians)
		{
			// calc the distance of Template and user gesture at 
			Vector2 center = CalcCenterOfGesture(pointArray);
			ArrayList newpoints = RotateGesture(pointArray, radians, center);

			return CalcGestureTemplateDistance(newpoints, T);
		}
		*/

		#endregion
	}
}