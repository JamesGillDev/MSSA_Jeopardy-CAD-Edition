namespace MSSA_Jeopardy.Services;

public class JeopardyGameService
{
    public static readonly string[] AllCategoryNames =
    [
        "Azure Fundamentals",
        "C# Programming",
        "Web Development",
        "DevOps & CI/CD",
        "Databases",
        "Security",
        "Networking",
        "Cloud Architecture",
        "Software Testing",
        "Data Structures",
        "Operating Systems",
        "APIs & Integration",
        "Machine Learning Basics",
        "PowerShell & CLI",
        "Agile & Scrum",
        "AZ-900 Exam Prep",
        "AZ-204 Exam Prep",
        "AI-900 Exam Prep",
        "Algorithms (C#)",
        "DP-3001 (Azure Data)",
        "DP-080 (Data Fundamentals)",
        "DP-3020 (Advanced Data)",
        "MS-4010 (Security)"
    ];

    public IReadOnlyList<string> CategoryNames => AllCategoryNames;
}
