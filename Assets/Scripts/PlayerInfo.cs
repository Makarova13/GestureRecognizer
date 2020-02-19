using Assets.Scripts.Recognition;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    [Serializable]
    public class PlayerInfo
    {
        private static PlayerInfo _instance;

        public static int HighScore { get; set; }

        public static int CurrentScore { get; set; }


        private PlayerInfo()
        { }

        public static PlayerInfo GetInfo(FileHandler<int> fileHandler)
        {
            if (_instance == null)
            {
                _instance = new PlayerInfo();
                CurrentScore = 0;
            }

            if (!File.Exists(fileHandler.FilePath))
            {
                HighScore = 0;
                fileHandler.Save(HighScore);

                return _instance;
            }

            HighScore = fileHandler.Load();

            return _instance;
        }
    }
}
