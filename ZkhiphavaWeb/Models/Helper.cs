using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using Newtonsoft.Json;

namespace ZkhiphavaWeb.Models
{
    public static class Helper
    {
        public static List<Indawo> GetNearByLocations(string Currentlat, string Currentlng,int distance,List<Indawo> indawo)
        {
            var currentLocation = DbGeography.FromText("POINT( " + Currentlng + " " + Currentlat + " )");
            try
            {
                var userLocationLat = Convert.ToDouble(Currentlat, CultureInfo.InvariantCulture);
                var userLocationLong = Convert.ToDouble(Currentlng, CultureInfo.InvariantCulture);

                foreach (var item in indawo)
                {
                    var locationLat = Convert.ToDouble(item.lat, CultureInfo.InvariantCulture);
                    var locationLon = Convert.ToDouble(item.lon, CultureInfo.InvariantCulture);
                    var ndawoLocation = DbGeography.FromText("POINT( " + item.lon + " " + item.lat + " )");
                    var distanceToIndawo = distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K');
                    item.distance = Math.Round(distanceToIndawo);
                }
                List<Indawo> nearLocations = getPlacesWithIn(indawo, distance);
                return nearLocations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); 
            }
        }

        internal static List<OperatingHours> checkOPeratingHours(List<OperatingHours> opHours, List<Indawo> izindawo,ApplicationDbContext db)
        {
            var retOpHours = new List<OperatingHours>();
            var ids = getIndawoIds(izindawo);
            foreach (var opHour in opHours)
            {
                if (!ids.Contains(opHour.indawoId)){
                    db.OperatingHours.Remove(opHour);
                }
            }
            db.SaveChanges();
            return db.OperatingHours.ToList();
        }

        private static List<int> getIndawoIds(List<Indawo> izindawo){
            var ret = new List<int>();
            foreach (var item in izindawo){ret.Add(item.id);}
            return ret;
        }

        public static List<Image> LoadJson(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<Image> items = JsonConvert.DeserializeObject<List<Image>>(json);
                return items;
            }
        }
        public static string getLocationInfo(Indawo location) {

            if (location.distance > 1 && location.entranceFee > 0)
            {
                return location.distance + "KM | " + "R" + location.entranceFee;
            }
            else if (location.distance <= 1 && location.entranceFee == 0)
            {
                return "LESS THAN A KM AWAY | " + "FREE ";
            }
            else if (location.distance <= 1 && location.entranceFee != 0)
            {
                return "LESS THAN A KM AWAY | " + "R" + location.entranceFee;
            }
            else if (location.distance > 1 && location.entranceFee == 0)
            {
                return location.distance + "KM | " + "FREE";
            }
            else
            {
                return "";
            }
        }
        
        public static string getClosedStatus(Indawo location) {
            if (location.open == false && location.closingSoon == false && location.openingSoon == false) {
                return "CLOSED";
            } else if (location.open == true && location.closingSoon == true) {
                return "CLOSING SOON";
            }else if (location.open == false && location.openingSoon == true){
                return "OPENING SOON";
            }else if (location.open == true && location.closingSoon == false){
                return "OPEN";
            }
            return "";
        }

        internal static void getOpratingHoursStr(Indawo item)
        {
            var str = "";
            List<string> retStr = new List<string>();
            int i = 0;
            var operatingHours = item.oparatingHours;
            foreach (var opHour in operatingHours) {
                i++;
                str += opHour.day + " | " + opHour.openingHour.ToString().Split(' ')[1].Substring(0, 5) + " to "
                    + opHour.closingHour.ToString().Split(' ')[1].Substring(0, 5) + " " + opHour.occation ;
                item.operatingHoursStr.Add(str);
                str = "";
            }
        }

        internal static void IncrementAppStats(ApplicationDbContext db)
        {
            if (db.AppStats.Count() != 0) {
                if (db.AppStats.ToList().Last().dayOfWeek != DateTime.Now.DayOfWeek)
                {
                    db.AppStats.Add(new AppStat());
                    db.AppStats.ToList().Last().counter++;
                }
                else
                    db.AppStats.ToList().Last().counter++;
            }else
                db.AppStats.Add(new AppStat() { counter = 1 });
            db.SaveChanges();
        }

        internal static Dictionary<int,string> getIndawoNames(List<Indawo> list)
        {
            var strList = new Dictionary<int,string>();
            foreach (var item in list)
            {
                strList.Add(item.id,item.name);
            }
            return strList;
        }

        internal static Dictionary<int, string> getEventNames(List<Event> @events)
        {
            var strList = new Dictionary<int, string>();
            foreach (var item in events)
            {
                strList.Add(item.id, item.title);
            }
            return strList;
        }
        internal static Dictionary<int, string> getArtistNames(List<Artist> artists)
        {
            var strList = new Dictionary<int, string>();
            foreach (var item in artists)
            {
                strList.Add(item.id, item.name);
            }
            return strList;
        }



        public static void Shuffle<T>(this List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static List<Indawo> getPlacesWithIn(List<Indawo> indawo,int distance)
        {
            var finalList = new List<Indawo>();
            foreach (var item in indawo)
                if (item.distance <= distance)
                    finalList.Add(item);
            return finalList;
        }

        public static bool IsEmail(string email) {
            if (!email.Contains('@')) {
                return false;
            }
            return true;
        }

        private static double Distance(double lon1,double lat1,double lon2,double lat2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = (lat2 - lat1).ToRadians();  // Javascript functions in radians
            var dLon = (lon2 - lon1).ToRadians();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1.ToRadians()) * Math.Cos(lat2.ToRadians()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        public static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static double distanceToo(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }

        public static double ToRadians(this double angle) {
            return (angle * Math.PI) / 180;
        }

        public static int CreateUser(string username)
        {
            var userStore = new UserStore<IdentityUser>();
            var manager = new UserManager<IdentityUser>(userStore);
            if (manager.Users.Where(x =>x.UserName.ToLower() 
            == username.ToLower()).Count() != 0) {
                return -1; // username taken
            }
                var user = new IdentityUser() { UserName = username };
            IdentityResult result = manager.Create(user);

            if (result.Succeeded)
            {
                return 1;// success
            }
            else
            {
                return 0; // failed 
            }

        }

        public static bool assignSatus(Indawo indawo) {
            var dateToday   = DateTime.Now;
            var nextDay   = DateTime.Now.AddDays(1);
            var dayToday    = dateToday.DayOfWeek;
            foreach (var item in indawo.oparatingHours)
            {
                item.closingHour = new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, item.closingHour.Hour,
                    item.closingHour.Minute, item.closingHour.Second);
                if (item.closingHour.TimeOfDay.ToString().First() == '0') {
                    item.closingHour = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, item.closingHour.Hour,
                    item.closingHour.Minute, item.closingHour.Second);
                }
                item.openingHour = new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, item.openingHour.Hour,
                        item.openingHour.Minute, item.openingHour.Second);

            }
            var opHours     = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower() 
                                == dayToday.ToString().ToLower());
            if (opHours == null)
                return false;
            else
                return openOrClosed(opHours,indawo);
        }

        internal static void convertDates(List<Event> list)
        {
            foreach (var item in list){
                item.date = treatDate(item.date);
            }
        }

        public static string treatDate(string date)
        {
            var retStr = "";
            var strArr = date.Split(' ');
            for (int i = 0; i < strArr.Length - 1; i++){
                retStr += strArr[i] + " ";
            }
            return retStr.Trim();
        }

        public static bool openOrClosed(OperatingHours opHours,Indawo indawo) {
            if (opHours.openingHour <= DateTime.Now
                && opHours.closingHour >= DateTime.Now && CheckDayBefore(opHours,indawo))
                return true;
            else
                return false;
        }

        internal static List<Artist> getArtists(IEnumerable<ArtistEvent> eventArtistIds,ApplicationDbContext db)
        {
            List<Artist> artists = new List<Artist>();
            foreach (var item in eventArtistIds){
                artists.Add(db.Artists.First(x => x.id == item.artistId));
            }
            return artists;
        }

        private static bool CheckDayBefore(OperatingHours opHours, Indawo indawo)
        {
            var daybefore = getDayBefore(opHours);
            OperatingHours dayBeforeOphour = getDayBeforeOpHour(daybefore,indawo);
            if (dayBeforeOphour != null)
            {
                var dayBeforeClosingHour = dayBeforeOphour.closingHour.ToString();
                if (dayBeforeClosingHour.Split(' ')[1].Substring(0, 5).StartsWith("0"))
                {
                    if (dayBeforeOphour.closingHour.Hour > DateTime.Now.Hour)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static OperatingHours getDayBeforeOpHour(string daybefore, Indawo indawo)
        {
            foreach (var item in indawo.oparatingHours)
            {
                if (item.day.ToLower().Trim() == daybefore) {
                    return item;
                }
            }
            return null;
        }

        private static string  getDayBefore(OperatingHours opHours)
        {
            var dayOfWeek = "monday,tuesday,wednesday,thursday,friday,saturday,sunday".Split(',');
            var prevDay = "";
            for (int i = 0; i < dayOfWeek.Count(); i++)
            {
                if (opHours.day.ToLower().Trim() == dayOfWeek[i]) {
                    if (i == 0)
                    {
                        prevDay = dayOfWeek[6];
                    }
                    else {
                        prevDay = dayOfWeek[i - 1];
                    }
                }
            }
            return prevDay;
        }

        internal static TimeSpan calcTimeLeft(DateTime endDate){
            return   endDate - DateTime.Now;
        }

        public static bool isClosingSoon(Indawo indawo) {
            var now = DateTime.Now;
            var dayToday = now.DayOfWeek;
            var opHours = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower()
                            == dayToday.ToString().ToLower());
            
            if (opHours != null)
            {
                var closingHours = opHours.closingHour.TimeOfDay;
                var timeNow = DateTime.Now.TimeOfDay;
                var timeLeft = opHours.closingHour.TimeOfDay.Subtract(timeNow);
                if (closingHours.ToString().First() == '0')
                    timeLeft = getTimeLeft(closingHours, now, timeLeft);
                var anHour = new TimeSpan(1, 0, 0);

                if (timeLeft <= anHour && timeLeft > new TimeSpan(0,0,0))
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static bool isOpeningSoon(Indawo indawo)
        {
            var now = DateTime.Now;
            var dayToday = now.DayOfWeek;
            var opHours = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower()
                            == dayToday.ToString().ToLower());

            if (opHours != null)
            {
                var closingHours = opHours.openingHour.TimeOfDay;
                var timeNow = DateTime.Now.TimeOfDay;
                var timeLeft = opHours.openingHour.TimeOfDay.Subtract(timeNow);
                var anHour = new TimeSpan(1, 0, 0);
                if (timeLeft <= anHour && timeLeft > new TimeSpan(0, 0, 0))
                    return true;
                else
                    return false;
            }
            return false;
        }



        public static TimeSpan getTimeLeft(TimeSpan closingHours, DateTime now, TimeSpan timeLeft) {
            var numAfterZero = closingHours.ToString().ElementAt(1);
            var hoursAftertwelve = Convert.ToInt32(numAfterZero.ToString());
            var timeTilTwelve = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59)
                .TimeOfDay.Subtract(DateTime.Now.TimeOfDay).Duration();
            timeLeft = timeTilTwelve + new TimeSpan(hoursAftertwelve, 0, 0); // get minuts and hours from closing hour
            return timeLeft;
        }
    }
}