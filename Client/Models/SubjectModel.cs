using System.ComponentModel.DataAnnotations;
using Client.Enums;

namespace Client.Models
{
    public class SubjectModel
    {
        public string SubjectName { get; set; } = string.Empty;

        [EnumDataType(typeof(SubjectEnums))]
        public SubjectEnums SubjectType { get; set; } = SubjectEnums.Other;
        
        // Time Range utc time
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        // Day of the week: 1 = Monday, 2 = Tuesday, 3 = Wednesday, 4 = Thursday, 5 = Friday
        [Range(1, 5)]
        public int DayOfWeek { get; set; }

        // Every second Week of Month (Between 1 and 4, where 0 means every week)
        [Range(0, 4)]
        public List<int> WeeksOfMonth { get; set; } = new List<int> { 0 };
    }
}
