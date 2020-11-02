using PerudoBot.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PerudoBot.Extensions
{
    public static class TopRecordServices
    {
        public static string ToStringWithNewlines(this List<TopRecord> topRecords)
        {
            return string.Join('\n', topRecords);
        }
    }
}