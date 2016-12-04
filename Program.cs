using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var ini = new IniFile();
            var val = ini.GetValue(nameof(ini.TestSection),nameof(ini.TestValue));
        }
    }
}
