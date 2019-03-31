using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using MyMiniOrm;
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
            expr = expr.And(s => !s.IsDel);

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
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Students/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Students/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private void InitUi()
        {
            var schools = _db.Fetch<School>();
            ViewBag.Schools = schools.Select(s => new SelectListItem {Text = s.SchoolName, Value = s.Id.ToString()});
        }
    }
}
