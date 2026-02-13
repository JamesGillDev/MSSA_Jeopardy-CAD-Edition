using MSSA_Jeopardy_.Models;
using System.Collections.Generic;

namespace MSSA_Jeopardy_.Services
{
    public class JeopardyGameService
    {
        public List<string> AllCategoryNames { get; set; } = new List<string>
        {
            "Azure Fundamentals", "C# Programming", "Web Development", "DevOps & CI/CD", "Databases", "Security",
            "Networking", "Cloud Architecture", "Software Testing", "Data Structures", "Operating Systems",
            "APIs & Integration", "Machine Learning Basics", "PowerShell & CLI", "Agile & Scrum",
            "AZ-900 Exam Prep", "AZ-204 Exam Prep", "AI-900 Exam Prep", "Algorithms (C#)",
            "DP-3001 (Azure Data)", "DP-080 (Data Fundamentals)", "DP-3020 (Advanced Data)", "MS-4010 (Security)"
        };

        public bool GameStarted { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string> { { 1, "Player 1" }, { 2, "Player 2" }, { 3, "Player 3" } };
        public Dictionary<int, int> PlayerScores { get; set; } = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
        public int TotalPlayers => PlayerNames.Count;
        public int CurrentPlayer { get; set; } = 1;
        public List<Category> Categories { get; set; } = new List<Category>();
        public JeopardyQuestion? CurrentQuestion { get; set; }

        public List<int> GetPlayerIds() => new List<int>(PlayerNames.Keys);

        public bool IsGameComplete() => false;
        public int GetWinner() => 1;
        public int GetHighestScore() => 0;

        public void SetSelectedCategories(List<string> categories) { }
        public void StartGame() { GameStarted = true; }
        public void ResetToMenu() { GameStarted = false; }
        public void SelectQuestion(JeopardyQuestion question) { CurrentQuestion = question; }
        public void AnswerQuestion(bool isCorrect, int playerNumber) { }
        public void CloseQuestion() { CurrentQuestion = null; }
        public void InitializeGameKeepPlayers() { }
        public void AddPlayer() { int next = PlayerNames.Count + 1; PlayerNames[next] = $"Player {next}"; PlayerScores[next] = 0; }
        public void RemovePlayer(int playerId) { PlayerNames.Remove(playerId); PlayerScores.Remove(playerId); }
        public void SetPlayerName(int playerNum, string name) { PlayerNames[playerNum] = name; }
    }

    public class Category
    {
        public string Name { get; set; } = "";
        public List<JeopardyQuestion> Questions { get; set; } = new List<JeopardyQuestion>();
    }
}
