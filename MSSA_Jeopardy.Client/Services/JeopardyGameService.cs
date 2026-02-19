using MSSA_Jeopardy.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSSA_Jeopardy.Services;

public class JeopardyGameService
{
    public List<JeopardyCategory> Categories { get; private set; } = [];
    public Dictionary<int, int> PlayerScores { get; private set; } = [];
    public Dictionary<int, string> PlayerNames { get; private set; } = [];
    public int CurrentPlayer { get; private set; } = 1;
    public int TotalPlayers => PlayerScores.Count;
    public JeopardyQuestion? CurrentQuestion { get; private set; }
    public List<string> SelectedCategories { get; private set; } = [];
    public bool GameStarted { get; private set; } = false;

    private static readonly Random _random = new();
    private int _nextPlayerId = 1;

    // All available category names
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
        "AZ-900 (Azure Fundamentals)",
        "AZ-204 (Azure Developer)",
        "AI-900 (Azure AI Fundamentals)",
        // New categories
        "Algorithms (C#)", // C# Algorithms
        "DP-3001 (Azure Data)", // Azure Data
        "DP-080 (Data Fundamentals)", // Data Fundamentals
        "DP-3020 (Advanced Data)", // Advanced Data
        "MS-4010 (Security)", // Security
        ".NET Core",
        "Git & Version Control",
        "Cloud Security",
        "Containers & Kubernetes",
        "Microsoft Power Platform",
        "Data Analytics",
        "Azure Blob Storage",
        "Key Vault",
        "App Service",
        "App Configuration",
        "Container Apps",
        "Container Registry",
        "Service Bus",
        "Event Grid",
        "Event Hub",
        "Functions",
        "Bicep",
        "Application Insights",
        "Azure Resource Manager",
        "Azure DevOps",
        "Terraform",
        "Ansible",
        "Kubernetes",
        "Docker",
        "API Management",
        "Logic Apps",
        "Data Factory",
        "Synapse Analytics",
        "Cosmos DB",
        "SQL Database",
        "MySQL",
        "PostgreSQL",
        "MariaDB",
        "Blob Storage",
        "Queue Storage",
        "Table Storage",
        "File Storage",
        "Redis Cache",
        "Windows Virtual Desktop",
        "Azure Monitor",
        "Azure Sentinel",
        "Azure Security Center",
        "Microsoft Sentinel",
        "Key Vault",
        "Azure Firewall",
        "Azure Bastion",
        "Azure Front Door",
        "Azure Content Delivery Network",
        "ExpressRoute",
        "Virtual Network",
        "Network Security Group",
        "Application Gateway",
        "Azure Load Balancer",
        "Azure Traffic Manager",
        "Azure Site Recovery",
        "Azure Backup",
        "Microsoft 365 Compliance",
        "Azure Blueprints",
        "Azure Policy",
        "Role-Based Access Control",
        "Azure Active Directory",
        "Multi-Factor Authentication",
        "Conditional Access",
        "Identity Protection",
        "Azure Information Protection",
        "Azure Key Vault",
        "Azure Secrets Manager",
        "Azure Logs",
        "Azure Metrics",
        "Application Insights",
        "Azure Network Watcher",
        "Azure Service Health",
        "Azure Dashboard",
        "Log Analytics",
        "Azure Policy",
        "Azure Blueprints",
        "Azure DevOps",
        "GitHub",
        "Visual Studio",
        "Azure Data Studio",
        "Postman",
        "Swagger",
        "Azure Management Portal",
        "Azure CLI",
        "Azure PowerShell",
        "Azure SDKs",
        "Azure Resource Manager",
        "Azure Distributed Data Implementation",
        "Azure Application Gateway",
        "Azure Web Application Firewall",
        "Azure DDoS Protection",
        "Azure Security Center",
        "Microsoft Defender for Cloud"

    ];

    public JeopardyGameService()
    {
        InitializePlayers();
    }

    private void InitializePlayers()
    {
        PlayerScores.Clear();
        PlayerNames.Clear();
        _nextPlayerId = 1;

        for (int i = 0; i < 3; i++)
        {
            AddPlayer();
        }
        CurrentPlayer = 1;
    }

    public void SetSelectedCategories(List<string> categories)
    {
        SelectedCategories = categories.Take(6).ToList(); // Max 6 categories
    }

    public void StartGame()
    {
        if (SelectedCategories.Count == 0)
        {
            // Default categories if none selected
            SelectedCategories = AllCategoryNames.Take(6).ToList();
        }

        // Defensive: Ensure at least one player exists
        if (PlayerScores.Count == 0)
        {
            AddPlayer();
        }

        foreach (var key in PlayerScores.Keys.ToList())
        {
            PlayerScores[key] = 0;
        }
        CurrentPlayer = PlayerScores.Keys.First();
        Categories = BuildGameBoard();
        GameStarted = true;
    }

    public void ResetToMenu()
    {
        GameStarted = false;
        Categories.Clear();
        foreach (var key in PlayerScores.Keys.ToList())
        {
            PlayerScores[key] = 0;
        }
    }

    public void InitializeGameKeepPlayers()
    {
        foreach (var key in PlayerScores.Keys.ToList())
        {
            PlayerScores[key] = 0;
        }
        CurrentPlayer = PlayerScores.Keys.First();
        Categories = BuildGameBoard();
    }

    public int AddPlayer(string? name = null)
    {
        int playerId = _nextPlayerId++;
        PlayerScores[playerId] = 0;
        PlayerNames[playerId] = name ?? $"Player {playerId}";
        return playerId;
    }

    public bool RemovePlayer(int playerId)
    {
        if (TotalPlayers <= 1) return false;

        if (PlayerScores.Remove(playerId))
        {
            PlayerNames.Remove(playerId);
            if (CurrentPlayer == playerId || !PlayerScores.ContainsKey(CurrentPlayer))
            {
                CurrentPlayer = PlayerScores.Keys.First();
            }
            return true;
        }
        return false;
    }

    public List<int> GetPlayerIds()
    {
        return PlayerScores.Keys.OrderBy(k => k).ToList();
    }

    private List<JeopardyCategory> BuildGameBoard()
    {
        var allQuestions = GetAllQuestionPool();
        var categories = new List<JeopardyCategory>();
        var pointValues = new[] { 100, 200, 300, 400, 500 };

        foreach (var categoryName in SelectedCategories)
        {
            var categoryQuestions = allQuestions
                .Where(q => q.Category == categoryName)
                .GroupBy(q => q.PointValue)
                .ToDictionary(g => g.Key, g => g.ToList());

            var selectedQuestions = new List<JeopardyQuestion>();

            foreach (var pointValue in pointValues)
            {
                if (categoryQuestions.TryGetValue(pointValue, out var questionsForValue) && questionsForValue.Count > 0)
                {
                    var randomIndex = _random.Next(questionsForValue.Count);
                    var question = questionsForValue[randomIndex];
                    question.IsBonus = _random.Next(10) == 0;
                    question.IsAnswered = false;
                    selectedQuestions.Add(question);
                }
                else
                {
                    // Defensive: Add a placeholder question if missing
                    selectedQuestions.Add(new JeopardyQuestion
                    {
                        Category = categoryName,
                        PointValue = pointValue,
                        Question = $"No question available for {categoryName} ({pointValue} points).",
                        Answer = "N/A",
                        IsAnswered = false,
                        IsBonus = false
                    });
                }
            }

            categories.Add(new JeopardyCategory
            {
                Name = categoryName,
                Questions = selectedQuestions
            });
        }

        return categories;
    }

    public void SetPlayerName(int playerNumber, string name)
    {
        if (PlayerNames.ContainsKey(playerNumber) && !string.IsNullOrWhiteSpace(name))
        {
            PlayerNames[playerNumber] = name;
        }
    }

    public void SelectQuestion(JeopardyQuestion question)
    {
        if (!question.IsAnswered)
        {
            CurrentQuestion = question;
        }
    }

    public void AnswerQuestion(bool isCorrect, int playerNumber)
    {
        if (CurrentQuestion != null && PlayerScores.ContainsKey(playerNumber))
        {
            CurrentQuestion.IsAnswered = true;
            int points = CurrentQuestion.IsBonus ? CurrentQuestion.PointValue * 2 : CurrentQuestion.PointValue;

            if (isCorrect)
                PlayerScores[playerNumber] += points;
            else
                PlayerScores[playerNumber] -= points;

            CurrentQuestion = null;
            NextPlayer();
        }
    }

    public void NextPlayer()
    {
        var playerIds = GetPlayerIds();
        int currentIndex = playerIds.IndexOf(CurrentPlayer);
        int nextIndex = (currentIndex + 1) % playerIds.Count;
        CurrentPlayer = playerIds[nextIndex];
    }

    public void CloseQuestion() => CurrentQuestion = null;

    public bool IsGameComplete() => Categories.All(c => c.Questions.All(q => q.IsAnswered));

    public int GetWinner() => PlayerScores.OrderByDescending(p => p.Value).First().Key;

    public int GetHighestScore() => PlayerScores.Values.Max();

    private static List<JeopardyQuestion> GetAllQuestionPool()
    {
        return
        [
            // ==================== AZURE FUNDAMENTALS ====================
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This is the basic unit of deployment in Azure that contains resources like VMs, storage, and databases.", Answer = "What is a Resource Group?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This portal provides a web-based interface to manage all Azure services.", Answer = "What is the Azure Portal?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This term describes Azure's worldwide network of data centers.", Answer = "What are Azure Regions?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This Azure service lets you create isolated networks in the cloud.", Answer = "What is Virtual Network (VNet)?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This is Microsoft's command-line tool for managing Azure resources.", Answer = "What is Azure CLI?" },

            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure service provides serverless compute that lets you run code without managing servers.", Answer = "What is Azure Functions?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This service provides virtual machines in the cloud.", Answer = "What is Azure Virtual Machines?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure storage type is optimized for storing massive amounts of unstructured data.", Answer = "What is Blob Storage?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure service provides a platform for hosting web applications.", Answer = "What is Azure App Service?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This feature allows you to organize Azure resources using key-value pairs.", Answer = "What are Tags?" },

            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This cloud model combines on-premises infrastructure with cloud resources.", Answer = "What is Hybrid Cloud?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure feature automatically adjusts resources based on demand.", Answer = "What is Auto-scaling?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This service provides managed Kubernetes orchestration in Azure.", Answer = "What is Azure Kubernetes Service (AKS)?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure service provides message queuing for decoupling applications.", Answer = "What is Azure Queue Storage or Service Bus?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure feature helps estimate costs before deploying resources.", Answer = "What is the Azure Pricing Calculator?" },

            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides a fully managed relational database with built-in intelligence.", Answer = "What is Azure SQL Database?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides a content delivery network for fast global content delivery.", Answer = "What is Azure CDN?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This tool allows you to define Azure infrastructure as code using JSON templates.", Answer = "What is ARM Templates?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service monitors the health and performance of your applications.", Answer = "What is Azure Monitor?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides DNS hosting and domain management.", Answer = "What is Azure DNS?" },

            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This pricing model charges you only for resources you actually use, with no upfront costs.", Answer = "What is Pay-As-You-Go?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This Azure governance feature helps you organize resources and apply consistent policies.", Answer = "What is Azure Policy?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This SLA percentage guarantees about 8.76 hours of downtime per year.", Answer = "What is 99.9%?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This Azure feature provides cost management recommendations and optimization tips.", Answer = "What is Azure Advisor?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This hierarchy level sits above subscriptions for managing multiple Azure environments.", Answer = "What is a Management Group?" },

            // ==================== C# PROGRAMMING ====================
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword is used to define a method that doesn't return a value.", Answer = "What is void?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword creates a new instance of a class.", Answer = "What is new?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This data type stores true or false values.", Answer = "What is bool?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword is used to define a constant value that cannot be changed.", Answer = "What is const?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This operator is used to concatenate strings in C#.", Answer = "What is + (plus)?" },

            new() { Category = "C# Programming", PointValue = 200, Question = "This feature allows you to define a blueprint for creating objects with properties and methods.", Answer = "What is a Class?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This keyword makes a class member accessible only within the same class.", Answer = "What is private?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This collection type stores key-value pairs for fast lookups.", Answer = "What is a Dictionary?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This keyword is used to check if an object is of a specific type.", Answer = "What is is?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This type of loop iterates through each element in a collection.", Answer = "What is foreach?" },

            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword is used to handle exceptions that may occur during program execution.", Answer = "What is try-catch?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This OOP principle allows a derived class to provide a specific implementation of a base class method.", Answer = "What is Polymorphism?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword allows a class to inherit from another class.", Answer = "What is the colon (:) or inheritance?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword prevents a class from being inherited.", Answer = "What is sealed?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This type of method belongs to the class itself rather than an instance.", Answer = "What is static?" },

            new() { Category = "C# Programming", PointValue = 400, Question = "This C# feature allows methods to run concurrently without blocking the main thread.", Answer = "What is async/await?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This feature lets you define type-safe generic classes and methods.", Answer = "What are Generics?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This delegate type represents a method that can be passed as a parameter.", Answer = "What is Action or Func?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This keyword ensures a variable can only be assigned once, but at runtime.", Answer = "What is readonly?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This operator returns the left operand if not null, otherwise the right operand.", Answer = "What is ?? (null-coalescing)?" },

            new() { Category = "C# Programming", PointValue = 500, Question = "This LINQ method filters a sequence of values based on a predicate.", Answer = "What is Where()?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This pattern uses events to notify subscribers when state changes.", Answer = "What is the Observer Pattern?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This C# 9+ feature provides concise syntax for immutable data types.", Answer = "What are Records?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This feature allows you to add methods to existing types without modifying them.", Answer = "What are Extension Methods?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This C# feature enables pattern matching with type checking and property inspection.", Answer = "What is switch expression?" },

            // ==================== WEB DEVELOPMENT ====================
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to retrieve data from a server.", Answer = "What is GET?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to submit data to be processed by a server.", Answer = "What is POST?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This markup language structures content on web pages.", Answer = "What is HTML?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This language is used to style web pages with colors, fonts, and layouts.", Answer = "What is CSS?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to delete a resource on the server.", Answer = "What is DELETE?" },

            new() { Category = "Web Development", PointValue = 200, Question = "This Microsoft framework allows you to build interactive web UIs using C# instead of JavaScript.", Answer = "What is Blazor?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This data format is commonly used for API responses and is based on JavaScript object notation.", Answer = "What is JSON?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This ASP.NET Core feature handles incoming HTTP requests.", Answer = "What is a Controller?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This HTTP method is used to update an entire resource on the server.", Answer = "What is PUT?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This attribute in ASP.NET Core maps HTTP routes to controller actions.", Answer = "What is [Route]?" },

            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates that a resource was not found on the server.", Answer = "What is 404?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates a successful HTTP request.", Answer = "What is 200?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates the server encountered an internal error.", Answer = "What is 500?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates a resource was created successfully.", Answer = "What is 201?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates the client is not authorized.", Answer = "What is 401?" },

            new() { Category = "Web Development", PointValue = 400, Question = "This architectural style uses HTTP methods and is commonly used for building web APIs.", Answer = "What is REST?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This security header prevents clickjacking attacks by controlling iframe embedding.", Answer = "What is X-Frame-Options?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This technique allows servers to push updates to clients in real-time.", Answer = "What is SignalR or WebSockets?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This HTTP header tells the browser what content type to expect.", Answer = "What is Content-Type?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This mechanism allows cross-origin requests from web browsers.", Answer = "What is CORS?" },

            new() { Category = "Web Development", PointValue = 500, Question = "This ASP.NET Core feature allows you to add cross-cutting concerns like logging and authentication to your request pipeline.", Answer = "What is Middleware?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This design pattern separates an application into Model, View, and Controller components.", Answer = "What is MVC?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This Blazor hosting model runs entirely in the browser via WebAssembly.", Answer = "What is Blazor WebAssembly?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This ASP.NET Core feature injects dependencies into classes automatically.", Answer = "What is Dependency Injection?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This query language developed by Facebook provides an alternative to REST APIs.", Answer = "What is GraphQL?" },

            // ==================== DEVOPS & CI/CD ====================
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This version control system tracks changes to source code and is widely used in software development.", Answer = "What is Git?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command creates a copy of a remote repository on your local machine.", Answer = "What is git clone?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command stages changes for the next commit.", Answer = "What is git add?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command saves staged changes to the repository.", Answer = "What is git commit?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command uploads local commits to a remote repository.", Answer = "What is git push?" },

            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Azure service provides unlimited private Git repositories and agile planning tools.", Answer = "What is Azure DevOps?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This file defines the steps in an Azure DevOps pipeline.", Answer = "What is azure-pipelines.yml?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This GitHub feature automates workflows when code is pushed or PRs are created.", Answer = "What is GitHub Actions?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Git command downloads changes from a remote repository.", Answer = "What is git pull or git fetch?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Git command creates a new branch.", Answer = "What is git branch or git checkout -b?" },

            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This practice involves automatically building and testing code changes when they're committed.", Answer = "What is Continuous Integration (CI)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This practice automatically deploys code changes to production after passing tests.", Answer = "What is Continuous Deployment (CD)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This branching strategy uses feature branches that merge into a main branch.", Answer = "What is Git Flow or Feature Branching?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This Git command combines changes from one branch into another.", Answer = "What is git merge?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This type of test verifies individual units of code work correctly.", Answer = "What is Unit Testing?" },

            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This containerization platform packages applications with their dependencies for consistent deployment.", Answer = "What is Docker?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This file defines how to build a Docker container image.", Answer = "What is a Dockerfile?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This tool defines multi-container Docker applications.", Answer = "What is Docker Compose?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This Git command replays commits on top of another branch.", Answer = "What is git rebase?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This practice stores and manages container images.", Answer = "What is a Container Registry?" },

            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Azure service orchestrates containerized applications at scale using Kubernetes.", Answer = "What is Azure Kubernetes Service (AKS)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Infrastructure as Code tool by HashiCorp provisions cloud resources.", Answer = "What is Terraform?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Azure service provides serverless container hosting without managing infrastructure.", Answer = "What is Azure Container Apps?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This deployment strategy gradually shifts traffic from old to new versions.", Answer = "What is Blue-Green or Canary Deployment?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This practice treats infrastructure configuration like application code.", Answer = "What is Infrastructure as Code (IaC)?" },

            // ==================== DATABASES ====================
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command is used to retrieve data from a database table.", Answer = "What is SELECT?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command adds new records to a database table.", Answer = "What is INSERT?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command modifies existing records in a table.", Answer = "What is UPDATE?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command removes records from a table.", Answer = "What is DELETE?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command creates a new table in a database.", Answer = "What is CREATE TABLE?" },

            new() { Category = "Databases", PointValue = 200, Question = "This type of database stores data in JSON-like documents rather than tables.", Answer = "What is a NoSQL/Document Database?" },
            new() { Category = "Databases", PointValue = 200, Question = "This SQL clause filters records based on a condition.", Answer = "What is WHERE?" },
            new() { Category = "Databases", PointValue = 200, Question = "This database object enforces unique values in a column.", Answer = "What is a Primary Key?" },
            new() { Category = "Databases", PointValue = 200, Question = "This SQL clause sorts the results of a query.", Answer = "What is ORDER BY?" },
            new() { Category = "Databases", PointValue = 200, Question = "This database constraint links records between two tables.", Answer = "What is a Foreign Key?" },

            new() { Category = "Databases", PointValue = 300, Question = "This Azure service is a globally distributed, multi-model database for any scale.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Databases", PointValue = 300, Question = "This SQL operation combines rows from two or more tables based on a related column.", Answer = "What is JOIN?" },
            new() { Category = "Databases", PointValue = 300, Question = "This database design process eliminates redundancy by organizing data into related tables.", Answer = "What is Normalization?" },
            new() { Category = "Databases", PointValue = 300, Question = "This SQL clause groups rows that have the same values.", Answer = "What is GROUP BY?" },
            new() { Category = "Databases", PointValue = 300, Question = "This type of JOIN returns all records from the left table.", Answer = "What is LEFT JOIN?" },

            new() { Category = "Databases", PointValue = 400, Question = "This .NET technology maps database tables to C# classes and provides an abstraction layer.", Answer = "What is Entity Framework?" },
            new() { Category = "Databases", PointValue = 400, Question = "This EF Core approach generates database schema from C# model classes.", Answer = "What is Code-First?" },
            new() { Category = "Databases", PointValue = 400, Question = "This technique improves query performance by creating data structures for fast lookups.", Answer = "What is Indexing?" },
            new() { Category = "Databases", PointValue = 400, Question = "This SQL clause filters groups after GROUP BY.", Answer = "What is HAVING?" },
            new() { Category = "Databases", PointValue = 400, Question = "This database object stores a precompiled SQL query for reuse.", Answer = "What is a Stored Procedure?" },

            new() { Category = "Databases", PointValue = 500, Question = "This database concept ensures that transactions are processed reliably using Atomicity, Consistency, Isolation, and Durability.", Answer = "What is ACID?" },
            new() { Category = "Databases", PointValue = 500, Question = "This SQL injection prevention technique uses parameterized queries.", Answer = "What are Prepared Statements?" },
            new() { Category = "Databases", PointValue = 500, Question = "This Cosmos DB feature automatically replicates data across multiple Azure regions.", Answer = "What is Global Distribution?" },
            new() { Category = "Databases", PointValue = 500, Question = "This database technique splits data across multiple servers for scalability.", Answer = "What is Sharding?" },
            new() { Category = "Databases", PointValue = 500, Question = "This consistency model provides high availability but eventual consistency.", Answer = "What is BASE (Basically Available, Soft state, Eventually consistent)?" },

            // ==================== SECURITY ====================
            new() { Category = "Security", PointValue = 100, Question = "This process verifies who a user claims to be.", Answer = "What is Authentication?" },
            new() { Category = "Security", PointValue = 100, Question = "This process determines what actions an authenticated user can perform.", Answer = "What is Authorization?" },
            new() { Category = "Security", PointValue = 100, Question = "This cryptographic technique converts data into a fixed-size string.", Answer = "What is Hashing?" },
            new() { Category = "Security", PointValue = 100, Question = "This security practice requires users to provide multiple forms of verification.", Answer = "What is Multi-Factor Authentication (MFA)?" },
            new() { Category = "Security", PointValue = 100, Question = "This security feature locks an account after too many failed login attempts.", Answer = "What is Account Lockout?" },

            new() { Category = "Security", PointValue = 200, Question = "This Azure service manages identities and access for cloud applications.", Answer = "What is Microsoft Entra ID (Azure AD)?" },
            new() { Category = "Security", PointValue = 200, Question = "This protocol provides secure communication over the internet using encryption.", Answer = "What is HTTPS/TLS?" },
            new() { Category = "Security", PointValue = 200, Question = "This type of token is commonly used for API authentication and contains encoded claims.", Answer = "What is JWT (JSON Web Token)?" },
            new() { Category = "Security", PointValue = 200, Question = "This encryption type uses the same key for encryption and decryption.", Answer = "What is Symmetric Encryption?" },
            new() { Category = "Security", PointValue = 200, Question = "This security practice stores passwords as hashed values, not plain text.", Answer = "What is Password Hashing?" },

            new() { Category = "Security", PointValue = 300, Question = "This protocol uses tokens to securely authorize access to resources without sharing passwords.", Answer = "What is OAuth?" },
            new() { Category = "Security", PointValue = 300, Question = "This security practice limits user permissions to only what is necessary.", Answer = "What is the Principle of Least Privilege?" },
            new() { Category = "Security", PointValue = 300, Question = "This attack exploits trust a website has in a user's browser by sending unauthorized requests.", Answer = "What is CSRF (Cross-Site Request Forgery)?" },
            new() { Category = "Security", PointValue = 300, Question = "This encryption type uses a public key for encryption and private key for decryption.", Answer = "What is Asymmetric Encryption?" },
            new() { Category = "Security", PointValue = 300, Question = "This security layer adds authentication between services in a microservices architecture.", Answer = "What is a Service Mesh or mTLS?" },

            new() { Category = "Security", PointValue = 400, Question = "This type of attack tricks users into executing malicious scripts in their browser.", Answer = "What is Cross-Site Scripting (XSS)?" },
            new() { Category = "Security", PointValue = 400, Question = "This attack inserts malicious SQL code into application queries.", Answer = "What is SQL Injection?" },
            new() { Category = "Security", PointValue = 400, Question = "This security header helps prevent XSS attacks by controlling resource loading.", Answer = "What is Content Security Policy (CSP)?" },
            new() { Category = "Security", PointValue = 400, Question = "This attack floods a server with traffic to make it unavailable.", Answer = "What is DDoS (Distributed Denial of Service)?" },
            new() { Category = "Security", PointValue = 400, Question = "This security tool scans code for vulnerabilities before deployment.", Answer = "What is Static Application Security Testing (SAST)?" },

            new() { Category = "Security", PointValue = 500, Question = "This Azure service stores secrets, keys, and certificates securely for cloud applications.", Answer = "What is Azure Key Vault?" },
            new() { Category = "Security", PointValue = 500, Question = "This authentication method eliminates passwords using devices and biometrics.", Answer = "What is Passwordless Authentication?" },
            new() { Category = "Security", PointValue = 500, Question = "This Azure feature scans code repositories for exposed secrets and credentials.", Answer = "What is GitHub Advanced Security or Credential Scanning?" },
            new() { Category = "Security", PointValue = 500, Question = "This security framework provides guidelines for managing cybersecurity risk.", Answer = "What is NIST Cybersecurity Framework?" },
            new() { Category = "Security", PointValue = 500, Question = "This security concept assumes no user or system should be trusted by default.", Answer = "What is Zero Trust?" },

            // ==================== NETWORKING ====================
            new() { Category = "Networking", PointValue = 100, Question = "This protocol assigns IP addresses to devices on a network automatically.", Answer = "What is DHCP?" },
            new() { Category = "Networking", PointValue = 100, Question = "This protocol translates domain names to IP addresses.", Answer = "What is DNS?" },
            new() { Category = "Networking", PointValue = 100, Question = "This network device forwards data between different networks.", Answer = "What is a Router?" },
            new() { Category = "Networking", PointValue = 100, Question = "This type of IP address is not routable on the public internet.", Answer = "What is a Private IP Address?" },
            new() { Category = "Networking", PointValue = 100, Question = "This command tests connectivity between two networked devices.", Answer = "What is ping?" },

            new() { Category = "Networking", PointValue = 200, Question = "This layer of the OSI model handles routing and IP addressing.", Answer = "What is the Network Layer (Layer 3)?" },
            new() { Category = "Networking", PointValue = 200, Question = "This protocol provides reliable, ordered delivery of data over networks.", Answer = "What is TCP?" },
            new() { Category = "Networking", PointValue = 200, Question = "This protocol provides fast, connectionless data transmission.", Answer = "What is UDP?" },
            new() { Category = "Networking", PointValue = 200, Question = "This Azure service provides load balancing for incoming traffic.", Answer = "What is Azure Load Balancer?" },
            new() { Category = "Networking", PointValue = 200, Question = "This network security device filters traffic based on rules.", Answer = "What is a Firewall?" },

            new() { Category = "Networking", PointValue = 300, Question = "This Azure feature allows private connectivity between Azure services.", Answer = "What is Private Endpoint?" },
            new() { Category = "Networking", PointValue = 300, Question = "This networking concept divides a network into smaller segments.", Answer = "What is Subnetting?" },
            new() { Category = "Networking", PointValue = 300, Question = "This secure tunnel encrypts traffic between your network and Azure.", Answer = "What is VPN (Virtual Private Network)?" },
            new() { Category = "Networking", PointValue = 300, Question = "This port number is used by HTTPS.", Answer = "What is 443?" },
            new() { Category = "Networking", PointValue = 300, Question = "This Azure feature controls inbound and outbound traffic for resources.", Answer = "What is Network Security Group (NSG)?" },

            new() { Category = "Networking", PointValue = 400, Question = "This Azure service provides a dedicated private connection to Azure.", Answer = "What is Azure ExpressRoute?" },
            new() { Category = "Networking", PointValue = 400, Question = "This layer of the OSI model handles data encryption and compression.", Answer = "What is the Presentation Layer (Layer 6)?" },
            new() { Category = "Networking", PointValue = 400, Question = "This IP version uses 128-bit addresses.", Answer = "What is IPv6?" },
            new() { Category = "Networking", PointValue = 400, Question = "This Azure service provides application-level load balancing.", Answer = "What is Azure Application Gateway?" },
            new() { Category = "Networking", PointValue = 400, Question = "This technique translates private IP addresses to public ones.", Answer = "What is NAT (Network Address Translation)?" },

            new() { Category = "Networking", PointValue = 500, Question = "This Azure networking feature connects multiple VNets together.", Answer = "What is VNet Peering?" },
            new() { Category = "Networking", PointValue = 500, Question = "This networking architecture centralizes connectivity through a hub network.", Answer = "What is Hub and Spoke?" },
            new() { Category = "Networking", PointValue = 500, Question = "This Azure service provides global DNS-based traffic routing.", Answer = "What is Azure Traffic Manager?" },
            new() { Category = "Networking", PointValue = 500, Question = "This Azure service provides web application firewall and global load balancing.", Answer = "What is Azure Front Door?" },
            new() { Category = "Networking", PointValue = 500, Question = "This command displays the network path packets take to reach a destination.", Answer = "What is traceroute (or tracert)?" },

            // ==================== CLOUD ARCHITECTURE ====================
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides virtual machines and storage.", Answer = "What is IaaS (Infrastructure as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides a platform for deploying applications.", Answer = "What is PaaS (Platform as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides complete applications over the internet.", Answer = "What is SaaS (Software as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud deployment uses resources from multiple cloud providers.", Answer = "What is Multi-Cloud?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This term describes the ability to increase resources as demand grows.", Answer = "What is Scalability?" },

            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This architectural pattern breaks applications into small, independent services.", Answer = "What are Microservices?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This pattern keeps applications running even when components fail.", Answer = "What is High Availability?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This Azure feature duplicates data across regions for disaster recovery.", Answer = "What is Geo-Redundancy?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This pattern stores frequently accessed data for faster retrieval.", Answer = "What is Caching?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This Azure service provides in-memory caching.", Answer = "What is Azure Cache for Redis?" },

            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern handles failures gracefully by stopping requests to failing services.", Answer = "What is Circuit Breaker?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern separates read and write operations for better performance.", Answer = "What is CQRS (Command Query Responsibility Segregation)?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This messaging pattern decouples producers and consumers of messages.", Answer = "What is Publish-Subscribe (Pub/Sub)?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern ensures changes are tracked and can be replayed.", Answer = "What is Event Sourcing?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This Azure service provides event-driven serverless compute.", Answer = "What is Azure Event Grid?" },

            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern distributes incoming requests across multiple servers.", Answer = "What is Load Balancing?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This architecture runs code only when triggered by events.", Answer = "What is Serverless?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern limits the rate of requests to protect services.", Answer = "What is Throttling or Rate Limiting?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This Azure framework provides best practices for cloud architecture.", Answer = "What is the Azure Well-Architected Framework?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern stores data closer to users for better performance.", Answer = "What is Content Delivery Network (CDN)?" },

            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This pattern handles long-running transactions across microservices.", Answer = "What is the Saga Pattern?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This CAP theorem states you can only have two of three: Consistency, Availability, Partition tolerance.", Answer = "What is the CAP Theorem?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This pattern provides a single entry point for multiple backend services.", Answer = "What is API Gateway?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This recovery metric measures acceptable data loss in time.", Answer = "What is RPO (Recovery Point Objective)?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This recovery metric measures acceptable downtime.", Answer = "What is RTO (Recovery Time Objective)?" },

            // ==================== SOFTWARE TESTING ====================
            new() { Category = "Software Testing", PointValue = 100, Question = "This type of testing verifies individual units of code work correctly.", Answer = "What is Unit Testing?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This testing framework is commonly used for C# unit tests.", Answer = "What is xUnit, NUnit, or MSTest?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This practice writes tests before writing the actual code.", Answer = "What is TDD (Test-Driven Development)?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This keyword in testing asserts an expected value equals an actual value.", Answer = "What is Assert?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This type of testing checks if the entire system works as expected.", Answer = "What is End-to-End (E2E) Testing?" },

            new() { Category = "Software Testing", PointValue = 200, Question = "This type of testing verifies multiple components work together correctly.", Answer = "What is Integration Testing?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This testing technique replaces dependencies with fake implementations.", Answer = "What is Mocking?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This metric measures what percentage of code is executed by tests.", Answer = "What is Code Coverage?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This pattern organizes tests into Arrange, Act, Assert sections.", Answer = "What is AAA (Arrange-Act-Assert)?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This tool automates browser-based testing.", Answer = "What is Selenium or Playwright?" },

            new() { Category = "Software Testing", PointValue = 300, Question = "This type of testing checks if the application meets business requirements.", Answer = "What is Acceptance Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This testing approach tests the system without knowing internal code.", Answer = "What is Black Box Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This testing approach tests with knowledge of internal code structure.", Answer = "What is White Box Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This type of testing ensures new changes don't break existing functionality.", Answer = "What is Regression Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This C# library is commonly used for creating mock objects.", Answer = "What is Moq?" },

            new() { Category = "Software Testing", PointValue = 400, Question = "This type of testing measures system performance under load.", Answer = "What is Load Testing or Performance Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This type of testing pushes the system beyond normal limits.", Answer = "What is Stress Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This practice tests software with random or unexpected inputs.", Answer = "What is Fuzz Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This testing technique tests boundaries between valid and invalid inputs.", Answer = "What is Boundary Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This CI/CD practice automatically runs tests when code is pushed.", Answer = "What is Automated Testing or Test Automation?" },

            new() { Category = "Software Testing", PointValue = 500, Question = "This practice intentionally introduces failures to test system resilience.", Answer = "What is Chaos Engineering?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This testing pyramid suggests having more unit tests than integration tests.", Answer = "What is the Testing Pyramid?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This type of testing verifies the application is secure from attacks.", Answer = "What is Security Testing or Penetration Testing?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This practice tests the behavior of the system from the user's perspective.", Answer = "What is BDD (Behavior-Driven Development)?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This tool by Netflix randomly terminates instances to test resilience.", Answer = "What is Chaos Monkey?" },

            // ==================== DATA STRUCTURES ====================
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure stores elements in a linear order with indices.", Answer = "What is an Array?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure follows Last-In-First-Out (LIFO) principle.", Answer = "What is a Stack?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure follows First-In-First-Out (FIFO) principle.", Answer = "What is a Queue?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This C# collection dynamically resizes as elements are added.", Answer = "What is a List?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure stores unique elements only.", Answer = "What is a Set or HashSet?" },

            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure stores key-value pairs for O(1) lookups.", Answer = "What is a Hash Table or Dictionary?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure consists of nodes with pointers to next elements.", Answer = "What is a Linked List?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This tree structure maintains sorted data for efficient searching.", Answer = "What is a Binary Search Tree?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure maps keys to values using a hash function.", Answer = "What is a Hash Map?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This linked list type has pointers in both directions.", Answer = "What is a Doubly Linked List?" },

            new() { Category = "Data Structures", PointValue = 300, Question = "This balanced tree ensures O(log n) operations.", Answer = "What is an AVL Tree or Red-Black Tree?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This data structure represents relationships between nodes.", Answer = "What is a Graph?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This tree-based structure always removes the minimum (or maximum) element.", Answer = "What is a Heap or Priority Queue?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This tree structure stores strings efficiently with shared prefixes.", Answer = "What is a Trie?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This graph traversal visits all neighbors before going deeper.", Answer = "What is Breadth-First Search (BFS)?" },

            new() { Category = "Data Structures", PointValue = 400, Question = "This graph traversal goes as deep as possible before backtracking.", Answer = "What is Depth-First Search (DFS)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes constant time operations.", Answer = "What is O(1)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes linear time operations.", Answer = "What is O(n)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes logarithmic time operations.", Answer = "What is O(log n)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This data structure tracks disjoint sets efficiently.", Answer = "What is Union-Find or Disjoint Set?" },

            new() { Category = "Data Structures", PointValue = 500, Question = "This algorithm finds the shortest path in a weighted graph.", Answer = "What is Dijkstra's Algorithm?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This sorting algorithm has O(n log n) average complexity.", Answer = "What is Quick Sort or Merge Sort?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This tree structure segments ranges for efficient queries.", Answer = "What is a Segment Tree?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This data structure combines hash tables with linked lists for ordering.", Answer = "What is a LinkedHashMap?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This probabilistic data structure tests if an element might be in a set.", Answer = "What is a Bloom Filter?" },

            // ==================== OPERATING SYSTEMS ====================
            new() { Category = "Operating Systems", PointValue = 100, Question = "This component manages memory, processes, and hardware.", Answer = "What is the Kernel?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This running instance of a program has its own memory space.", Answer = "What is a Process?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This lightweight unit of execution shares memory with its process.", Answer = "What is a Thread?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This OS command lists files in a directory on Linux.", Answer = "What is ls?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This OS command lists files in a directory on Windows.", Answer = "What is dir?" },

            new() { Category = "Operating Systems", PointValue = 200, Question = "This memory management technique gives processes virtual address spaces.", Answer = "What is Virtual Memory?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Linux command changes file permissions.", Answer = "What is chmod?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Windows feature isolates processes from each other.", Answer = "What is Process Isolation?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This environment variable contains the directories to search for executables.", Answer = "What is PATH?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Linux command displays running processes.", Answer = "What is ps or top?" },

            new() { Category = "Operating Systems", PointValue = 300, Question = "This condition occurs when two processes wait for each other indefinitely.", Answer = "What is Deadlock?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This scheduling algorithm gives each process equal time slices.", Answer = "What is Round Robin?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This Linux command searches for text patterns in files.", Answer = "What is grep?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This mechanism allows processes to communicate with each other.", Answer = "What is IPC (Inter-Process Communication)?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This Linux command displays disk usage.", Answer = "What is df or du?" },

            new() { Category = "Operating Systems", PointValue = 400, Question = "This synchronization primitive allows only one thread access at a time.", Answer = "What is a Mutex?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This synchronization primitive limits concurrent access to a resource.", Answer = "What is a Semaphore?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This Linux command creates a symbolic link.", Answer = "What is ln -s?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This memory issue occurs when memory is allocated but never freed.", Answer = "What is a Memory Leak?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This Linux command shows network connections and ports.", Answer = "What is netstat or ss?" },

            new() { Category = "Operating Systems", PointValue = 500, Question = "This OS concept moves memory pages between RAM and disk.", Answer = "What is Paging or Swapping?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This condition occurs when page faults happen excessively.", Answer = "What is Thrashing?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This Linux feature isolates process trees with namespaces.", Answer = "What are Containers (cgroups/namespaces)?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This scheduling algorithm prioritizes shorter jobs first.", Answer = "What is Shortest Job First (SJF)?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This OS architecture runs services in user space rather than kernel.", Answer = "What is Microkernel?" },

            // ==================== APIS & INTEGRATION ====================
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This acronym stands for Application Programming Interface.", Answer = "What is API?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This HTTP header identifies the client making the request.", Answer = "What is User-Agent?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This API architectural style uses XML for message format.", Answer = "What is SOAP?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This API documentation format describes RESTful APIs.", Answer = "What is OpenAPI (Swagger)?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This HTTP method retrieves a resource without modifying it.", Answer = "What is GET?" },

            new() { Category = "APIs & Integration", PointValue = 200, Question = "This pattern returns a subset of results with pagination info.", Answer = "What is Paging or Pagination?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This header carries authentication credentials for APIs.", Answer = "What is Authorization?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This API versioning approach puts version in the URL path.", Answer = "What is URL Versioning?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This Azure service manages, publishes, and secures APIs.", Answer = "What is Azure API Management?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This data format is lighter than XML and commonly used in REST APIs.", Answer = "What is JSON?" },

            new() { Category = "APIs & Integration", PointValue = 300, Question = "This authentication flow is used for server-to-server API calls.", Answer = "What is Client Credentials?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This pattern limits API requests per time period.", Answer = "What is Rate Limiting?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This HTTP status code indicates too many requests.", Answer = "What is 429?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This notification mechanism calls a URL when events occur.", Answer = "What is a Webhook?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This protocol enables real-time bidirectional communication.", Answer = "What is WebSocket?" },

            new() { Category = "APIs & Integration", PointValue = 400, Question = "This query language lets clients request specific data from APIs.", Answer = "What is GraphQL?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This messaging protocol is lightweight and used in IoT.", Answer = "What is MQTT?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This Google-developed framework uses Protocol Buffers for RPC.", Answer = "What is gRPC?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This integration pattern transforms data between different formats.", Answer = "What is Data Transformation or ETL?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This Azure service connects applications and data across cloud and on-premises.", Answer = "What is Azure Logic Apps?" },

            new() { Category = "APIs & Integration", PointValue = 500, Question = "This pattern aggregates calls to multiple backend services.", Answer = "What is Backend for Frontend (BFF)?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This messaging pattern ensures messages are delivered at least once.", Answer = "What is At-Least-Once Delivery?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This Azure service provides enterprise messaging with topics and queues.", Answer = "What is Azure Service Bus?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This protocol enables secure service-to-service authentication.", Answer = "What is mTLS (Mutual TLS)?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This architectural style makes APIs self-descriptive with hypermedia links.", Answer = "What is HATEOAS?" },

            // ==================== MACHINE LEARNING BASICS ====================
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This type of ML learns from labeled training data.", Answer = "What is Supervised Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This type of ML finds patterns in unlabeled data.", Answer = "What is Unsupervised Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This ML technique predicts continuous numeric values.", Answer = "What is Regression?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This ML technique categorizes data into discrete classes.", Answer = "What is Classification?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This Azure service provides pre-built AI models and APIs.", Answer = "What is Azure Cognitive Services?" },

            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This unsupervised technique groups similar data points together.", Answer = "What is Clustering?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This metric measures how well a classification model performs.", Answer = "What is Accuracy?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This technique splits data into training and testing sets.", Answer = "What is Train-Test Split?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This Azure service provides a platform for building ML models.", Answer = "What is Azure Machine Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This problem occurs when a model performs well on training data but poorly on new data.", Answer = "What is Overfitting?" },

            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This neural network architecture is commonly used for image recognition.", Answer = "What is CNN (Convolutional Neural Network)?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This technique reduces the number of features in a dataset.", Answer = "What is Dimensionality Reduction or PCA?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This ML technique learns through trial and error with rewards.", Answer = "What is Reinforcement Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This simple algorithm classifies based on nearest neighbors.", Answer = "What is K-Nearest Neighbors (KNN)?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This tree-based algorithm is used for classification and regression.", Answer = "What is Decision Tree?" },

            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This ensemble method combines multiple decision trees.", Answer = "What is Random Forest?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This neural network architecture processes sequential data.", Answer = "What is RNN (Recurrent Neural Network) or LSTM?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This technique iteratively builds models that correct previous errors.", Answer = "What is Gradient Boosting?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This metric measures the trade-off between precision and recall.", Answer = "What is F1 Score?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This AI service provides natural language understanding capabilities.", Answer = "What is Azure Language Service or LUIS?" },

            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This architecture powers large language models like GPT.", Answer = "What is Transformer?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This technique uses pre-trained models on new tasks.", Answer = "What is Transfer Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This validation technique trains on all data except one fold.", Answer = "What is Cross-Validation?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This Azure service provides AI-powered search capabilities.", Answer = "What is Azure AI Search (Cognitive Search)?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This technique generates new data similar to training data.", Answer = "What is Data Augmentation or GANs?" },

            // ==================== MS-4010 (Security) ====================
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the primary goal of the Zero Trust security model?", Answer = "What is to never trust, always verify?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of verifying a user's identity called?", Answer = "What is Authentication?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of determining what actions an authenticated user can perform?", Answer = "What is Authorization?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the practice of requiring more than one form of verification called?", Answer = "What is Multi-Factor Authentication (MFA)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of converting data into a fixed-size string called?", Answer = "What is Hashing?" },

            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "Which Microsoft service is used for managing secrets, keys, and certificates in Azure?", Answer = "What is Azure Key Vault?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What protocol provides secure communication over the internet using encryption?", Answer = "What is HTTPS/TLS?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What type of token is commonly used for API authentication and contains encoded claims?", Answer = "What is JWT (JSON Web Token)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What encryption type uses the same key for encryption and decryption?", Answer = "What is Symmetric Encryption?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What security practice stores passwords as hashed values, not plain text?", Answer = "What is Password Hashing?" },

            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What protocol is commonly used for secure authentication and authorization in cloud applications?", Answer = "What is OAuth 2.0?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What security practice limits user permissions to only what is necessary?", Answer = "What is the Principle of Least Privilege?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What attack exploits trust a website has in a user's browser by sending unauthorized requests?", Answer = "What is CSRF (Cross-Site Request Forgery)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What encryption type uses a public key for encryption and private key for decryption?", Answer = "What is Asymmetric Encryption?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What security layer adds authentication between services in a microservices architecture?", Answer = "What is a Service Mesh or mTLS?" },

            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What is the process of identifying and reducing vulnerabilities in code before deployment called?", Answer = "What is Static Application Security Testing (SAST)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What type of attack tricks users into executing malicious scripts in their browser?", Answer = "What is Cross-Site Scripting (XSS)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What attack inserts malicious SQL code into application queries?", Answer = "What is SQL Injection?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What security header helps prevent XSS attacks by controlling resource loading?", Answer = "What is Content Security Policy (CSP)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What attack floods a server with traffic to make it unavailable?", Answer = "What is DDoS (Distributed Denial of Service)?" },

            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What Azure service provides recommendations to improve security posture?", Answer = "What is Microsoft Defender for Cloud?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What Azure service stores secrets, keys, and certificates securely for cloud applications?", Answer = "What is Azure Key Vault?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What authentication method eliminates passwords using devices and biometrics?", Answer = "What is Passwordless Authentication?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What security framework provides guidelines for managing cybersecurity risk?", Answer = "What is NIST Cybersecurity Framework?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What security concept assumes no user or system should be trusted by default?", Answer = "What is Zero Trust?" },

            // ==================== Agile & Scrum ====================
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "What’s a time-boxed Scrum iteration called?", Answer = "Sprint" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "What’s the ordered list of work called?", Answer = "Product Backlog" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "Daily 15-minute sync is called what?", Answer = "Daily Scrum (standup)" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "Who owns backlog priority?", Answer = "Product Owner" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "End-of-sprint demo is called what?", Answer = "Sprint Review" },

            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What ceremony plans sprint work + goal?", Answer = "Sprint Planning" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What meeting improves the team’s process?", Answer = "Retrospective" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What’s a 'user story' format often used?", Answer = "As a <user>, I want <goal>, so <benefit>" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What’s 'story points' measuring?", Answer = "Relative effort/complexity" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What is the Sprint Backlog?", Answer = "Items selected for the sprint + plan to deliver them" },

            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s 'Definition of Done' (DoD)?", Answer = "Shared completion criteria for work" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s a burndown chart show?", Answer = "Remaining work over time" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What does velocity represent?", Answer = "Story points completed per sprint (trend)" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s backlog refinement for?", Answer = "Clarify/split/estimate/prioritize future work" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s the biggest anti-pattern in standup?", Answer = "Turning it into a status meeting for managers" },

            new() { Category = "Agile & Scrum", PointValue = 400, Question = "Sprint Goal is at risk mid-sprint—best Scrum move?", Answer = "Re-negotiate scope with PO, protect goal" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s scope creep in Scrum usually caused by?", Answer = "Uncontrolled work added mid-sprint" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "Why keep sprints time-boxed?", Answer = "Predictable cadence + forces prioritization" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s the difference between epic and story?", Answer = "Epic is large; stories are small deliverable slices" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s 'WIP limit' trying to prevent?", Answer = "Too much work in progress, too little finishing" },

            new() { Category = "Agile & Scrum", PointValue = 500, Question = "If velocity is unstable, what should you improve first?", Answer = "Story slicing + estimation consistency" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Best metric to optimize for: 'busy' or 'done'?", Answer = "Done (delivered value)" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Team keeps missing sprint commitments—most likely root cause?", Answer = "Over-commitment + poor slicing" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "What’s the cleanest way to handle urgent new work?", Answer = "Put it in backlog; swap only with PO agreement" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Why do retrospectives fail?", Answer = "No action items, no ownership, no follow-through" },

            // ==================== PowerShell & CLI ====================
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What PowerShell cmdlet lists files and folders?", Answer = "Get-ChildItem" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What does the | pipe do?", Answer = "Sends output to the next command" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What Azure CLI command signs you in interactively?", Answer = "az login" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What cmdlet shows help for a command?", Answer = "Get-Help" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What PowerShell switch simulates actions without making them?", Answer = "-WhatIf" },

            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What symbol starts a PowerShell variable?", Answer = "$" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the Azure CLI command to list resource groups?", Answer = "az group list" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the PowerShell operator for 'not equal'?", Answer = "-ne" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the purpose of an execution policy?", Answer = "Controls script running rules" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the command to run a local script in PowerShell?", Answer = ".\\script.ps1" },

            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Which command loops over pipeline items in PowerShell?", Answer = "ForEach-Object" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Azure CLI command to set the active subscription?", Answer = "az account set" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "PowerShell cmdlet to convert an object to JSON?", Answer = "ConvertTo-Json" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What variable holds the last external program exit code?", Answer = "$LASTEXITCODE" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What does -ErrorAction Stop force?", Answer = "Converts non-terminating errors into terminating" },

            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does try { } catch { } finally { } enable?", Answer = "Structured error handling + cleanup" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s az rest used for?", Answer = "Call Azure REST APIs directly" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s the difference: Write-Host vs Write-Output?", Answer = "Host-only display vs pipeline output" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does 'idempotent' mean in deployments?", Answer = "Re-running yields same end state" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "How do you stop on any error in a script globally?", Answer = "$ErrorActionPreference = 'Stop'" },

            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Azure CLI command to deploy a Bicep/ARM template to a resource group?", Answer = "az deployment group create" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "What’s the safest way to avoid hardcoding secrets in scripts?", Answer = "Use Managed Identity + Key Vault" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "You get AuthorizationFailed in Azure CLI. Most likely fix?", Answer = "Correct subscription/role + re-login (az login)" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Best practice for automation scripts: 'fail fast' means what?", Answer = "Validate inputs early and stop on errors" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Why use tags in automation cleanup scripts?", Answer = "Filter and manage resources predictably" },
            // ==================== API MANAGEMENT ====================
            new() { Category = "API Management", PointValue = 100, Question = "This Azure service acts as a gateway in front of your APIs to publish, secure, and monitor them.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature lets you require a key to call an API without full OAuth setup.", Answer = "What is a subscription key?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM component is the entry point that receives client requests and forwards them to backends.", Answer = "What is the API Gateway?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature provides a site for developers to discover and test your APIs.", Answer = "What is the Developer Portal?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM concept bundles APIs and controls access with subscriptions.", Answer = "What is a Product?" },

            new() { Category = "API Management", PointValue = 200, Question = "This standard format is commonly imported into APIM to define endpoints, schemas, and operations.", Answer = "What is OpenAPI (Swagger)?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you enforce rules like rate limits, transformations, and auth checks without changing code.", Answer = "What are Policies?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature stores reusable configuration values like backend URLs or tokens (non-secret), referenced by policies.", Answer = "What are Named Values?" },
            new() { Category = "API Management", PointValue = 200, Question = "This security method commonly used with APIM validates a bearer token issued by an identity provider.", Answer = "What is OAuth 2.0 / JWT validation?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you group operations under a single API name and base URL.", Answer = "What is an API (in APIM)?" },

            new() { Category = "API Management", PointValue = 300, Question = "This policy is used to limit how many calls a client can make in a given time window.", Answer = "What is rate limiting (rate-limit policy)?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM capability can store responses to reduce backend load and improve latency.", Answer = "What is response caching?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM feature lets you simulate an API response without calling the real backend.", Answer = "What is a mock response?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM concept allows you to create a new API definition without breaking existing consumers.", Answer = "What are Versions (or Revisions)?" },
            new() { Category = "API Management", PointValue = 300, Question = "In APIM, policies can run in multiple stages of the request pipeline, including inbound and this stage.", Answer = "What is outbound?" },

            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy validates and enforces access based on a JWT token’s issuer, audience, and claims.", Answer = "What is validate-jwt?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy changes the request path or query before sending it to the backend.", Answer = "What is rewrite-uri?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM feature provides centralized analytics and troubleshooting for calls, failures, and latency.", Answer = "What is monitoring/diagnostics (often via Azure Monitor/Application Insights)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM scenario improves security by keeping the backend private while exposing only the gateway publicly.", Answer = "What is placing APIM in front of a private backend (gateway as the single entry point)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This is the safest design principle when retries can cause the same request to be processed twice.", Answer = "What is idempotency?" },

            new() { Category = "API Management", PointValue = 500, Question = "You need a single front door for multiple microservices with consistent auth, quotas, and logging. This service is built for that.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 500, Question = "You want a policy to run only when an error occurs (like a backend 500). This APIM policy section is used.", Answer = "What is on-error?" },
            new() { Category = "API Management", PointValue = 500, Question = "Your backend is protected by Entra ID and requires a valid token. APIM should enforce this at the gateway using this.", Answer = "What is JWT validation (OAuth 2.0)?" },
            new() { Category = "API Management", PointValue = 500, Question = "Clients are abusing an endpoint and driving costs. The fastest APIM control to apply is this.", Answer = "What is rate limiting or quotas?" },
            new() { Category = "API Management", PointValue = 500, Question = "A breaking change must be introduced safely while keeping old clients working. The APIM approach is to use this.", Answer = "What is API versioning?" },

            // ==================== LOGIC APPS ====================
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Azure service creates serverless workflows using triggers and actions.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow component starts a Logic App, such as an HTTP request or a schedule.", Answer = "What is a Trigger?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow step performs work after the trigger fires, such as calling an API or sending an email.", Answer = "What is an Action?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Logic Apps feature connects to services like Outlook, Service Bus, or SQL without writing SDK code.", Answer = "What is a Connector?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This trigger runs a workflow on a timer schedule.", Answer = "What is the Recurrence trigger?" },

            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step lets you branch logic based on true/false evaluation.", Answer = "What is a Condition?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step repeats actions for each item in a collection.", Answer = "What is a For each loop?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This action is used to call a REST endpoint directly from a workflow.", Answer = "What is the HTTP action?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This Logic Apps feature stores values you can reuse later in the workflow.", Answer = "What are Variables?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This is the benefit of Logic Apps compared to custom code for integrations.", Answer = "What is low-code workflow automation?" },

            new() { Category = "Logic Apps", PointValue = 300, Question = "This Logic Apps capability records every run so you can inspect inputs, outputs, and failures.", Answer = "What is Run History?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the recommended way for a Logic App to access Azure resources without storing passwords or keys.", Answer = "What is Managed Identity?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the integration pattern where a workflow waits for and reacts to messages.", Answer = "What is event-driven processing?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the built-in reliability behavior that tries an action again after a transient failure.", Answer = "What is retry policy?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the common use of Logic Apps in enterprise: connecting SaaS, APIs, and data without heavy custom plumbing.", Answer = "What is system integration?" },

            new() { Category = "Logic Apps", PointValue = 400, Question = "This B2B feature supports standards like X12 and EDIFACT when paired with the right resource.", Answer = "What is an Integration Account?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This design principle prevents duplicate side effects when a workflow step might run more than once.", Answer = "What is idempotency?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "You need a workflow that might take hours or days, waiting on approvals. This type of orchestration is ideal for that.", Answer = "What is a long-running workflow?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the main reason to use connectors instead of writing direct API code everywhere.", Answer = "What is standardized auth and built-in integration logic?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the feature that lets you see exactly which action failed and what data it received.", Answer = "What is run details (inputs/outputs in Run History)?" },

            new() { Category = "Logic Apps", PointValue = 500, Question = "You must integrate dozens of systems with approvals, retries, and minimal code. This Azure service is the best fit.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow triggers twice and creates duplicate records. The correct fix is to make the workflow actions do this.", Answer = "What is be idempotent?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need secure access to Key Vault from a Logic App without secrets in configuration. Use this.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow intermittently fails calling an external API. The reliability control you tune first is this.", Answer = "What is retry policy (with backoff)?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need a workflow to respond instantly to an event and kick off downstream steps without polling. The architecture style is.", Answer = "What is event-driven architecture?" },

            // ==================== DATA FACTORY ====================
            new() { Category = "Data Factory", PointValue = 100, Question = "This Azure service is used to build and orchestrate ETL/ELT pipelines.", Answer = "What is Azure Data Factory (ADF)?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF component is a group of activities that perform a data movement or transformation process.", Answer = "What is a Pipeline?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object defines the connection information to a data source like SQL or Blob Storage.", Answer = "What is a Linked Service?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object represents the structure/location of data used by activities.", Answer = "What is a Dataset?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF activity is commonly used to move data from one system to another.", Answer = "What is Copy activity?" },

            new() { Category = "Data Factory", PointValue = 200, Question = "This type of Data Flow reads data from one or more sources, applies transformations, and writes to destinations.", Answer = "What is a Mapping Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF feature runs a pipeline at scheduled intervals or in response to events.", Answer = "What is a Trigger?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF activity executes a SQL script on a database.", Answer = "What is Stored Procedure activity?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF capability allows data transformation using a visual interface without writing code.", Answer = "What is Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "In ADF, this integration runtime type is used to securely access on-premises data sources.", Answer = "What is Self-hosted Integration Runtime?" },

            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF component provides the compute and network resources used to move and transform data.", Answer = "What is an Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern loads data into the data lake or warehouse first, then transforms it as needed.", Answer = "What is ELT (Extract, Load, Transform)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern moves data, transforms it, and then loads it into the destination system.", Answer = "What is ETL (Extract, Transform, Load)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF feature allows you to define parameters to pass values into your pipelines at runtime.", Answer = "What are Pipeline Parameters?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This trigger type in ADF runs pipelines at regular intervals, like a cron job.", Answer = "What is a scheduled trigger?" },

            new() { Category = "Data Factory", PointValue = 400, Question = "If a self-hosted integration runtime is not working, the first thing to check is this.", Answer = "What is whether the integration runtime is running/stopped?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is the file format of the default retry policy in ADF activities.", Answer = "What is JSON?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is a common reason for a pipeline to fail if it was running fine before.", Answer = "What is schema changes in the source data?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "You need to ensure data quality after ingestion; best practice is to add this step.", Answer = "What is validation checks (row counts, checksums, critical query tests)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Slow queries and table scans are a common symptom of missing this.", Answer = "What are indexes?" },

            new() { Category = "Data Factory", PointValue = 500, Question = "You must load data incrementally; the key pattern is what?", Answer = "What is watermarking (incremental loads based on a high-water mark)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To ensure no duplicate data is loaded, this property must be part of your design.", Answer = "What is idempotency (dedupe/upsert strategy)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "After a migration, reports show wrong totals even though loads succeed; the most likely cause?", Answer = "What is transformation logic/mapping differences or data type/collation issues?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To prove compliance, you need to log all data access and changes—key solution components?", Answer = "What are Azure Monitor, storage logs, and activity logs?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "For sensitive data, you must reduce accidental exposure in monitoring—what principle?", Answer = "What is minimize/secure logging and protect secrets/PII?" },

            // ==================== SYNAPSE ANALYTICS ====================
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Azure service combines data warehousing and big data analytics in one workspace.", Answer = "What is Azure Synapse Analytics?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse component lets you run Apache Spark for big data processing.", Answer = "What is a Spark pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option lets you query data in a lake without provisioning a dedicated warehouse.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option is a provisioned, scalable data warehouse engine.", Answer = "What is a dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This web UI is used to author queries, pipelines, and notebooks in Synapse.", Answer = "What is Synapse Studio?" },

            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This storage is commonly used as the central data lake for Synapse.", Answer = "What is Azure Data Lake Storage Gen2?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This Synapse feature can orchestrate data movement and transformations similar to ADF.", Answer = "What are Synapse Pipelines?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This describes a warehouse pattern where raw data is loaded first and transformed inside the warehouse.", Answer = "What is ELT?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This is a key benefit of serverless SQL pool.", Answer = "What is pay-per-query on data in the lake?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This type of query object is commonly used in Synapse for reusable logic over data.", Answer = "What is a view?" },

            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the biggest difference: dedicated SQL pools are provisioned, while serverless SQL pools are this.", Answer = "What is on-demand (pay-per-query)?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This best practice improves warehouse performance by reducing data movement across compute nodes.", Answer = "What is choosing a good distribution key?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This index type is commonly used in large analytic warehouses for compression and fast scans.", Answer = "What is a clustered columnstore index?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This technique speeds up repeated analytics queries by precomputing results.", Answer = "What are materialized views?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the purpose of partitioning large tables in analytics workloads.", Answer = "What is improving query performance and manageability?" },

            new() { Category = "Synapse Analytics", PointValue = 400, Question = "You are querying a data lake and costs are high. The first optimization is usually to do this.", Answer = "What is reduce scanned data (filter columns/rows and use partitioned files)?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "A dedicated SQL pool is slow due to data shuffles. The likely cause is this.", Answer = "What is poor distribution causing data movement?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This describes controlling resource usage and concurrency for multiple workloads.", Answer = "What is workload management?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This design principle prevents duplicates when pipelines are retried or rerun.", Answer = "What is idempotency?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This is the best reason to use Spark in Synapse instead of only SQL.", Answer = "What is large-scale transformations and advanced processing?" },

            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You need a fully provisioned enterprise data warehouse with predictable performance. The Synapse choice is this.", Answer = "What is a dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You want to run ad-hoc SQL directly over files in a data lake without provisioning compute. The Synapse choice is this.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "Your warehouse queries are slow because joins cause huge data movement. The best fix is usually this.", Answer = "What is redesign distribution (choose a better distribution key)?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "Pipelines rerun and duplicate rows in the warehouse. The correct requirement for your load process is this.", Answer = "What is idempotent loading?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You need a single platform for SQL, Spark, pipelines, and lake queries under one umbrella. The service is.", Answer = "What is Azure Synapse Analytics?" },

            // ==================== COSMOS DB ====================
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Azure database is designed for globally distributed, low-latency NoSQL workloads.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB resource is like a table/collection that stores JSON documents.", Answer = "What is a Container?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB concept is the unique identifier for a stored document.", Answer = "What is an Item (document) id?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB feature spreads data across partitions for scale.", Answer = "What is partitioning?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB choice determines how reads and writes behave across replicas.", Answer = "What is a consistency level?" },

            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is the unit used to measure and bill throughput for Cosmos DB operations.", Answer = "What are Request Units (RU/s)?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is the most important design decision for scalability and performance in Cosmos DB.", Answer = "What is choosing a good partition key?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This feature automatically removes items after a specified time.", Answer = "What is Time To Live (TTL)?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This feature controls which fields are indexed and how queries perform.", Answer = "What is an indexing policy?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This Cosmos DB capability replicates data to multiple Azure regions.", Answer = "What is global distribution?" },

            new() { Category = "Cosmos DB", PointValue = 300, Question = "This feature provides an ordered stream of changes for a container that can trigger downstream processing.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is a common reason Cosmos DB gets expensive unexpectedly.", Answer = "What is high RU usage from inefficient queries or hot partitions?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is the best practice for app authentication to Cosmos DB without storing keys.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This describes the problem when too many requests hit a single partition due to a bad partition key.", Answer = "What is a hot partition?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is a typical Cosmos DB data model advantage compared to strict relational schemas.", Answer = "What is flexible (schema-less) JSON documents?" },

            new() { Category = "Cosmos DB", PointValue = 400, Question = "You need to process events from data changes in near real time. Cosmos DB provides this for free without polling.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "This consistency level offers the strongest guarantees but can reduce performance.", Answer = "What is Strong consistency?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "This design principle prevents duplicate side effects when events or retries cause repeated processing.", Answer = "What is idempotency?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "To reduce RU consumption, one of the best first steps is to do this.", Answer = "What is optimize queries and indexing?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "If your partition key has low cardinality (few values), the likely outcome is this.", Answer = "What is poor distribution and hot partitions?" },

            new() { Category = "Cosmos DB", PointValue = 500, Question = "Your app needs global low-latency reads and automatic replication across regions. This database is designed for that.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You are getting 429 throttling responses. The most direct fix is to increase this or reduce RU usage.", Answer = "What is provisioned throughput (RU/s)?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "A single partition key value receives most writes and causes throttling. The real fix is to redesign this.", Answer = "What is the partition key strategy?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You want downstream services triggered by database changes without building a polling job. Use this Cosmos feature.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "To keep retries from duplicating records, your writes should be designed to be this.", Answer = "What is idempotent?" },
            // ==================== SERVICE BUS ====================
            new() { Category = "Service Bus", PointValue = 100, Question = "This Service Bus entity is designed for point-to-point messaging between a sender and a single receiver.", Answer = "What is a Queue?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This Service Bus pattern allows one message to be delivered to multiple subscribers.", Answer = "What is a Topic and Subscription?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This is the main benefit of using Service Bus between services.", Answer = "What is decoupling (asynchronous messaging)?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This feature lets a receiver temporarily lock a message while it is being processed.", Answer = "What is Peek-Lock?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This is where messages end up when they cannot be delivered or processed successfully.", Answer = "What is the Dead-Letter Queue (DLQ)?" },

            new() { Category = "Service Bus", PointValue = 200, Question = "This queue type ensures messages are delivered in FIFO order when enabled.", Answer = "What are Sessions?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This is the Service Bus option that removes a message immediately when it is received.", Answer = "What is Receive-and-Delete?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This message property controls how long a message can live before it expires.", Answer = "What is Time-To-Live (TTL)?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This security mechanism is commonly used with Azure services to validate a bearer token.", Answer = "What is OAuth 2.0 / JWT validation?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This capability prevents the same message from being processed more than once within a time window.", Answer = "What is Duplicate Detection?" },

            new() { Category = "Service Bus", PointValue = 300, Question = "This Azure messaging feature lets you decouple application components by using queues.", Answer = "What is Azure Service Bus?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This Service Bus feature allows one message to be delivered to multiple subscriptions.", Answer = "What is Publish/Subscribe?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This property of a message indicates its position in the queue.", Answer = "What is a Sequence Number?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This feature allows you to delay the processing of a message until a specific time.", Answer = "What is Scheduled Delivery?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This capability lets you move a message to the side for later processing.", Answer = "What is Message Deferral?" },

            new() { Category = "Service Bus", PointValue = 400, Question = "This feature provides transactional support for manipulating multiple messages/activities.", Answer = "What are transactions?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This design principle ensures that processing a message multiple times does not change the outcome.", Answer = "What is idempotency?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the first thing to check when a message is not received after being sent.", Answer = "What is the Dead-Letter Queue (DLQ)?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This feature lets you restrict who can send or receive messages using Azure AD roles.", Answer = "What is RBAC (Role-Based Access Control)?"},
            new() { Category = "Service Bus", PointValue = 400, Question = "This property controls how long a message remains locked for processing.", Answer = "What is Lock Duration?" },

            new() { Category = "Service Bus", PointValue = 500, Question = "To ensure a workflow processes each message exactly once, it’s crucial to design for this.", Answer = "What is idempotency?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "For stateful processing per group with strict ordering, use this Service Bus feature.", Answer = "What are Sessions?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "High throughput and strict order are required; the best Azure messaging pattern is?", Answer = "What is a queue with session ID?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "For secure access from an Azure-hosted app without storing secrets, you should authenticate using this.", Answer = "What is Managed Identity (with RBAC)?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You need to guarantee message processing order and exact-once delivery; the main feature for that is?", Answer = "What are sessions?" },

            // ==================== EVENT GRID ====================
            new() { Category = "Event Grid", PointValue = 100, Question = "This Azure service routes events from publishers to subscribers using event-driven architecture.", Answer = "What is Event Grid?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This is what an Event Grid subscription connects: an event source to an event handler.", Answer = "What is an Event Subscription?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This type of topic is automatically created for supported Azure resources.", Answer = "What is a System Topic?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This is a common Event Grid use case: reacting when a blob is created or deleted.", Answer = "What is event-driven automation?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This term describes the component that receives and processes Event Grid events.", Answer = "What is an Event Handler?" },

            new() { Category = "Event Grid", PointValue = 200, Question = "This feature lets you route only certain events to a subscriber (for example, only 'created' events).", Answer = "What is Event Filtering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This Event Grid feature sends undeliverable events to storage for later inspection.", Answer = "What is Dead-lettering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This is a common built-in event handler target for running code on an event.", Answer = "What is Azure Functions?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This schema is commonly used by Event Grid for standardized event formats.", Answer = "What is CloudEvents?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This is the concept where publishers emit events without knowing who will receive them.", Answer = "What is loose coupling?" },

            new() { Category = "Event Grid", PointValue = 300, Question = "This notification mechanism calls a URL when events occur.", Answer = "What is a Webhook?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This subscription filter allows routing based on event type, subject, and metadata.", Answer = "What is advanced filtering?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This feature allows testing of event delivery and processing without actual events.", Answer = "What is Event Grid Simulator?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This capability ensures events are delivered at least once, even in failures.", Answer = "What is retry policy?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is a key difference between Event Grid and Service Bus: Event Grid does not require this.", Answer = "What is explicit message handling (like Complete or Ack)?" },

            new() { Category = "Event Grid", PointValue = 400, Question = "To secure API access from Event Grid, you can use this Azure feature.", Answer = "What is Managed Identity (system-assigned or user-assigned)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "When scaling out event handling, this pattern helps maintain order for related events.", Answer = "What is partitioning by key (event deduplication)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "Events should be handled idempotently because this common feature can cause duplicates.", Answer = "What is at-least-once delivery?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "To debug why an event wasn’t processed, this is the first place to check.", Answer = "What are the event delivery logs and dead-letter details?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This term describes the failure to deliver an event after all retry attempts.", Answer = "What isdead-lettering (sending to DLQ)?" },

            new() { Category = "Event Grid", PointValue = 500, Question = "For reliable event processing, the best pattern is to combine Event Grid with this.", Answer = "What is a durable queue (Service Bus or Storage Queue)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "To prevent abuse, you limit how many requests a client can make in a time window using this.", Answer = "What is rate limiting (throttling)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You need to guarantee message order and exactly-once processing; the main feature for that is?", Answer = "What are sessions (session-based processing)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You want to filter events from a specific Azure resource (like a storage account). You should use this topic type.", Answer = "What is a System Topic?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "For large systems, the cleanest approach to event routing is to keep producers unaware of consumers. This principle is called.", Answer = "What is loose coupling?" },

            // ==================== EVENT HUB ====================
            new() { Category = "Event Hub", PointValue = 100, Question = "This Azure service is designed for ingesting large volumes of streaming events like telemetry and logs.", Answer = "What is Event Hubs?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This Event Hub concept is a logical group used to enable multiple independent readers of the same stream.", Answer = "What is a Consumer Group?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This term describes how Event Hub scales: the stream is split into these.", Answer = "What are Partitions?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This setting determines how long events are kept in an Event Hub before they expire.", Answer = "What is Retention?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This is a common Event Hub use case: collecting device or application telemetry at high scale.", Answer = "What is streaming ingestion?" },

            new() { Category = "Event Hub", PointValue = 200, Question = "This feature automatically writes Event Hub data to storage for analytics or archiving.", Answer = "What is Capture?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the identifier for where a consumer is reading in a partition.", Answer = "What is an Offset?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the recommended approach so a consumer can resume after a restart.", Answer = "What is checkpointing?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This Event Hub unit is used to describe capacity in some tiers.", Answer = "What is a Throughput Unit (TU)?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the best mental model: Event Hub is like a big, durable stream log rather than a work queue.", Answer = "What is an event stream?" },

            new() { Category = "Event Hub", PointValue = 300, Question = "This is a key difference: Event Hub is for telemetry streams, while this service is for reliable command/work messaging.", Answer = "What is Service Bus?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is why partitions matter: ordering is guaranteed only within a single partition.", Answer = "What is per-partition ordering?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is the common SDK helper pattern used to process events and manage checkpoints.", Answer = "What is the Event Processor pattern?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is a typical consumer scaling rule: one active reader per partition in a consumer group.", Answer = "What is partition-based parallelism?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This best practice prevents duplicate side effects in stream processing.", Answer = "What is idempotent processing?" },

            new() { Category = "Event Hub", PointValue = 400, Question = "You need to land raw stream data for later queries without building a consumer app. This feature is designed for that.", Answer = "What is Event Hub Capture?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "Your consumer falls behind. The first Event Hub setting to check that can cause data loss is this.", Answer = "What is retention period?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "This concept describes that Event Hub consumers pull data at their own pace instead of the service pushing it.", Answer = "What is a pull-based consumption model?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "To keep related events in order, producers typically send them with the same value for this.", Answer = "What is a partition key?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "You want to read the same stream with two different apps without interfering with each other. You should use two of these.", Answer = "What are Consumer Groups?" },

            new() { Category = "Event Hub", PointValue = 500, Question = "A consumer app restarts and reprocesses old events. The missing piece it likely needs is this.", Answer = "What is checkpointing?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You need strict ordering across ALL events. Event Hub cannot guarantee that because ordering is only within these.", Answer = "What are partitions?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Telemetry processing must scale. The main scaling dimension for consumers in Event Hub is this.", Answer = "What is number of partitions?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You must avoid duplicate writes when retries or restarts occur. Your processing must be designed to be this.", Answer = "What is idempotent?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You want Event Hub data available for big analytics queries later. The built-in feature that lands data to storage is this.", Answer = "What is Capture?" },

            // ==================== AZURE MONITOR ====================
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This Azure service is the umbrella platform for collecting metrics, logs, and alerts across Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "These are numeric time-series measurements like CPU percentage or request count.", Answer = "What are metrics?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "These are detailed records/events you can query, often stored in a Log Analytics workspace.", Answer = "What are logs?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This Azure resource commonly stores and queries Azure Monitor logs with KQL.", Answer = "What is a Log Analytics workspace?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This feature notifies you when a condition is met (like high CPU or too many 5xx).", Answer = "What is an alert?" },

            new() { Category = "Azure Monitor", PointValue = 200, Question = "This query language is used to search and analyze Azure Monitor Logs.", Answer = "What is KQL (Kusto Query Language)?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This type of alert triggers based on numeric time-series data like CPU or memory.", Answer = "What is a metric alert?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This type of alert triggers based on the results of a log query.", Answer = "What is a log alert (scheduled query alert)?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This config routes platform/resource logs (like App Service logs) into Log Analytics, Storage, or Event Hubs.", Answer = "What are diagnostic settings?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This component defines who gets notified and how (email, SMS, webhook) when an alert fires.", Answer = "What is an action group?" },

            new() { Category = "Azure Monitor", PointValue = 300, Question = "This built-in log records subscription-level operations like resource creation and RBAC changes.", Answer = "What is the Activity Log?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This concept links related telemetry/events so you can trace one request across multiple services.", Answer = "What is correlation?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This practice reduces noisy or expensive telemetry by sending only a percentage of it.", Answer = "What is sampling?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This visualization surface lets you pin charts and queries for an at-a-glance view of health.", Answer = "What are Azure Monitor dashboards (or Azure dashboards)?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This is the key difference: metrics are time-series numbers, while logs are this.", Answer = "What are rich event records you query with KQL?" },

            new() { Category = "Azure Monitor", PointValue = 400, Question = "Your alert fired but nobody got notified—this is the first thing to verify.", Answer = "What is the action group configuration (and whether it’s attached to the alert rule)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "You need to alert when error rate exceeds a threshold; this data usually comes from here.", Answer = "What are application logs/telemetry in Log Analytics (or Application Insights)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "You want consistent monitoring across many subscriptions; this is a common governance approach.", Answer = "What is using Policy/initiatives to deploy diagnostic settings and alerts at scale?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "Your KQL query is slow and expensive; the best quick fix is usually to do this.", Answer = "What is narrowing the time range and filtering early (reduce scanned data)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "A classic reason metrics look fine but users complain is that you’re missing this type of signal.", Answer = "What are dependency/transaction logs (end-to-end traces)?" },

            new() { Category = "Azure Monitor", PointValue = 500, Question = "You’re drowning in alerts; the best first engineering fix is usually to do this.", Answer = "What is reduce noise with better thresholds, suppression, and actionable alerts only?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "A service goes down but metrics were green—most likely explanation?", Answer = "What is you were monitoring the wrong signal (or missing synthetic/user-facing checks)?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "To troubleshoot a production incident fastest, the best workflow is: failures → dependencies → correlated traces.", Answer = "What is starting with failures, then drilling into dependencies, then correlating logs/traces?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "You must prove who changed what and when across Azure—what log source is essential?", Answer = "What is the Activity Log (plus resource diagnostic logs)?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "Your log ingestion costs exploded overnight; the most common first lever is this.", Answer = "What is reduce ingestion with sampling/filtering and stricter diagnostic settings?" },

            // ==================== AZURE RESOURCE MANAGER ====================
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is Azure’s deployment and management layer used to create and manage resources consistently.", Answer = "What is Azure Resource Manager (ARM)?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This JSON-based approach defines infrastructure in code for repeatable deployments.", Answer = "What are ARM templates?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the logical container that holds related Azure resources for lifecycle and billing.", Answer = "What is a Resource Group?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the billing and access boundary that contains resource groups.", Answer = "What is a subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This ARM concept describes declaring what you want, not the steps to do it.", Answer = "What is declarative deployment?" },

            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element is an input value you supply at deployment time.", Answer = "What is a parameter?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element returns values after deployment (like a URL or resource ID).", Answer = "What is an output?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This feature previews what will change before actually deploying.", Answer = "What is What-If?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This is the ARM scope where you deploy most application resources.", Answer = "What is resource group scope?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This concept means deployments converge to the same end state when re-run.", Answer = "What is idempotency?" },

            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode adds/updates resources but does not delete resources not in the template.", Answer = "What is Incremental mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode can remove resources not declared in the template at that scope.", Answer = "What is Complete mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This provides organization-wide structure above subscriptions for governance.", Answer = "What are management groups?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This is the service namespace you deploy under (like Microsoft.Web or Microsoft.Storage).", Answer = "What is a resource provider?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This helps prevent config drift by making infrastructure changes repeatable and reviewable.", Answer = "What is Infrastructure as Code (IaC) in source control?" },

            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "A deployment fails with 'MissingSubscriptionRegistration'—most likely fix?", Answer = "What is registering the required resource provider for the subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You want safer multi-environment deployments; the best practice is to separate these.", Answer = "What are parameters/config per environment (dev/test/prod)?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to pass secrets to a deployment without exposing them in logs—best practice?", Answer = "What is use secure parameters and Key Vault references?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "Two deployments clash because names must be globally unique—common fix pattern?", Answer = "What is unique naming (uniqueString/guid) plus environment prefixes?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to deploy across multiple resource groups as one solution—what pattern helps?", Answer = "What is modular templates (or Bicep modules) with orchestrated deployments?" },

            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "Your template deploys fine in dev but fails in prod; most common root cause?", Answer = "What is differences in permissions/policy restrictions or missing providers/quotas?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "You must prevent unauthorized regions/SKUs at scale—what governance tool pairs well with ARM?", Answer = "What is Azure Policy (initiatives)?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "You need reproducible environments fast for many teams—what’s the big win of ARM/Bicep?", Answer = "What is repeatable, reviewable, one-command infrastructure deployments?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "A deployment succeeded but the app still can’t connect—first place to check?", Answer = "What are the dependent resources’ outputs/config (connection strings, network rules, identity perms)?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "Your 'What-If' shows deletes you didn’t expect—what mode/scope issue is likely?", Answer = "What is Complete mode or deploying at the wrong scope?" },


            // ==================== AZURE DEVOPS ====================
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps feature runs automated builds and deployments.", Answer = "What are Pipelines?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service provides Git repositories for source control.", Answer = "What are Repos?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service tracks work items like user stories, tasks, and bugs.", Answer = "What are Boards?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically building and testing on every commit.", Answer = "What is CI (Continuous Integration)?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically releasing changes through environments like dev/test/prod.", Answer = "What is CD (Continuous Delivery/Deployment)?" },

            new() { Category = "Azure DevOps", PointValue = 200, Question = "This pipeline format stores build/release steps as code in a YAML file.", Answer = "What is a YAML pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This component runs pipeline jobs (hosted or self-hosted).", Answer = "What is an agent?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This Azure DevOps concept groups steps like build, test, and deploy under a single logical unit.", Answer = "What is a stage?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This securely connects a pipeline to Azure resources for deployments.", Answer = "What is a service connection?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This stores build outputs so later stages/releases can download them.", Answer = "What are artifacts?" },

            new() { Category = "Azure DevOps", PointValue = 300, Question = "This pipeline feature allows reusing common build/deploy logic across multiple repos.", Answer = "What are templates?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "These help avoid hardcoding values and allow environment-specific configuration.", Answer = "What are variables and variable groups?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is the best practice for reviewing code changes before merging into main.", Answer = "What is a pull request (PR) workflow?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is a common way to protect the main branch in Azure Repos.", Answer = "What are branch policies (required reviewers/build validation)?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This lets you require manual approval before deploying to production.", Answer = "What are environment approvals/checks?" },

            new() { Category = "Azure DevOps", PointValue = 400, Question = "A pipeline can’t deploy to Azure due to auth errors—most likely root cause?", Answer = "What is a misconfigured service connection or missing RBAC permissions?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You need to keep secrets out of YAML; the cleanest approach is to use this.", Answer = "What is Key Vault integration (or secret variables)?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "Your pipeline is slow because dependencies download every run—best optimization?", Answer = "What is caching (NuGet/npm) and incremental build strategies?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You want different deployment behavior per environment; the standard approach is to use this.", Answer = "What are stages with environment-specific variables and approvals?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "A PR shows huge unrelated changes—most likely cause?", Answer = "What is the wrong base branch or a branch that wasn’t kept up-to-date?" },

            new() { Category = "Azure DevOps", PointValue = 500, Question = "Production deploy must be safe and reversible—best pipeline strategy?", Answer = "What is staged rollout with approvals plus blue/green or slot-based deployment?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A secret leaked in configuration. The correct response is to rotate it and move it into this service.", Answer = "What is Azure Key Vault?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need repeatable infrastructure deployments from pipelines—what tool fits best?", Answer = "What is deploying IaC (Bicep/ARM/Terraform) from a pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A release succeeds but the app is broken—what should your pipeline have prevented?", Answer = "What is missing automated tests/health checks and deployment verification gates?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need traceability from backlog item to code to deployment—what combination enables this?", Answer = "What is linking Boards work items to commits/PRs and pipeline runs/releases?" },
            // ==================== TERRAFORM ====================
            new() { Category = "Terraform", PointValue = 100, Question = "This is the open-source Infrastructure as Code tool that uses configuration files to provision resources.", Answer = "What is Terraform?" },
            new() { Category = "Terraform", PointValue = 100, Question = "Terraform configuration files are typically written in this language.", Answer = "What is HCL (HashiCorp Configuration Language)?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this block defines an infrastructure component like an Azure resource.", Answer = "What is a resource?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this defines which cloud/platform you’re talking to, like Azure.", Answer = "What is a provider?" },
            new() { Category = "Terraform", PointValue = 100, Question = "This file often holds pinned provider versions and dependency metadata.", Answer = "What is .terraform.lock.hcl?" },

            new() { Category = "Terraform", PointValue = 200, Question = "This command initializes a Terraform working directory and downloads providers.", Answer = "What is terraform init?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command previews changes Terraform will make without applying them.", Answer = "What is terraform plan?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command creates/updates infrastructure to match the configuration.", Answer = "What is terraform apply?" },
            new() { Category = "Terraform", PointValue = 200, Question = "Terraform tracks deployed resources and mappings in this file/object.", Answer = "What is state?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This Terraform feature lets you parameterize values like names, locations, and sizes.", Answer = "What are variables?" },

            new() { Category = "Terraform", PointValue = 300, Question = "This reusable packaging mechanism helps standardize Terraform infrastructure patterns.", Answer = "What is a module?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This is the best practice for storing state in teams instead of on a developer laptop.", Answer = "What is remote state (a remote backend)?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This detects changes made outside Terraform by comparing real infrastructure to state.", Answer = "What is drift detection?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command formats Terraform code into a standard style.", Answer = "What is terraform fmt?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command checks Terraform configuration for syntax and basic correctness.", Answer = "What is terraform validate?" },

            // 400
            new() { Category = "Terraform", PointValue = 400, Question = "This feature prevents multiple people/pipelines from applying changes to the same state at the same time.", Answer = "What is state locking?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This command updates state to match real infrastructure without changing resources.", Answer = "What is terraform refresh (or refresh-only planning)?" },
            new() { Category = "Terraform", PointValue = 400, Question = "You already have resources created manually; this command brings them under Terraform management.", Answer = "What is terraform import?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This capability creates multiple isolated environments using the same configuration.", Answer = "What are workspaces?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This lifecycle setting prevents accidental deletion of critical resources.", Answer = "What is prevent_destroy?" },

            new() { Category = "Terraform", PointValue = 500, Question = "In team deployments, the safest state setup combines remote state with this feature to prevent corruption.", Answer = "What is state locking?" },
            new() { Category = "Terraform", PointValue = 500, Question = "You need secrets for deployments; Terraform best practice is to avoid putting them in state by doing this.", Answer = "What is using a secret store (Key Vault) and passing secrets at runtime, not hardcoding them?" },
            new() { Category = "Terraform", PointValue = 500, Question = "A pipeline shows huge unexpected changes; the first forensic step is to do this.", Answer = "What is run terraform plan and compare state/config drift and provider/version changes?" },
            new() { Category = "Terraform", PointValue = 500, Question = "To enforce consistent standards across projects, teams typically centralize this.", Answer = "What are shared modules (plus policy/guardrails)?" },
            new() { Category = "Terraform", PointValue = 500, Question = "If you must deploy safely to prod, the best pattern is plan → review → apply using this control.", Answer = "What is gated approvals with a saved plan (and locked state)?" },

            // ==================== ANSIBLE ====================
            new() { Category = "Ansible", PointValue = 100, Question = "This is the automation tool that uses playbooks to configure systems and run tasks.", Answer = "What is Ansible?" },
            new() { Category = "Ansible", PointValue = 100, Question = "Ansible automation instructions are typically stored in this type of file.", Answer = "What is a playbook?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This defines the target machines/groups Ansible will manage.", Answer = "What is an inventory?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores non-secret configuration data.", Answer = "What is a ConfigMap?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores sensitive configuration data.", Answer = "What is a Secret?" },

            new() { Category = "Ansible", PointValue = 200, Question = "This is the Ansible command-line tool that runs playbooks.", Answer = "What is ansible-playbook?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This mechanism organizes reusable automation into a standard structure.", Answer = "What is a role?" },
            new() { Category = "Ansible", PointValue = 200, Question = "These run only when notified, often used to restart a service after a config change.", Answer = "What are handlers?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This binds configuration into a strongly-typed options class.", Answer = "What is IOptions<T>?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This syntax runs a script located in the current folder in PowerShell.", Answer = "What is .\\script.ps1?" },

            new() { Category = "Ansible", PointValue = 300, Question = "This feature gathers system information like OS, IP addresses, and disks.", Answer = "What are facts (setup)?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This lets you select which tasks to run based on tags.", Answer = "What are tags?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This mode shows what will change without applying changes.", Answer = "What is check mode?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This templating system is commonly used for generating config files in Ansible.", Answer = "What is Jinja2?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This is the best practice for keeping your playbooks readable and modular.", Answer = "What is using roles with small, focused tasks?" },

            new() { Category = "Ansible", PointValue = 400, Question = "This Ansible feature encrypts secrets like passwords inside your repo.", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 400, Question = "Your playbook keeps reporting changes even when nothing changed; the likely issue is what?", Answer = "What is a non-idempotent task/module usage?" },
            new() { Category = "Ansible", PointValue = 400, Question = "You need cloud hosts to appear automatically in inventory; the standard solution is what?", Answer = "What is dynamic inventory?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This debugging option shows detailed output about which tasks ran and why.", Answer = "What is running with increased verbosity (-v/-vv/-vvv)?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This is the clean way to avoid repeating the same steps across many playbooks.", Answer = "What are roles (and includes)?" },

            new() { Category = "Ansible", PointValue = 500, Question = "Best practice: store no plaintext secrets in Git—Ansible’s built-in answer is what?", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 500, Question = "Your automation must be safe to rerun in CI/CD; the core requirement is what?", Answer = "What is idempotency?" },
            new() { Category = "Ansible", PointValue = 500, Question = "A playbook works manually but fails in pipeline; the first suspect is usually what?", Answer = "What is environment/credentials/inventory differences in CI?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To standardize systems at scale without “snowflake servers,” your approach should be what?", Answer = "What is configuration as code with repeatable playbooks/roles?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To reduce blast radius in automation, you should design playbooks to do this.", Answer = "What is scope changes narrowly, validate first, and fail fast on errors?" },

            // ==================== CONTAINER REGISTRY ====================
            new() { Category = "Container Registry", PointValue = 100, Question = "This Azure service stores and manages Docker container images.", Answer = "What is Azure Container Registry (ACR)?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "A container image is stored in a registry under this logical grouping name.", Answer = "What is a repository?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This label identifies a specific version of an image, like v1.2.3 or latest.", Answer = "What is a tag?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action uploads a local image to a registry.", Answer = "What is a push?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action downloads an image from a registry to a machine or cluster.", Answer = "What is a pull?" },

            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command sends a built image to the registry.", Answer = "What is docker push?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command downloads an image from the registry.", Answer = "What is docker pull?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This ACR authentication helper command logs Docker into your registry.", Answer = "What is az acr login?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the main security risk of using the registry admin user.", Answer = "What is broad shared credentials (high blast radius)?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the preferred way for Azure services to pull images without storing passwords.", Answer = "What is managed identity with RBAC?" },

            new() { Category = "Container Registry", PointValue = 300, Question = "This ACR feature can build images in the cloud when you push code or run a task.", Answer = "What are ACR Tasks?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This security practice ensures images are regularly checked for known CVEs.", Answer = "What is vulnerability scanning?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This network feature keeps registry access private inside Azure networks.", Answer = "What is a private endpoint (Private Link)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This controls who can pull images using roles like AcrPull.", Answer = "What is RBAC (role-based access control)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This best practice keeps versions predictable and reduces production surprises.", Answer = "What is using immutable, versioned tags (avoid relying on latest)?" },

            new() { Category = "Container Registry", PointValue = 400, Question = "Your pods can’t pull images from ACR; the first thing to verify is this.", Answer = "What is registry permissions (AcrPull) and authentication configuration?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "You need images available close to multiple regions; the ACR capability is what?", Answer = "What is geo-replication?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "This reduces supply-chain risk by preventing unsigned/untrusted images from being used.", Answer = "What is content trust / image signing (conceptually)?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "Logging on storage is used for auditing and this.", Answer = "What is troubleshooting?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "A common place to query storage logs/metrics is Azure Monitor and this workspace.", Answer = "What is Log Analytics?" },

            new() { Category = "Container Registry", PointValue = 500, Question = "For least privilege, your AKS/Container Apps should pull images using what identity approach?", Answer = "What is managed identity (or workload identity) with AcrPull RBAC?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "A build pipeline needs secure image provenance; the best pattern is what?", Answer = "What is build in CI, scan, sign, then deploy only signed images?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "Your registry must not be publicly reachable; the key configuration is what?", Answer = "What is private endpoint plus restricted public network access?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You deployed the wrong image because a tag moved; the prevention is what?", Answer = "What is pinning by digest or immutable version tags?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You suspect compromised images; the first immediate action is what?", Answer = "What is block/rotate credentials, quarantine images, and audit pull activity/logs?" },
            // ==================== APP CONFIGURATION ====================
            new() { Category = "App Configuration", PointValue = 100, Question = "This Azure service centrally stores application settings as key-value pairs.", Answer = "What is Azure App Configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "In App Configuration, this is the basic unit used to store a setting name and value.", Answer = "What is a key-value pair?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This feature lets you logically separate settings for dev/test/prod using the same key names.", Answer = "What are labels?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This describes reading config from a centralized store instead of hardcoding values in code.", Answer = "What is externalized configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This is the most common reason to use App Configuration instead of appsettings.json only.", Answer = "What is centralized configuration management?" },

            new() { Category = "App Configuration", PointValue = 200, Question = "This App Configuration feature lets you safely turn features on/off without redeploying.", Answer = "What is Feature Management (feature flags)?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This Azure service should store secrets, not App Configuration.", Answer = "What is Azure Key Vault?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This practice helps avoid restarting apps constantly by updating config without redeploy.", Answer = "What is dynamic configuration refresh?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is the standard .NET concept used to bind configuration into strongly typed objects.", Answer = "What is IOptions<T>?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is a best practice for config keys so teams can find things quickly.", Answer = "What is a consistent naming convention (namespacing)?" },

            new() { Category = "App Configuration", PointValue = 300, Question = "This enables automatic refresh when a sentinel key changes.", Answer = "What is a refresh sentinel key?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the common pattern: settings in App Configuration, secrets in Key Vault, linked by this.", Answer = "What are Key Vault references?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This principle means your app should still run with safe defaults if config is unavailable.", Answer = "What is graceful degradation (fallback defaults)?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the best reason to use labels rather than separate apps for each environment.", Answer = "What is environment isolation with one shared configuration store?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This access approach avoids storing credentials in code when reading App Configuration from Azure.", Answer = "What is managed identity?" },

            new() { Category = "App Configuration", PointValue = 400, Question = "Your app reads old values after you updated keys—most likely missing feature?", Answer = "What is configuration refresh (or caching invalidation)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "To prevent config drift across teams, you should treat configuration like this.", Answer = "What is versioned, reviewed change management (config as code/process)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You want different feature flag behavior per environment; you should use this.", Answer = "What are labels (and environment-specific flags)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You need to limit who can change production settings; the right control is this.", Answer = "What is RBAC with least privilege?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Your app fails only in Azure but works locally; the first suspect for App Configuration is this.", Answer = "What is identity/permissions (managed identity not granted)?" },

            new() { Category = "App Configuration", PointValue = 500, Question = "You need safe releases with instant rollback without redeploying—best tool combo?", Answer = "What are feature flags plus staged rollout?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "A bad config change took prod down—best prevention pattern?", Answer = "What is validation + approvals + gradual rollout (and safe defaults)?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You must rotate secrets without redeploy; where should secrets live and how should apps reference them?", Answer = "What is Key Vault with Key Vault references from configuration?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "Your system needs consistent settings across microservices; the architecture goal is this.", Answer = "What is centralized configuration with controlled refresh and governance?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "If a feature flag system causes outages, the top design mistake is this.", Answer = "What is not planning for flag failure (no fallback, hard dependency)?" },

            // ==================== CONTAINER APPS ====================
            new() { Category = "Container Apps", PointValue = 100, Question = "This Azure service runs containerized apps without you managing Kubernetes directly.", Answer = "What is Azure Container Apps?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This feature exposes your container app to HTTP traffic.", Answer = "What is Ingress?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This Container Apps concept represents a versioned deployment of your app.", Answer = "What is a Revision?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This is the main benefit of Container Apps compared to managing your own cluster.", Answer = "What is reduced infrastructure management?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This is what Container Apps runs under the hood: container images.", Answer = "What is a Container Image?" },

            new() { Category = "Container Apps", PointValue = 200, Question = "This scaling approach increases or decreases instances based on events like queue length.", Answer = "What is event-driven autoscaling?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is commonly used to store settings for your container app without changing the image.", Answer = "What are environment variables (app settings)?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the recommended way for a container app to access Azure resources without secrets.", Answer = "What is Managed Identity?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This describes how Container Apps can scale down when idle to reduce cost.", Answer = "What is scale to zero?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the safest way to roll out a new version while keeping the old one available.", Answer = "What is traffic splitting between revisions?" },

            new() { Category = "Container Apps", PointValue = 300, Question = "This feature provides service-to-service helpers like retries and service discovery when enabled.", Answer = "What is Dapr integration?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This type of ingress keeps the app private and reachable only within the environment/network.", Answer = "What is internal ingress?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is a best practice for containerized apps: keep state out of the container and store it here.", Answer = "What is an external data store (like a database or storage)?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is a common reason a container app fails to start: the app listens on the wrong port.", Answer = "What is incorrect port configuration?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is the purpose of health probes in containerized hosting.", Answer = "What is detecting unhealthy instances?" },

            new() { Category = "Container Apps", PointValue = 400, Question = "You need to run background processing triggered by a queue without managing servers. This pairing is common.", Answer = "What are Container Apps plus Service Bus (or Queue)?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "Your container app works locally but fails in Azure. The first thing to confirm is that it binds to this address.", Answer = "What is 0.0.0.0?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This deployment technique avoids breaking production by testing the new revision first.", Answer = "What is deploying a new revision with zero traffic initially?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This is the key benefit of 'scale to zero' for spiky workloads.", Answer = "What is cost efficiency?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This is the main security win of Managed Identity compared to connection strings with passwords.", Answer = "What is eliminating stored secrets?" },

            new() { Category = "Container Apps", PointValue = 500, Question = "Your API must stay responsive during deployments. The most important Container Apps feature to use is this.", Answer = "What is revision-based deployment with traffic splitting?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "You want your app private to a network and not public on the internet. The correct choice is this.", Answer = "What is internal ingress?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your processing must handle duplicates because event-driven scaling can cause repeats. The required design principle is this.", Answer = "What is idempotency?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "You need cross-service reliability features like retries and pub/sub without writing tons of plumbing. Container Apps can use this.", Answer = "What is Dapr?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "A container app keeps crashing on startup. The fastest first diagnostic to check is this.", Answer = "What are the container logs?" },

            // ==================== POWERSHELL & CLI ====================
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell cmdlet lists files and folders in a directory.", Answer = "Get-ChildItem" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This symbol starts a variable name in PowerShell.", Answer = "$" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This operator sends the output of one command into another command.", Answer = "Pipe (|)" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This Azure CLI command signs you in interactively.", Answer = "az login" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This cmdlet shows help for a command.", Answer = "Get-Help" },

            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the Azure CLI command to list resource groups?", Answer = "az group list" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the PowerShell operator for 'not equal'?", Answer = "-ne" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the purpose of an execution policy?", Answer = "Controls script running rules" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This cmdlet runs a script file in the current directory.", Answer = ".\\script.ps1" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What PowerShell switch simulates actions without making them?", Answer = "-WhatIf" },

            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Azure CLI command to set the active subscription?", Answer = "az account set" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "PowerShell cmdlet to convert an object to JSON?", Answer = "ConvertTo-Json" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What variable holds the last external program exit code?", Answer = "$LASTEXITCODE" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What does -ErrorAction Stop force?", Answer = "Converts non-terminating errors into terminating" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Which command loops over pipeline items in PowerShell?", Answer = "ForEach-Object" },

            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does try { } catch { } finally { } enable?", Answer = "Structured error handling + cleanup" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s az rest used for?", Answer = "Call Azure REST APIs directly" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s the difference: Write-Host vs Write-Output?", Answer = "Host-only display vs pipeline output" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does 'idempotent' mean in deployments?", Answer = "Re-running yields same end state" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "How do you stop on any error in a script globally?", Answer = "$ErrorActionPreference = 'Stop'" },

            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Azure CLI command to deploy a Bicep/ARM template to a resource group?", Answer = "az deployment group create" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "What’s the safest way to avoid hardcoding secrets in scripts?", Answer = "Use Managed Identity + Key Vault" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "You get AuthorizationFailed in Azure CLI. Most likely fix?", Answer = "Correct subscription/role + re-login (az login)" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Best practice for automation scripts: 'fail fast' means what?", Answer = "Validate inputs early and stop on errors" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Why use tags in automation cleanup scripts?", Answer = "Filter and manage resources predictably" },
            // ==================== API MANAGEMENT ====================
            new() { Category = "API Management", PointValue = 100, Question = "This Azure service acts as a gateway in front of your APIs to publish, secure, and monitor them.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature lets you require a key to call an API without full OAuth setup.", Answer = "What is a subscription key?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM component is the entry point that receives client requests and forwards them to backends.", Answer = "What is the API Gateway?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature provides a site for developers to discover and test your APIs.", Answer = "What is the Developer Portal?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM concept bundles APIs and controls access with subscriptions.", Answer = "What is a Product?" },

            new() { Category = "API Management", PointValue = 200, Question = "This standard format is commonly imported into APIM to define endpoints, schemas, and operations.", Answer = "What is OpenAPI (Swagger)?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you enforce rules like rate limits, transformations, and auth checks without changing code.", Answer = "What are Policies?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature stores reusable configuration values like backend URLs or tokens (non-secret), referenced by policies.", Answer = "What are Named Values?" },
            new() { Category = "API Management", PointValue = 200, Question = "This security method commonly used with APIM validates a bearer token issued by an identity provider.", Answer = "What is OAuth 2.0 / JWT validation?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you group operations under a single API name and base URL.", Answer = "What is an API (in APIM)?" },

            new() { Category = "API Management", PointValue = 300, Question = "This policy is used to limit how many calls a client can make in a given time window.", Answer = "What is rate limiting (rate-limit policy)?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM capability can store responses to reduce backend load and improve latency.", Answer = "What is response caching?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM feature lets you simulate an API response without calling the real backend.", Answer = "What is a mock response?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM concept allows you to create a new API definition without breaking existing consumers.", Answer = "What are Versions (or Revisions)?" },
            new() { Category = "API Management", PointValue = 300, Question = "In APIM, policies can run in multiple stages of the request pipeline, including inbound and this stage.", Answer = "What is outbound?" },

            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy validates and enforces access based on a JWT token’s issuer, audience, and claims.", Answer = "What is validate-jwt?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy changes the request path or query before sending it to the backend.", Answer = "What is rewrite-uri?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM feature provides centralized analytics and troubleshooting for calls, failures, and latency.", Answer = "What is monitoring/diagnostics (often via Azure Monitor/Application Insights)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM scenario improves security by keeping the backend private while exposing only the gateway publicly.", Answer = "What is placing APIM in front of a private backend (gateway as the single entry point)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This is the safest design principle when retries can cause the same request to be processed twice.", Answer = "What is idempotency?" },

            new() { Category = "API Management", PointValue = 500, Question = "You need a single front door for multiple microservices with consistent auth, quotas, and logging. This service is built for that.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 500, Question = "You want a policy to run only when an error occurs (like a backend 500). This APIM policy section is used.", Answer = "What is on-error?" },
            new() { Category = "API Management", PointValue = 500, Question = "Your backend is protected by Entra ID and requires a valid token. APIM should enforce this at the gateway using this.", Answer = "What is JWT validation (OAuth 2.0)?" },
            new() { Category = "API Management", PointValue = 500, Question = "Clients are abusing an endpoint and driving costs. The fastest APIM control to apply is this.", Answer = "What is rate limiting or quotas?" },
            new() { Category = "API Management", PointValue = 500, Question = "A breaking change must be introduced safely while keeping old clients working. The APIM approach is to use this.", Answer = "What is API versioning?" },

            // ==================== LOGIC APPS ====================
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Azure service creates serverless workflows using triggers and actions.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow component starts a Logic App, such as an HTTP request or a schedule.", Answer = "What is a Trigger?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow step performs work after the trigger fires, such as calling an API or sending an email.", Answer = "What is an Action?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Logic Apps feature connects to services like Outlook, Service Bus, or SQL without writing SDK code.", Answer = "What is a Connector?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This trigger runs a workflow on a timer schedule.", Answer = "What is the Recurrence trigger?" },

            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step lets you branch logic based on true/false evaluation.", Answer = "What is a Condition?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step repeats actions for each item in a collection.", Answer = "What is a For each loop?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This action is used to call a REST endpoint directly from a workflow.", Answer = "What is the HTTP action?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This Logic Apps feature stores values you can reuse later in the workflow.", Answer = "What are Variables?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This is the benefit of Logic Apps compared to custom code for integrations.", Answer = "What is low-code workflow automation?" },

            new() { Category = "Logic Apps", PointValue = 300, Question = "This Logic Apps capability records every run so you can inspect inputs, outputs, and failures.", Answer = "What is Run History?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the recommended way for a Logic App to access Azure resources without storing passwords or keys.", Answer = "What is Managed Identity?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the integration pattern where a workflow waits for and reacts to messages.", Answer = "What is event-driven processing?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the built-in reliability behavior that tries an action again after a transient failure.", Answer = "What is retry policy?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the common use of Logic Apps in enterprise: connecting SaaS, APIs, and data without heavy custom plumbing.", Answer = "What is system integration?" },

            new() { Category = "Logic Apps", PointValue = 400, Question = "This B2B feature supports standards like X12 and EDIFACT when paired with the right resource.", Answer = "What is an Integration Account?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This design principle prevents duplicate side effects when a workflow step might run more than once.", Answer = "What is idempotency?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "You need a workflow that might take hours or days, waiting on approvals. This type of orchestration is ideal for that.", Answer = "What is a long-running workflow?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the main reason to use connectors instead of writing direct API code everywhere.", Answer = "What is standardized auth and built-in integration logic?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the feature that lets you see exactly which action failed and what data it received.", Answer = "What is run details (inputs/outputs in Run History)?" },

            new() { Category = "Logic Apps", PointValue = 500, Question = "You must integrate dozens of systems with approvals, retries, and minimal code. This Azure service is the best fit.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow triggers twice and creates duplicate records. The correct fix is to make the workflow actions do this.", Answer = "What is be idempotent?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need secure access to Key Vault from a Logic App without secrets in configuration. Use this.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow intermittently fails calling an external API. The reliability control you tune first is this.", Answer = "What is retry policy (with backoff)?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need a workflow to respond instantly to an event and kick off downstream steps without polling. The architecture style is.", Answer = "What is event-driven architecture?" },

            // ==================== DATA FACTORY ====================
            new() { Category = "Data Factory", PointValue = 100, Question = "This Azure service is used to build and orchestrate ETL/ELT pipelines.", Answer = "What is Azure Data Factory (ADF)?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF component is a group of activities that perform a data movement or transformation process.", Answer = "What is a Pipeline?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object defines the connection information to a data source like SQL or Blob Storage.", Answer = "What is a Linked Service?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object represents the structure/location of data used by activities.", Answer = "What is a Dataset?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF activity is commonly used to move data from one system to another.", Answer = "What is Copy activity?" },

            new() { Category = "Data Factory", PointValue = 200, Question = "This type of Data Flow reads data from one or more sources, applies transformations, and writes to destinations.", Answer = "What is a Mapping Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF feature runs a pipeline at scheduled intervals or in response to events.", Answer = "What is a Trigger?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF activity executes a SQL script on a database.", Answer = "What is Stored Procedure activity?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF capability allows data transformation using a visual interface without writing code.", Answer = "What is Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "In ADF, this integration runtime type is used to securely access on-premises data sources.", Answer = "What is Self-hosted Integration Runtime?" },

            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF component provides the compute and network resources used to move and transform data.", Answer = "What is an Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern loads data into the data lake or warehouse first, then transforms it as needed.", Answer = "What is ELT (Extract, Load, Transform)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern moves data, transforms it, and then loads it into the destination system.", Answer = "What is ETL (Extract, Transform, Load)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF feature allows you to define parameters to pass values into your pipelines at runtime.", Answer = "What are Pipeline Parameters?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This trigger type in ADF runs pipelines at regular intervals, like a cron job.", Answer = "What is a scheduled trigger?" },

            new() { Category = "Data Factory", PointValue = 400, Question = "If a self-hosted integration runtime is not working, the first thing to check is this.", Answer = "What is whether the integration runtime is running/stopped?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is the file format of the default retry policy in ADF activities.", Answer = "What is JSON?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is a common reason for a pipeline to fail if it was running fine before.", Answer = "What is schema changes in the source data?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "You need to ensure data quality after ingestion; best practice is to add this step.", Answer = "What is validation checks (row counts, checksums, critical query tests)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Slow queries and table scans are a common symptom of missing this.", Answer = "What are indexes?" },

            new() { Category = "Data Factory", PointValue = 500, Question = "You must load data incrementally; the key pattern is what?", Answer = "What is watermarking (incremental loads based on a high-water mark)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To ensure no duplicate data is loaded, this property must be part of your design.", Answer = "What is idempotency (dedupe/upsert strategy)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "After a migration, reports show wrong totals even though loads succeed; the most likely cause?", Answer = "What is transformation logic/mapping differences or data type/collation issues?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To prove compliance, you need to log all data access and changes—key solution components?", Answer = "What are Azure Monitor, storage logs, and activity logs?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "For sensitive data, you must reduce accidental exposure in monitoring—what principle?", Answer = "What is minimize/secure logging and protect secrets/PII?" },

            // ==================== SYNAPSE ANALYTICS ====================
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Azure service combines data warehousing and big data analytics in one workspace.", Answer = "What is Azure Synapse Analytics?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse component lets you run Apache Spark for big data processing.", Answer = "What is a Spark pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option lets you query data in a lake without provisioning a dedicated warehouse.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option is a provisioned, scalable data warehouse engine.", Answer = "What is a dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This web UI is used to author queries, pipelines, and notebooks in Synapse.", Answer = "What is Synapse Studio?" },

            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This storage is commonly used as the central data lake for Synapse.", Answer = "What is Azure Data Lake Storage Gen2?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This Synapse feature can orchestrate data movement and transformations similar to ADF.", Answer = "What are Synapse Pipelines?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This describes a warehouse pattern where raw data is loaded first and transformed inside the warehouse.", Answer = "What is ELT?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This is a key benefit of serverless SQL pool.", Answer = "What is pay-per-query on data in the lake?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This type of query object is commonly used in Synapse for reusable logic over data.", Answer = "What is a view?" },

            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the biggest difference: OLTP is transactions; Synapse is commonly used for this.", Answer = "What is OLAP (analytics)?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "To reduce cost when querying huge files in a lake, you should use this columnar format.", Answer = "What is Parquet?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This pattern describes storing raw, cleaned, and curated data layers.", Answer = "What is medallion architecture (bronze/silver/gold)?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is a common way to secure Synapse access using Azure identities.", Answer = "What is Microsoft Entra ID (Azure AD) with RBAC?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the first troubleshooting step when queries are slow or expensive.", Answer = "What is check the query plan and data layout/partitioning?" },

            new() { Category = "Synapse Analytics", PointValue = 400, Question = "Your dedicated SQL pool performance is poor; the top design lever is often this.", Answer = "What is proper distribution/partitioning and indexing strategy?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "A common reason serverless SQL queries are slow is this file problem.", Answer = "What is too many small files (or inefficient formats)?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "You need to keep analytics traffic off the public internet; what feature matters?", Answer = "What is private endpoints (Private Link)?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "After ingestion, your dashboards show wrong numbers; most likely cause?", Answer = "What is transformation/aggregation logic issues or join duplication?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "You need repeatable enterprise deployments of Synapse assets; best practice?", Answer = "What is IaC + CI/CD for Synapse workspace artifacts?" },

            new() { Category = "Synapse Analytics", PointValue = 500, Question = "To keep costs down while retaining capability, you should use dedicated compute only for this.", Answer = "What is predictable heavy workloads; otherwise use serverless/on-demand?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "Your lakehouse is unreliable due to messy data; the most important discipline is what?", Answer = "What is strong data governance and quality validation (schemas, contracts)?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You need near-real-time analytics from event streams; best ingestion pairing with Synapse?", Answer = "What is Event Hubs/Stream processing feeding the lake/warehouse?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You must support audits on who queried sensitive data and when—what must be enabled?", Answer = "What is auditing and activity logs (admin monitoring)?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "High 5xx errors right after a slot swap often point to misconfigured this.", Answer = "What are slot-sticky settings (slot settings)?" },

            // ==================== MICROSOFT POWER PLATFORM ====================
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool is used to build low-code business apps.", Answer = "What is Power Apps?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool automates workflows between services.", Answer = "What is Power Automate?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool is used for dashboards and business intelligence reporting.", Answer = "What is Power BI?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform data service stores tables/rows used by apps and flows.", Answer = "What is Dataverse?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "These are prebuilt integrations that let Power Platform talk to other services.", Answer = "What are connectors?" },

            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This is the secure boundary where Power Platform apps, flows, and data live.", Answer = "What is an environment?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This packaging method groups Power Platform components for deployment and reuse.", Answer = "What is a solution?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This Power Apps type builds responsive apps that run in browsers and mobile devices.", Answer = "What is a canvas app?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This Power Apps type is model-driven and uses Dataverse as the data layer.", Answer = "What is a model-driven app?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This feature restricts which connectors can be used together to prevent data leakage.", Answer = "What are DLP (Data Loss Prevention) policies?" },

            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is used to connect on-premises data sources to Power Platform securely.", Answer = "What is an on-premises data gateway?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is the event that starts a Power Automate flow.", Answer = "What is a trigger?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is a best practice for deploying changes: build inside a solution and move between these.", Answer = "What are dev/test/prod environments?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This controls who can create, run, or share Power Platform resources.", Answer = "What is role-based access control (security roles)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is the Power Platform approach to storing reusable business logic and rules for Dataverse tables.", Answer = "What are business rules (and Dataverse logic)?" },

            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "You need a flow to call an API securely without storing a password in the flow—best approach?", Answer = "What is using managed identity or secure connections (plus Key Vault where applicable)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "A flow keeps running twice for the same event—most likely explanation?", Answer = "What is trigger retries/at-least-once delivery requiring idempotent design?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "Your Power BI dataset refresh fails only in the service, not locally—common root cause?", Answer = "What is gateway/credentials/network access not configured correctly?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "You want controlled releases and rollback for Power Platform components—best practice?", Answer = "What is solutions with versioning and environment-based deployment?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "An org needs consistent connector restrictions across environments—what control enforces that?", Answer = "What are DLP policies scoped to environments?" },

            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A low-code app must meet enterprise governance: the key success requirement is what?", Answer = "What is environment strategy plus security roles plus DLP and auditing?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A flow fails intermittently calling a third-party API—what reliability pattern should you add?", Answer = "What is retries with exponential backoff and circuit-breaker style handling?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A solution import succeeds but the app behaves wrong—most likely cause?", Answer = "What is missing environment variables/connection references or mismatched permissions?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "You must prove who changed an app/flow and when—what capability do you rely on?", Answer = "What is auditing and activity logs (admin monitoring)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "Power Platform is adopted fast and chaos follows—what’s the cleanest first fix?", Answer = "What is establish governance standards before scaling adoption?" },

            // ==================== DATA ANALYTICS ====================
            new() { Category = "Data Analytics", PointValue = 100, Question = "This type of analytics summarizes what happened in the past.", Answer = "What is descriptive analytics?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "A metric used to measure business success like conversion rate is called a what?", Answer = "What is a KPI (Key Performance Indicator)?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This is a visual collection of charts that tells a data story at a glance.", Answer = "What is a dashboard?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This is raw facts collected from systems before cleaning or modeling.", Answer = "What is raw data?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This process finds patterns and insights from data to support decisions.", Answer = "What is data analysis?" },

            new() { Category = "Data Analytics", PointValue = 200, Question = "This type of analytics predicts what is likely to happen next.", Answer = "What is predictive analytics?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This type of analytics recommends actions to take based on data.", Answer = "What is prescriptive analytics?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This is the process of extracting, transforming, and loading data.", Answer = "What is ETL?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This is the process of extracting and loading first, then transforming in the destination.", Answer = "What is ELT?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "A structured store optimized for reporting and analysis is called a what?", Answer = "What is a data warehouse?" },

            new() { Category = "Data Analytics", PointValue = 300, Question = "A storage system for large volumes of raw and semi-structured data is called a what?", Answer = "What is a data lake?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This model organizes analytics data into fact tables and dimension tables.", Answer = "What is a star schema?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This problem happens when the same metric is calculated differently by different teams.", Answer = "What is metric inconsistency (definition drift)?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This is a common technique to make large datasets faster to query.", Answer = "What is partitioning?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This is the practice of validating accuracy, completeness, and consistency of data.", Answer = "What is data quality management?" },

            new() { Category = "Data Analytics", PointValue = 400, Question = "A dashboard number is wrong even though pipelines succeeded—most likely cause?", Answer = "What is transformation/join logic errors or duplicated records?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "A report is slow at scale; the most common first fix is to reduce this.", Answer = "What is the amount of data scanned (filter early, aggregate, use partitions)?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "If you need trusted definitions for KPIs across the org, you should create this.", Answer = "What is a semantic layer (shared model)?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "This governance practice tracks where data comes from and how it changes.", Answer = "What is data lineage?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "You must control who can see sensitive columns—what capability is required?", Answer = "What is access control and data classification (least privilege)?" },

            new() { Category = "Data Analytics", PointValue = 500, Question = "Your analytics pipeline produces different results each run—what’s the likely root cause?", Answer = "What is non-deterministic transformations or missing versioned source snapshots?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "Leadership wants real-time insights; the correct architecture shift is toward this.", Answer = "What is streaming analytics (event-driven ingestion)?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "A model performs great in test but fails in production due to changing patterns. The most likely cause is this.", Answer = "What is data drift or concept drift?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "The fastest way to destroy trust in analytics is this failure mode.", Answer = "What is inconsistent metric definitions and ungoverned changes?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "You need audit-ready reporting; the key operational requirement is what?", Answer = "What is governed change control plus immutable logs/auditing?" },

            // ==================== COSMOS DB ====================
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Azure database service is globally distributed and supports NoSQL data models.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "Cosmos DB commonly stores data as JSON documents in this API model.", Answer = "What is the Core (SQL) API?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "In Cosmos DB, this is the top-level container for one or more containers/collections.", Answer = "What is a database?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "In Cosmos DB, this is where documents/items live (similar to a collection).", Answer = "What is a container?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "Cosmos DB performance is billed using this unit.", Answer = "What are RU/s (Request Units)?" },

            new() { Category = "Cosmos DB", PointValue = 200, Question = "This key determines how data is distributed across partitions.", Answer = "What is a partition key?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This describes splitting data across multiple partitions for scale.", Answer = "What is partitioning?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This feature automatically indexes items so many queries are fast by default.", Answer = "What is automatic indexing?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is the main benefit of Cosmos DB for global apps.", Answer = "What is multi-region distribution?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is a time-based feature that automatically deletes items after a period.", Answer = "What is TTL (time to live)?" },

            new() { Category = "Cosmos DB", PointValue = 300, Question = "This feature streams inserts/updates from a container for event-driven processing.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This design choice most strongly impacts scale, cost, and query performance.", Answer = "What is choosing the correct partition key?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "Cosmos DB consistency can be tuned; the strongest level is this.", Answer = "What is strong consistency?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "A balanced, commonly used consistency level in Cosmos DB is this.", Answer = "What is session consistency?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is the main reason ‘hot partitions’ cause throttling.", Answer = "What is uneven partition key distribution concentrating RU usage?" },

            new() { Category = "Cosmos DB", PointValue = 400, Question = "You get 429 errors from Cosmos DB—what does that usually mean?", Answer = "What is request throttling due to insufficient RU/s?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "Your queries are expensive; the first fix is usually to do this.", Answer = "What is optimize queries and indexing policy (and reduce cross-partition queries)?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "A global app needs low latency reads near users; the best capability is this.", Answer = "What is multi-region replication with local reads?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "You must access Cosmos DB from Azure without secrets in code—best pattern?", Answer = "What is managed identity (where supported) or least-privilege credential handling?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "A partition key mistake is the hardest to fix later because it impacts this.", Answer = "What is physical data distribution and scalability?" },

            new() { Category = "Cosmos DB", PointValue = 500, Question = "Your RU cost exploded after a feature release—most likely root cause?", Answer = "What is more queries per request or inefficient cross-partition queries/indexing changes?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You must guarantee ‘exactly-once effect’ when reacting to Change Feed—key design rule?", Answer = "What is idempotent processing with deduplication/state tracking?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "A query is correct but too slow at scale; what’s the most common high-impact fix?", Answer = "What is partitioning, columnar formats, and reducing scanned data (filter early)?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "To secure API access from Event Grid, you can use this Azure feature.", Answer = "What is Managed Identity (system-assigned or user-assigned)?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "For compliance and cost control, this setting determines how long events are stored.", Answer = "What is retention period?" },

            // ==================== LOGGING & MONITORING ====================
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Azure service collects and analyzes log data from Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Azure feature provides a centralized place to view and query logs from multiple resources.", Answer = "What is Log Analytics?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This command line tool allows you to query Azure resources using a SQL-like syntax.", Answer = "What is Azure CLI?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Kusto query language clause filters results based on a condition.", Answer = "What is where?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This is the default retention period for logs in Log Analytics.", Answer = "What is 30 days?" },

            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This Azure service provides real-time monitoring and alerting for Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This metric measures the time taken to process requests.", Answer = "What is response time?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This setting in Azure Monitor specifies the action taken when a condition is met.", Answer = "What is an alert rule?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This Azure feature helps you visualize and analyze metrics over time.", Answer = "What are metric charts?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This query language is used to analyze log data in Azure Monitor.", Answer = "What is KQL (Kusto Query Language)?" },

            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This Log Analytics query calculates the average of a numerical field.", Answer = "What is avg()?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This query summarizes data into bins based on time intervals.", Answer = "What is bin()?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This table in Log Analytics contains every request to your Azure resources.", Answer = "What is the Request table?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This command sends custom log data to Azure Monitor.", Answer = "What is the Azure Monitor HTTP Data Collector API?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This alert type in Azure Monitor triggers based on log query results.", Answer = "What is a log alert?" },

            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This Log Analytics function joins two tables based on a common key.", Answer = "What is join?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This feature helps you analyze the impact of changes by comparing metrics before and after.", Answer = "What is metric baseline?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "You need to keep certain logs for 5 years for compliance; this is how you achieve that.", Answer = "What is configuring log retention policies?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This KQL keyword specifies that a field must exist in the results.", Answer = "What is has?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This feature enables real-time updating of dashboards and views in Azure Monitor.", Answer = "What is live metrics stream?" },

            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "A sudden outage; first, ensure this critical monitoring signal is intact.", Answer = "What is alerting on failure rates and response times?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "To troubleshoot high latency, correlate these signals: request logs, dependencies, and this.", Answer = "What is performance (duration) metrics?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "Logs show an app is slow, but metrics are fine—most likely issue?", Answer = "What is a dependency (DB, API) latency causing overall slowness?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "Your first day in a new Azure environment; this is the best first health check.", Answer = "What is review the Azure Activity Log for recent changes and errors?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "To improve query performance and reduce costs, always filter on this in Log Analytics.", Answer = "What is time (where timestamp >= ...)?" },


            // ==================== AZURE RESOURCE MANAGER ====================
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is Azure’s deployment and management layer used to create and manage resources consistently.", Answer = "What is Azure Resource Manager (ARM)?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This JSON-based approach defines infrastructure in code for repeatable deployments.", Answer = "What are ARM templates?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the logical container that holds related Azure resources for lifecycle and billing.", Answer = "What is a Resource Group?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the billing and access boundary that contains resource groups.", Answer = "What is a subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This ARM concept describes declaring what you want, not the steps to do it.", Answer = "What is declarative deployment?" },

            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element is an input value you supply at deployment time.", Answer = "What is a parameter?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element returns values after deployment (like a URL or resource ID).", Answer = "What is an output?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This feature previews what will change before actually deploying.", Answer = "What is What-If?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This is the ARM scope where you deploy most application resources.", Answer = "What is resource group scope?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This concept means deployments converge to the same end state when re-run.", Answer = "What is idempotency?" },

            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode adds/updates resources but does not delete resources not in the template.", Answer = "What is Incremental mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode can remove resources not declared in the template at that scope.", Answer = "What is Complete mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This provides organization-wide structure above subscriptions for governance.", Answer = "What are management groups?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This is the service namespace you deploy under (like Microsoft.Web or Microsoft.Storage).", Answer = "What is a resource provider?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This helps prevent config drift by making infrastructure changes repeatable and reviewable.", Answer = "What is Infrastructure as Code (IaC) in source control?" },

            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "A deployment fails with 'MissingSubscriptionRegistration'—most likely fix?", Answer = "What is registering the required resource provider for the subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You want safer multi-environment deployments; the best practice is to separate these.", Answer = "What are parameters/config per environment (dev/test/prod)?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to pass secrets to a deployment without exposing them in logs—best practice?", Answer = "What is use secure parameters and Key Vault references?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "Two deployments clash because names must be globally unique—common fix pattern?", Answer = "What is unique naming (uniqueString/guid) plus environment prefixes?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to deploy across multiple resource groups as one solution—what pattern helps?", Answer = "What is modular templates (or Bicep modules) with orchestrated deployments?" },

            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "In team deployments, the safest state setup combines remote state with this feature to prevent corruption.", Answer = "What is state locking?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "You need secrets for deployments; Terraform best practice is to avoid putting them in state by doing this.", Answer = "What is using a secret store (Key Vault) and passing secrets at runtime, not hardcoding them?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "A pipeline shows huge unexpected changes; the first forensic step is to do this.", Answer = "What is run terraform plan and compare state/config drift and provider/version changes?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "To enforce consistent standards across projects, teams typically centralize this.", Answer = "What are shared modules (plus policy/guardrails)?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "If you must deploy safely to prod, the best pattern is plan → review → apply using this control.", Answer = "What is gated approvals with a saved plan (and locked state)?" },

            // ==================== AZURE DEVOPS ====================
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps feature runs automated builds and deployments.", Answer = "What are Pipelines?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service provides Git repositories for source control.", Answer = "What are Repos?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service tracks work items like user stories, tasks, and bugs.", Answer = "What are Boards?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically building and testing on every commit.", Answer = "What is CI (Continuous Integration)?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically releasing changes through environments like dev/test/prod.", Answer = "What is CD (Continuous Delivery/Deployment)?" },

            new() { Category = "Azure DevOps", PointValue = 200, Question = "This pipeline format stores build/release steps as code in a YAML file.", Answer = "What is a YAML pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This component runs pipeline jobs (hosted or self-hosted).", Answer = "What is an agent?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This Azure DevOps concept groups steps like build, test, and deploy under a single logical unit.", Answer = "What is a stage?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This securely connects a pipeline to Azure resources for deployments.", Answer = "What is a service connection?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This stores build outputs so later stages/releases can download them.", Answer = "What are artifacts?" },

            new() { Category = "Azure DevOps", PointValue = 300, Question = "This pipeline feature allows reusing common build/deploy logic across multiple repos.", Answer = "What are templates?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "These help avoid hardcoding values and allow environment-specific configuration.", Answer = "What are variables and variable groups?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is the best practice for reviewing code changes before merging into main.", Answer = "What is a pull request (PR) workflow?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is a common way to protect the main branch in Azure Repos.", Answer = "What are branch policies (required reviewers/build validation)?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This lets you require manual approval before deploying to production.", Answer = "What are environment approvals/checks?" },

            new() { Category = "Azure DevOps", PointValue = 400, Question = "A pipeline can’t deploy to Azure due to auth errors—most likely root cause?", Answer = "What is a misconfigured service connection or missing RBAC permissions?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You need to keep secrets out of YAML; the cleanest approach is to use this.", Answer = "What is Key Vault integration (or secret variables)?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "Your pipeline is slow because dependencies download every run—best optimization?", Answer = "What is caching (NuGet/npm) and incremental build strategies?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You want different deployment behavior per environment; the standard approach is to use this.", Answer = "What are stages with environment-specific variables and approvals?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "A PR shows huge unrelated changes—most likely cause?", Answer = "What is the wrong base branch or a branch that wasn’t kept up-to-date?" },

            new() { Category = "Azure DevOps", PointValue = 500, Question = "Production deploy must be safe and reversible—best pipeline strategy?", Answer = "What is staged rollout with approvals plus blue/green or slot-based deployment?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A secret was exposed in logs—first response?", Answer = "What is rotate/revoke the secret immediately and remove it from the pipeline output/history?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need repeatable infrastructure deployments from pipelines—what tool fits best?", Answer = "What is deploying IaC (Bicep/ARM/Terraform) from a pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A release succeeds but the app is broken—what should your pipeline have prevented?", Answer = "What is missing automated tests/health checks and deployment verification gates?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need traceability from backlog item to code to deployment—what combination enables this?", Answer = "What is linking Boards work items to commits/PRs and pipeline runs/releases?" },
            // ==================== TERRAFORM ====================
            new() { Category = "Terraform", PointValue = 100, Question = "This is the open-source Infrastructure as Code tool that uses configuration files to provision resources.", Answer = "What is Terraform?" },
            new() { Category = "Terraform", PointValue = 100, Question = "Terraform configuration files are typically written in this language.", Answer = "What is HCL (HashiCorp Configuration Language)?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this block defines an infrastructure component like an Azure resource.", Answer = "What is a resource?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this defines which cloud/platform you’re talking to, like Azure.", Answer = "What is a provider?" },
            new() { Category = "Terraform", PointValue = 100, Question = "This file often holds pinned provider versions and dependency metadata.", Answer = "What is .terraform.lock.hcl?" },

            new() { Category = "Terraform", PointValue = 200, Question = "This command initializes a Terraform working directory and downloads providers.", Answer = "What is terraform init?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command previews changes Terraform will make without applying them.", Answer = "What is terraform plan?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command creates/updates infrastructure to match the configuration.", Answer = "What is terraform apply?" },
            new() { Category = "Terraform", PointValue = 200, Question = "Terraform tracks deployed resources and mappings in this file/object.", Answer = "What is state?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This feature prevents multiple people/pipelines from applying changes to the same state at the same time.", Answer = "What is state locking?" },

            new() { Category = "Terraform", PointValue = 300, Question = "This reusable packaging mechanism helps standardize Terraform infrastructure patterns.", Answer = "What is a module?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This is the best practice for storing state in teams instead of on a developer laptop.", Answer = "What is remote state (a remote backend)?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This detects changes made outside Terraform by comparing real infrastructure to state.", Answer = "What is drift detection?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command formats Terraform code into a standard style.", Answer = "What is terraform fmt?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command checks Terraform configuration for syntax and basic correctness.", Answer = "What is terraform validate?" },

            // 400
            new() { Category = "Terraform", PointValue = 400, Question = "This command updates state to match real infrastructure without changing resources.", Answer = "What is terraform refresh (or refresh-only planning)?" },
            new() { Category = "Terraform", PointValue = 400, Question = "You already have resources created manually; this command brings them under Terraform management.", Answer = "What is terraform import?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This capability creates multiple isolated environments using the same configuration.", Answer = "What are workspaces?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This lifecycle setting prevents accidental deletion of critical resources.", Answer = "What is prevent_destroy?" },
            new() { Category = "Terraform", PointValue = 400, Question = "To enforce consistent standards across projects, teams typically centralize this.", Answer = "What are shared modules (plus policy/guardrails)?" },

            new() { Category = "Terraform", PointValue = 500, Question = "In team deployments, the safest state setup combines remote state with this feature to prevent corruption.", Answer = "What is state locking?" },
            new() { Category = "Terraform", PointValue = 500, Question = "You need secrets for deployments; Terraform best practice is to avoid putting them in state by doing this.", Answer = "What is using a secret store (Key Vault) and passing secrets at runtime, not hardcoding them?" },
            new() { Category = "Terraform", PointValue = 500, Question = "A pipeline shows huge unexpected changes; the first forensic step is to do this.", Answer = "What is run terraform plan and compare state/config drift and provider/version changes?" },
            new() { Category = "Terraform", PointValue = 500, Question = "To enforce consistent standards across projects, teams typically centralize this.", Answer = "What are shared modules (plus policy/guardrails)?" },
            new() { Category = "Terraform", PointValue = 500, Question = "If you must deploy safely to prod, the best pattern is plan → review → apply using this control.", Answer = "What is gated approvals with a saved plan (and locked state)?" },

            // ==================== ANSIBLE ====================
            new() { Category = "Ansible", PointValue = 100, Question = "This is the automation tool that uses playbooks to configure systems and run tasks.", Answer = "What is Ansible?" },
            new() { Category = "Ansible", PointValue = 100, Question = "Ansible automation instructions are typically stored in this type of file.", Answer = "What is a playbook?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This defines the target machines/groups Ansible will manage.", Answer = "What is an inventory?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores non-secret configuration data.", Answer = "What is a ConfigMap?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores sensitive configuration data.", Answer = "What is a Secret?" },

            new() { Category = "Ansible", PointValue = 200, Question = "This is the Ansible command-line tool that runs playbooks.", Answer = "What is ansible-playbook?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This mechanism organizes reusable automation into a standard structure.", Answer = "What is a role?" },
            new() { Category = "Ansible", PointValue = 200, Question = "These run only when notified, often used to restart a service after a config change.", Answer = "What are handlers?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This binds configuration into a strongly-typed options class.", Answer = "What is IOptions<T>?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This syntax runs a script located in the current folder in PowerShell.", Answer = "What is .\\script.ps1?" },

            new() { Category = "Ansible", PointValue = 300, Question = "This feature gathers system information like OS, IP addresses, and disks.", Answer = "What are facts (setup)?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This lets you select which tasks to run based on tags.", Answer = "What are tags?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This mode shows what will change without applying changes.", Answer = "What is check mode?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This templating system is commonly used for generating config files in Ansible.", Answer = "What is Jinja2?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This is the best practice for keeping your playbooks readable and modular.", Answer = "What is using roles with small, focused tasks?" },

            new() { Category = "Ansible", PointValue = 400, Question = "This Ansible feature encrypts secrets like passwords inside your repo.", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 400, Question = "Your playbook keeps reporting changes even when nothing changed; the likely issue is what?", Answer = "What is a non-idempotent task/module usage?" },
            new() { Category = "Ansible", PointValue = 400, Question = "You need cloud hosts to appear automatically in inventory; the standard solution is what?", Answer = "What is dynamic inventory?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This debugging option shows detailed output about which tasks ran and why.", Answer = "What is running with increased verbosity (-v/-vv/-vvv)?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This is the clean way to avoid repeating the same steps across many playbooks.", Answer = "What are roles (and includes)?" },

            new() { Category = "Ansible", PointValue = 500, Question = "Best practice: store no plaintext secrets in Git—Ansible’s built-in answer is what?", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 500, Question = "Your automation must be safe to rerun in CI/CD; the core requirement is what?", Answer = "What is idempotency?" },
            new() { Category = "Ansible", PointValue = 500, Question = "A playbook works manually but fails in pipeline; the first suspect is usually what?", Answer = "What is environment/credentials/inventory differences in CI?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To standardize systems at scale without “snowflake servers,” your approach should be what?", Answer = "What is configuration as code with repeatable playbooks/roles?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To reduce blast radius in automation, you should design playbooks to do this.", Answer = "What is scope changes narrowly, validate first, and fail fast on errors?" },

            // ==================== CONTAINER REGISTRY ====================
            new() { Category = "Container Registry", PointValue = 100, Question = "This Azure service stores and manages Docker container images.", Answer = "What is Azure Container Registry (ACR)?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "A container image is stored in a registry under this logical grouping name.", Answer = "What is a repository?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This label identifies a specific version of an image, like v1.2.3 or latest.", Answer = "What is a tag?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action uploads a local image to a registry.", Answer = "What is a push?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action downloads an image from a registry to a machine or cluster.", Answer = "What is a pull?" },

            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command sends a built image to the registry.", Answer = "What is docker push?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command downloads an image from the registry.", Answer = "What is docker pull?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This ACR authentication helper command logs Docker into your registry.", Answer = "What is az acr login?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the main security risk of using the registry admin user.", Answer = "What is broad shared credentials (high blast radius)?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the preferred way for Azure services to pull images without storing passwords.", Answer = "What is managed identity with RBAC?" },

            new() { Category = "Container Registry", PointValue = 300, Question = "This ACR feature can build images in the cloud when you push code or run a task.", Answer = "What are ACR Tasks?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This security practice ensures images are regularly checked for known CVEs.", Answer = "What is vulnerability scanning?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This network feature keeps registry access private inside Azure networks.", Answer = "What is a private endpoint (Private Link)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This controls who can pull images using roles like AcrPull.", Answer = "What is RBAC (role-based access control)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This best practice keeps versions predictable and reduces production surprises.", Answer = "What is using immutable, versioned tags (avoid relying on latest)?" },

            new() { Category = "Container Registry", PointValue = 400, Question = "Your pods can’t pull images from ACR; the first thing to verify is this.", Answer = "What is registry permissions (AcrPull) and authentication configuration?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "You need images available close to multiple regions; the ACR capability is what?", Answer = "What is geo-replication?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "This reduces supply-chain risk by preventing unsigned/untrusted images from being used.", Answer = "What is content trust / image signing (conceptually)?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "Logging on storage is used for auditing and this.", Answer = "What is troubleshooting?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "A common place to query storage logs/metrics is Azure Monitor and this workspace.", Answer = "What is Log Analytics?" },

            new() { Category = "Container Registry", PointValue = 500, Question = "For least privilege, your AKS/Container Apps should pull images using what identity approach?", Answer = "What is managed identity (or workload identity) with AcrPull RBAC?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "A build pipeline needs secure image provenance; the best pattern is what?", Answer = "What is build in CI, scan, sign, then deploy only signed images?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "Your registry must not be publicly reachable; the key configuration is what?", Answer = "What is private endpoint plus restricted public network access?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You deployed the wrong image because a tag moved; the prevention is what?", Answer = "What is pinning by digest or immutable version tags?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You suspect compromised images; the first immediate action is what?", Answer = "What is block/rotate credentials, quarantine images, and audit pull activity/logs?" },
            // ==================== APP CONFIGURATION ====================
            new() { Category = "App Configuration", PointValue = 100, Question = "This Azure service centrally stores application settings as key-value pairs.", Answer = "What is Azure App Configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "In App Configuration, this is the basic unit used to store a setting name and value.", Answer = "What is a key-value pair?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This feature lets you logically separate settings for dev/test/prod using the same key names.", Answer = "What are labels?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This describes reading config from a centralized store instead of hardcoding values in code.", Answer = "What is externalized configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This is the most common reason to use App Configuration instead of appsettings.json only.", Answer = "What is centralized configuration management?" },

            new() { Category = "App Configuration", PointValue = 200, Question = "This App Configuration feature lets you safely turn features on or off without redeploying.", Answer = "What is Feature Management (feature flags)?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This Azure service should store secrets, not App Configuration.", Answer = "What is Azure Key Vault?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This practice helps avoid restarting apps constantly by updating config without redeploy.", Answer = "What is dynamic configuration refresh?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is the standard .NET concept used to bind configuration into strongly typed objects.", Answer = "What is IOptions<T>?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is a best practice for config keys so teams can find things quickly.", Answer = "What is a consistent naming convention (namespacing)?" },

            new() { Category = "App Configuration", PointValue = 300, Question = "This enables automatic refresh when a sentinel key changes.", Answer = "What is a refresh sentinel key?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the common pattern: settings in App Configuration, secrets in Key Vault, linked by this.", Answer = "What are Key Vault references?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This principle means your app should still run with safe defaults if config is unavailable.", Answer = "What is graceful degradation (fallback defaults)?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the best reason to use labels rather than separate apps for each environment.", Answer = "What is environment isolation with one shared configuration store?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This access approach avoids storing credentials in code when reading App Configuration from Azure.", Answer = "What is managed identity?" },

            new() { Category = "App Configuration", PointValue = 400, Question = "Your app reads old values after you updated keys—most likely missing feature?", Answer = "What is configuration refresh (or caching invalidation)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "To prevent config drift across teams, you should treat configuration like this.", Answer = "What is versioned, reviewed change management (config as code/process)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You want different feature flag behavior per environment; you should use this.", Answer = "What are labels (and environment-specific flags)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You need to limit who can change production settings; the right control is this.", Answer = "What is RBAC with least privilege?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Your app fails only in Azure but works locally; the first suspect for App Configuration is this.", Answer = "What is identity/permissions (managed identity not granted)?" },

            new() { Category = "App Configuration", PointValue = 500, Question = "You need safe releases with instant rollback without redeploying—best tool combo?", Answer = "What are feature flags plus staged rollout?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "A bad config change took prod down—best prevention pattern?", Answer = "What is validation + approvals + gradual rollout (and safe defaults)?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You must rotate secrets without redeploy; where should secrets live and how should apps reference them?", Answer = "What is Key Vault with Key Vault references from configuration?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "Your system needs consistent settings across microservices; the architecture goal is this.", Answer = "What is centralized configuration with controlled refresh and governance?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "If a feature flag system causes outages, the top design mistake is this.", Answer = "What is not planning for flag failure (no fallback, hard dependency)?" },
                    // ==================== AZURE FUNDAMENTALS ====================
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This is the basic unit of deployment in Azure that contains resources like VMs, storage, and databases.", Answer = "What is a Resource Group?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This portal provides a web-based interface to manage all Azure services.", Answer = "What is the Azure Portal?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This term describes Azure's worldwide network of data centers.", Answer = "What are Azure Regions?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This Azure service lets you create isolated networks in the cloud.", Answer = "What is Virtual Network (VNet)?" },
            new() { Category = "Azure Fundamentals", PointValue = 100, Question = "This is Microsoft's command-line tool for managing Azure resources.", Answer = "What is Azure CLI?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure service provides serverless compute that lets you run code without managing servers.", Answer = "What is Azure Functions?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This service provides virtual machines in the cloud.", Answer = "What is Azure Virtual Machines?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure storage type is optimized for storing massive amounts of unstructured data.", Answer = "What is Blob Storage?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This Azure service provides a platform for hosting web applications.", Answer = "What is Azure App Service?" },
            new() { Category = "Azure Fundamentals", PointValue = 200, Question = "This feature allows you to organize Azure resources using key-value pairs.", Answer = "What are Tags?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This cloud model combines on-premises infrastructure with cloud resources.", Answer = "What is Hybrid Cloud?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure feature automatically adjusts resources based on demand.", Answer = "What is Auto-scaling?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This service provides managed Kubernetes orchestration in Azure.", Answer = "What is Azure Kubernetes Service (AKS)?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure service provides message queuing for decoupling applications.", Answer = "What is Azure Queue Storage or Service Bus?" },
            new() { Category = "Azure Fundamentals", PointValue = 300, Question = "This Azure feature helps estimate costs before deploying resources.", Answer = "What is the Azure Pricing Calculator?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides a fully managed relational database with built-in intelligence.", Answer = "What is Azure SQL Database?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides a content delivery network for fast global content delivery.", Answer = "What is Azure CDN?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This tool allows you to define Azure infrastructure as code using JSON templates.", Answer = "What is ARM Templates?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service monitors the health and performance of your applications.", Answer = "What is Azure Monitor?" },
            new() { Category = "Azure Fundamentals", PointValue = 400, Question = "This Azure service provides DNS hosting and domain management.", Answer = "What is Azure DNS?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This pricing model charges you only for resources you actually use, with no upfront costs.", Answer = "What is Pay-As-You-Go?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This Azure governance feature helps you organize resources and apply consistent policies.", Answer = "What is Azure Policy?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This SLA percentage guarantees about 8.76 hours of downtime per year.", Answer = "What is 99.9%?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This Azure feature provides cost management recommendations and optimization tips.", Answer = "What is Azure Advisor?" },
            new() { Category = "Azure Fundamentals", PointValue = 500, Question = "This hierarchy level sits above subscriptions for managing multiple Azure environments.", Answer = "What is a Management Group?" },

            // ==================== C# PROGRAMMING ====================
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword is used to define a method that doesn't return a value.", Answer = "What is void?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword creates a new instance of a class.", Answer = "What is new?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This data type stores true or false values.", Answer = "What is bool?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This keyword is used to define a constant value that cannot be changed.", Answer = "What is const?" },
            new() { Category = "C# Programming", PointValue = 100, Question = "This operator is used to concatenate strings in C#.", Answer = "What is + (plus)?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This feature allows you to define a blueprint for creating objects with properties and methods.", Answer = "What is a Class?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This keyword makes a class member accessible only within the same class.", Answer = "What is private?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This collection type stores key-value pairs for fast lookups.", Answer = "What is a Dictionary?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This keyword is used to check if an object is of a specific type.", Answer = "What is is?" },
            new() { Category = "C# Programming", PointValue = 200, Question = "This type of loop iterates through each element in a collection.", Answer = "What is foreach?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword is used to handle exceptions that may occur during program execution.", Answer = "What is try-catch?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This OOP principle allows a derived class to provide a specific implementation of a base class method.", Answer = "What is Polymorphism?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword allows a class to inherit from another class.", Answer = "What is the colon (:) or inheritance?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This keyword prevents a class from being inherited.", Answer = "What is sealed?" },
            new() { Category = "C# Programming", PointValue = 300, Question = "This type of method belongs to the class itself rather than an instance.", Answer = "What is static?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This C# feature allows methods to run concurrently without blocking the main thread.", Answer = "What is async/await?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This feature lets you define type-safe generic classes and methods.", Answer = "What are Generics?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This delegate type represents a method that can be passed as a parameter.", Answer = "What is Action or Func?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This keyword ensures a variable can only be assigned once, but at runtime.", Answer = "What is readonly?" },
            new() { Category = "C# Programming", PointValue = 400, Question = "This operator returns the left operand if not null, otherwise the right operand.", Answer = "What is ?? (null-coalescing)?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This LINQ method filters a sequence of values based on a predicate.", Answer = "What is Where()?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This pattern uses events to notify subscribers when state changes.", Answer = "What is the Observer Pattern?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This C# 9+ feature provides concise syntax for immutable data types.", Answer = "What are Records?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This feature allows you to add methods to existing types without modifying them.", Answer = "What are Extension Methods?" },
            new() { Category = "C# Programming", PointValue = 500, Question = "This C# feature enables pattern matching with type checking and property inspection.", Answer = "What is switch expression?" },

            // ==================== WEB DEVELOPMENT ====================
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to retrieve data from a server.", Answer = "What is GET?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to submit data to be processed by a server.", Answer = "What is POST?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This markup language structures content on web pages.", Answer = "What is HTML?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This language is used to style web pages with colors, fonts, and layouts.", Answer = "What is CSS?" },
            new() { Category = "Web Development", PointValue = 100, Question = "This HTTP method is used to delete a resource on the server.", Answer = "What is DELETE?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This Microsoft framework allows you to build interactive web UIs using C# instead of JavaScript.", Answer = "What is Blazor?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This data format is commonly used for API responses and is based on JavaScript object notation.", Answer = "What is JSON?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This ASP.NET Core feature handles incoming HTTP requests.", Answer = "What is a Controller?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This HTTP method is used to update an entire resource on the server.", Answer = "What is PUT?" },
            new() { Category = "Web Development", PointValue = 200, Question = "This attribute in ASP.NET Core maps HTTP routes to controller actions.", Answer = "What is [Route]?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates that a resource was not found on the server.", Answer = "What is 404?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates a successful HTTP request.", Answer = "What is 200?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates the server encountered an internal error.", Answer = "What is 500?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates a resource was created successfully.", Answer = "What is 201?" },
            new() { Category = "Web Development", PointValue = 300, Question = "This status code indicates the client is not authorized.", Answer = "What is 401?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This architectural style uses HTTP methods and is commonly used for building web APIs.", Answer = "What is REST?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This security header prevents clickjacking attacks by controlling iframe embedding.", Answer = "What is X-Frame-Options?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This technique allows servers to push updates to clients in real-time.", Answer = "What is SignalR or WebSockets?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This HTTP header tells the browser what content type to expect.", Answer = "What is Content-Type?" },
            new() { Category = "Web Development", PointValue = 400, Question = "This mechanism allows cross-origin requests from web browsers.", Answer = "What is CORS?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This ASP.NET Core feature allows you to add cross-cutting concerns like logging and authentication to your request pipeline.", Answer = "What is Middleware?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This design pattern separates an application into Model, View, and Controller components.", Answer = "What is MVC?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This Blazor hosting model runs entirely in the browser via WebAssembly.", Answer = "What is Blazor WebAssembly?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This ASP.NET Core feature injects dependencies into classes automatically.", Answer = "What is Dependency Injection?" },
            new() { Category = "Web Development", PointValue = 500, Question = "This query language developed by Facebook provides an alternative to REST APIs.", Answer = "What is GraphQL?" },

            // ==================== DEVOPS & CI/CD ====================
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This version control system tracks changes to source code and is widely used in software development.", Answer = "What is Git?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command creates a copy of a remote repository on your local machine.", Answer = "What is git clone?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command stages changes for the next commit.", Answer = "What is git add?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command saves staged changes to the repository.", Answer = "What is git commit?" },
            new() { Category = "DevOps & CI/CD", PointValue = 100, Question = "This Git command uploads local commits to a remote repository.", Answer = "What is git push?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Azure service provides unlimited private Git repositories and agile planning tools.", Answer = "What is Azure DevOps?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This file defines the steps in an Azure DevOps pipeline.", Answer = "What is azure-pipelines.yml?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This GitHub feature automates workflows when code is pushed or PRs are created.", Answer = "What is GitHub Actions?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Git command downloads changes from a remote repository.", Answer = "What is git pull or git fetch?" },
            new() { Category = "DevOps & CI/CD", PointValue = 200, Question = "This Git command creates a new branch.", Answer = "What is git branch or git checkout -b?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This practice involves automatically building and testing code changes when they're committed.", Answer = "What is Continuous Integration (CI)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This practice automatically deploys code changes to production after passing tests.", Answer = "What is Continuous Deployment (CD)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This branching strategy uses feature branches that merge into a main branch.", Answer = "What is Git Flow or Feature Branching?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This Git command combines changes from one branch into another.", Answer = "What is git merge?" },
            new() { Category = "DevOps & CI/CD", PointValue = 300, Question = "This type of test verifies individual units of code work correctly.", Answer = "What is Unit Testing?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This containerization platform packages applications with their dependencies for consistent deployment.", Answer = "What is Docker?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This file defines how to build a Docker container image.", Answer = "What is a Dockerfile?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This tool defines multi-container Docker applications.", Answer = "What is Docker Compose?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This Git command replays commits on top of another branch.", Answer = "What is git rebase?" },
            new() { Category = "DevOps & CI/CD", PointValue = 400, Question = "This practice stores and manages container images.", Answer = "What is a Container Registry?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Azure service orchestrates containerized applications at scale using Kubernetes.", Answer = "What is Azure Kubernetes Service (AKS)?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Infrastructure as Code tool by HashiCorp provisions cloud resources.", Answer = "What is Terraform?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This Azure service provides serverless container hosting without managing infrastructure.", Answer = "What is Azure Container Apps?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This deployment strategy gradually shifts traffic from old to new versions.", Answer = "What is Blue-Green or Canary Deployment?" },
            new() { Category = "DevOps & CI/CD", PointValue = 500, Question = "This practice treats infrastructure configuration like application code.", Answer = "What is Infrastructure as Code (IaC)?" },

            // ==================== DATABASES ====================
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command is used to retrieve data from a database table.", Answer = "What is SELECT?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command adds new records to a database table.", Answer = "What is INSERT?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command modifies existing records in a table.", Answer = "What is UPDATE?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command removes records from a table.", Answer = "What is DELETE?" },
            new() { Category = "Databases", PointValue = 100, Question = "This SQL command creates a new table in a database.", Answer = "What is CREATE TABLE?" },
            new() { Category = "Databases", PointValue = 200, Question = "This type of database stores data in JSON-like documents rather than tables.", Answer = "What is a NoSQL/Document Database?" },
            new() { Category = "Databases", PointValue = 200, Question = "This SQL clause filters records based on a condition.", Answer = "What is WHERE?" },
            new() { Category = "Databases", PointValue = 200, Question = "This database object enforces unique values in a column.", Answer = "What is a Primary Key?" },
            new() { Category = "Databases", PointValue = 200, Question = "This SQL clause sorts the results of a query.", Answer = "What is ORDER BY?" },
            new() { Category = "Databases", PointValue = 200, Question = "This database constraint links records between two tables.", Answer = "What is a Foreign Key?" },
            new() { Category = "Databases", PointValue = 300, Question = "This Azure service is a globally distributed, multi-model database for any scale.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Databases", PointValue = 300, Question = "This SQL operation combines rows from two or more tables based on a related column.", Answer = "What is JOIN?" },
            new() { Category = "Databases", PointValue = 300, Question = "This database design process eliminates redundancy by organizing data into related tables.", Answer = "What is Normalization?" },
            new() { Category = "Databases", PointValue = 300, Question = "This SQL clause groups rows that have the same values.", Answer = "What is GROUP BY?" },
            new() { Category = "Databases", PointValue = 300, Question = "This type of JOIN returns all records from the left table.", Answer = "What is LEFT JOIN?" },
            new() { Category = "Databases", PointValue = 400, Question = "This .NET technology maps database tables to C# classes and provides an abstraction layer.", Answer = "What is Entity Framework?" },
            new() { Category = "Databases", PointValue = 400, Question = "This EF Core approach generates database schema from C# model classes.", Answer = "What is Code-First?" },
            new() { Category = "Databases", PointValue = 400, Question = "This technique improves query performance by creating data structures for fast lookups.", Answer = "What is Indexing?" },
            new() { Category = "Databases", PointValue = 400, Question = "This SQL clause filters groups after GROUP BY.", Answer = "What is HAVING?" },
            new() { Category = "Databases", PointValue = 400, Question = "This database object stores a precompiled SQL query for reuse.", Answer = "What is a Stored Procedure?" },
            new() { Category = "Databases", PointValue = 500, Question = "This database concept ensures that transactions are processed reliably using Atomicity, Consistency, Isolation, and Durability.", Answer = "What is ACID?" },
            new() { Category = "Databases", PointValue = 500, Question = "This SQL injection prevention technique uses parameterized queries.", Answer = "What are Prepared Statements?" },
            new() { Category = "Databases", PointValue = 500, Question = "This Cosmos DB feature automatically replicates data across multiple Azure regions.", Answer = "What is Global Distribution?" },
            new() { Category = "Databases", PointValue = 500, Question = "This database technique splits data across multiple servers for scalability.", Answer = "What is Sharding?" },
            new() { Category = "Databases", PointValue = 500, Question = "This consistency model provides high availability but eventual consistency.", Answer = "What is BASE (Basically Available, Soft state, Eventually consistent)?" },

            // ==================== SECURITY ====================
            new() { Category = "Security", PointValue = 100, Question = "This process verifies who a user claims to be.", Answer = "What is Authentication?" },
            new() { Category = "Security", PointValue = 100, Question = "This process determines what actions an authenticated user can perform.", Answer = "What is Authorization?" },
            new() { Category = "Security", PointValue = 100, Question = "This cryptographic technique converts data into a fixed-size string.", Answer = "What is Hashing?" },
            new() { Category = "Security", PointValue = 100, Question = "This security practice requires users to provide multiple forms of verification.", Answer = "What is Multi-Factor Authentication (MFA)?" },
            new() { Category = "Security", PointValue = 100, Question = "This security feature locks an account after too many failed login attempts.", Answer = "What is Account Lockout?" },
            new() { Category = "Security", PointValue = 200, Question = "This Azure service manages identities and access for cloud applications.", Answer = "What is Microsoft Entra ID (Azure AD)?" },
            new() { Category = "Security", PointValue = 200, Question = "This protocol provides secure communication over the internet using encryption.", Answer = "What is HTTPS/TLS?" },
            new() { Category = "Security", PointValue = 200, Question = "This type of token is commonly used for API authentication and contains encoded claims.", Answer = "What is JWT (JSON Web Token)?" },
            new() { Category = "Security", PointValue = 200, Question = "This encryption type uses the same key for encryption and decryption.", Answer = "What is Symmetric Encryption?" },
            new() { Category = "Security", PointValue = 200, Question = "This security practice stores passwords as hashed values, not plain text.", Answer = "What is Password Hashing?" },
            new() { Category = "Security", PointValue = 300, Question = "This protocol uses tokens to securely authorize access to resources without sharing passwords.", Answer = "What is OAuth?" },
            new() { Category = "Security", PointValue = 300, Question = "This security practice limits user permissions to only what is necessary.", Answer = "What is the Principle of Least Privilege?" },
            new() { Category = "Security", PointValue = 300, Question = "This attack exploits trust a website has in a user's browser by sending unauthorized requests.", Answer = "What is CSRF (Cross-Site Request Forgery)?" },
            new() { Category = "Security", PointValue = 300, Question = "This encryption type uses a public key for encryption and private key for decryption.", Answer = "What is Asymmetric Encryption?" },
            new() { Category = "Security", PointValue = 300, Question = "This security layer adds authentication between services in a microservices architecture.", Answer = "What is a Service Mesh or mTLS?" },
            new() { Category = "Security", PointValue = 400, Question = "This type of attack tricks users into executing malicious scripts in their browser.", Answer = "What is Cross-Site Scripting (XSS)?" },
            new() { Category = "Security", PointValue = 400, Question = "This attack inserts malicious SQL code into application queries.", Answer = "What is SQL Injection?" },
            new() { Category = "Security", PointValue = 400, Question = "This security header helps prevent XSS attacks by controlling resource loading.", Answer = "What is Content Security Policy (CSP)?" },
            new() { Category = "Security", PointValue = 400, Question = "This attack floods a server with traffic to make it unavailable.", Answer = "What is DDoS (Distributed Denial of Service)?" },
            new() { Category = "Security", PointValue = 400, Question = "This security tool scans code for vulnerabilities before deployment.", Answer = "What is Static Application Security Testing (SAST)?" },
            new() { Category = "Security", PointValue = 500, Question = "This Azure service stores secrets, keys, and certificates securely for cloud applications.", Answer = "What is Azure Key Vault?" },
            new() { Category = "Security", PointValue = 500, Question = "This authentication method eliminates passwords using devices and biometrics.", Answer = "What is Passwordless Authentication?" },
            new() { Category = "Security", PointValue = 500, Question = "This Azure feature scans code repositories for exposed secrets and credentials.", Answer = "What is GitHub Advanced Security or Credential Scanning?" },
            new() { Category = "Security", PointValue = 500, Question = "This security framework provides guidelines for managing cybersecurity risk.", Answer = "What is NIST Cybersecurity Framework?" },
            new() { Category = "Security", PointValue = 500, Question = "This security concept assumes no user or system should be trusted by default.", Answer = "What is Zero Trust?" },

            // ==================== NETWORKING ====================
            new() { Category = "Networking", PointValue = 100, Question = "This protocol assigns IP addresses to devices on a network automatically.", Answer = "What is DHCP?" },
            new() { Category = "Networking", PointValue = 100, Question = "This protocol translates domain names to IP addresses.", Answer = "What is DNS?" },
            new() { Category = "Networking", PointValue = 100, Question = "This network device forwards data between different networks.", Answer = "What is a Router?" },
            new() { Category = "Networking", PointValue = 100, Question = "This type of IP address is not routable on the public internet.", Answer = "What is a Private IP Address?" },
            new() { Category = "Networking", PointValue = 100, Question = "This command tests connectivity between two networked devices.", Answer = "What is ping?" },
            new() { Category = "Networking", PointValue = 200, Question = "This layer of the OSI model handles routing and IP addressing.", Answer = "What is the Network Layer (Layer 3)?" },
            new() { Category = "Networking", PointValue = 200, Question = "This protocol provides reliable, ordered delivery of data over networks.", Answer = "What is TCP?" },
            new() { Category = "Networking", PointValue = 200, Question = "This protocol provides fast, connectionless data transmission.", Answer = "What is UDP?" },
            new() { Category = "Networking", PointValue = 200, Question = "This Azure service provides load balancing for incoming traffic.", Answer = "What is Azure Load Balancer?" },
            new() { Category = "Networking", PointValue = 200, Question = "This network security device filters traffic based on rules.", Answer = "What is a Firewall?" },
            new() { Category = "Networking", PointValue = 300, Question = "This Azure feature allows private connectivity between Azure services.", Answer = "What is Private Endpoint?" },
            new() { Category = "Networking", PointValue = 300, Question = "This networking concept divides a network into smaller segments.", Answer = "What is Subnetting?" },
            new() { Category = "Networking", PointValue = 300, Question = "This secure tunnel encrypts traffic between your network and Azure.", Answer = "What is VPN (Virtual Private Network)?" },
            new() { Category = "Networking", PointValue = 300, Question = "This port number is used by HTTPS.", Answer = "What is 443?" },
            new() { Category = "Networking", PointValue = 300, Question = "This Azure feature controls inbound and outbound traffic for resources.", Answer = "What is Network Security Group (NSG)?" },
            new() { Category = "Networking", PointValue = 400, Question = "This Azure service provides a dedicated private connection to Azure.", Answer = "What is Azure ExpressRoute?" },
            new() { Category = "Networking", PointValue = 400, Question = "This layer of the OSI model handles data encryption and compression.", Answer = "What is the Presentation Layer (Layer 6)?" },
            new() { Category = "Networking", PointValue = 400, Question = "This IP version uses 128-bit addresses.", Answer = "What is IPv6?" },
            new() { Category = "Networking", PointValue = 400, Question = "This Azure service provides application-level load balancing.", Answer = "What is Azure Application Gateway?" },
            new() { Category = "Networking", PointValue = 400, Question = "This technique translates private IP addresses to public ones.", Answer = "What is NAT (Network Address Translation)?" },
            new() { Category = "Networking", PointValue = 500, Question = "This Azure networking feature connects multiple VNets together.", Answer = "What is VNet Peering?" },
            new() { Category = "Networking", PointValue = 500, Question = "This networking architecture centralizes connectivity through a hub network.", Answer = "What is Hub and Spoke?" },
            new() { Category = "Networking", PointValue = 500, Question = "This Azure service provides global DNS-based traffic routing.", Answer = "What is Azure Traffic Manager?" },
            new() { Category = "Networking", PointValue = 500, Question = "This Azure service provides web application firewall and global load balancing.", Answer = "What is Azure Front Door?" },
            new() { Category = "Networking", PointValue = 500, Question = "This command displays the network path packets take to reach a destination.", Answer = "What is traceroute (or tracert)?" },

            // ==================== CLOUD ARCHITECTURE ====================
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides virtual machines and storage.", Answer = "What is IaaS (Infrastructure as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides a platform for deploying applications.", Answer = "What is PaaS (Platform as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud service model provides complete applications over the internet.", Answer = "What is SaaS (Software as a Service)?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This cloud deployment uses resources from multiple cloud providers.", Answer = "What is Multi-Cloud?" },
            new() { Category = "Cloud Architecture", PointValue = 100, Question = "This term describes the ability to increase resources as demand grows.", Answer = "What is Scalability?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This architectural pattern breaks applications into small, independent services.", Answer = "What are Microservices?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This pattern keeps applications running even when components fail.", Answer = "What is High Availability?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This Azure feature duplicates data across regions for disaster recovery.", Answer = "What is Geo-Redundancy?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This pattern stores frequently accessed data for faster retrieval.", Answer = "What is Caching?" },
            new() { Category = "Cloud Architecture", PointValue = 200, Question = "This Azure service provides in-memory caching.", Answer = "What is Azure Cache for Redis?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern handles failures gracefully by stopping requests to failing services.", Answer = "What is Circuit Breaker?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern separates read and write operations for better performance.", Answer = "What is CQRS (Command Query Responsibility Segregation)?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This messaging pattern decouples producers and consumers of messages.", Answer = "What is Publish-Subscribe (Pub/Sub)?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This pattern ensures changes are tracked and can be replayed.", Answer = "What is Event Sourcing?" },
            new() { Category = "Cloud Architecture", PointValue = 300, Question = "This Azure service provides event-driven serverless compute.", Answer = "What is Azure Event Grid?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern distributes incoming requests across multiple servers.", Answer = "What is Load Balancing?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This architecture runs code only when triggered by events.", Answer = "What is Serverless?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern limits the rate of requests to protect services.", Answer = "What is Throttling or Rate Limiting?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This Azure framework provides best practices for cloud architecture.", Answer = "What is the Azure Well-Architected Framework?" },
            new() { Category = "Cloud Architecture", PointValue = 400, Question = "This pattern stores data closer to users for better performance.", Answer = "What is Content Delivery Network (CDN)?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This pattern handles long-running transactions across microservices.", Answer = "What is the Saga Pattern?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This CAP theorem states you can only have two of three: Consistency, Availability, Partition tolerance.", Answer = "What is the CAP Theorem?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This pattern provides a single entry point for multiple backend services.", Answer = "What is API Gateway?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This recovery metric measures acceptable data loss in time.", Answer = "What is RPO (Recovery Point Objective)?" },
            new() { Category = "Cloud Architecture", PointValue = 500, Question = "This recovery metric measures acceptable downtime.", Answer = "What is RTO (Recovery Time Objective)?" },

            // ==================== SOFTWARE TESTING ====================
            new() { Category = "Software Testing", PointValue = 100, Question = "This type of testing verifies individual units of code work correctly.", Answer = "What is Unit Testing?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This testing framework is commonly used for C# unit tests.", Answer = "What is xUnit, NUnit, or MSTest?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This practice writes tests before writing the actual code.", Answer = "What is TDD (Test-Driven Development)?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This keyword in testing asserts an expected value equals an actual value.", Answer = "What is Assert?" },
            new() { Category = "Software Testing", PointValue = 100, Question = "This type of testing checks if the entire system works as expected.", Answer = "What is End-to-End (E2E) Testing?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This type of testing verifies multiple components work together correctly.", Answer = "What is Integration Testing?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This testing technique replaces dependencies with fake implementations.", Answer = "What is Mocking?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This metric measures what percentage of code is executed by tests.", Answer = "What is Code Coverage?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This pattern organizes tests into Arrange, Act, Assert sections.", Answer = "What is AAA (Arrange-Act-Assert)?" },
            new() { Category = "Software Testing", PointValue = 200, Question = "This tool automates browser-based testing.", Answer = "What is Selenium or Playwright?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This type of testing checks if the application meets business requirements.", Answer = "What is Acceptance Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This testing approach tests the system without knowing internal code.", Answer = "What is Black Box Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This testing approach tests with knowledge of internal code structure.", Answer = "What is White Box Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This type of testing ensures new changes don't break existing functionality.", Answer = "What is Regression Testing?" },
            new() { Category = "Software Testing", PointValue = 300, Question = "This C# library is commonly used for creating mock objects.", Answer = "What is Moq?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This type of testing measures system performance under load.", Answer = "What is Load Testing or Performance Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This type of testing pushes the system beyond normal limits.", Answer = "What is Stress Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This practice tests software with random or unexpected inputs.", Answer = "What is Fuzz Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This testing technique tests boundaries between valid and invalid inputs.", Answer = "What is Boundary Testing?" },
            new() { Category = "Software Testing", PointValue = 400, Question = "This CI/CD practice automatically runs tests when code is pushed.", Answer = "What is Automated Testing or Test Automation?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This practice intentionally introduces failures to test system resilience.", Answer = "What is Chaos Engineering?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This testing pyramid suggests having more unit tests than integration tests.", Answer = "What is the Testing Pyramid?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This type of testing verifies the application is secure from attacks.", Answer = "What is Security Testing or Penetration Testing?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This practice tests the behavior of the system from the user's perspective.", Answer = "What is BDD (Behavior-Driven Development)?" },
            new() { Category = "Software Testing", PointValue = 500, Question = "This tool by Netflix randomly terminates instances to test resilience.", Answer = "What is Chaos Monkey?" },

            // ==================== DATA STRUCTURES ====================
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure stores elements in a linear order with indices.", Answer = "What is an Array?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure follows Last-In-First-Out (LIFO) principle.", Answer = "What is a Stack?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure follows First-In-First-Out (FIFO) principle.", Answer = "What is a Queue?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This C# collection dynamically resizes as elements are added.", Answer = "What is a List?" },
            new() { Category = "Data Structures", PointValue = 100, Question = "This data structure stores unique elements only.", Answer = "What is a Set or HashSet?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure stores key-value pairs for O(1) lookups.", Answer = "What is a Hash Table or Dictionary?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure consists of nodes with pointers to next elements.", Answer = "What is a Linked List?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This tree structure maintains sorted data for efficient searching.", Answer = "What is a Binary Search Tree?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This data structure maps keys to values using a hash function.", Answer = "What is a Hash Map?" },
            new() { Category = "Data Structures", PointValue = 200, Question = "This linked list type has pointers in both directions.", Answer = "What is a Doubly Linked List?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This balanced tree ensures O(log n) operations.", Answer = "What is an AVL Tree or Red-Black Tree?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This data structure represents relationships between nodes.", Answer = "What is a Graph?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This tree-based structure always removes the minimum (or maximum) element.", Answer = "What is a Heap or Priority Queue?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This tree structure stores strings efficiently with shared prefixes.", Answer = "What is a Trie?" },
            new() { Category = "Data Structures", PointValue = 300, Question = "This graph traversal visits all neighbors before going deeper.", Answer = "What is Breadth-First Search (BFS)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This graph traversal goes as deep as possible before backtracking.", Answer = "What is Depth-First Search (DFS)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes constant time operations.", Answer = "What is O(1)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes linear time operations.", Answer = "What is O(n)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This algorithmic complexity describes logarithmic time operations.", Answer = "What is O(log n)?" },
            new() { Category = "Data Structures", PointValue = 400, Question = "This data structure tracks disjoint sets efficiently.", Answer = "What is Union-Find or Disjoint Set?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This algorithm finds the shortest path in a weighted graph.", Answer = "What is Dijkstra's Algorithm?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This sorting algorithm has O(n log n) average complexity.", Answer = "What is Quick Sort or Merge Sort?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This tree structure segments ranges for efficient queries.", Answer = "What is a Segment Tree?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This data structure combines hash tables with linked lists for ordering.", Answer = "What is a LinkedHashMap?" },
            new() { Category = "Data Structures", PointValue = 500, Question = "This probabilistic data structure tests if an element might be in a set.", Answer = "What is a Bloom Filter?" },

            // ==================== OPERATING SYSTEMS ====================
            new() { Category = "Operating Systems", PointValue = 100, Question = "This component manages memory, processes, and hardware.", Answer = "What is the Kernel?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This running instance of a program has its own memory space.", Answer = "What is a Process?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This lightweight unit of execution shares memory with its process.", Answer = "What is a Thread?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This OS command lists files in a directory on Linux.", Answer = "What is ls?" },
            new() { Category = "Operating Systems", PointValue = 100, Question = "This OS command lists files in a directory on Windows.", Answer = "What is dir?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This memory management technique gives processes virtual address spaces.", Answer = "What is Virtual Memory?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Linux command changes file permissions.", Answer = "What is chmod?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Windows feature isolates processes from each other.", Answer = "What is Process Isolation?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This environment variable contains the directories to search for executables.", Answer = "What is PATH?" },
            new() { Category = "Operating Systems", PointValue = 200, Question = "This Linux command displays running processes.", Answer = "What is ps or top?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This condition occurs when two processes wait for each other indefinitely.", Answer = "What is Deadlock?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This scheduling algorithm gives each process equal time slices.", Answer = "What is Round Robin?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This Linux command searches for text patterns in files.", Answer = "What is grep?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This mechanism allows processes to communicate with each other.", Answer = "What is IPC (Inter-Process Communication)?" },
            new() { Category = "Operating Systems", PointValue = 300, Question = "This Linux command displays disk usage.", Answer = "What is df or du?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This synchronization primitive allows only one thread access at a time.", Answer = "What is a Mutex?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This synchronization primitive limits concurrent access to a resource.", Answer = "What is a Semaphore?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This Linux command creates a symbolic link.", Answer = "What is ln -s?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This memory issue occurs when memory is allocated but never freed.", Answer = "What is a Memory Leak?" },
            new() { Category = "Operating Systems", PointValue = 400, Question = "This Linux command shows network connections and ports.", Answer = "What is netstat or ss?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This OS concept moves memory pages between RAM and disk.", Answer = "What is Paging or Swapping?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This condition occurs when page faults happen excessively.", Answer = "What is Thrashing?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This Linux feature isolates process trees with namespaces.", Answer = "What are Containers (cgroups/namespaces)?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This scheduling algorithm prioritizes shorter jobs first.", Answer = "What is Shortest Job First (SJF)?" },
            new() { Category = "Operating Systems", PointValue = 500, Question = "This OS architecture runs services in user space rather than kernel.", Answer = "What is Microkernel?" },

            // ==================== APIS & INTEGRATION ====================
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This acronym stands for Application Programming Interface.", Answer = "What is API?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This HTTP header identifies the client making the request.", Answer = "What is User-Agent?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This API architectural style uses XML for message format.", Answer = "What is SOAP?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This API documentation format describes RESTful APIs.", Answer = "What is OpenAPI (Swagger)?" },
            new() { Category = "APIs & Integration", PointValue = 100, Question = "This HTTP method retrieves a resource without modifying it.", Answer = "What is GET?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This pattern returns a subset of results with pagination info.", Answer = "What is Paging or Pagination?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This header carries authentication credentials for APIs.", Answer = "What is Authorization?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This API versioning approach puts version in the URL path.", Answer = "What is URL Versioning?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This Azure service manages, publishes, and secures APIs.", Answer = "What is Azure API Management?" },
            new() { Category = "APIs & Integration", PointValue = 200, Question = "This data format is lighter than XML and commonly used in REST APIs.", Answer = "What is JSON?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This authentication flow is used for server-to-server API calls.", Answer = "What is Client Credentials?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This pattern limits API requests per time period.", Answer = "What is Rate Limiting?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This HTTP status code indicates too many requests.", Answer = "What is 429?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This notification mechanism calls a URL when events occur.", Answer = "What is a Webhook?" },
            new() { Category = "APIs & Integration", PointValue = 300, Question = "This protocol enables real-time bidirectional communication.", Answer = "What is WebSocket?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This query language lets clients request specific data from APIs.", Answer = "What is GraphQL?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This messaging protocol is lightweight and used in IoT.", Answer = "What is MQTT?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This Google-developed framework uses Protocol Buffers for RPC.", Answer = "What is gRPC?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This integration pattern transforms data between different formats.", Answer = "What is Data Transformation or ETL?" },
            new() { Category = "APIs & Integration", PointValue = 400, Question = "This Azure service connects applications and data across cloud and on-premises.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This pattern aggregates calls to multiple backend services.", Answer = "What is Backend for Frontend (BFF)?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This messaging pattern ensures messages are delivered at least once.", Answer = "What is At-Least-Once Delivery?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This Azure service provides enterprise messaging with topics and queues.", Answer = "What is Azure Service Bus?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This protocol enables secure service-to-service authentication.", Answer = "What is mTLS (Mutual TLS)?" },
            new() { Category = "APIs & Integration", PointValue = 500, Question = "This architectural style makes APIs self-descriptive with hypermedia links.", Answer = "What is HATEOAS?" },

            // ==================== MACHINE LEARNING BASICS ====================
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This type of ML learns from labeled training data.", Answer = "What is Supervised Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This type of ML finds patterns in unlabeled data.", Answer = "What is Unsupervised Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This ML technique predicts continuous numeric values.", Answer = "What is Regression?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This ML technique categorizes data into discrete classes.", Answer = "What is Classification?" },
            new() { Category = "Machine Learning Basics", PointValue = 100, Question = "This Azure service provides pre-built AI models and APIs.", Answer = "What is Azure Cognitive Services?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This unsupervised technique groups similar data points together.", Answer = "What is Clustering?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This metric measures how well a classification model performs.", Answer = "What is Accuracy?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This technique splits data into training and testing sets.", Answer = "What is Train-Test Split?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This Azure service provides a platform for building ML models.", Answer = "What is Azure Machine Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 200, Question = "This problem occurs when a model performs well on training data but poorly on new data.", Answer = "What is Overfitting?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This neural network architecture is commonly used for image recognition.", Answer = "What is CNN (Convolutional Neural Network)?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This technique reduces the number of features in a dataset.", Answer = "What is Dimensionality Reduction or PCA?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This ML technique learns through trial and error with rewards.", Answer = "What is Reinforcement Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This simple algorithm classifies based on nearest neighbors.", Answer = "What is K-Nearest Neighbors (KNN)?" },
            new() { Category = "Machine Learning Basics", PointValue = 300, Question = "This tree-based algorithm is used for classification and regression.", Answer = "What is Decision Tree?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This ensemble method combines multiple decision trees.", Answer = "What is Random Forest?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This neural network architecture processes sequential data.", Answer = "What is RNN (Recurrent Neural Network) or LSTM?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This technique iteratively builds models that correct previous errors.", Answer = "What is Gradient Boosting?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This metric measures the trade-off between precision and recall.", Answer = "What is F1 Score?" },
            new() { Category = "Machine Learning Basics", PointValue = 400, Question = "This AI service provides natural language understanding capabilities.", Answer = "What is Azure Language Service or LUIS?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This architecture powers large language models like GPT.", Answer = "What is Transformer?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This technique uses pre-trained models on new tasks.", Answer = "What is Transfer Learning?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This validation technique trains on all data except one fold.", Answer = "What is Cross-Validation?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This Azure service provides AI-powered search capabilities.", Answer = "What is Azure AI Search (Cognitive Search)?" },
            new() { Category = "Machine Learning Basics", PointValue = 500, Question = "This technique generates new data similar to training data.", Answer = "What is Data Augmentation or GANs?" },

            // ==================== MS-4010 (SECURITY) ====================
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the primary goal of the Zero Trust security model?", Answer = "What is to never trust, always verify?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of verifying a user's identity called?", Answer = "What is Authentication?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of determining what actions an authenticated user can perform?", Answer = "What is Authorization?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the practice of requiring more than one form of verification called?", Answer = "What is Multi-Factor Authentication (MFA)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 100, Question = "What is the process of converting data into a fixed-size string called?", Answer = "What is Hashing?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "Which Microsoft service is used for managing secrets, keys, and certificates in Azure?", Answer = "What is Azure Key Vault?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What protocol provides secure communication over the internet using encryption?", Answer = "What is HTTPS/TLS?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What type of token is commonly used for API authentication and contains encoded claims?", Answer = "What is JWT (JSON Web Token)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What encryption type uses the same key for encryption and decryption?", Answer = "What is Symmetric Encryption?" },
            new() { Category = "MS-4010 (Security)", PointValue = 200, Question = "What security practice stores passwords as hashed values, not plain text?", Answer = "What is Password Hashing?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What protocol is commonly used for secure authentication and authorization in cloud applications?", Answer = "What is OAuth 2.0?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What security practice limits user permissions to only what is necessary?", Answer = "What is the Principle of Least Privilege?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What attack exploits trust a website has in a user's browser by sending unauthorized requests?", Answer = "What is CSRF (Cross-Site Request Forgery)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What encryption type uses a public key for encryption and private key for decryption?", Answer = "What is Asymmetric Encryption?" },
            new() { Category = "MS-4010 (Security)", PointValue = 300, Question = "What security layer adds authentication between services in a microservices architecture?", Answer = "What is a Service Mesh or mTLS?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What is the process of identifying and reducing vulnerabilities in code before deployment called?", Answer = "What is Static Application Security Testing (SAST)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What type of attack tricks users into executing malicious scripts in their browser?", Answer = "What is Cross-Site Scripting (XSS)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What attack inserts malicious SQL code into application queries?", Answer = "What is SQL Injection?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What security header helps prevent XSS attacks by controlling resource loading?", Answer = "What is Content Security Policy (CSP)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 400, Question = "What attack floods a server with traffic to make it unavailable?", Answer = "What is DDoS (Distributed Denial of Service)?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What Azure service provides recommendations to improve security posture?", Answer = "What is Microsoft Defender for Cloud?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What Azure service stores secrets, keys, and certificates securely for cloud applications?", Answer = "What is Azure Key Vault?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What authentication method eliminates passwords using devices and biometrics?", Answer = "What is Passwordless Authentication?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What security framework provides guidelines for managing cybersecurity risk?", Answer = "What is NIST Cybersecurity Framework?" },
            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What security concept assumes no user or system should be trusted by default?", Answer = "What is Zero Trust?" },

            // ==================== AGILE & SCRUM ====================
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "What’s a time-boxed Scrum iteration called?", Answer = "Sprint" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "What’s the ordered list of work called?", Answer = "Product Backlog" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "Daily 15-minute sync is called what?", Answer = "Daily Scrum (standup)" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "Who owns backlog priority?", Answer = "Product Owner" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "End-of-sprint demo is called what?", Answer = "Sprint Review" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What ceremony plans sprint work + goal?", Answer = "Sprint Planning" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What meeting improves the team’s process?", Answer = "Retrospective" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What’s a 'user story' format often used?", Answer = "As a <user>, I want <goal>, so <benefit>" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What’s 'story points' measuring?", Answer = "Relative effort/complexity" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "What is the Sprint Backlog?", Answer = "Items selected for the sprint + plan to deliver them" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s 'Definition of Done' (DoD)?", Answer = "Shared completion criteria for work" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s a burndown chart show?", Answer = "Remaining work over time" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What does velocity represent?", Answer = "Story points completed per sprint (trend)" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s backlog refinement for?", Answer = "Clarify/split/estimate/prioritize future work" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "What’s the biggest anti-pattern in standup?", Answer = "Turning it into a status meeting for managers" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "Sprint Goal is at risk mid-sprint—best Scrum move?", Answer = "Re-negotiate scope with PO, protect goal" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s scope creep in Scrum usually caused by?", Answer = "Uncontrolled work added mid-sprint" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "Why keep sprints time-boxed?", Answer = "Predictable cadence + forces prioritization" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s the difference between epic and story?", Answer = "Epic is large; stories are small deliverable slices" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "What’s 'WIP limit' trying to prevent?", Answer = "Too much work in progress, too little finishing" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "If velocity is unstable, what should you improve first?", Answer = "Story slicing + estimation consistency" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Best metric to optimize for: 'busy' or 'done'?", Answer = "Done (delivered value)" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Team keeps missing sprint commitments—most likely root cause?", Answer = "Over-commitment + poor slicing" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "What’s the cleanest way to handle urgent new work?", Answer = "Put it in backlog; swap only with PO agreement" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Why do retrospectives fail?", Answer = "No action items, no ownership, no follow-through" },

            // ==================== POWERSHELL & CLI ====================
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What PowerShell cmdlet lists files and folders?", Answer = "Get-ChildItem" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What does the | pipe do?", Answer = "Sends output to the next command" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What Azure CLI command signs you in interactively?", Answer = "az login" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What cmdlet shows help for a command?", Answer = "Get-Help" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "What PowerShell switch simulates actions without making them?", Answer = "-WhatIf" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What symbol starts a PowerShell variable?", Answer = "$" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the Azure CLI command to list resource groups?", Answer = "az group list" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the PowerShell operator for 'not equal'?", Answer = "-ne" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the purpose of an execution policy?", Answer = "Controls script running rules" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "What’s the command to run a local script in PowerShell?", Answer = ".\\script.ps1" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Which command loops over pipeline items in PowerShell?", Answer = "ForEach-Object" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "Azure CLI command to set the active subscription?", Answer = "az account set" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "PowerShell cmdlet to convert an object to JSON?", Answer = "ConvertTo-Json" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What variable holds the last external program exit code?", Answer = "$LASTEXITCODE" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "What does -ErrorAction Stop force?", Answer = "Converts non-terminating errors into terminating" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does try { } catch { } finally { } enable?", Answer = "Structured error handling + cleanup" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s az rest used for?", Answer = "Call Azure REST APIs directly" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What’s the difference: Write-Host vs Write-Output?", Answer = "Host-only display vs pipeline output" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "What does 'idempotent' mean in deployments?", Answer = "Re-running yields same end state" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "How do you stop on any error in a script globally?", Answer = "$ErrorActionPreference = 'Stop'" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Azure CLI command to deploy a Bicep/ARM template to a resource group?", Answer = "az deployment group create" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "What’s the safest way to avoid hardcoding secrets in scripts?", Answer = "Use Managed Identity + Key Vault" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "You get AuthorizationFailed in Azure CLI. Most likely fix?", Answer = "Correct subscription/role + re-login (az login)" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Best practice for automation scripts: 'fail fast' means what?", Answer = "Validate inputs early and stop on errors" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Why use tags in automation cleanup scripts?", Answer = "Filter and manage resources predictably" },

            // ==================== API MANAGEMENT ====================
            new() { Category = "API Management", PointValue = 100, Question = "This Azure service acts as a gateway in front of your APIs to publish, secure, and monitor them.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature lets you require a key to call an API without full OAuth setup.", Answer = "What is a subscription key?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM component is the entry point that receives client requests and forwards them to backends.", Answer = "What is the API Gateway?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature provides a site for developers to discover and test your APIs.", Answer = "What is the Developer Portal?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM concept bundles APIs and controls access with subscriptions.", Answer = "What is a Product?" },
            new() { Category = "API Management", PointValue = 200, Question = "This standard format is commonly imported into APIM to define endpoints, schemas, and operations.", Answer = "What is OpenAPI (Swagger)?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you enforce rules like rate limits, transformations, and auth checks without changing code.", Answer = "What are Policies?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature stores reusable configuration values like backend URLs or tokens (non-secret), referenced by policies.", Answer = "What are Named Values?" },
            new() { Category = "API Management", PointValue = 200, Question = "This security method commonly used with APIM validates a bearer token issued by an identity provider.", Answer = "What is OAuth 2.0 / JWT validation?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM feature lets you group operations under a single API name and base URL.", Answer = "What is an API (in APIM)?" },
            new() { Category = "API Management", PointValue = 300, Question = "This policy is used to limit how many calls a client can make in a given time window.", Answer = "What is rate limiting (rate-limit policy)?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM capability can store responses to reduce backend load and improve latency.", Answer = "What is response caching?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM feature lets you simulate an API response without calling the real backend.", Answer = "What is a mock response?" },
            new() { Category = "API Management", PointValue = 300, Question = "This APIM concept allows you to create a new API definition without breaking existing consumers.", Answer = "What are Versions (or Revisions)?" },
            new() { Category = "API Management", PointValue = 300, Question = "In APIM, policies can run in multiple stages of the request pipeline, including inbound and this stage.", Answer = "What is outbound?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy validates and enforces access based on a JWT token’s issuer, audience, and claims.", Answer = "What is validate-jwt?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM policy changes the request path or query before sending it to the backend.", Answer = "What is rewrite-uri?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM feature provides centralized analytics and troubleshooting for calls, failures, and latency.", Answer = "What is monitoring/diagnostics (often via Azure Monitor/Application Insights)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This APIM scenario improves security by keeping the backend private while exposing only the gateway publicly.", Answer = "What is placing APIM in front of a private backend (gateway as the single entry point)?" },
            new() { Category = "API Management", PointValue = 400, Question = "This is the safest design principle when retries can cause the same request to be processed twice.", Answer = "What is idempotency?" },
            new() { Category = "API Management", PointValue = 500, Question = "You need a single front door for multiple microservices with consistent auth, quotas, and logging. This service is built for that.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 500, Question = "You want a policy to run only when an error occurs (like a backend 500). This APIM policy section is used.", Answer = "What is on-error?" },
            new() { Category = "API Management", PointValue = 500, Question = "Your backend is protected by Entra ID and requires a valid token. APIM should enforce this at the gateway using this.", Answer = "What is JWT validation (OAuth 2.0)?" },
            new() { Category = "API Management", PointValue = 500, Question = "Clients are abusing an endpoint and driving costs. The fastest APIM control to apply is this.", Answer = "What is rate limiting or quotas?" },
            new() { Category = "API Management", PointValue = 500, Question = "A breaking change must be introduced safely while keeping old clients working. The APIM approach is to use this.", Answer = "What is API versioning?" },

            // ==================== LOGIC APPS ====================
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Azure service creates serverless workflows using triggers and actions.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow component starts a Logic App, such as an HTTP request or a schedule.", Answer = "What is a Trigger?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This workflow step performs work after the trigger fires, such as calling an API or sending an email.", Answer = "What is an Action?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Logic Apps feature connects to services like Outlook, Service Bus, or SQL without writing SDK code.", Answer = "What is a Connector?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This trigger runs a workflow on a timer schedule.", Answer = "What is the Recurrence trigger?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step lets you branch logic based on true/false evaluation.", Answer = "What is a Condition?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This control step repeats actions for each item in a collection.", Answer = "What is a For each loop?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This action is used to call a REST endpoint directly from a workflow.", Answer = "What is the HTTP action?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This Logic Apps feature stores values you can reuse later in the workflow.", Answer = "What are Variables?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This is the benefit of Logic Apps compared to custom code for integrations.", Answer = "What is low-code workflow automation?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This Logic Apps capability records every run so you can inspect inputs, outputs, and failures.", Answer = "What is Run History?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the recommended way for a Logic App to access Azure resources without storing passwords or keys.", Answer = "What is Managed Identity?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the integration pattern where a workflow waits for and reacts to messages.", Answer = "What is event-driven processing?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the built-in reliability behavior that tries an action again after a transient failure.", Answer = "What is retry policy?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the common use of Logic Apps in enterprise: connecting SaaS, APIs, and data without heavy custom plumbing.", Answer = "What is system integration?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This B2B feature supports standards like X12 and EDIFACT when paired with the right resource.", Answer = "What is an Integration Account?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This design principle prevents duplicate side effects when a workflow step might run more than once.", Answer = "What is idempotency?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "You need a workflow that might take hours or days, waiting on approvals. This type of orchestration is ideal for that.", Answer = "What is a long-running workflow?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the main reason to use connectors instead of writing direct API code everywhere.", Answer = "What is standardized auth and built-in integration logic?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "This is the feature that lets you see exactly which action failed and what data it received.", Answer = "What is run details (inputs/outputs in Run History)?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You must integrate dozens of systems with approvals, retries, and minimal code. This Azure service is the best fit.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow triggers twice and creates duplicate records. The correct fix is to make the workflow actions do this.", Answer = "What is be idempotent?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need secure access to Key Vault from a Logic App without secrets in configuration. Use this.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow intermittently fails calling an external API. The reliability control you tune first is this.", Answer = "What is retry policy (with backoff)?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need a workflow to respond instantly to an event and kick off downstream steps without polling. The architecture style is.", Answer = "What is event-driven architecture?" },

            // ==================== DATA FACTORY ====================
            new() { Category = "Data Factory", PointValue = 100, Question = "This Azure service is used to build and orchestrate ETL/ELT pipelines.", Answer = "What is Azure Data Factory (ADF)?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF component is a group of activities that perform a data movement or transformation process.", Answer = "What is a Pipeline?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object defines the connection information to a data source like SQL or Blob Storage.", Answer = "What is a Linked Service?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF object represents the structure/location of data used by activities.", Answer = "What is a Dataset?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF activity is commonly used to move data from one system to another.", Answer = "What is Copy activity?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This type of Data Flow reads data from one or more sources, applies transformations, and writes to destinations.", Answer = "What is a Mapping Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF feature runs a pipeline at scheduled intervals or in response to events.", Answer = "What is a Trigger?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF activity executes a SQL script on a database.", Answer = "What is Stored Procedure activity?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF capability allows data transformation using a visual interface without writing code.", Answer = "What is Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "In ADF, this integration runtime type is used to securely access on-premises data sources.", Answer = "What is Self-hosted Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF component provides the compute and network resources used to move and transform data.", Answer = "What is an Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern loads data into the data lake or warehouse first, then transforms it as needed.", Answer = "What is ELT (Extract, Load, Transform)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This pattern moves data, transforms it, and then loads it into the destination system.", Answer = "What is ETL (Extract, Transform, Load)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF feature allows you to define parameters to pass values into your pipelines at runtime.", Answer = "What are Pipeline Parameters?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This trigger type in ADF runs pipelines at regular intervals, like a cron job.", Answer = "What is a scheduled trigger?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "If a self-hosted integration runtime is not working, the first thing to check is this.", Answer = "What is whether the integration runtime is running/stopped?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is the file format of the default retry policy in ADF activities.", Answer = "What is JSON?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is a common reason for a pipeline to fail if it was running fine before.", Answer = "What is schema changes in the source data?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "You need to ensure data quality after ingestion; best practice is to add this step.", Answer = "What is validation checks (row counts, checksums, critical query tests)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Slow queries and table scans are a common symptom of missing this.", Answer = "What are indexes?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "You must load data incrementally; the key pattern is what?", Answer = "What is watermarking (incremental loads based on a high-water mark)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To ensure no duplicate data is loaded, this property must be part of your design.", Answer = "What is idempotency (dedupe/upsert strategy)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "After a migration, reports show wrong totals even though loads succeed; the most likely cause?", Answer = "What is transformation logic/mapping differences or data type/collation issues?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "To prove compliance, you need to log all data access and changes—key solution components?", Answer = "What are Azure Monitor, storage logs, and activity logs?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "For sensitive data, you must reduce accidental exposure in monitoring—what principle?", Answer = "What is minimize/secure logging and protect secrets/PII?" },

            // ==================== SYNAPSE ANALYTICS ====================
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Azure service combines data warehousing and big data analytics in one workspace.", Answer = "What is Azure Synapse Analytics?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse component lets you run Apache Spark for big data processing.", Answer = "What is a Spark pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option lets you query data in a lake without provisioning a dedicated warehouse.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse option is a provisioned, scalable data warehouse engine.", Answer = "What is a dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This web UI is used to author queries, pipelines, and notebooks in Synapse.", Answer = "What is Synapse Studio?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This storage is commonly used as the central data lake for Synapse.", Answer = "What is Azure Data Lake Storage Gen2?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This Synapse feature can orchestrate data movement and transformations similar to ADF.", Answer = "What are Synapse Pipelines?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This describes a warehouse pattern where raw data is loaded first and transformed inside the warehouse.", Answer = "What is ELT?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This is a key benefit of serverless SQL pool.", Answer = "What is pay-per-query on data in the lake?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This type of query object is commonly used in Synapse for reusable logic over data.", Answer = "What is a view?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the biggest difference: dedicated SQL pools are provisioned, while serverless SQL pools are this.", Answer = "What is on-demand (pay-per-query)?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This best practice improves warehouse performance by reducing data movement across compute nodes.", Answer = "What is choosing a good distribution key?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This index type is commonly used in large analytic warehouses for compression and fast scans.", Answer = "What is a clustered columnstore index?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This technique speeds up repeated analytics queries by precomputing results.", Answer = "What are materialized views?" },
            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the purpose of partitioning large tables in analytics workloads.", Answer = "What is improving query performance and manageability?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "You are querying a data lake and costs are high. The first optimization is usually to do this.", Answer = "What is reduce scanned data (filter columns/rows and use partitioned files)?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "A dedicated SQL pool is slow due to data shuffles. The likely cause is this.", Answer = "What is poor distribution causing data movement?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This describes controlling resource usage and concurrency for multiple workloads.", Answer = "What is workload management?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This design principle prevents duplicates when pipelines are retried or rerun.", Answer = "What is idempotency?" },
            new() { Category = "Synapse Analytics", PointValue = 400, Question = "This is the best reason to use Spark in Synapse instead of only SQL.", Answer = "What is large-scale transformations and advanced processing?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You need a fully provisioned enterprise data warehouse with predictable performance. The Synapse choice is this.", Answer = "What is a dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You want to run ad-hoc SQL directly over files in a data lake without provisioning compute. The Synapse choice is this.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "Your warehouse queries are slow because joins cause huge data movement. The best fix is usually this.", Answer = "What is redesign distribution (choose a better distribution key)?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "Pipelines rerun and duplicate rows in the warehouse. The correct requirement for your load process is this.", Answer = "What is idempotent loading?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You need a single platform for SQL, Spark, pipelines, and lake queries under one umbrella. The service is.", Answer = "What is Azure Synapse Analytics?" },

            // ==================== COSMOS DB ====================
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Azure database is designed for globally distributed, low-latency NoSQL workloads.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB resource is like a table/collection that stores JSON documents.", Answer = "What is a Container?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB concept is the unique identifier for a stored document.", Answer = "What is an Item (document) id?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB feature spreads data across partitions for scale.", Answer = "What is partitioning?" },
            new() { Category = "Cosmos DB", PointValue = 100, Question = "This Cosmos DB choice determines how reads and writes behave across replicas.", Answer = "What is a consistency level?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is the unit used to measure and bill throughput for Cosmos DB operations.", Answer = "What are Request Units (RU/s)?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This is the most important design decision for scalability and performance in Cosmos DB.", Answer = "What is choosing a good partition key?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This feature automatically removes items after a specified time.", Answer = "What is Time To Live (TTL)?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This feature controls which fields are indexed and how queries perform.", Answer = "What is an indexing policy?" },
            new() { Category = "Cosmos DB", PointValue = 200, Question = "This Cosmos DB capability replicates data to multiple Azure regions.", Answer = "What is global distribution?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This feature provides an ordered stream of changes for a container that can trigger downstream processing.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is a common reason Cosmos DB gets expensive unexpectedly.", Answer = "What is high RU usage from inefficient queries or hot partitions?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is the best practice for app authentication to Cosmos DB without storing keys.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This describes the problem when too many requests hit a single partition due to a bad partition key.", Answer = "What is a hot partition?" },
            new() { Category = "Cosmos DB", PointValue = 300, Question = "This is a typical Cosmos DB data model advantage compared to strict relational schemas.", Answer = "What is flexible (schema-less) JSON documents?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "You need to process events from data changes in near real time. Cosmos DB provides this for free without polling.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "This consistency level offers the strongest guarantees but can reduce performance.", Answer = "What is Strong consistency?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "This design principle prevents duplicate side effects when events or retries cause repeated processing.", Answer = "What is idempotency?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "To reduce RU consumption, one of the best first steps is to do this.", Answer = "What is optimize queries and indexing?" },
            new() { Category = "Cosmos DB", PointValue = 400, Question = "If your partition key has low cardinality (few values), the likely outcome is this.", Answer = "What is poor distribution and hot partitions?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "Your app needs global low-latency reads and automatic replication across regions. This database is designed for that.", Answer = "What is Azure Cosmos DB?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You are getting 429 throttling responses. The most direct fix is to increase this or reduce RU usage.", Answer = "What is provisioned throughput (RU/s)?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "A single partition key value receives most writes and causes throttling. The real fix is to redesign this.", Answer = "What is the partition key strategy?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You want downstream services triggered by database changes without building a polling job. Use this Cosmos feature.", Answer = "What is Change Feed?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "To keep retries from duplicating records, your writes should be designed to be this.", Answer = "What is idempotent?" },

            // ==================== SERVICE BUS ====================
            new() { Category = "Service Bus", PointValue = 100, Question = "This Service Bus entity is designed for point-to-point messaging between a sender and a single receiver.", Answer = "What is a Queue?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This Service Bus pattern allows one message to be delivered to multiple subscribers.", Answer = "What is a Topic and Subscription?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This is the main benefit of using Service Bus between services.", Answer = "What is decoupling (asynchronous messaging)?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This feature lets a receiver temporarily lock a message while it is being processed.", Answer = "What is Peek-Lock?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This is where messages end up when they cannot be delivered or processed successfully.", Answer = "What is the Dead-Letter Queue (DLQ)?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This queue type ensures messages are delivered in FIFO order when enabled.", Answer = "What are Sessions?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This is the Service Bus option that removes a message immediately when it is received.", Answer = "What is Receive-and-Delete?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This message property controls how long a message can live before it expires.", Answer = "What is Time-To-Live (TTL)?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This security mechanism is commonly used with Azure services to validate a bearer token.", Answer = "What is OAuth 2.0 / JWT validation?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This capability prevents the same message from being processed more than once within a time window.", Answer = "What is Duplicate Detection?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This Azure messaging feature lets you decouple application components by using queues.", Answer = "What is Azure Service Bus?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This Service Bus feature allows one message to be delivered to multiple subscriptions.", Answer = "What is Publish/Subscribe?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This property of a message indicates its position in the queue.", Answer = "What is a Sequence Number?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This feature allows you to delay the processing of a message until a specific time.", Answer = "What is Scheduled Delivery?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This capability lets you move a message to the side for later processing.", Answer = "What is Message Deferral?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This feature provides transactional support for manipulating multiple messages/activities.", Answer = "What are transactions?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This design principle ensures that processing a message multiple times does not change the outcome.", Answer = "What is idempotency?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the first thing to check when a message is not received after being sent.", Answer = "What is the Dead-Letter Queue (DLQ)?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This feature lets you restrict who can send or receive messages using Azure AD roles.", Answer = "What is RBAC (Role-Based Access Control)?"},
            new() { Category = "Service Bus", PointValue = 400, Question = "This property controls how long a message remains locked for processing.", Answer = "What is Lock Duration?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "To ensure a workflow processes each message exactly once, it’s crucial to design for this.", Answer = "What is idempotency?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "For stateful processing per group with strict ordering, use this Service Bus feature.", Answer = "What are Sessions?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "High throughput and strict order are required; the best Azure messaging pattern is?", Answer = "What is a queue with session ID?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "For secure access from an Azure-hosted app without storing secrets, you should authenticate using this.", Answer = "What is Managed Identity (with RBAC)?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You need to guarantee message processing order and exact-once delivery; the main feature for that is?", Answer = "What are sessions?" },

            // ==================== EVENT GRID ====================
            new() { Category = "Event Grid", PointValue = 100, Question = "This Azure service routes events from publishers to subscribers using event-driven architecture.", Answer = "What is Event Grid?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This is what an Event Grid subscription connects: an event source to an event handler.", Answer = "What is an Event Subscription?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This type of topic is automatically created for supported Azure resources.", Answer = "What is a System Topic?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This is a common Event Grid use case: reacting when a blob is created or deleted.", Answer = "What is event-driven automation?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This term describes the component that receives and processes Event Grid events.", Answer = "What is an Event Handler?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This feature lets you route only certain events to a subscriber (for example, only 'created' events).", Answer = "What is Event Filtering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This Event Grid feature sends undeliverable events to storage for later inspection.", Answer = "What is Dead-lettering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This is a common built-in event handler target for running code on an event.", Answer = "What is Azure Functions?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This schema is commonly used by Event Grid for standardized event formats.", Answer = "What is CloudEvents?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This is the concept where publishers emit events without knowing who will receive them.", Answer = "What is loose coupling?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This notification mechanism calls a URL when events occur.", Answer = "What is a Webhook?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This subscription filter allows routing based on event type, subject, and metadata.", Answer = "What is advanced filtering?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This feature allows testing of event delivery and processing without actual events.", Answer = "What is Event Grid Simulator?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This capability ensures events are delivered at least once, even in failures.", Answer = "What is retry policy?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is a key difference between Event Grid and Service Bus: Event Grid does not require this.", Answer = "What is explicit message handling (like Complete or Ack)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "To secure API access from Event Grid, you can use this Azure feature.", Answer = "What is Managed Identity (system-assigned or user-assigned)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "When scaling out event handling, this pattern helps maintain order for related events.", Answer = "What is partitioning by key (event deduplication)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "Events should be handled idempotently because this common feature can cause duplicates.", Answer = "What is at-least-once delivery?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "To debug why an event wasn’t processed, this is the first place to check.", Answer = "What are the event delivery logs and dead-letter details?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This term describes the failure to deliver an event after all retry attempts.", Answer = "What isdead-lettering (sending to DLQ)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "For reliable event processing, the best pattern is to combine Event Grid with this.", Answer = "What is a durable queue (Service Bus or Storage Queue)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "To prevent abuse, you limit how many requests a client can make in a time window using this.", Answer = "What is rate limiting (throttling)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You need to guarantee message order and exactly-once processing; the main feature for that is?", Answer = "What are sessions (session-based processing)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You want to filter events from a specific Azure resource (like a storage account). You should use this topic type.", Answer = "What is a System Topic?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "For large systems, the cleanest approach to event routing is to keep producers unaware of consumers. This principle is called.", Answer = "What is loose coupling?" },

            // ==================== EVENT HUB ====================
            new() { Category = "Event Hub", PointValue = 100, Question = "This Azure service is designed for ingesting large volumes of streaming events like telemetry and logs.", Answer = "What is Event Hubs?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This Event Hub concept is a logical group used to enable multiple independent readers of the same stream.", Answer = "What is a Consumer Group?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This term describes how Event Hub scales: the stream is split into these.", Answer = "What are Partitions?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This setting determines how long events are kept in an Event Hub before they expire.", Answer = "What is Retention?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This is a common Event Hub use case: collecting device or application telemetry at high scale.", Answer = "What is streaming ingestion?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This feature automatically writes Event Hub data to storage for analytics or archiving.", Answer = "What is Capture?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the identifier for where a consumer is reading in a partition.", Answer = "What is an Offset?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the recommended approach so a consumer can resume after a restart.", Answer = "What is checkpointing?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This Event Hub unit is used to describe capacity in some tiers.", Answer = "What is a Throughput Unit (TU)?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the best mental model: Event Hub is like a big, durable stream log rather than a work queue.", Answer = "What is an event stream?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is a key difference: Event Hub is for telemetry streams, while this service is for reliable command/work messaging.", Answer = "What is Service Bus?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is why partitions matter: ordering is guaranteed only within a single partition.", Answer = "What is per-partition ordering?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is the common SDK helper pattern used to process events and manage checkpoints.", Answer = "What is the Event Processor pattern?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is a typical consumer scaling rule: one active reader per partition in a consumer group.", Answer = "What is partition-based parallelism?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This best practice prevents duplicate side effects in stream processing.", Answer = "What is idempotent processing?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "You need to land raw stream data for later queries without building a consumer app. This feature is designed for that.", Answer = "What is Event Hub Capture?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "Your consumer falls behind. The first Event Hub setting to check that can cause data loss is this.", Answer = "What is retention period?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "This concept describes that Event Hub consumers pull data at their own pace instead of the service pushing it.", Answer = "What is a pull-based consumption model?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "To keep related events in order, producers typically send them with the same value for this.", Answer = "What is a partition key?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "You want to read the same stream with two different apps without interfering with each other. You should use two of these.", Answer = "What are Consumer Groups?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "A consumer app restarts and reprocesses old events. The missing piece it likely needs is this.", Answer = "What is checkpointing?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You need strict ordering across ALL events. Event Hub cannot guarantee that because ordering is only within these.", Answer = "What are partitions?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Telemetry processing must scale. The main scaling dimension for consumers in Event Hub is this.", Answer = "What is number of partitions?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You must avoid duplicate writes when retries or restarts occur. Your processing must be designed to be this.", Answer = "What is idempotent?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "You want Event Hub data available for big analytics queries later. The built-in feature that lands data to storage is this.", Answer = "What is Capture?" },

            // ==================== AZURE MONITOR ====================
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This Azure service is the umbrella platform for collecting metrics, logs, and alerts across Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "These are numeric time-series measurements like CPU percentage or request count.", Answer = "What are metrics?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "These are detailed records/events you can query, often stored in a Log Analytics workspace.", Answer = "What are logs?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This Azure resource commonly stores and queries Azure Monitor logs with KQL.", Answer = "What is a Log Analytics workspace?" },
            new() { Category = "Azure Monitor", PointValue = 100, Question = "This feature notifies you when a condition is met (like high CPU or too many 5xx).", Answer = "What is an alert?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This query language is used to search and analyze Azure Monitor Logs.", Answer = "What is KQL (Kusto Query Language)?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This type of alert triggers based on numeric time-series data like CPU or memory.", Answer = "What is a metric alert?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This type of alert triggers based on the results of a log query.", Answer = "What is a log alert (scheduled query alert)?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This config routes platform/resource logs (like App Service logs) into Log Analytics, Storage, or Event Hubs.", Answer = "What are diagnostic settings?" },
            new() { Category = "Azure Monitor", PointValue = 200, Question = "This component defines who gets notified and how (email, SMS, webhook) when an alert fires.", Answer = "What is an action group?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This built-in log records subscription-level operations like resource creation and RBAC changes.", Answer = "What is the Activity Log?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This concept links related telemetry/events so you can trace one request across multiple services.", Answer = "What is correlation?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This practice reduces noisy or expensive telemetry by sending only a percentage of it.", Answer = "What is sampling?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This visualization surface lets you pin charts and queries for an at-a-glance view of health.", Answer = "What are Azure Monitor dashboards (or Azure dashboards)?" },
            new() { Category = "Azure Monitor", PointValue = 300, Question = "This is the key difference: metrics are time-series numbers, while logs are this.", Answer = "What are rich event records you query with KQL?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "Your alert fired but nobody got notified—this is the first thing to verify.", Answer = "What is the action group configuration (and whether it’s attached to the alert rule)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "You need to alert when error rate exceeds a threshold; this data usually comes from here.", Answer = "What are application logs/telemetry in Log Analytics (or Application Insights)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "You want consistent monitoring across many subscriptions; this is a common governance approach.", Answer = "What is using Policy/initiatives to deploy diagnostic settings and alerts at scale?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "Your KQL query is slow and expensive; the best quick fix is usually to do this.", Answer = "What is narrowing the time range and filtering early (reduce scanned data)?" },
            new() { Category = "Azure Monitor", PointValue = 400, Question = "A classic reason metrics look fine but users complain is that you’re missing this type of signal.", Answer = "What are dependency/transaction logs (end-to-end traces)?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "You’re drowning in alerts; the best first engineering fix is usually to do this.", Answer = "What is reduce noise with better thresholds, suppression, and actionable alerts only?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "A service goes down but metrics were green—most likely explanation?", Answer = "What is you were monitoring the wrong signal (or missing synthetic/user-facing checks)?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "To troubleshoot a production incident fastest, the best workflow is: failures → dependencies → correlated traces.", Answer = "What is starting with failures, then drilling into dependencies, then correlating logs/traces?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "You must prove who changed what and when across Azure—what log source is essential?", Answer = "What is the Activity Log (plus resource diagnostic logs)?" },
            new() { Category = "Azure Monitor", PointValue = 500, Question = "Your log ingestion costs exploded overnight; the most common first lever is this.", Answer = "What is reduce ingestion with sampling/filtering and stricter diagnostic settings?" },

            // ==================== AZURE RESOURCE MANAGER ====================
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is Azure’s deployment and management layer used to create and manage resources consistently.", Answer = "What is Azure Resource Manager (ARM)?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This JSON-based approach defines infrastructure in code for repeatable deployments.", Answer = "What are ARM templates?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the logical container that holds related Azure resources for lifecycle and billing.", Answer = "What is a Resource Group?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the billing and access boundary that contains resource groups.", Answer = "What is a subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This ARM concept describes declaring what you want, not the steps to do it.", Answer = "What is declarative deployment?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element is an input value you supply at deployment time.", Answer = "What is a parameter?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This template element returns values after deployment (like a URL or resource ID).", Answer = "What is an output?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This feature previews what will change before actually deploying.", Answer = "What is What-If?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This is the ARM scope where you deploy most application resources.", Answer = "What is resource group scope?" },
            new() { Category = "Azure Resource Manager", PointValue = 200, Question = "This concept means deployments converge to the same end state when re-run.", Answer = "What is idempotency?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode adds/updates resources but does not delete resources not in the template.", Answer = "What is Incremental mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This ARM deployment mode can remove resources not declared in the template at that scope.", Answer = "What is Complete mode?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This provides organization-wide structure above subscriptions for governance.", Answer = "What are management groups?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This is the service namespace you deploy under (like Microsoft.Web or Microsoft.Storage).", Answer = "What is a resource provider?" },
            new() { Category = "Azure Resource Manager", PointValue = 300, Question = "This helps prevent config drift by making infrastructure changes repeatable and reviewable.", Answer = "What is Infrastructure as Code (IaC) in source control?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "A deployment fails with 'MissingSubscriptionRegistration'—most likely fix?", Answer = "What is registering the required resource provider for the subscription?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You want safer multi-environment deployments; the best practice is to separate these.", Answer = "What are parameters/config per environment (dev/test/prod)?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to pass secrets to a deployment without exposing them in logs—best practice?", Answer = "What is use secure parameters and Key Vault references?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "Two deployments clash because names must be globally unique—common fix pattern?", Answer = "What is unique naming (uniqueString/guid) plus environment prefixes?" },
            new() { Category = "Azure Resource Manager", PointValue = 400, Question = "You need to deploy across multiple resource groups as one solution—what pattern helps?", Answer = "What is modular templates (or Bicep modules) with orchestrated deployments?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "Your template deploys fine in dev but fails in prod; most common root cause?", Answer = "What is differences in permissions/policy restrictions or missing providers/quotas?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "You must prevent unauthorized regions/SKUs at scale—what governance tool pairs well with ARM?", Answer = "What is Azure Policy (initiatives)?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "You need reproducible environments fast for many teams—what’s the big win of ARM/Bicep?", Answer = "What is repeatable, reviewable, one-command infrastructure deployments?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "A deployment succeeded but the app still can’t connect—first place to check?", Answer = "What are the dependent resources’ outputs/config (connection strings, network rules, identity perms)?" },
            new() { Category = "Azure Resource Manager", PointValue = 500, Question = "Your 'What-If' shows deletes you didn’t expect—what mode/scope issue is likely?", Answer = "What is Complete mode or deploying at the wrong scope?" },

            // ==================== AZURE DEVOPS ====================
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps feature runs automated builds and deployments.", Answer = "What are Pipelines?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service provides Git repositories for source control.", Answer = "What are Repos?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This Azure DevOps service tracks work items like user stories, tasks, and bugs.", Answer = "What are Boards?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically building and testing on every commit.", Answer = "What is CI (Continuous Integration)?" },
            new() { Category = "Azure DevOps", PointValue = 100, Question = "This describes automatically releasing changes through environments like dev/test/prod.", Answer = "What is CD (Continuous Delivery/Deployment)?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This pipeline format stores build/release steps as code in a YAML file.", Answer = "What is a YAML pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This component runs pipeline jobs (hosted or self-hosted).", Answer = "What is an agent?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This Azure DevOps concept groups steps like build, test, and deploy under a single logical unit.", Answer = "What is a stage?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This securely connects a pipeline to Azure resources for deployments.", Answer = "What is a service connection?" },
            new() { Category = "Azure DevOps", PointValue = 200, Question = "This stores build outputs so later stages/releases can download them.", Answer = "What are artifacts?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This pipeline feature allows reusing common build/deploy logic across multiple repos.", Answer = "What are templates?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "These help avoid hardcoding values and allow environment-specific configuration.", Answer = "What are variables and variable groups?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is the best practice for reviewing code changes before merging into main.", Answer = "What is a pull request (PR) workflow?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This is a common way to protect the main branch in Azure Repos.", Answer = "What are branch policies (required reviewers/build validation)?" },
            new() { Category = "Azure DevOps", PointValue = 300, Question = "This lets you require manual approval before deploying to production.", Answer = "What are environment approvals/checks?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "A pipeline can’t deploy to Azure due to auth errors—most likely root cause?", Answer = "What is a misconfigured service connection or missing RBAC permissions?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You need to keep secrets out of YAML; the cleanest approach is to use this.", Answer = "What is Key Vault integration (or secret variables)?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "Your pipeline is slow because dependencies download every run—best optimization?", Answer = "What is caching (NuGet/npm) and incremental build strategies?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "You want different deployment behavior per environment; the standard approach is to use this.", Answer = "What are stages with environment-specific variables and approvals?" },
            new() { Category = "Azure DevOps", PointValue = 400, Question = "A PR shows huge unrelated changes—most likely cause?", Answer = "What is the wrong base branch or a branch that wasn’t kept up-to-date?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "Production deploy must be safe and reversible—best pipeline strategy?", Answer = "What is staged rollout with approvals plus blue/green or slot-based deployment?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A secret leaked in configuration. The correct response is to rotate it and move it into this service.", Answer = "What is Azure Key Vault?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need repeatable infrastructure deployments from pipelines—what tool fits best?", Answer = "What is deploying IaC (Bicep/ARM/Terraform) from a pipeline?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "A release succeeds but the app is broken—what should your pipeline have prevented?", Answer = "What is missing automated tests/health checks and deployment verification gates?" },
            new() { Category = "Azure DevOps", PointValue = 500, Question = "You need traceability from backlog item to code to deployment—what combination enables this?", Answer = "What is linking Boards work items to commits/PRs and pipeline runs/releases?" },

            // ==================== TERRAFORM ====================
            new() { Category = "Terraform", PointValue = 100, Question = "This is the open-source Infrastructure as Code tool that uses configuration files to provision resources.", Answer = "What is Terraform?" },
            new() { Category = "Terraform", PointValue = 100, Question = "Terraform configuration files are typically written in this language.", Answer = "What is HCL (HashiCorp Configuration Language)?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this block defines an infrastructure component like an Azure resource.", Answer = "What is a resource?" },
            new() { Category = "Terraform", PointValue = 100, Question = "In Terraform, this defines which cloud/platform you’re talking to, like Azure.", Answer = "What is a provider?" },
            new() { Category = "Terraform", PointValue = 100, Question = "This file often holds pinned provider versions and dependency metadata.", Answer = "What is .terraform.lock.hcl?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command initializes a Terraform working directory and downloads providers.", Answer = "What is terraform init?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command previews changes Terraform will make without applying them.", Answer = "What is terraform plan?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This command creates/updates infrastructure to match the configuration.", Answer = "What is terraform apply?" },
            new() { Category = "Terraform", PointValue = 200, Question = "Terraform tracks deployed resources and mappings in this file/object.", Answer = "What is state?" },
            new() { Category = "Terraform", PointValue = 200, Question = "This Terraform feature lets you parameterize values like names, locations, and sizes.", Answer = "What are variables?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This reusable packaging mechanism helps standardize Terraform infrastructure patterns.", Answer = "What is a module?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This is the best practice for storing state in teams instead of on a developer laptop.", Answer = "What is remote state (a remote backend)?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This detects changes made outside Terraform by comparing real infrastructure to state.", Answer = "What is drift detection?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command formats Terraform code into a standard style.", Answer = "What is terraform fmt?" },
            new() { Category = "Terraform", PointValue = 300, Question = "This command checks Terraform configuration for syntax and basic correctness.", Answer = "What is terraform validate?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This feature prevents multiple people/pipelines from applying changes to the same state at the same time.", Answer = "What is state locking?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This command updates state to match real infrastructure without changing resources.", Answer = "What is terraform refresh (or refresh-only planning)?" },
            new() { Category = "Terraform", PointValue = 400, Question = "You already have resources created manually; this command brings them under Terraform management.", Answer = "What is terraform import?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This capability creates multiple isolated environments using the same configuration.", Answer = "What are workspaces?" },
            new() { Category = "Terraform", PointValue = 400, Question = "This lifecycle setting prevents accidental deletion of critical resources.", Answer = "What is prevent_destroy?" },
            new() { Category = "Terraform", PointValue = 500, Question = "In team deployments, the safest state setup combines remote state with this feature to prevent corruption.", Answer = "What is state locking?" },
            new() { Category = "Terraform", PointValue = 500, Question = "You need secrets for deployments; Terraform best practice is to avoid putting them in state by doing this.", Answer = "What is using a secret store (Key Vault) and passing secrets at runtime, not hardcoding them?" },
            new() { Category = "Terraform", PointValue = 500, Question = "A pipeline shows huge unexpected changes; the first forensic step is to do this.", Answer = "What is run terraform plan and compare state/config drift and provider/version changes?" },
            new() { Category = "Terraform", PointValue = 500, Question = "To enforce consistent standards across projects, teams typically centralize this.", Answer = "What are shared modules (plus policy/guardrails)?" },
            new() { Category = "Terraform", PointValue = 500, Question = "If you must deploy safely to prod, the best pattern is plan → review → apply using this control.", Answer = "What is gated approvals with a saved plan (and locked state)?" },

            // ==================== ANSIBLE ====================
            new() { Category = "Ansible", PointValue = 100, Question = "This is the automation tool that uses playbooks to configure systems and run tasks.", Answer = "What is Ansible?" },
            new() { Category = "Ansible", PointValue = 100, Question = "Ansible automation instructions are typically stored in this type of file.", Answer = "What is a playbook?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This defines the target machines/groups Ansible will manage.", Answer = "What is an inventory?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores non-secret configuration data.", Answer = "What is a ConfigMap?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This stores sensitive configuration data.", Answer = "What is a Secret?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This is the Ansible command-line tool that runs playbooks.", Answer = "What is ansible-playbook?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This mechanism organizes reusable automation into a standard structure.", Answer = "What is a role?" },
            new() { Category = "Ansible", PointValue = 200, Question = "These run only when notified, often used to restart a service after a config change.", Answer = "What are handlers?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This binds configuration into a strongly-typed options class.", Answer = "What is IOptions<T>?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This syntax runs a script located in the current folder in PowerShell.", Answer = "What is .\\script.ps1?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This feature gathers system information like OS, IP addresses, and disks.", Answer = "What are facts (setup)?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This lets you select which tasks to run based on tags.", Answer = "What are tags?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This mode shows what will change without applying changes.", Answer = "What is check mode?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This templating system is commonly used for generating config files in Ansible.", Answer = "What is Jinja2?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This is the best practice for keeping your playbooks readable and modular.", Answer = "What is using roles with small, focused tasks?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This Ansible feature encrypts secrets like passwords inside your repo.", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 400, Question = "Your playbook keeps reporting changes even when nothing changed; the likely issue is what?", Answer = "What is a non-idempotent task/module usage?" },
            new() { Category = "Ansible", PointValue = 400, Question = "You need cloud hosts to appear automatically in inventory; the standard solution is what?", Answer = "What is dynamic inventory?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This debugging option shows detailed output about which tasks ran and why.", Answer = "What is running with increased verbosity (-v/-vv/-vvv)?" },
            new() { Category = "Ansible", PointValue = 400, Question = "This is the clean way to avoid repeating the same steps across many playbooks.", Answer = "What are roles (and includes)?" },
            new() { Category = "Ansible", PointValue = 500, Question = "Best practice: store no plaintext secrets in Git—Ansible’s built-in answer is what?", Answer = "What is Ansible Vault?" },
            new() { Category = "Ansible", PointValue = 500, Question = "Your automation must be safe to rerun in CI/CD; the core requirement is what?", Answer = "What is idempotency?" },
            new() { Category = "Ansible", PointValue = 500, Question = "A playbook works manually but fails in pipeline; the first suspect is usually what?", Answer = "What is environment/credentials/inventory differences in CI?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To standardize systems at scale without “snowflake servers,” your approach should be what?", Answer = "What is configuration as code with repeatable playbooks/roles?" },
            new() { Category = "Ansible", PointValue = 500, Question = "To reduce blast radius in automation, you should design playbooks to do this.", Answer = "What is scope changes narrowly, validate first, and fail fast on errors?" },

            // ==================== CONTAINER REGISTRY ====================
            new() { Category = "Container Registry", PointValue = 100, Question = "This Azure service stores and manages Docker container images.", Answer = "What is Azure Container Registry (ACR)?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "A container image is stored in a registry under this logical grouping name.", Answer = "What is a repository?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This label identifies a specific version of an image, like v1.2.3 or latest.", Answer = "What is a tag?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action uploads a local image to a registry.", Answer = "What is a push?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This action downloads an image from a registry to a machine or cluster.", Answer = "What is a pull?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command sends a built image to the registry.", Answer = "What is docker push?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This Docker command downloads an image from the registry.", Answer = "What is docker pull?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This ACR authentication helper command logs Docker into your registry.", Answer = "What is az acr login?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the main security risk of using the registry admin user.", Answer = "What is broad shared credentials (high blast radius)?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the preferred way for Azure services to pull images without storing passwords.", Answer = "What is managed identity with RBAC?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This ACR feature can build images in the cloud when you push code or run a task.", Answer = "What are ACR Tasks?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This security practice ensures images are regularly checked for known CVEs.", Answer = "What is vulnerability scanning?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This network feature keeps registry access private inside Azure networks.", Answer = "What is a private endpoint (Private Link)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This controls who can pull images using roles like AcrPull.", Answer = "What is RBAC (role-based access control)?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This best practice keeps versions predictable and reduces production surprises.", Answer = "What is using immutable, versioned tags (avoid relying on latest)?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "Your pods can’t pull images from ACR; the first thing to verify is this.", Answer = "What is registry permissions (AcrPull) and authentication configuration?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "You need images available close to multiple regions; the ACR capability is what?", Answer = "What is geo-replication?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "This reduces supply-chain risk by preventing unsigned/untrusted images from being used.", Answer = "What is content trust / image signing (conceptually)?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "Logging on storage is used for auditing and this.", Answer = "What is troubleshooting?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "A common place to query storage logs/metrics is Azure Monitor and this workspace.", Answer = "What is Log Analytics?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "For least privilege, your AKS/Container Apps should pull images using what identity approach?", Answer = "What is managed identity (or workload identity) with AcrPull RBAC?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "A build pipeline needs secure image provenance; the best pattern is what?", Answer = "What is build in CI, scan, sign, then deploy only signed images?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "Your registry must not be publicly reachable; the key configuration is what?", Answer = "What is private endpoint plus restricted public network access?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You deployed the wrong image because a tag moved; the prevention is what?", Answer = "What is pinning by digest or immutable version tags?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You suspect compromised images; the first immediate action is what?", Answer = "What is block/rotate credentials, quarantine images, and audit pull activity/logs?" },

            // ==================== APP CONFIGURATION ====================
            new() { Category = "App Configuration", PointValue = 100, Question = "This Azure service centrally stores application settings as key-value pairs.", Answer = "What is Azure App Configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "In App Configuration, this is the basic unit used to store a setting name and value.", Answer = "What is a key-value pair?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This feature lets you logically separate settings for dev/test/prod using the same key names.", Answer = "What are labels?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This describes reading config from a centralized store instead of hardcoding values in code.", Answer = "What is externalized configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This is the most common reason to use App Configuration instead of appsettings.json only.", Answer = "What is centralized configuration management?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This App Configuration feature lets you safely turn features on/off without redeploying.", Answer = "What is Feature Management (feature flags)?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This Azure service should store secrets, not App Configuration.", Answer = "What is Azure Key Vault?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This practice helps avoid restarting apps constantly by updating config without redeploy.", Answer = "What is dynamic configuration refresh?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is the standard .NET concept used to bind configuration into strongly typed objects.", Answer = "What is IOptions<T>?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is a best practice for config keys so teams can find things quickly.", Answer = "What is a consistent naming convention (namespacing)?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This enables automatic refresh when a sentinel key changes.", Answer = "What is a refresh sentinel key?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the common pattern: settings in App Configuration, secrets in Key Vault, linked by this.", Answer = "What are Key Vault references?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This principle means your app should still run with safe defaults if config is unavailable.", Answer = "What is graceful degradation (fallback defaults)?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the best reason to use labels rather than separate apps for each environment.", Answer = "What is environment isolation with one shared configuration store?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This access approach avoids storing credentials in code when reading App Configuration from Azure.", Answer = "What is managed identity?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Your app reads old values after you updated keys—most likely missing feature?", Answer = "What is configuration refresh (or caching invalidation)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "To prevent config drift across teams, you should treat configuration like this.", Answer = "What is versioned, reviewed change management (config as code/process)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You want different feature flag behavior per environment; you should use this.", Answer = "What are labels (and environment-specific flags)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You need to limit who can change production settings; the right control is this.", Answer = "What is RBAC with least privilege?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Your app fails only in Azure but works locally; the first suspect for App Configuration is this.", Answer = "What is identity/permissions (managed identity not granted)?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You need safe releases with instant rollback without redeploying—best tool combo?", Answer = "What are feature flags plus staged rollout?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "A bad config change took prod down—best prevention pattern?", Answer = "What is validation + approvals + gradual rollout (and safe defaults)?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You must rotate secrets without redeploy; where should secrets live and how should apps reference them?", Answer = "What is Key Vault with Key Vault references from configuration?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "Your system needs consistent settings across microservices; the architecture goal is this.", Answer = "What is centralized configuration with controlled refresh and governance?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "If a feature flag system causes outages, the top design mistake is this.", Answer = "What is not planning for flag failure (no fallback, hard dependency)?" },

            // ==================== CONTAINER APPS ====================
            new() { Category = "Container Apps", PointValue = 100, Question = "This Azure service runs containerized apps without you managing Kubernetes directly.", Answer = "What is Azure Container Apps?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This feature exposes your container app to HTTP traffic.", Answer = "What is Ingress?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This Container Apps concept represents a versioned deployment of your app.", Answer = "What is a Revision?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This is the main benefit of Container Apps compared to managing your own cluster.", Answer = "What is reduced infrastructure management?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This is what Container Apps runs under the hood: container images.", Answer = "What is a Container Image?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This scaling approach increases or decreases instances based on events like queue length.", Answer = "What is event-driven autoscaling?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is commonly used to store settings for your container app without changing the image.", Answer = "What are environment variables (app settings)?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the recommended way for a container app to access Azure resources without secrets.", Answer = "What is Managed Identity?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This describes how Container Apps can scale down when idle to reduce cost.", Answer = "What is scale to zero?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the safest way to roll out a new version while keeping the old one available.", Answer = "What is traffic splitting between revisions?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This feature provides service-to-service helpers like retries and service discovery when enabled.", Answer = "What is Dapr integration?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This type of ingress keeps the app private and reachable only within the environment/network.", Answer = "What is internal ingress?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is a best practice for containerized apps: keep state out of the container and store it here.", Answer = "What is an external data store (like a database or storage)?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is a common reason a container app fails to start: the app listens on the wrong port.", Answer = "What is incorrect port configuration?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is the purpose of health probes in containerized hosting.", Answer = "What is detecting unhealthy instances?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "You need to run background processing triggered by a queue without managing servers. This pairing is common.", Answer = "What are Container Apps plus Service Bus (or Queue)?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "Your container app works locally but fails in Azure. The first thing to confirm is that it binds to this address.", Answer = "What is 0.0.0.0?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This deployment technique avoids breaking production by testing the new revision first.", Answer = "What is deploying a new revision with zero traffic initially?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This is the key benefit of 'scale to zero' for spiky workloads.", Answer = "What is cost efficiency?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "This is the main security win of Managed Identity compared to connection strings with passwords.", Answer = "What is eliminating stored secrets?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your API must stay responsive during deployments. The most important Container Apps feature to use is this.", Answer = "What is revision-based deployment with traffic splitting?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "You want your app private to a network and not public on the internet. The correct choice is this.", Answer = "What is internal ingress?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your processing must handle duplicates because event-driven scaling can cause repeats. The required design principle is this.", Answer = "What is idempotency?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "You need cross-service reliability features like retries and pub/sub without writing tons of plumbing. Container Apps can use this.", Answer = "What is Dapr?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "A container app keeps crashing on startup. The fastest first diagnostic to check is this.", Answer = "What are the container logs?" },

            // ==================== MICROSOFT POWER PLATFORM ====================
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool is used to build low-code business apps.", Answer = "What is Power Apps?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool automates workflows between services.", Answer = "What is Power Automate?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform tool is used for dashboards and business intelligence reporting.", Answer = "What is Power BI?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "This Power Platform data service stores tables/rows used by apps and flows.", Answer = "What is Dataverse?" },
            new() { Category = "Microsoft Power Platform", PointValue = 100, Question = "These are prebuilt integrations that let Power Platform talk to other services.", Answer = "What are connectors?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This is the secure boundary where Power Platform apps, flows, and data live.", Answer = "What is an environment?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This packaging method groups Power Platform components for deployment and reuse.", Answer = "What is a solution?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This Power Apps type builds responsive apps that run in browsers and mobile devices.", Answer = "What is a canvas app?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This Power Apps type is model-driven and uses Dataverse as the data layer.", Answer = "What is a model-driven app?" },
            new() { Category = "Microsoft Power Platform", PointValue = 200, Question = "This feature restricts which connectors can be used together to prevent data leakage.", Answer = "What are DLP (Data Loss Prevention) policies?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is used to connect on-premises data sources to Power Platform securely.", Answer = "What is an on-premises data gateway?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is the event that starts a Power Automate flow.", Answer = "What is a trigger?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is a best practice for deploying changes: build inside a solution and move between these.", Answer = "What are dev/test/prod environments?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This controls who can create, run, or share Power Platform resources.", Answer = "What is role-based access control (security roles)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 300, Question = "This is the Power Platform approach to storing reusable business logic and rules for Dataverse tables.", Answer = "What are business rules (and Dataverse logic)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "You need a flow to call an API securely without storing a password in the flow—best approach?", Answer = "What is using managed identity or secure connections (plus Key Vault where applicable)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "A flow keeps running twice for the same event—most likely explanation?", Answer = "What is trigger retries/at-least-once delivery requiring idempotent design?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "Your Power BI dataset refresh fails only in the service, not locally—common root cause?", Answer = "What is gateway/credentials/network access not configured correctly?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "You want controlled releases and rollback for Power Platform components—best practice?", Answer = "What is solutions with versioning and environment-based deployment?" },
            new() { Category = "Microsoft Power Platform", PointValue = 400, Question = "An org needs consistent connector restrictions across environments—what control enforces that?", Answer = "What are DLP policies scoped to environments?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A low-code app must meet enterprise governance: the key success requirement is what?", Answer = "What is environment strategy plus security roles plus DLP and auditing?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A flow fails intermittently calling a third-party API—what reliability pattern should you add?", Answer = "What is retries with exponential backoff and circuit-breaker style handling?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "A solution import succeeds but the app behaves wrong—most likely cause?", Answer = "What is missing environment variables/connection references or mismatched permissions?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "You must prove who changed an app/flow and when—what capability do you rely on?", Answer = "What is auditing and activity logs (admin monitoring)?" },
            new() { Category = "Microsoft Power Platform", PointValue = 500, Question = "Power Platform is adopted fast and chaos follows—what’s the cleanest first fix?", Answer = "What is establish governance standards before scaling adoption?" },

            // ==================== DATA ANALYTICS ====================
            new() { Category = "Data Analytics", PointValue = 100, Question = "This type of analytics summarizes what happened in the past.", Answer = "What is descriptive analytics?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "A metric used to measure business success like conversion rate is called a what?", Answer = "What is a KPI (Key Performance Indicator)?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This is a visual collection of charts that tells a data story at a glance.", Answer = "What is a dashboard?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This is raw facts collected from systems before cleaning or modeling.", Answer = "What is raw data?" },
            new() { Category = "Data Analytics", PointValue = 100, Question = "This process finds patterns and insights from data to support decisions.", Answer = "What is data analysis?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This type of analytics predicts what is likely to happen next.", Answer = "What is predictive analytics?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This type of analytics recommends actions to take based on data.", Answer = "What is prescriptive analytics?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This is the process of extracting, transforming, and loading data.", Answer = "What is ETL?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "This is the process of extracting and loading first, then transforming in the destination.", Answer = "What is ELT?" },
            new() { Category = "Data Analytics", PointValue = 200, Question = "A structured store optimized for reporting and analysis is called a what?", Answer = "What is a data warehouse?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "A storage system for large volumes of raw and semi-structured data is called a what?", Answer = "What is a data lake?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This model organizes analytics data into fact tables and dimension tables.", Answer = "What is a star schema?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This problem happens when the same metric is calculated differently by different teams.", Answer = "What is metric inconsistency (definition drift)?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This is a common technique to make large datasets faster to query.", Answer = "What is partitioning?" },
            new() { Category = "Data Analytics", PointValue = 300, Question = "This is the practice of validating accuracy, completeness, and consistency of data.", Answer = "What is data quality management?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "A dashboard number is wrong even though pipelines succeeded—most likely cause?", Answer = "What is transformation/join logic errors or duplicated records?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "A report is slow at scale; the most common first fix is to reduce this.", Answer = "What is the amount of data scanned (filter early, aggregate, use partitions)?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "If you need trusted definitions for KPIs across the org, you should create this.", Answer = "What is a semantic layer (shared model)?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "This governance practice tracks where data comes from and how it changes.", Answer = "What is data lineage?" },
            new() { Category = "Data Analytics", PointValue = 400, Question = "You must control who can see sensitive columns—what capability is required?", Answer = "What is access control and data classification (least privilege)?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "Your analytics pipeline produces different results each run—what’s the likely root cause?", Answer = "What is non-deterministic transformations or missing versioned source snapshots?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "Leadership wants real-time insights; the correct architecture shift is toward this.", Answer = "What is streaming analytics (event-driven ingestion)?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "A model performs great in test but fails in production due to changing patterns. The most likely cause is this.", Answer = "What is data drift or concept drift?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "The fastest way to destroy trust in analytics is this failure mode.", Answer = "What is inconsistent metric definitions and ungoverned changes?" },
            new() { Category = "Data Analytics", PointValue = 500, Question = "You need audit-ready reporting; the key operational requirement is what?", Answer = "What is governed change control plus immutable logs/auditing?" },

            // ==================== LOGGING & MONITORING ====================
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Azure service collects and analyzes log data from Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Azure feature provides a centralized place to view and query logs from multiple resources.", Answer = "What is Log Analytics?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This command line tool allows you to query Azure resources using a SQL-like syntax.", Answer = "What is Azure CLI?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This Kusto query language clause filters results based on a condition.", Answer = "What is where?" },
            new() { Category = "Logging & Monitoring", PointValue = 100, Question = "This is the default retention period for logs in Log Analytics.", Answer = "What is 30 days?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This Azure service provides real-time monitoring and alerting for Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This metric measures the time taken to process requests.", Answer = "What is response time?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This setting in Azure Monitor specifies the action taken when a condition is met.", Answer = "What is an alert rule?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This Azure feature helps you visualize and analyze metrics over time.", Answer = "What are metric charts?" },
            new() { Category = "Logging & Monitoring", PointValue = 200, Question = "This query language is used to analyze log data in Azure Monitor.", Answer = "What is KQL (Kusto Query Language)?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This Log Analytics query calculates the average of a numerical field.", Answer = "What is avg()?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This query summarizes data into bins based on time intervals.", Answer = "What is bin()?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This table in Log Analytics contains every request to your Azure resources.", Answer = "What is the Request table?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This command sends custom log data to Azure Monitor.", Answer = "What is the Azure Monitor HTTP Data Collector API?" },
            new() { Category = "Logging & Monitoring", PointValue = 300, Question = "This alert type in Azure Monitor triggers based on log query results.", Answer = "What is a log alert?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This Log Analytics function joins two tables based on a common key.", Answer = "What is join?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This feature helps you analyze the impact of changes by comparing metrics before and after.", Answer = "What is metric baseline?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "You need to keep certain logs for 5 years for compliance; this is how you achieve that.", Answer = "What is configuring log retention policies?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This KQL keyword specifies that a field must exist in the results.", Answer = "What is has?" },
            new() { Category = "Logging & Monitoring", PointValue = 400, Question = "This feature enables real-time updating of dashboards and views in Azure Monitor.", Answer = "What is live metrics stream?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "A sudden outage; first, ensure this critical monitoring signal is intact.", Answer = "What is alerting on failure rates and response times?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "To troubleshoot high latency, correlate these signals: request logs, dependencies, and this.", Answer = "What is performance (duration) metrics?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "Logs show an app is slow, but metrics are fine—most likely issue?", Answer = "What is a dependency (DB, API) latency causing overall slowness?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "Your first day in a new Azure environment; this is the best first health check.", Answer = "What is review the Azure Activity Log for recent changes and errors?" },
            new() { Category = "Logging & Monitoring", PointValue = 500, Question = "To improve query performance and reduce costs, always filter on this in Log Analytics.", Answer = "What is time (where timestamp >= ...)?" },
        ];
    }
}