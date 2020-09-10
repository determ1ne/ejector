using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Ejector.Services;
using Ejector.Utils.Calender;
using EjectorTest.Mock;

namespace EjectorTest
{
    public class ZjuServiceTests
    {
        private string _username;
        private string _password;
        private SemaphoreSlim _semaphore;
        private ZjuService _zjuService;
        
        [SetUp]
        public void Setup()
        {
            _username = EnvVar.GetEnv("Username");
            _password = EnvVar.GetEnv("Password");
            _semaphore = new SemaphoreSlim(1);
        }

        private async Task init()
        {
            await _semaphore.WaitAsync();
            
            // Init zjuservice
            if (_zjuService == null)
            {
                var mockFactory = new MockHttpClientFactory();
                var mockSqlService = new MockSqlService($"{TestContext.CurrentContext.WorkDirectory}\\test.db");
                _zjuService = new ZjuService(mockFactory, mockSqlService);
            }
            
            // Init cookie
            if (EnvVar.GetInternalEnv("__ZjuCookies") == null)
            {
                if (EnvVar.GetEnv("UseSpecificWisportalId") == "true")
                {
                    EnvVar.SetEnv("__ZjuCookies", $"wisportalId={EnvVar.GetEnv("WisportalId")};");
                }
                else
                {
                    var cookies = await _zjuService.LoginAsync(_username, _password);
                    EnvVar.SetEnv("__ZjuCookies", string.Join(' ', cookies));
                }
            }
            
            // Init stuid
            if (EnvVar.GetEnv("OverrideStuId") == "false")
                EnvVar.SetEnv("StuId", EnvVar.GetEnv("UserName"));
            
            // Init test start time
            EnvVar.SetEnv("__testStarted", DateTime.Now.ToString("yyyyMMddHHmm"));
            
            _semaphore.Release();
        }

        private void WriteToFile(string fileName, string content)
        {
            File.WriteAllText($"{TestContext.CurrentContext.WorkDirectory}\\{EnvVar.GetInternalEnv("__testStarted")}{fileName}", content);
        }
        
        [Test]
        public async Task TestZjuAppServiceCookie()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            if (cookies.IndexOf("wisportalId") is int wi)
                Assert.Pass($"{cookies.Substring(wi, 16)}****");
            else
                Assert.Fail();
        }

        [Test]
        public async Task TestUserInfo()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuId = await _zjuService.GetStuIdAsync(cookies);
            if (stuId != null)
                Assert.Pass(stuId);
            else
                Assert.Fail();
        }

        [Test]
        public async Task TestExamOutline()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuid = EnvVar.GetEnv("StuId");
            var examOutline = await _zjuService.GetExamInfoAsync(cookies, "2020-2021", ExamTerm.AutumnWinter, stuid);
            if (examOutline != null)
            {
                var json = JsonSerializer.Serialize(examOutline);
                WriteToFile("ExamOutline.json", json);
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public async Task TestExamCal()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuid = EnvVar.GetEnv("StuId");
            var examOutline = await _zjuService.GetExamInfoAsync(cookies, "2020-2021", ExamTerm.AutumnWinter, stuid); 
            var vCal = new VCalendar();
            foreach (var zjuExamOutline in examOutline)
            {
                var list = zjuExamOutline.ToVEventList();
                vCal.VEvents.AddRange(list);
            }
            WriteToFile("exam.ics", vCal.ToString("Ejector 考试安排"));
            Assert.Pass();
        }

        [Test]
        public async Task TestClassOutline()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuid = EnvVar.GetEnv("StuId");
            var classOutline = await _zjuService.GetClassTimeTableAsync(cookies, "2020-2021", ClassTerm.Autumn, stuid);
            if (classOutline != null)
            {
                var json = JsonSerializer.Serialize(classOutline);
                WriteToFile("ClassOutline.json", json);
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
        
        [Test]
        public async Task TestClassCal()
        {
            await init();
            TestContext.WriteLine("Init Done");
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuid = EnvVar.GetEnv("StuId");
            var vCal = new VCalendar();
            
            var termConfigs = await _zjuService.GetTermConfigsAsync();
            var tweaks = await _zjuService.GetTweaksAsync();
            TestContext.WriteLine("Getting Autumn Class Table");
            var classOutlineAutumn = await _zjuService.GetClassTimeTableAsync(cookies, "2020-2021", ClassTerm.Autumn, stuid);
            TestContext.WriteLine("Done");
            Thread.Sleep(1000);
            TestContext.WriteLine("Getting Winter Class Table");
            var classOutlineWinter = await _zjuService.GetClassTimeTableAsync(cookies, "2020-2021", ClassTerm.Winter, stuid);
            TestContext.WriteLine("Done");

            vCal.VEvents.AddRange(
                ZjuClassCalendarParser.ClassToVEvents(classOutlineAutumn, termConfigs.First(x => x.Term == ClassTerm.Autumn), tweaks));
            vCal.VEvents.AddRange(
                ZjuClassCalendarParser.ClassToVEvents(classOutlineWinter, termConfigs.First(x => x.Term == ClassTerm.Winter), tweaks));
            
            WriteToFile("class.ics", vCal.ToString());
            Assert.Pass();
        }
    }
}