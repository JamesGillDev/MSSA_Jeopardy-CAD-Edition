namespace MSSA_Jeopardy_.Models
{
    public class JeopardyQuestion
    {
        public bool IsAnswered { get; set; }
        public bool IsBonus { get; set; }
        public string Category { get; set; } = "";
        public int PointValue { get; set; }
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
    }
}
