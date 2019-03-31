using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm;
using MyMiniOrm.Commons;
using MyMiniOrm.Queryable;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression<Func<Student, bool>> expr = s => !s.IsDel && true;

            Console.Read();
        }
    }
}
