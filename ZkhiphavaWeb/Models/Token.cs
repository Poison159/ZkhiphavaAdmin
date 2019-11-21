using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class Token
    {
        public Token(string userId, string access_token,int expiresIn, DateTime expiryDate)
        {
            _access_token = access_token;
            _userId = userId;
            _expiresIn = expiresIn;
            _expiryDate = expiryDate;
        }
        public int id { get; set; }
        public string _userId { get; set; }
        public string _access_token { get; set; }
        public int _expiresIn { get; set; }
        public DateTime _expiryDate { get; set; }

    }
}