using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResponseApp.Models
{
    public class Response
    {
        public int Id { get; set; }
        public string ResponseTitle { get; set; }
        public string ResponseText { get; set; }
        public int AssignmentId { get; set; }
    }
}
