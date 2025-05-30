using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AvailablePlaces { get; set; } // Число свободных мест
        public double AreaPerPlace { get; set; } // Полезная площадь на место
    }
}
