namespace MSSA_Jeoporady_.Client.Models;

public class JeopardyQuestion
{
    public string Category { get; set; } = string.Empty;
    public int PointValue { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
}

public class JeopardyCategory
{
    public string Name { get; set; } = string.Empty;
    public List<JeopardyQuestion> Questions { get; set; } = [];
}
