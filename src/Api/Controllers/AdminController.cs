using System;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class AdminController
    {
        public TimeSpan StaticProp => TimeSpan.FromHours(12);
        public TimeSpan Uptime => TimeSpan.FromHours(12);

        [HttpGet]
        public static string Static()
        {
            return "static works";
        }

        [HttpGet]
        public static string Instance()
        {
            return "instance works";
        }
    }
}