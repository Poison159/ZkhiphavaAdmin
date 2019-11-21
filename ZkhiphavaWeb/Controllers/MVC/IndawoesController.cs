using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.mvc
{
    [Authorize]
    public class IndawoesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public List<string> vibes = new List<string>() { "Chilled", "Club", "Outdoor", "Pub/Bar" };
        // GET: Indawoes
        public ActionResult Index()
        {
            return View(db.Indawoes.ToList());
        }

        // GET: Indawoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }

        // GET: Indawoes/Create
        public ActionResult Create()
        {
            Indawo indawo = new Indawo();
            ViewBag.type = new SelectList(vibes);
            return View(indawo);
        }

        // POST: Indawoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,type,rating,entranceFee,name,lat,lon,address,imgPath,instaHandle")] Indawo indawo)
        {
            ViewBag.type = new SelectList(vibes);
            if (ModelState.IsValid)
            {

                db.Indawoes.Add(indawo);
                db.SaveChanges();
                return RedirectToAction("Create", "Images", new { area = "" });
            }

            return View();
        }

        // GET: Indawoes/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.type = new SelectList(vibes);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }

        // POST: Indawoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,type,rating,entranceFee,name,lat,lon,address,imgPath,instaHandle")] Indawo indawo)
        {
            ViewBag.type = new SelectList(vibes);
            if (ModelState.IsValid)
            {
                db.Entry(indawo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(indawo);
        }

        // GET: Indawoes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }

        // POST: Indawoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            db.Indawoes.Remove(indawo);
            foreach (var item in db.Events.Where(x => x.indawoId == indawo.id)) { db.Events.Remove(item); }
            foreach (var item in db.Images.Where(x => x.indawoId == indawo.id)) { db.Images.Remove(item); }
            foreach (var item in db.OperatingHours.Where(x => x.indawoId == indawo.id)) { db.OperatingHours.Remove(item); }
            foreach (var item in db.SpecialInstructions.Where(x => x.indawoId == indawo.id)) { db.SpecialInstructions.Remove(item); }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
