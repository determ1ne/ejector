using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Ejector.Utils.Calender;
using Microsoft.Extensions.Configuration;

namespace Ejector.Services
{
    public partial class ZjuService : IZjuService, IDisposable
    {
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private const string kPublicKeyUrl = "https://zjuam.zju.edu.cn/cas/v2/getPubKey";
        private const string kAppServiceLoginUrl = "https://zjuam.zju.edu.cn/cas/login?service=http%3A%2F%2Fappservice.zju.edu.cn%2Findex";
        private const string kAppServiceGetUserInfo = "http://appservice.zju.edu.cn/zju-smartcampus/zdydjy/stuInfo/getUserInfo";
        private const string kAppServiceGetExamOutlineInfo = "http://appservice.zju.edu.cn/zju-smartcampus/zdydjw/api/kkqk_cxXsksxx";
        private const string kAppServiceGetWeekClassInfo = "http://appservice.zju.edu.cn/zju-smartcampus/zdydjw/api/kbdy_cxXsZKbxx";
        private ZjuScheduleConfig _schedule;

        public ZjuService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("ZjuClient");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36 Edg/85.0.564.41");
            _config = config;
        }

        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
        }
        
        private BigInteger parsePositiveHexBigInteger(string num) => BigInteger.Parse(string.Concat("0", num), NumberStyles.HexNumber);

        public async Task<List<string>> LoginAsync(string username, string password)
        {
            // TODO: Make custom exception type
            var cookies = new List<string>();
            
            // STAGE 1
            // Getting the csrf key
            var loginRes = await _httpClient.GetAsync(kAppServiceLoginUrl);
            var loginPage = await loginRes.Content.ReadAsStringAsync();
            var executionTag = loginPage.IndexOf("execution\"", StringComparison.Ordinal);
            var executionStart = loginPage.IndexOf('"', executionTag+10)+1;
            var executionEnd = loginPage.IndexOf('"', executionStart+1);
            var execution = loginPage[executionStart..executionEnd];
            
            // Cookie: JSESSIONID, _csrf
            if (!loginRes.Headers.TryGetValues("Set-Cookie", out var loginPageCookies))
                throw new Exception("Failed to get cookies (stage 1)");
            foreach (var rawCookie in loginPageCookies)
                cookies.Add(rawCookie.Substring(0, rawCookie.IndexOf(';') + 1));
            
            // STAGE 2
            // Getting the RSA public key
            var pubKeyReq = new HttpRequestMessage(HttpMethod.Get, kPublicKeyUrl);
            pubKeyReq.Headers.Add("Cookie", string.Join(' ', cookies));
            var pubKeyMessage = await _httpClient.SendAsync(pubKeyReq);
            var pubKey = pubKeyMessage.Content.ReadAsStreamAsync();
            var pubKeyObj = await JsonSerializer.DeserializeAsync<ZjuamPubKey>(await pubKey);
            var modulus = parsePositiveHexBigInteger(pubKeyObj.Modulus);
            var exponent = parsePositiveHexBigInteger(pubKeyObj.Exponent);
            var passwordHex = string.Join("", password.Select(x => Convert.ToInt32(x).ToString("X")));
            var passwordInt = parsePositiveHexBigInteger(passwordHex);
            var encryptedPassword = BigInteger.ModPow(passwordInt, exponent, modulus).ToString("x").PadLeft(128, '0');
            // Cookie: _pv0
            if (!pubKeyMessage.Headers.TryGetValues("Set-Cookie", out var pubKeyCookies))
                throw new Exception("Failed to get cookies (stage 2)");
            foreach (var rawCookie in pubKeyCookies)
                cookies.Add(rawCookie.Substring(0, rawCookie.IndexOf(';') + 1));

            // STAGE 3
            // Real Log in
            var readLoginReq = new HttpRequestMessage(HttpMethod.Post, kAppServiceLoginUrl);
            readLoginReq.Headers.Add("Cookie", string.Join(' ', cookies));
            readLoginReq.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", encryptedPassword),
                new KeyValuePair<string, string>("authcode", string.Empty),
                new KeyValuePair<string, string>("execution", execution),
                new KeyValuePair<string, string>("_eventId", "submit")
            });
            var realLoginRes = await _httpClient.SendAsync(readLoginReq);
                
            // HttpClient doesn't support 302 from https to http
            // So the request will stop here
            if (realLoginRes.StatusCode != HttpStatusCode.Redirect)
            {
                var errorPage = await realLoginRes.Content.ReadAsStringAsync();
                if (errorPage.Contains("用户名或密码错误"))
                    throw new Exception("用户名或密码错误");
                throw new Exception("未知错误");
            }
            
            // Cookie: CASPRIVACY, _pf0, _pm0, _pc0, iPlanetDirectoryPro
            if (!realLoginRes.Headers.TryGetValues("Set-Cookie", out var realLoginCookies))
                throw new Exception("Failed to get cookies (stage 3)");
            foreach (var rawCookie in realLoginCookies)
                cookies.Add(rawCookie.Substring(0, rawCookie.IndexOf(';') + 1));

            if (realLoginRes.Headers.Location != null)
            {
                // STAGE 4
                var appServiceReq = new HttpRequestMessage(HttpMethod.Get, realLoginRes.Headers.Location);
                appServiceReq.Headers.Add("Cookie", string.Join(' ', cookies));
                var appServiceRes = await _httpClient.SendAsync(appServiceReq);
                // Cookie: wisportalId
                // Cookie is set at http://appservice.zju.edu.cn/?ticket=
                if (!appServiceRes.Headers.TryGetValues("Set-Cookie", out var appServiceCookies))
                    throw new Exception("Failed to get cookies (stage 4)");
                foreach (var rawCookie in appServiceCookies)
                    cookies.Add(rawCookie.Substring(0, rawCookie.IndexOf(';') + 1));
            }

            return cookies;
        }

        public async Task<string> GetStuIdAsync(string cookie)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, kAppServiceGetUserInfo);
            req.Headers.Add("Cookie", cookie);
            var res = await _httpClient.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var streamTask = res.Content.ReadAsStreamAsync();
            var userInfo = await JsonSerializer.DeserializeAsync<ZjuResWrapperNum<ZjuUserInfoRes>>(await streamTask);
            return userInfo.Data.Id;
        }
        
        public async Task<ZjuClass[]> GetClassTimeTableAsync(string cookie, string academicYear, ClassTerm term, string stuId)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, kAppServiceGetWeekClassInfo);
            req.Headers.Add("Cookie", cookie);
            req.Content = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("xn", academicYear), 
                new KeyValuePair<string, string>("xq", term.ToQueryString()),
                new KeyValuePair<string, string>("xh", stuId), 
            });
            var res = await _httpClient.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var streamTask = res.Content.ReadAsStreamAsync();
            var classTimeTable =
                await JsonSerializer.DeserializeAsync<ZjuResWrapperStr<ZjuWeeklyScheduleRes>>(await streamTask);

            return classTimeTable.Data.ClassList.Select(x => x.ToZjuClass()).Where(x => x!=null).ToArray();
        }
        
        public async Task<ZjuExamOutline[]> GetExamInfoAsync(string cookie, string academicYear, ExamTerm term, string stuId)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, kAppServiceGetExamOutlineInfo);
            req.Headers.Add("Cookie", cookie);
            req.Content = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("xh", stuId),
                new KeyValuePair<string, string>("xn", academicYear),
                new KeyValuePair<string, string>("xq", term.ToQueryString()) 
            });
            var res = await _httpClient.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var streamTask = res.Content.ReadAsStreamAsync();
            var examOutlines =
                await JsonSerializer.DeserializeAsync<ZjuResWrapperStr<ZjuExamOutlineRes>>(await streamTask);
            return examOutlines.Data.ExamOutlineList;
        }

        public async Task<TermConfig[]> GetTermConfigsAsync()
        {
            return _schedule.TermConfigs.Select(x => x.ToTermConfig()).ToArray();
        }

        public async Task<Tweak[]> GetTweaksAsync()
        {
            return _schedule.Tweaks.Select(x => x.ToTweak()).ToArray();
        }

        public async Task<(string, ClassTerm)[]> GetClassTermsAsync()
        {
            return _schedule.GetClassYearAndTerms().ToArray();
        }
        
        public async Task<(string, ExamTerm)[]> GetExamTermsAsync()
        {
            return _schedule.GetExamYearAndTerms().ToArray();
        }

        public async Task<bool> UpdateConfigAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _config["CONFIG_URL"]);
            var res = await _httpClient.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var streamTask = res.Content.ReadAsStreamAsync();
            _schedule = await JsonSerializer.DeserializeAsync<ZjuScheduleConfig>(await streamTask);
            return true;
        }
    }

}