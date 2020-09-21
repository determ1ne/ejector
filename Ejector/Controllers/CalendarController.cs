using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ejector.Services;
using Ejector.Utils.Calender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ejector.Controllers
{
    [ApiController]
    [Route("ical")]
    public class CalendarController : ControllerBase
    {

        private readonly IZjuService _zjuService;
        // TODO: Substitute IRedisService with specific services
        private readonly IRedisService _redisService;
        
        public CalendarController(IZjuService zjuService, IRedisService redisService)
        {
            _zjuService = zjuService;
            _redisService = redisService;
        }

        [HttpGet("getClass")]
        public async Task<IActionResult> GetClassCalendarFile(string username, string password)
        {
            var redisDb = _redisService.GetRedisDb();
            MemoryStream stream;
            
            // TODO: 
            // 建用户的时候就应该绑定学号
            // 直接保存在数据库里
            // 这个 endpoint 的 username 是多余的
            // 先充个数
            var stuId = username;
            
            // No parallel request
            // Note: You may also add freq limit in nginx
            if (await redisDb.StringIncrementAsync($"{stuId}_fetching_state") != 1)
                return BadRequest();
            var calCache = await redisDb.StringGetAsync($"{stuId}_class_cal");
            if (calCache != RedisValue.Null)
            {
                stream = new MemoryStream(Encoding.UTF8.GetBytes(calCache));
                return File(stream, "text/calendar");
            }
            

            // TODO: use token instead of username and password
            // 可能还要做一个 service
            var cookieList = await _zjuService.LoginAsync(username, password);
            var cookies = string.Join(' ', cookieList);
            
            // TODO: termConfig 和 tweaks 不常发生变动
            // 考虑一下常驻内存 & SQL 中只获取当前学期的信息
            var termConfigs = await _zjuService.GetTermConfigsAsync();
            var tweaks = await _zjuService.GetTweaksAsync();
            
            var vCal = new VCalendar();

            // TODO: 这个应该放进 SQL 的系统设置里
            // 所有不涉及初始化的都放进 SQL 里
            // 还可以做缓存（另一个 service）
            // Exception 处理要做好
            var currentAcademicYear = "2020-2021";
            var currentTermList = new List<ClassTerm>();
            foreach (var classTerm in currentTermList)
            {
                var classes = await _zjuService.GetClassTimeTableAsync(cookies, currentAcademicYear, classTerm, stuId);
                vCal.VEvents.AddRange(ZjuClassCalendarParser.ClassToVEvents(classes, termConfigs.First(x => x.Term == classTerm), tweaks));
            }

            var cal = vCal.ToString();
            await redisDb.StringSetAsync($"{stuId}_class_cal", cal);
            await redisDb.KeyDeleteAsync($"{stuId}_fetching_state");
            stream = new MemoryStream(Encoding.UTF8.GetBytes(cal));
            return File(stream, "text/calendar");
        }
    }
}