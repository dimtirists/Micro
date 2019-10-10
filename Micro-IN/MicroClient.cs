using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micro_IN
{
    public class MicroClient
    {
        

        public int idUser
        { get; set; }

        public string Name
        { get; set; }

        public string Sirname
        { get; set; }
        public string Telephone
        {
            get; set;
        }

        public string Comments
        {
            get; set;
        }

        public bool Active
        { get; set; }

        public DateTime DateTime;

        public int idClients
        {
            get; set;
        }

    }
}
