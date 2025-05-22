using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResponseApp.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string AssignmentTitle { get; set; }
        public int CourseId { get; set; }
        public string AssignmentDescription { get; set; }
        public int AssignmentPosition { get; set; }
    }
}
