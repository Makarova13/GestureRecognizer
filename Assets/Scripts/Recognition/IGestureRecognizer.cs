using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Recognition
{
    public interface IGestureRecognizer
    {
        bool Recognize(List<Vector2> gesture);
        void RecordTemplate(List<Vector2> points);
        List<Vector2> Template { get; set; }
    }
}
