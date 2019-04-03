# MyMiniOrm
一个简单的Orm实现，项目作业的数据访问工具层。
---
### 约定
数据实体对象继承自 IEntity 接口，该接口定义了一个 int 类型的Id属性。

```
public interface IEntity
{
    int Id { get; set; }
}

// 数据实体类
public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ClazzId { get; set; }
    public Clazz Clazz { get; set; }

    // 更新时忽略该属性
    [MyColumn(UpdateIgnore = true)]
    public DateTime CreateAt { get; set; }
}

public class Clazz : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

## 用法

实例化对象：

```
var db = new MyDb("DataSource=.;Database=Test;USER ID=sa;Password=1234");
```
### 查询单个实体：
```
var student = db.Load<Student>(1);

var student = db.Load<Student>(s => s.Name == "张三");
```
### 查询多个实体：
```
var student = db.Fetch<Student>();

var student = db.PageList<T>(2, 10, out var recordCount, s => s.Name.Contains("张三"), s=>s.Name);
```
### Fluent 查询
```
var query = db.Query<Student>()
    .Include(s => s.Clazz)
    .Where(s => s.CreateAt > DateTime.Today.AddDays(-1))
    .OrderBy(s => s.Clazz.Id)
    .ThenOrderByDesc(s => s.Name);

var student = query.FirstOrDefault();
var students = query.ToList();
var students2 = query.ToPageList(2, 2, out var recordCount);
```
### 插入
```
var student = new Student
{
    Name = "张三",
    ClazzId = 1,
    CreateAt = DateTime.Now
};
db.Insert(student);    // 会将新产生的Id赋值到student.Id属性
Console.WriteLine($"{student.Id}");

// 批量插入

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

// 如果不存在，则插入
// 如限制用户名不能重复 InsertIfNotExist(user, u => u.Name == user.Name)

int InsertIfNotExists<T>(T entity, Expression<Func<T, bool>> where) where T : class, IEntity, new()
```

### 更新
```
var student = db.Load<Student>(1);
student.Name = student.Name + "修改过";
var result = db.Update(student);

// 批量更新
var students = db.Fetch<Student>(s => s.Id > 1);
foreach (var student in students)
{
    student.Name += student.Name + "批量修改过";
}
var count = db.Update(students);
Console.WriteLine($"修改了 {count} 行");

// 如果不存在则更新
// UpdateIfNotExists(user, u=>u.Name == user.Name && u.Id != user.Id)

int UpdateIfNotExits<T>(T entity, Expression<Func<T, bool>> where)
```

### 更新-注意，以下内容未经过测试
```
// 通过Id修改指定列
db.Update<Student>(1, DbKvs.New().Add("Name", "张三"));

var student = db.Load<Student>(1);
student.Name = student.Name + "测试修改";
student.ClazzId = 2;

// 更新指定对象的指定属性（指定忽略属性）
// 注意，下面方法传入的是属性名而不是列名

var count = db.UpdateInclude<Student>(student, new[] {"Name", "ClazzId"});
var count2 = db.UpdateInclude<Student>(student, s => new { s.Name, s.ClazzId };
var count3 = db.UpdateIgnore<Student>(student, new[] {"CreateAt"});
var count4 = db.UpdateInclude<Student>(student, s => new { s.CreateAt, s.Creator, s.IsDel };

// 通过指定条件修改指定列，注意第一个参数传入的是属性名而不是列名

db.Update<Student>(DbKvs.New().Add("ClazzId", 2), s => s.ClazzId == 1);
```

### 删除
```
// 如果实体继承ISoftDelete，此方法将IsDel列赋值为0，可通过传入 isForce=true，强制delete

// int Delete<T>(int id, bool isForce = false) where T : class, IEntity, new()
db.Delete<Student>(1, true);

// int Delete<T>(IEnumerable<int> idList, bool isForce = false) where T : class, IEntity, new()
db.Delete<Student>(new[] {1,2,3}, true);
```

暂时这么多，后继功能将陆续添加进来。注意：此项目仅用于作业内部交流，代码为经过比较严格的测试，可能会有坑，慎入。
