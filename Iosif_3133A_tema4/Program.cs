using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iosif_3133A_tema4
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Cub cub = new Cub())
            {
                cub.Run(30, 0);
            }
        }
    }
}
