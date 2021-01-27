using System;
using System.Collections.Generic;
using System.Text;

namespace TesonetTask
{
    public class Server
    {
        public string name;
        public int distance;

        public override string ToString()
        {
            return $"{name}, Distance: {distance}";
        }
    }
}
