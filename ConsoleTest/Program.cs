using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm;
using MyMiniOrm.Commons;
using MyMiniOrm.Expressions;
using MyMiniOrm.Queryable;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression<Func<Student, bool>> expr = s => (s.StudentName.Contains("name") || s.School.SchoolName.Contains("name")) && s.IsDel == false && true && s.School.SchoolName.Contains("测试");
            var visitor = new ConditionExpressionVisitor();
            visitor.Visit(expr);
            var stack = visitor.GetStack();

            Console.Read();
        }
    }
}
