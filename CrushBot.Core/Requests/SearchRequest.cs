using CrushBot.Core.Entities;
using CrushBot.Core.Enums;

namespace CrushBot.Core.Requests
{
    public class SearchRequest(BotUser user)
    {
        public string CityId { get; set; } = user.CityId!;

        public int MinAge { get; set; } = user.Filter!.AgeAfter!.Value;

        public int MaxAge { get; set; } = user.Filter.AgeUntil!.Value;

        public Sex Sex { get; set; } = user.Filter.Sex!.Value;
    }
}
