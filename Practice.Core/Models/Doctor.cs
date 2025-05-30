using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Core.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Patronymic { get; set; }

        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public int SpecialtyId { get; set; }
        public int EducationId { get; set; }
        public int HospitalId { get; set; }
    }
}
