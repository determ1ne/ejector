using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ejector.Services
{
    public interface IZjuService
    {
        Task<List<string>> LoginAsync(string username, string password);
        Task<string> GetStuIdAsync(string cookie);
        Task<ZjuClass[]> GetClassTimeTableAsync(string cookie, string academicYear, ClassTerm term, string stuId);
        Task<ZjuExamOutline[]> GetExamInfoAsync(string cookie, string academicYear, ExamTerm term, string stuId);
        Task<TermConfig[]> GetTermConfigsAsync();
        Task<Tweak[]> GetTweaksAsync();
    }
}