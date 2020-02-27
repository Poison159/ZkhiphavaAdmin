using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using ZkhiphavaWeb;
using ZkhiphavaWeb.Models;
using Microsoft.Owin.Host.SystemWeb;

namespace ZkhiphavaWeb.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    
    public class IndawoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Indawoes
        
        public List<Indawo> GetIndawoes(string userLocation, string distance, string vibe, string filter)
        {
            Helper.IncrementAppStats(db,vibe);
            
            if (userLocation.Split(',')[0] == "undefined") {
                return null;
            }
            if (string.IsNullOrEmpty(vibe)){
                return null;
            }
            var lon = userLocation.Split(',')[0];
            var lat = userLocation.Split(',')[1];
            var vibes = new List<string>() {"Chilled","Club","Outdoor", "Pub/Bar" };
            var filters = new List<string>() { "distance", "rating", "damage" };
            var rnd = new Random();
            var locations = db.Indawoes.ToList().Where(x => x.id != 9 
                && x.type.ToLower().Trim() == vibe.ToLower().Trim()).OrderBy(x => rnd.Next()).ToList(); 
            
            var listOfIndawoes = Helper.GetNearByLocations(lat, lon, Convert.ToInt32(distance), locations); // TODO: Use distance to narrow search
            //var listOfIndawoes = LoadJson(@"C:\Users\Siya\Desktop\Indawo.json");
            
            foreach (var item in listOfIndawoes) {
                var OpHours = db.OperatingHours.Where(x => x.indawoId == item.id).ToArray();
                item.images = db.Images.Where(x => x.indawoId == item.id).ToList();
                item.events = db.Events.Where(x => x.indawoId == item.id).ToList();
                item.specialInstructions = db.SpecialInstructions.Where(x => x.indawoId == item.id).ToList();
                item.oparatingHours = SortHours(OpHours);
                Helper.makeAllOpHoursToday(item);
                item.open = Helper.assignSatus(item);
                item.closingSoon = Helper.isClosingSoon(item);
                item.openingSoon = Helper.isOpeningSoon(item);
                item.info = Helper.getLocationInfo(item);
                item.openOrClosedInfo = Helper.getClosedStatus(item);
                Helper.getOpratingHoursStr(item);
            }

            if (!string.IsNullOrEmpty(filter) && filter != "None" && filters.Contains(filter)) {
                if (filter == "distance")
                    listOfIndawoes = listOfIndawoes.OrderBy(x => x.distance).ToList();
                else if (filter == "rating")
                    listOfIndawoes = listOfIndawoes.OrderByDescending(x => x.rating).ToList();
                else if (filter == "damage")
                    listOfIndawoes = listOfIndawoes.OrderBy(x => x.entranceFee).ToList();
            }
            return listOfIndawoes;
        }

        [Route("api/IncIndawoStats")]
        [HttpGet]
        public void IncDirStats(int indawoId, string plat)
        {
            if (db.IndawoStats.Count() == 0) {
                var tempStat = new IndawoStat() { indawoId = indawoId };
                if (plat == "maps")
                    tempStat.dirCounter = 1;
                if (plat == "insta")
                    tempStat.instaCounter = 1;
                db.IndawoStats.Add(tempStat);
            }
            else
            {
                var indawoStats = db.IndawoStats.Where(x => x.indawoId == indawoId).ToList();
                if (indawoStats.Count() != 0 && indawoStats.Last().dayOfWeek == DateTime.Now.DayOfWeek)// if it's same day the find the last one to be made & increament
                {
                    if (plat == "maps")
                        indawoStats.Last().dirCounter++;
                    if (plat == "insta")
                        indawoStats.Last().instaCounter++;
                }
                else { // else create new
                    var tempStat = new IndawoStat() { indawoId = indawoId };
                    if (plat == "maps")
                        tempStat.dirCounter = 1;
                    if (plat == "insta")
                        tempStat.instaCounter = 1;
                    db.IndawoStats.Add(tempStat);
                }
            }
            db.SaveChanges();
        }

        [Route("api/RegisterUser")]
        [HttpGet]
        public User Register(string name, string email, string mobileNumber, string password) {
            var user = new User { name = name, email = email, mobileNumber = mobileNumber, password = Helper.GetHashString(password)};
            if (db.AppUsers.ToList().First(x => x.password == Helper.GetHashString(password)) == null)
            {
                db.AppUsers.Add(user);
                db.SaveChanges();
            }
            else {
                return db.AppUsers.ToList().First(x => x.password == Helper.GetHashString(password));
            }
            return db.AppUsers.ToList().Last();
        }
       

        public string getNextDay(string curDay)
        {
            if (curDay == "Sunday")
                return "Monday";
            var days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            return days[days.IndexOf(curDay) + 1];
        }

        public OperatingHours[] SortHours(OperatingHours[] opHors) {
            var days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            List<OperatingHours> final  = new List<OperatingHours>();
            List<OperatingHours> tempList = new List<OperatingHours>();
            string dayToday             = DateTime.Now.DayOfWeek.ToString(); // today's operating hours
            OperatingHours todayOp      = opHors.FirstOrDefault(x => x.day == dayToday);

            if (todayOp == null)
                return opHors;
            else {
                final.Add(todayOp);
                var nextDay = getNextDay(dayToday);
                foreach (var item in opHors.Where(x => x.day != dayToday))
                {
                    if (item.day != nextDay) {
                        tempList.Add(item);
                        continue;
                    }
                    nextDay = getNextDay(item.day);
                    final.Add(item);
                }
                foreach (var item in tempList)
                {
                    final.Add(item);
                }
            }
            return final.ToArray();
        }

        [Route("api/Favorites")]
        [HttpGet]
        
        public List<Indawo> getFavorites(string idString)
        {
            var fav = new List<Indawo>();
            foreach (var item in idString.Split(',').Where(x => x != ""))
            {
                fav.Add(db.Indawoes.Find(Convert.ToInt32(item)));
            }
            return fav;
        }
        [Route("api/addFavorite")]
        [HttpGet]
        public void addFavorite(int userId, int indawoId) {
            
            var user = db.AppUsers.Find(userId);
            if(!user.LikesLocations.Split(',').Contains(indawoId.ToString()))
                user.LikesLocations += indawoId.ToString() + ",";
            db.SaveChanges();
        }

        [Route("api/addInterested")]
        [HttpGet]
        public void addInterested(int userId, int eventId)
        {
            var user = db.AppUsers.Find(userId);
            user.interestedEvents += eventId.ToString() + ",";
            db.SaveChanges();
        }

        [Route("api/getUserData")]
        [HttpGet]

        public object getUserData(int userId)
        {
            var user = db.AppUsers.First(x => x.id == userId);
            return Helper.LiekdFromString(user.LikesLocations, user.interestedEvents , db.Indawoes.ToList(),db.Events.ToList());
            
        }


        [Route("api/Event")]
        [HttpGet]
        
        public Event Event(int id,string lat, string lon)
        {
            int outPut;
            try
            {
                var evnt = db.Events.Find(id);
                if (int.TryParse(lat[1].ToString(), out outPut) && int.TryParse(lon[0].ToString(), out outPut)) {
                    var locationLat = Convert.ToDouble(evnt.lat, CultureInfo.InvariantCulture);
                    var locationLon = Convert.ToDouble(evnt.lon, CultureInfo.InvariantCulture);
                    var userLocationLat = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
                    var userLocationLong = Convert.ToDouble(lon, CultureInfo.InvariantCulture);
                    evnt.distance = Math.Round(Helper.distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K'));
                }
                var eventArtistIds = db.ArtistEvents.ToList().Where(x => x.eventId == evnt.id);
                evnt.artists = Helper.getArtists(eventArtistIds, db);
                evnt.images = db.Images.Where(x => x.eventName.ToLower().Trim() == evnt.title.ToLower().Trim()).ToList();
                evnt.date = Helper.treatDate(evnt.date);
                return evnt;
            }
            catch {
                return null;
            }   
        }

        [Route("api/Events")]
        [HttpGet]
        public List<Event> Events(string lat, string lon)
        {
            int outPut;
            var rnd = new Random();
            try { 
                var events = db.Events.Where(x =>x.indawoId == 9).ToList();
                foreach (var evnt in events)
                {
                    if (int.TryParse(lat[1].ToString(), out outPut) && int.TryParse(lon[0].ToString(), out outPut))
                    {
                        var userLocationLat = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
                        var userLocationLong = Convert.ToDouble(lon, CultureInfo.InvariantCulture);
                        var locationLat = Convert.ToDouble(evnt.lat, CultureInfo.InvariantCulture);
                        var locationLon = Convert.ToDouble(evnt.lon, CultureInfo.InvariantCulture);
                        evnt.distance = Math.Round(Helper.distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K'));
                    }
                }
                var randEvents = events.OrderBy(x => rnd.Next()).ToList();
                Helper.convertDates(randEvents);
                return randEvents;
            }
            catch {
                return null;
            }
        }

        // GET: api/Indawoes/5
        [ResponseType(typeof(Indawo))]
        
        public IHttpActionResult GetIndawo(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            //Indawo indawo = LoadJson(@"C:\Users\sibongisenib\Documents\ImportantRecentProjects\listOfIndawoes.json").First(x => x.id == id);
            var OpHours = db.OperatingHours.Where(x => x.indawoId == indawo.id).ToArray();
            indawo.oparatingHours = SortHours(OpHours);
            indawo.open = Helper.assignSatus(indawo);
            indawo.closingSoon = Helper.isClosingSoon(indawo);
            indawo.openingSoon = Helper.isOpeningSoon(indawo);
            indawo.info = Helper.getLocationInfo(indawo);
            indawo.openOrClosedInfo = Helper.getClosedStatus(indawo);
            Helper.getOpratingHoursStr(indawo);
            if (indawo == null)
            {
                return NotFound();
            }
            return Ok(indawo);
        }

        // PUT: api/Indawoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutIndawo(int id, Indawo indawo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != indawo.id)
            {
                return BadRequest();
            }

            db.Entry(indawo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IndawoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        private List<Indawo> getPlacesWithInDistance(string userLocation, List<Indawo> listOfIndawo, string distance)
        {
            throw new NotImplementedException();
        }
        private List<Indawo> getIndawoWithIn50k(string userLocation)
        {
            //Using only userLocation return a list of places with in 50K of location
            throw new NotImplementedException();
        }

        // DELETE: api/Indawoes/5
        [ResponseType(typeof(Indawo))]
        public IHttpActionResult DeleteIndawo(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return NotFound();
            }

            db.Indawoes.Remove(indawo);
            db.SaveChanges();

            return Ok(indawo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IndawoExists(int id)
        {
            return db.Indawoes.Count(e => e.id == id) > 0;
        }
    }
}