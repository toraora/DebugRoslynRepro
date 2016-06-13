using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src
{
    class Test
    {
        public static void Hello()
        {
            System.Diagnostics.Debugger.Launch();
            throw new NotImplementedException("meow");
        }
    }
}
