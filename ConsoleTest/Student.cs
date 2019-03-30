using MyMiniOrm.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Commons;

namespace ConsoleTest
{
    public class Student : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Clazz Clazz { get; set; }
        
        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; }
    }
}
