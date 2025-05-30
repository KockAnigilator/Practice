using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Hospital
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; } // Число мест
    }
}
