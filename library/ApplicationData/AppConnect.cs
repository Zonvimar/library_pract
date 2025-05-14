using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library.ApplicationData
{
    class AppConnect
    {
        public static LibraryEntities modelOdb { get; set; }
        public static Users CurrentUser { get; set; }

        public static void Initialize()
        {
            modelOdb = new LibraryEntities();
        }
    }
}
