using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Recognition
{
    public class FileHandler<T> 
    {
        public string FilePath { get; private set; }

        private readonly BinaryFormatter bformatter;

        public FileHandler(string filePath)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new SerializationException();
            }

            FilePath = filePath + ".txt";
            bformatter = new BinaryFormatter();
        }

        public void Save(T data)
        {
            Stream stream = File.Open(FilePath, FileMode.OpenOrCreate);
            bformatter.Serialize(stream, data);
            stream.Close();

            Debug.Log("Saved.");
        }

        public T Load()
        {
            byte[] data = File.ReadAllBytes(FilePath);
            MemoryStream stream = new MemoryStream(data);
            T deserialized = (T)bformatter.Deserialize(stream);
            stream.Close();

            return deserialized;
        }
    }
}
