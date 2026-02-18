using MSSA_Jeopardy_.Models;
using System.Collections.Generic;
using System.Linq;

namespace MSSA_Jeopardy_.Services
{
    #nullable enable
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

        public void SetSelectedCategories(List<string> categories)
        {
            Categories = new List<Category>();
            foreach (var catName in categories)
            {
                var category = new Category
                {
                    Name = catName,
                    Questions = GenerateQuestionsForCategory(catName)
                };
                Categories.Add(category);
            }
        }

        private List<JeopardyQuestion> GenerateQuestionsForCategory(string categoryName)
        {
            // For demo: generate 5 questions per category
            var questions = new List<JeopardyQuestion>();
            for (int i = 1; i <= 5; i++)
            {
                questions.Add(new JeopardyQuestion
                {
                    Category = categoryName,
                    Question = $"Sample question {i} for {categoryName}?",
                    Answer = $"Sample answer {i}",
                    PointValue = i * 100,
                    IsAnswered = false,
                    IsBonus = (i == 3) // Make the 3rd question a bonus
                });
            }
            return questions;
        }

        public bool IsGameComplete()
        {
            // Game is complete if all questions are answered
            return Categories.Count > 0 && Categories.All(cat => cat.Questions.All(q => q.IsAnswered));
        }

        public int GetWinner()
        {
            // Return the player with the highest score
            if (PlayerScores.Count == 0) return 1;
            return PlayerScores.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }

        public int GetHighestScore()
        {
            if (PlayerScores.Count == 0) return 0;
            return PlayerScores.Values.Max();
        }

        public void StartGame() { GameStarted = true; }
        public void ResetToMenu() { GameStarted = false; }
        public void SelectQuestion(JeopardyQuestion question) { CurrentQuestion = question; }
        public void AnswerQuestion(bool isCorrect, int playerNumber)
        {
            if (CurrentQuestion != null && !CurrentQuestion.IsAnswered)
            {
                if (isCorrect)
                {
                    PlayerScores[playerNumber] += CurrentQuestion.IsBonus ? CurrentQuestion.PointValue * 2 : CurrentQuestion.PointValue;
                }
                else
                {
                    PlayerScores[playerNumber] -= CurrentQuestion.PointValue;
                }
                CurrentQuestion.IsAnswered = true;
            }
        }
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
