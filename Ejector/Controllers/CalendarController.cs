using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ejector.Services;
using Ejector.Utils.Calender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ejector.Controllers
{
    [ApiController]
    [Route("ical")]
    public class CalendarController : ControllerBase
    {

        private readonly IZjuService _zjuService;
        // TODO: Substitute IRedisService with specific services
        private readonly IConfiguration _config;
        
        public CalendarController(IZjuService zjuService, IConfiguration config)
        {
            _zjuService = zjuService;
            _config = config;
        }

        [HttpGet("getClass")]
        public async Task<IActionResult> GetClassCalendarFile(string secret)
        {
            var kSecret = _config["SECRET"];
            if (secret != kSecret)
            {
                return NotFound();
            }
            
            var username = _config["USERNAME"];
            var password = _config["PASSWORD"];
            MemoryStream stream;
            
            var cookieList = await _zjuService.LoginAsync(username, password);
            var cookies = string.Join(' ', cookieList);

            await _zjuService.UpdateConfigAsync();
            var termConfigs = await _zjuService.GetTermConfigsAsync();
            var tweaks = await _zjuService.GetTweaksAsync();
            
            var vCal = new VCalendar();

            foreach (var (year, term) in await _zjuService.GetClassTermsAsync())
            {
                var classOutline = await _zjuService.GetClassTimeTableAsync(cookies, year, term, username);
                vCal.VEvents.AddRange(ZjuClassCalendarParser.ClassToVEvents(classOutline, termConfigs.First(x => x.Term == term), tweaks));
            }

            var cal = vCal.ToString();
            stream = new MemoryStream(Encoding.UTF8.GetBytes(cal));
            return File(stream, "text/calendar");
        }
    }
}