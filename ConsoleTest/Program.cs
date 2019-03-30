using System;
using System.Collections.Generic;
using System.Linq;
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

            //var students = db.Fetch<Student>();
            //Console.WriteLine(students.Count);

            ////var count = db.Delete<Student>(new[] {6, 7});
            //var count = db.Delete<Student>(4);
            //Console.WriteLine($"删除了 {count} 行");



            //var query = db.Query<Student>()
            //    .Include(s => s.Clazz)
            //    .Where(s => s.CreateAt > DateTime.Today.AddDays(-1))
            //    .OrderBy(s => s.Clazz.Id)
            //    .ThenOrderByDesc(s => s.Name);

            //var student = query.FirstOrDefault();
            //var students = query.ToList();
            //var students2 = query.ToPageList(2, 2, out var recordCount);

            //var student = new Student
            //{
            //    Name = "张三",
            //    ClazzId = 1,
            //    CreateAt = DateTime.Now
            //};

            //// 会将新产生的Id赋值到student.Id属性
            //db.Insert(student);

            //Console.WriteLine($"{student.Id}-{student.Name}");

            //count = db.GetCount<Student>();

            //Console.WriteLine(count);

            var students = new List<Student>
            {
                new Student {Name = "张三", ClazzId = 1, CreateAt = DateTime.Now},
                new Student {Name = "李四", ClazzId = 1, CreateAt = DateTime.Now},
                new Student {Name = "王五", ClazzId = 1, CreateAt = DateTime.Now},
                new Student {Name = "赵六", ClazzId = 1, CreateAt = DateTime.Now}
            };

            db.Insert(students);

            foreach (var stu in students)
            {
                Console.WriteLine($"{stu.Id}-{stu.Name}");
            }

            db.Update<Student>(1, DbKvs.New().Add("Name", "张三"));

            var student = db.Load<Student>(1);
            student.Name = student.Name + "测试修改";
            student.ClazzId = 2;
            var count = db.Update<Student>(student, new[] {"Name", "ClazzId"});

            db.Update<Student>(DbKvs.New().Add("ClazzId", 2), s => s.ClazzId == 1);

            Console.Read();
        }
    }
}
