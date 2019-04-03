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
using MyMiniOrm.Reflections;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dt = DateTime.Today;
            //var isActive = true;
            //Expression<Func<Student, bool>> expr = s => (s.CreateAt > dt || s.UpdateAt > dt) &&
            //                                            s.IsDel &&
            //                                            true &&
            //                                            isActive &&
            //                                            s.School.SchoolName.Contains("测试") &&
            //                                            s.CreateAt < s.UpdateAt &&
            //                                            s.Birthday == null;
            ////Expression<Func<Student, bool>> expr = s => isActive && s.Birthday != null && true;

            //var resolver = new ConditionResolver(MyEntityContainer.Get(typeof(Student)));

            //resolver.Resolve(expr.Body);
            //Console.WriteLine(resolver.GetCondition());

            //var parameters = resolver.GetParameters();
            //foreach (var parameter in parameters)
            //{
            //    Console.WriteLine($"{parameter.Key} : {parameter.Value}");
            //}


            //var search = LinqExtensions.False<Student>();
            //search = search.And(s => s.CreateAt > dt || s.UpdateAt > dt);
            //search = search.And(s => !s.IsDel);
            //search = search.And(s => s.School.SchoolName.Contains("测试"));

            //var visitor = new WhereExpressionVisitor<Student>();
            //visitor.Visit(expr);

            //Console.WriteLine(visitor.GetCondition());

            //var parameters = visitor.GetParameters();
            //foreach (var parameter in parameters)
            //{
            //    Console.WriteLine($"{parameter.Key} = {parameter.Value}");
            //}

            //Console.WriteLine("======================");

            //var visitor2 = new WhereExpressionVisitor<Student>();
            //visitor2.Visit(search);

            //Console.WriteLine(visitor2.GetCondition());
            //var parameters2 = visitor2.GetParameters();
            //foreach (var parameter in parameters2)
            //{
            //    Console.WriteLine($"{parameter.Key} = {parameter.Value}");
            //}

            Expression<Func<Student, object>> expr = s => new {s.StudentName, s.FKSchoolId};

            Expression<Func<Student, object>> expr2 = s => s.StudentName;

            var visitor1 = new ObjectExpressionVisitor(MyEntityContainer.Get(typeof(Student)));
            visitor1.Visit(expr);

            var visitor2 = new ObjectExpressionVisitor(MyEntityContainer.Get(typeof(Student)));
            visitor1.Visit(expr2);

            var kvs1 = visitor1.GetPropertyList();

            foreach (var kv in kvs1)
            {
                Console.WriteLine($"{kv.Key}-{kv.Value}");
            }

            var kvs2 = visitor2.GetPropertyList();

            foreach (var kv in kvs2)
            {
                Console.WriteLine($"{kv.Key}-{kv.Value}");
            }

            var student = new Student();

            var db = MyDb.New("").Update<Student>(student,
                s => new { s.FKSchoolId, s.StudentName});

            Console.Read();
        }
    }
}
