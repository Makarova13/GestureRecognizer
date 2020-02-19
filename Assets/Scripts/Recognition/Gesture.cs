using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Assets.Scripts.Recognition
{
    public class Gesture : MonoBehaviour
    {
        #region public fields

        public Text scoreUI;
        public Text timeUI;
        public IGestureRecognizer gestureRecognizer;
        public GameObject Brush;
        public GameObject Trail;
        public LineRenderer TemplateDrawing;
        public Text TemplateName;

        #endregion

        #region private fields

        private List<GestureTemplate> allTemplates;
        private List<Vector2> points;
        private bool mouseDown;
        private GestureTemplate template;
        private FileHandler<List<GestureTemplate>> templatesFileHandler;
        private FileHandler<int> playerInfoFileHandler;
        private const string pathToTemplatesFile = @"Assets/Data/templates";
        private const string pathToPlayerInfoFile = @"Assets/Data/playerInfo";
        private float timeLeft = 30f;
        private float timeForLevel = 30f;

        #endregion

        void Start()
        {
            playerInfoFileHandler = new FileHandler<int>(pathToPlayerInfoFile);
            templatesFileHandler = new FileHandler<List<GestureTemplate>>(pathToTemplatesFile);

            Trail.SetActive(false);

            PlayerInfo.GetInfo(playerInfoFileHandler);

            allTemplates = templatesFileHandler.Load();

            foreach (GestureTemplate g in allTemplates)
            {
                g.AfterSerializing();
            }

            points = new List<Vector2>();
            gestureRecognizer = new RecognizerWithFormCoeficient();
            scoreUI.text = "0";
            timeUI.text = "30:00";

            Debug.Log("Templates loaded.");
            template = allTemplates[Randomizer()];
            Debug.Log(template.ToString());

            PlayerInfo.CurrentScore = 0;
            gestureRecognizer.Template = template.Points;
            TemplateName.text = template.Name;

       //     DrawTemplate(template.Points);
        }

        void Update()
        {
            if(timeLeft - 0.008f > 0)
            {
                timeLeft -= 0.008f;
                timeUI.text = String.Format(timeLeft.ToString("00.00"));
            }
            else
            {
                GameOver();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Trail.SetActive(true);
                mouseDown = true;
            }

            if (mouseDown)
            {
                Trail.SetActive(true);
                mouseDown = true;
                Brush.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
                Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                points.Add(p);

                StartCoroutine(WorldToScreenCoordinates());
            }

            if (Input.GetMouseButtonUp(0))
            {
                Trail.SetActive(false);

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    mouseDown = false;
                    gestureRecognizer.RecordTemplate(points);
                }
                else
                {
                    mouseDown = false;

                    if (gestureRecognizer.Recognize(points)) 
                    {
                        LevelPassed();
                    }                 

                    points.Clear();
                }
            }
        }

        private void LevelPassed()
        {
            template = allTemplates[Randomizer()];
            Debug.Log("Template: " + template.Name);

         //   DrawTemplate(template.Points);

            PlayerInfo.CurrentScore++;

            scoreUI.text = PlayerInfo.CurrentScore.ToString();
            timeForLevel *= 0.95f;
            timeLeft = (float)Math.Round(timeForLevel);
            gestureRecognizer.Template = template.Points;
            TemplateName.text = template.Name;
        }

        private void GameOver()
        {
            if(PlayerInfo.CurrentScore > PlayerInfo.HighScore)
            {
                PlayerInfo.HighScore = PlayerInfo.CurrentScore;
                playerInfoFileHandler.Save(PlayerInfo.HighScore);
            }

            SceneManager.LoadScene("GameOver");
        }

        private int Randomizer()
        {
            System.Random rnd = new System.Random();
            return rnd.Next(0, allTemplates.Count - 1);
        }

        private void DrawTemplate(List<Vector2> points)
        {
            List<Vector3> points3D = new List<Vector3>();
            ScaleGesture(points, 5);
           // TranslateGestureToOrigin(points);

            foreach (var p in points)
            {
                points3D.Add(Camera.main.ScreenToWorldPoint(p));
            }

            TemplateDrawing.positionCount = points.Count;
            TemplateDrawing.SetPositions(points3D.ToArray());
        }

        private IEnumerator WorldToScreenCoordinates()
        {
            // fix world coordinate to the viewport coordinate
            Vector3 screenSpace = Camera.main.WorldToScreenPoint(Brush.transform.position);

            while (Input.GetMouseButton(1))
            {
                Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace);
                Brush.transform.position = curPosition;
                yield return 0;
            }
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
    }
}
