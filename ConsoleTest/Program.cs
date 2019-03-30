using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm;
using MyMiniOrm.Queryable;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var query = new MyQueryable<Student>();
            //var list = query.Include(q => q.Clazz).Where(q => q.Id > 1).OrderBy(q => q.Name).ToList();

            //foreach (var student in list)
            //{
            //    Console.WriteLine($"{student.Name} - {student.Clazz.Name}");
            //}

            //Console.WriteLine("");

            //var query2 = new MyQueryable<Student>();
            //var list2 = query2
            //    .Include(q => q.Clazz)
            //    .Where(q => q.Clazz.Id > 1)
            //    .OrderBy(q => q.Name)
            //    .ToPageList(2, 2, out var recordCount);

            //foreach (var student in list2)
            //{
            //    Console.WriteLine($"{student.Name} - {student.Clazz.Name}");
            //}

            //var query2 = new MyQueryable<Student>();
            //var student = query2
            //    .Include(q => q.Clazz)
            //    .Where(q => q.Clazz.Id > 1)
            //    .OrderBy(q => q.Name)
            //    .SingleOrDefault();

            var db = new MyDb("Data Source=.;Database=Test;User ID=sa;Password=790825");
            //var student = db.Query<Student>().Include(s => s.Clazz).SingleOrDefault();
            //var student = db.Load<Student>(1);
            //Console.WriteLine($"修改前：{student.Name} - {student.Clazz?.Name}");
            //student.Name = student.Name + "修改过";
            //var result = db.Update(student);
            //Console.WriteLine(result > 0 ? "修改成功" : "修改失败");
            //student = db.Load<Student>(1);
            //Console.WriteLine($"修改后：{student.Name} - {student.Clazz?.Name}");
            //Console.WriteLine($"{student.Name} - {student.Clazz.Name}");

            //var students = db.Fetch<Student>(s => s.Id > 1);
            //foreach (var student in students)
            //{
            //    student.Name += student.Name + "批量修改过";
            //}

            //var count = db.Update(students);
            //Console.WriteLine($"修改了 {count} 行");

            //students = db.Fetch<Student>();

            //foreach (var student in students)
            //{
            //    Console.WriteLine($"{student.Name}");
            //}

            var students = db.Fetch<Student>();
            Console.WriteLine(students.Count);

            //var count = db.Delete<Student>(new[] {6, 7});
            var count = db.Delete<Student>(4);
            Console.WriteLine($"删除了 {count} 行");

            count = db.GetCount<Student>();

            Console.WriteLine(count);

            Console.Read();
        }
    }
}
