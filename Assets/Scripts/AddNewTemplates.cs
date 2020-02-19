using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using Assets.Scripts.Recognition;

public class AddNewTemplates : MonoBehaviour
{
    public InputField input;

    #region private fields

    private List<GestureTemplate> allTemplates;
    private List<Vector2> points;
    private bool mouseDown;
    private GameObject brush;
    private GameObject trail;
    private GestureTemplate template;
    private IGestureRecognizer gestureRecognizer;
    private FileHandler<List<GestureTemplate>> templatesFileHandler;
    const string pathToTemplatesFile = @"Assets/Data/templates";

    #endregion

    void Start()
    {
        try
        {
            templatesFileHandler = new FileHandler<List<GestureTemplate>>(pathToTemplatesFile);
            allTemplates = templatesFileHandler.Load();
            
            foreach(GestureTemplate g in allTemplates)
            {
                g.AfterSerializing();
            }
        }
        catch
        {
            allTemplates = new List<GestureTemplate>();
        }

        points = new List<Vector2>();
        trail = GameObject.Find("Trail");
        gestureRecognizer = new RecognizerWithFormCoeficient();
        brush = GameObject.Find("Brush");
        trail.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
        }

        if (mouseDown)
        {
            trail.SetActive(true);
            mouseDown = true;
            brush.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.016f);
            Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            points.Add(p);
            StartCoroutine(WorldToScreenCoordinates());
        }

        if (Input.GetMouseButtonUp(0))
        {
            trail.SetActive(false);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                points.Clear();
            }
            else
            {
                mouseDown = false;
                points.Clear();
            }
        }
    }

    public void Save()
    {
        template = new GestureTemplate()
        {
            Points = points,
            Name = input.text
        };

        allTemplates.Add(template);

        foreach(GestureTemplate g in allTemplates)
        {
            g.BeforeSerializing();
        }

        gestureRecognizer.RecordTemplate(points);
        templatesFileHandler.Save(allTemplates);
    }

    private IEnumerator WorldToScreenCoordinates()
    {
        Vector3 screenSpace = Camera.main.WorldToScreenPoint(brush.transform.position);

        while (Input.GetMouseButton(1))
        {
            Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace);
            brush.transform.position = curPosition;
            yield return 0;
        }
    }
}
