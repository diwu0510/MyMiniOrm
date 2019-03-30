# MyMiniOrm
一个简单的Orm实现
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
// 方法：T Load<T>(int id)
var student = db.Load<Student>(1);

// 方法：T Load<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy)
var student = db.Load<Student>(s => s.Name == "张三");
```
### 查询多个实体：
```
// 方法：List<T> Fetch<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy)
var student = db.Fetch<Student>();

// 方法：List<T> PageList<T>(int pageIndex,int pageSize,out int recordCount,
//          Expression<Func<T, bool>> where = null,params Expression<Func<T, object>>[] orderBy)
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
