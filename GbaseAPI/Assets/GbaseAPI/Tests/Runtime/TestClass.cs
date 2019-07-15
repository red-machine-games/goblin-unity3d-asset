using System;
using System.Collections.Generic;

namespace Gbase
{
    [Serializable]
    public class TestClass
    {
        public int a = 1;
        public float b = .1f;
        public string[] c = {"qwerty", "asdfgh", "zxcvbn"};
        public List<ttClass> d = new List<ttClass>( new [] { new ttClass() });
        public ttClass t = new ttClass();
    }

    [Serializable]
    public class ttClass
    {
        public int level = 10;
    }
}