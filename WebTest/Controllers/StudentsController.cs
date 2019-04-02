using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using MyMiniOrm;
using MyMiniOrm.Commons;
using MyMiniOrm.Expressions;
using WebTest.Models;

namespace WebTest.Controllers
{
    public class StudentsController : Controller
    {
        private readonly MyDb _db = MyDb.New();

        // GET: Students
        public ActionResult Index(int id, StudentSearchDto search)
        {
            var expr = LinqExtensions.True<Student>();
            expr = expr.And(s => s.IsDel == false);

            if (!string.IsNullOrWhiteSpace(search.Key))
            {
                expr = expr.And(s => s.StudentName.Contains(search.Key.Trim()) || s.Mobile.Contains(search.Key.Trim()));
            }

            if (search.School.HasValue)
            {
                expr = expr.And(s => s.FKSchoolId == search.School.Value);
            }

            if (search.CreateStart.HasValue)
            {
                expr = expr.And(s => s.CreateAt >= search.CreateStart.Value);
            }

            if (search.CreateEnd.HasValue)
            {
                expr = expr.And(s => s.CreateAt <= search.CreateEnd.Value);
            }

            id = id <= 0 ? 1 : id;

            var data = _db.Query<Student>()
                .Include(s => s.School)
                .Where(expr)
                .OrderByDesc(s => s.UpdateAt)
                .ToPageList(id, 20, out var recordCount);

            InitUi();
            return View(data);
        }

        
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            InitUi();
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            var entity = new Student();
            TryUpdateModel(entity);
            try
            {
                entity.CreateBy = "132";
                entity.UpdateBy = "132";
                entity.Owner = "132";

                var result = _db.InsertIfNotExists(entity, s => s.Mobile == entity.Mobile);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "创建失败");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            InitUi();
            return View(entity);
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            var entity = _db.Load<Student>(id);
            if (entity == null)
            {
                return HttpNotFound();
            }
            InitUi();
            return View(entity);
        }

        // POST: Students/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var entity = new Student();
            TryUpdateModel(entity);

            try
            {
                entity.CreateBy = "132";
                entity.UpdateBy = "132";

                var result = _db.UpdateIfNotExits(entity, s => s.Mobile == entity.Mobile && s.Id != entity.Id);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "更新失败");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            InitUi();
            return View(entity);
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            var entity = _db.Load<Student>(id);
            return View(entity);
        }

        // POST: Students/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var entity = new School();
            TryUpdateModel(entity);
            try
            {
                var result = _db.Update<Student>(DbKvs.New().Add("IsDel", true), s => s.Id == id);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "删除失败");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(entity);
        }

        private void InitUi()
        {
            var schools = _db.Query<School>().ToList();
            ViewBag.Schools = schools.Select(s => new SelectListItem {Text = s.SchoolName, Value = s.Id.ToString()});
        }
    }
}
