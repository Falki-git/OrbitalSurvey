using UnityEngine;

namespace OrbitalSurvey
{
    public class MyTestSaveData
    {
        public bool TestBool { get; set; }
        public string TestString { get; set; }
        public int TestInt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Texture2D TestTexture { get; set; }
        public bool[,] TestBoolArray { get; set; }
        public Color32[,] TestColorArray { get; set; }
    }
}
