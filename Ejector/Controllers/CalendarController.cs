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

            List<string> cookieList;
            try
            {
                cookieList = await _zjuService.LoginAsync(username, password);
            }
            catch (Exception e)
            {
                return StatusCode(503);
            }
            var cookies = string.Join(' ', cookieList);

            await _zjuService.UpdateConfigAsync();
            var termConfigs = await _zjuService.GetTermConfigsAsync();
            var tweaks = await _zjuService.GetTweaksAsync();
            
            var vCal = new VCalendar();

            foreach (var (year, term) in await _zjuService.GetClassTermsAsync())
            {
                var classOutline = await _zjuService.GetClassTimeTableAsync(cookies, year, term, username);
                vCal.VEvents.AddRange(ZjuClassCalendarParser.ClassToVEvents(classOutline, termConfigs.First(x => x.Term == term && x.Year == year), tweaks));
            }

            var cal = vCal.ToString();
            stream = new MemoryStream(Encoding.UTF8.GetBytes(cal));
            return File(stream, "text/calendar");
        }
        
        [HttpGet("getExam")]
        public async Task<IActionResult> GetExamCalendarFile(string secret)
        {
            var kSecret = _config["SECRET"];
            if (secret != kSecret)
            {
                return NotFound();
            }
            
            var username = _config["USERNAME"];
            var password = _config["PASSWORD"];
            MemoryStream stream;
            
            List<string> cookieList;
            try
            {
                cookieList = await _zjuService.LoginAsync(username, password);
            }
            catch (Exception e)
            {
                return StatusCode(503);
            }
            var cookies = string.Join(' ', cookieList);

            await _zjuService.UpdateConfigAsync();
            
            var vCal = new VCalendar();

            foreach (var (year, term) in await _zjuService.GetExamTermsAsync())
            {
                var examOutline = await _zjuService.GetExamInfoAsync(cookies, year, term, username);
                foreach (var zjuExamOutline in examOutline)
                {
                    var list = zjuExamOutline.ToVEventList();
                    vCal.VEvents.AddRange(list);
                }
            }

            var cal = vCal.ToString("Ejector 考试安排");
            stream = new MemoryStream(Encoding.UTF8.GetBytes(cal));
            return File(stream, "text/calendar");
        }
    }
}