using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Recognition
{
    [Serializable]
    public class GestureTemplate
    {
        public string Name { get; set; }

        [field:NonSerialized]
        public List<Vector2> Points { get; set; }

        public List<SerializableVector2> SerializablePoints { get; set; }


        public void BeforeSerializing()
        {
            SerializablePoints = new List<SerializableVector2>();

            foreach(Vector2 v in Points)
            {
                SerializablePoints.Add(v);
            }
        }

        public void AfterSerializing()
        {
            Points = new List<Vector2>();

            foreach (SerializableVector2 v in SerializablePoints)
            {
                Points.Add(v);           
            }
        }

        public override string ToString()
        {
            string a = this.Name + "\n";

            foreach(var v in Points)
            {
                a += v.ToString();
            }

            return a;
        }
    }
}
