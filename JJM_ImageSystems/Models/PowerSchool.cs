using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JJM_ImageSystems.Models
{
    public class PowerSchool
    {
        public int PowerSchoolId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
    }
}
