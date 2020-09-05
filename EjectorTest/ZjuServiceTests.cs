using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Ejector.Services;
using Microsoft.VisualBasic;

namespace EjectorTest
{
    public class ZjuServiceTests
    {
        private string _username;
        private string _password;
        private Mutex _mutex;
        private ZjuService _zjuService;
        
        [SetUp]
        public void Setup()
        {
            _username = EnvVar.GetEnv("Username");
            _password = EnvVar.GetEnv("Password");
            _mutex = new Mutex();
        }

        private async Task<string> init()
        {
            _mutex.WaitOne();
            
            // Init zjuservice
            if (_zjuService == null)
            {
                var mockFactory = new MockHttpClientFactory();
                _zjuService = new ZjuService(mockFactory);
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
                    var cookies = await _zjuService.Login(_username, _password);
                    EnvVar.SetEnv("__ZjuCookies", string.Join(' ', cookies));
                }
            }
            
            // Init stuid
            if (EnvVar.GetEnv("OverrideStuId") == "false")
                EnvVar.SetEnv("StuId", EnvVar.GetEnv("UserName"));
            
            _mutex.ReleaseMutex();
            return EnvVar.GetInternalEnv("__ZjuCookies");
        }

        [Test]
        public async Task TestZjuAppServiceCookie()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            if (cookies.IndexOf("wisportalId") is int wi)
                Assert.Pass($"{cookies.Substring(wi, 16)}****");
            Assert.Fail();
        }

        [Test]
        public async Task TestUserInfo()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuId = await _zjuService.GetStuId(cookies);
            if (stuId != null)
                Assert.Pass(stuId);
            Assert.Fail();
        }

        [Test]
        public async Task TestExamOutline()
        {
            await init();
            var cookies = EnvVar.GetInternalEnv("__ZjuCookies");
            var stuid = EnvVar.GetEnv("StuId");
            var examOutline = await _zjuService.GetExamInfo(cookies, "2020-2021", ExamTerm.AutumnWinter, stuid);
            if (examOutline != null)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}