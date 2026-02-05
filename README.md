# ?? MSSA Jeopardy! - Cloud Application Development Edition

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A fun, interactive Jeopardy!-style quiz game designed for the **Microsoft Software & Systems Academy (MSSA) Cloud Application Development Program**. Test your knowledge across key topics covered in the CAD curriculum!

![MSSA Jeopardy Banner](https://img.shields.io/badge/MSSA-Jeopardy!-gold?style=for-the-badge&labelColor=blue)

## ?? About

This application is built to help MSSA students review and reinforce their learning in a gamified format. The classic Jeopardy! game show format makes studying more engaging and competitive, whether you're preparing for certifications or just want to test your cloud development knowledge.

## ? Features

- ?? **Classic Jeopardy! Gameplay** - Select categories and point values from an interactive game board
- ?? **Score Tracking** - Earn points for correct answers, lose points for incorrect ones
- ?? **30 Questions** - 6 categories with 5 questions each (100-500 points)
- ?? **Responsive Design** - Works on desktop, tablet, and mobile devices
- ?? **Replay Anytime** - Reset the game and play again
- ?? **Authentic Styling** - Classic blue and gold Jeopardy! theme

## ?? Game Categories

| Category | Topics Covered |
|----------|---------------|
| ?? **Azure Fundamentals** | Resource Groups, Azure Functions, Hybrid Cloud, Azure SQL, Pay-As-You-Go |
| ?? **C# Programming** | void keyword, Classes, try-catch, async/await, LINQ |
| ?? **Web Development** | HTTP methods, Blazor, Status codes, REST APIs, Middleware |
| ?? **DevOps & CI/CD** | Git, Azure DevOps, Continuous Integration, Docker, AKS |
| ??? **Databases** | SELECT statements, NoSQL, Cosmos DB, Entity Framework, ACID |
| ?? **Security** | Authentication, Microsoft Entra ID, OAuth, XSS, Azure Key Vault |

## ??? Technologies Used

- **Framework:** [.NET 10](https://dotnet.microsoft.com/)
- **Frontend:** [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- **Language:** C# 14
- **Styling:** Scoped CSS with custom Jeopardy! theme
- **Architecture:** Client-Server with Interactive WebAssembly rendering

## ?? Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition.git
   cd MSSA_Jeopardy-CAD-Edition
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd "MSSA Jeoporady!"
   dotnet run
   ```

5. **Open your browser** and navigate to `https://localhost:5001` or the URL shown in the terminal

### Running in Visual Studio

1. Open `MSSA Jeoporady!.sln` in Visual Studio 2022
2. Set `MSSA Jeoporady!` as the startup project
3. Press `F5` to run with debugging or `Ctrl+F5` to run without debugging

## ?? How to Play

1. **Start the Game** - Click "Play Jeopardy!" from the home page
2. **Select a Question** - Click on any dollar amount on the game board
3. **Read the Clue** - A modal will appear with the question
4. **Think of Your Answer** - Formulate your response (remember: answers are in "What is...?" format)
5. **Reveal the Answer** - Click "Reveal Answer" to see the correct response
6. **Score Yourself** - Click "I Got It Right!" or "I Got It Wrong" to update your score
7. **Continue Playing** - Select another question until the board is complete
8. **Play Again** - Click "New Game" to reset and play again

### Scoring

| Result | Points |
|--------|--------|
| ? Correct | +Point Value |
| ? Incorrect | -Point Value |

## ?? Project Structure

```
MSSA Jeoporady!/
??? MSSA Jeoporady!/                    # Server project
?   ??? Components/
?   ?   ??? Layout/
?   ?   ?   ??? MainLayout.razor        # Main layout template
?   ?   ?   ??? NavMenu.razor           # Navigation menu
?   ?   ??? Pages/
?   ?   ?   ??? Home.razor              # Welcome/landing page
?   ?   ?   ??? Error.razor             # Error page
?   ?   ?   ??? NotFound.razor          # 404 page
?   ?   ??? App.razor                   # Root component
?   ?   ??? Routes.razor                # Routing configuration
?   ??? wwwroot/                        # Static assets
?   ??? Program.cs                      # Application entry point
?
??? MSSA Jeoporady!.Client/             # WebAssembly client project
?   ??? Models/
?   ?   ??? JeopardyQuestion.cs         # Data models
?   ??? Services/
?   ?   ??? JeopardyGameService.cs      # Game logic & questions
?   ??? Pages/
?   ?   ??? Jeopardy.razor              # Main game board component
?   ??? Program.cs                      # Client entry point
?
??? MSSA Jeoporady!.sln                 # Solution file
```

## ?? Contributing

Contributions are welcome! If you'd like to add more questions, fix bugs, or improve the UI:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Adding New Questions

To add new questions, edit `JeopardyGameService.cs` and add entries to the appropriate category:

```csharp
new JeopardyQuestion 
{ 
    Category = "Category Name", 
    PointValue = 100, // 100, 200, 300, 400, or 500
    Question = "This is the clue that players will see.", 
    Answer = "What is the correct answer?" 
}
```

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Acknowledgments

- **Microsoft Software & Systems Academy (MSSA)** - For providing world-class training to military veterans and transitioning service members
- **Jeopardy!** - The classic game show that inspired this educational tool
- **MSSA Instructors & Staff** - For their dedication to student success
- **Fellow MSSA Students** - For the camaraderie and collaborative learning environment

## ?? Contact

**James Gill** - [@JamesGillDev](https://github.com/JamesGillDev)

Project Link: [https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition](https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition)

---

<p align="center">
  <strong>Built with ?? by an MSSA Cloud Application Development Student</strong>
</p>

<p align="center">
  <em>"This is Jeopardy!" ??</em>
</p>
