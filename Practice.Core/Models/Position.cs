using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Название должности
    }

}
