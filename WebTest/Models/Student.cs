using MyMiniOrm.Attributes;
using MyMiniOrm.Commons;
using System;

namespace WebTest.Models
{
    public class Student : IEntity
    {
        [MyKey(FieldName = "StudentId")]
        public int Id { get; set; }

        public string StudentName { get; set; }

        public string Mobile { get; set; }

        public string Card { get; set; }

        public string State { get; set; }

        public DateTime? Birthday { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public int? FKSchoolId { get; set; }

        [MyForeignKey("FKSchoolId")]
        public School School { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public string Owner { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public bool IsDel { get; set; } = false;

        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; } = DateTime.Now;

        [MyColumn(UpdateIgnore = true)]
        public string CreateBy { get; set; }

        public DateTime UpdateAt { get; set; } = DateTime.Now;

        public string UpdateBy { get; set; }
    }
}
