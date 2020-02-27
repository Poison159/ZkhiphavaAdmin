using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.mvc
{
    
    public class IndawoesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public List<string> vibes = new List<string>() { "Chilled", "Club", "Outdoor", "Pub/Bar" };
        // GET: Indawoes
        public ActionResult Index(string name,string type)
        {
            var vibesList = new List<string>();
            var indawoes = from cr in db.Indawoes select cr;
            var vibequery = from gmq in db.Indawoes
                            orderby gmq.type
                            select gmq.type;
            foreach (var indawo in indawoes.ToList()){
                indawo.images = db.Images.Where(x => x.indawoId == indawo.id).ToList();
            }
            if (!string.IsNullOrEmpty(name))
                indawoes = indawoes.Where(x => x.name.Contains(name));

            if (!string.IsNullOrEmpty(type))
            {
                indawoes = indawoes.Where(x => x.type.ToString().Equals(type));
            }
            
            
            vibesList.AddRange(vibequery.Distinct());
            ViewBag.type = new SelectList(vibesList);
            ViewBag.Stats = db.AppStats.ToList().Last();
            ViewBag.Stats.counter += 1;
            return View(indawoes);
        }

        // GET: Indawoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Indawo indawo = db.Indawoes.Find(id);
            indawo.oparatingHours = db.OperatingHours.Where(x => x.indawoId == id).ToArray();
            indawo.images = db.Images.Where(x => x.indawoId == id).ToList();
            indawo.specialInstructions = db.SpecialInstructions.Where(x => x.indawoId == id).ToList();
            indawo.events = db.Events.ToList().Where(x => x.indawoId == indawo.id).ToList();
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }
        [Authorize]
        public ActionResult CreateImg(int? indawoId)
        {
            Image img = new Image();
            img.indawoId = (int)indawoId;
            return View(img);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImg([Bind(Include = "id,indawoId,imgPath,eventName")] Image img, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                img.indawoId = (int)indawoId;
                db.Images.Add(img);
                db.SaveChanges();
                return RedirectToAction("Index","Indawoes", new { area = "" });
            }

            return View();
        }

        [Authorize]
        public ActionResult CreateOp(int? indawoId)
        {
            OperatingHours op = new OperatingHours();
            op.indawoId = (int)indawoId;
            return View(op);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOp([Bind(Include = "id,indawoId,day,occation,openingHour,closingHour")] OperatingHours op, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                op.indawoId = (int)indawoId;
                db.OperatingHours.Add(op);
                db.SaveChanges();
                return RedirectToAction("Index", "Indawoes", new { area = "" });
            }

            return View();
        }

        [Authorize]
        public ActionResult CreateSp(int? indawoId)
        {
            SpecialInstruction sp = new SpecialInstruction();
            sp.indawoId = (int)indawoId;
            return View(sp);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSp([Bind(Include = "id,indawoId,instruction")] SpecialInstruction sp, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                sp.indawoId = (int)indawoId;
                db.SpecialInstructions.Add(sp);
                db.SaveChanges();
                return RedirectToAction("Index", "Indawoes", new { area = "" });
            }

            return View();
        }


        [Authorize]
        public ActionResult CreateEvent(int? indawoId)
        {
            Event @event = new Event();
            @event.indawoId = (int)indawoId;
            return View(@event);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent([Bind(Include = "id,indawoId,lat,lot,title,description,address,price,date,stratTime,endTime,imgPath")] Event @event, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                @event.indawoId = (int)indawoId;
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Index", "Indawoes", new { area = "" });
            }

            return View();
        }

        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
