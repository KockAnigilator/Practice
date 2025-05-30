using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Specialty
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Название специальности
    }
}
