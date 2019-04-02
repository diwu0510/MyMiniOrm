using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMiniOrm;
using MyMiniOrm.Commons;
using WebTest.Models;

namespace WebTest.Controllers
{
    public class SchoolsController : Controller
    {
        private readonly MyDb _db = MyDb.New();
        
        public ActionResult Index()
        {
            var list = _db.Query<School>().Where(s => !s.IsDel).OrderByDesc(s => s.UpdateAt).ToList();
            return View(list);
        }

        // GET: Schools/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Schools/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            var entity = new School();
            TryUpdateModel(entity);
            try
            {
                entity.CreateBy = "132";
                entity.UpdateBy = "132";
                entity.Owner = "132";

                var result = _db.InsertIfNotExists(entity, s => s.SchoolName == entity.SchoolName);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError(string.Empty, "创建失败");
                return View(entity);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(entity);
            }
        }

        // GET: Schools/Edit/5
        public ActionResult Edit(int id)
        {
            var entity = _db.Load<School>(id);
            return View(entity);
        }

        // POST: Schools/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var entity = new School();
            TryUpdateModel(entity);

            try
            {
                entity.CreateBy = "132";
                entity.UpdateBy = "132";

                var result = _db.UpdateIfNotExits(entity, s => s.SchoolName == entity.SchoolName && s.Id != entity.Id);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "更新失败");

                return View(entity);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(entity);
            }
        }

        // GET: Schools/Delete/5
        public ActionResult Delete(int id)
        {
            var entity = _db.Load<School>(id);
            return View(entity);
        }

        // POST: Schools/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var entity = new School();
            TryUpdateModel(entity);
            try
            {
                //var result = _db.Delete<School>(id);
                //if (result > 0)
                //{
                //    return RedirectToAction("Index");
                //}
                var result = _db.Update<School>(DbKvs.New().Add("IsDel", true), s => s.Id == id);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "删除失败");
                return View(entity);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(entity);
            }
        }
    }
}
