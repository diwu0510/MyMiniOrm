using System;

namespace WebTest.Models
{
    public class StudentSearchDto
    {
        public string Key { get; set; }

        public int? School { get; set; }

        public DateTime? CreateStart { get; set; } 

        public DateTime? CreateEnd { get; set; }
    }
}