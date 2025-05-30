using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Education
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty; // Что закончил
    }
}
