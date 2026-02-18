using MSSA_Jeopardy.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSSA_Jeopardy.Client.Services;

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

            new() { Category = "MS-4010 (Security)", PointValue = 500, Question = "What Azure feature provides recommendations to improve security posture?", Answer = "What is Microsoft Defender for Cloud?" },
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

            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF component provides the compute and networking used to move data.", Answer = "What is an Integration Runtime (IR)?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This feature runs a pipeline on a schedule or based on events.", Answer = "What is a Trigger?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF capability transforms data using a visual, Spark-based experience.", Answer = "What is Mapping Data Flow?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This pipeline feature lets you reuse logic by passing in values like table names or dates.", Answer = "What are Parameters?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This is the common difference: ETL transforms data before loading, while ELT transforms data after loading into this.", Answer = "What is the target data store (like a data warehouse)?" },

            new() { Category = "Data Factory", PointValue = 300, Question = "This Integration Runtime type is used to access on-premises data sources behind a firewall.", Answer = "What is Self-hosted Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF concept is used to branch pipeline logic based on success/failure or conditions.", Answer = "What is control flow?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This is the primary monitoring view in ADF to see pipeline runs and activity failures.", Answer = "What is Monitor hub (monitoring)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This is the best practice for secrets used in linked services.", Answer = "What is storing secrets in Key Vault?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This trigger type runs pipelines at fixed intervals with aligned time windows.", Answer = "What is a tumbling window trigger?" },

            new() { Category = "Data Factory", PointValue = 400, Question = "This common approach loads only new/changed rows using a 'last updated' value.", Answer = "What is incremental load (watermark pattern)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "A pipeline step fails intermittently due to network hiccups. The first reliability knob to use is this.", Answer = "What is retry policy?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This is the typical way to make pipelines reusable across dev/test/prod without rewriting them.", Answer = "What is parameterization and environment-specific configuration?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Your copy activity is slow because it moves too much data. The best first fix is to do this.", Answer = "What is filter and partition the copy (move only what you need)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "This ADF design principle improves reliability by making pipeline steps safe to rerun.", Answer = "What is idempotency?" },

            new() { Category = "Data Factory", PointValue = 500, Question = "You need to orchestrate dozens of sources, schedule loads, and monitor failures with minimal custom code. Use this service.", Answer = "What is Azure Data Factory (ADF)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "A data load ran twice and created duplicate records. Your pipeline must be designed to be this.", Answer = "What is idempotent?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "On-premises data can't be reached from ADF. The missing component is most likely this.", Answer = "What is Self-hosted Integration Runtime?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "A pipeline works in dev but fails in prod due to secrets. The correct secret handling approach is this.", Answer = "What is using Key Vault-backed secrets?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "You must minimize risk during big data migrations. The safest strategy is to validate counts and critical queries after this step.", Answer = "What is cutover?" },

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
            new() { Category = "Service Bus", PointValue = 200, Question = "This feature prevents the same message from being processed more than once within a time window.", Answer = "What is Duplicate Detection?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This is the name of the capability that lets you filter messages on a subscription.", Answer = "What are Subscription Rules (Filters)?" },

            new() { Category = "Service Bus", PointValue = 300, Question = "This scenario is a best fit for a Topic instead of a Queue: one message fans out to multiple independent consumers.", Answer = "What is publish-subscribe messaging?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This is the recommended way to process messages reliably: lock, process, then explicitly complete.", Answer = "What is Peek-Lock with Complete()?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This feature lets you postpone delivery of a message until a future date/time.", Answer = "What is Scheduled Delivery?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This capability lets you move a message aside for later retrieval without losing it.", Answer = "What is Message Deferral?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This is the most common reason a message gets dead-lettered automatically.", Answer = "What is max delivery count exceeded?" },

            new() { Category = "Service Bus", PointValue = 400, Question = "This is the best practice to safely handle retries where the same message might be delivered twice.", Answer = "What is idempotent processing?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This concept describes how long a receiver has to complete a locked message before it becomes available again.", Answer = "What is Lock Duration?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the strategy to scale message processing horizontally.", Answer = "What is using competing consumers (multiple receivers)?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the Service Bus tier typically used when you need predictable performance and higher throughput.", Answer = "What is Premium tier?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the protocol commonly used by Service Bus clients for efficient messaging.", Answer = "What is AMQP?" },

            new() { Category = "Service Bus", PointValue = 500, Question = "Your processor crashes after receiving a message. To avoid message loss, you should not use this receive mode.", Answer = "What is Receive-and-Delete?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You need strict ordering plus stateful processing per group. This Service Bus feature is the right choice.", Answer = "What are Sessions?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You see repeated processing of the same message after transient failures. The most correct design goal to enforce is this.", Answer = "What is idempotency?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "This technique helps prevent poisoned messages from blocking the queue forever.", Answer = "What is dead-lettering after max delivery attempts?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "For secure access from an Azure-hosted app without storing secrets, you should authenticate using this.", Answer = "What is Managed Identity (with RBAC)?" },

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

            new() { Category = "Event Grid", PointValue = 300, Question = "This is the correct design mindset for Event Grid: respond to events, not poll resources repeatedly.", Answer = "What is event-driven architecture?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This setting controls how Event Grid retries delivery before giving up.", Answer = "What is a Retry Policy?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is the Event Grid feature used to route events to different handlers based on event properties.", Answer = "What is advanced filtering?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is a key difference: Event Grid is for events, while this service is commonly used for command-style messaging.", Answer = "What is Service Bus?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is the best practice to avoid duplicate side effects when handlers might receive the same event more than once.", Answer = "What is idempotent event handling?" },

            new() { Category = "Event Grid", PointValue = 400, Question = "You need near-real-time reaction to Azure resource changes with minimal code. This service is typically the simplest.", Answer = "What is Event Grid?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "An event handler is down temporarily. Event Grid will do this to increase reliability.", Answer = "What is retry delivery?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This is the most common reason Event Grid deliveries fail repeatedly.", Answer = "What is a failing/unreachable endpoint?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "You want to subscribe to events from a specific Azure resource (like a storage account). You should use this topic type.", Answer = "What is a System Topic?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "To avoid public exposure while receiving events, you typically secure the handler with this.", Answer = "What is authentication/authorization (for example, Entra ID or function keys)?" },

            new() { Category = "Event Grid", PointValue = 500, Question = "You are building a workflow that must not lose messages and requires FIFO ordering. Event Grid is not ideal; this service is.", Answer = "What is Service Bus?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "Your handler processes the same event twice due to retries. The correct fix is to make the handler do this.", Answer = "What is be idempotent?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "Events are piling up in dead-letter storage. The most likely root cause to investigate first is this.", Answer = "What is endpoint failures or authorization errors?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You want to route events based on a subject prefix like '/blobServices/default/containers/images/'. You should use this.", Answer = "What is subject filtering?" },
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

            // ==================== CONTAINER REGISTRY ====================
            new() { Category = "Container Registry", PointValue = 100, Question = "This Azure service stores and manages private container images.", Answer = "What is Azure Container Registry (ACR)?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "A container image is stored in a registry under this grouping.", Answer = "What is a Repository?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This label identifies a specific version of an image like 'v1.2.0'.", Answer = "What is an Image Tag?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This operation uploads a local image to a registry.", Answer = "What is push?" },
            new() { Category = "Container Registry", PointValue = 100, Question = "This operation downloads an image from a registry to a machine.", Answer = "What is pull?" },

            new() { Category = "Container Registry", PointValue = 200, Question = "This ACR feature can build container images in the cloud automatically.", Answer = "What are ACR Tasks?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is the recommended auth method for Azure services pulling images without storing passwords.", Answer = "What is Managed Identity (or Entra ID integration)?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This feature can notify a service when an image is pushed.", Answer = "What are Webhooks?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This is a common reason image pulls fail from ACR.", Answer = "What is missing permissions/authorization?" },
            new() { Category = "Container Registry", PointValue = 200, Question = "This describes keeping your images private instead of using Docker Hub publicly.", Answer = "What is a private registry?" },

            new() { Category = "Container Registry", PointValue = 300, Question = "This is the security risk of enabling an admin user on a registry.", Answer = "What is long-lived shared credentials?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This is a best practice for production deployments: reference images by this for immutability.", Answer = "What is an image digest?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This feature replicates your registry closer to users in multiple regions.", Answer = "What is geo-replication?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This concept helps ensure your deployments use the exact image you tested.", Answer = "What is immutable versioning?" },
            new() { Category = "Container Registry", PointValue = 300, Question = "This type of artifact standard is commonly used by modern registries beyond just Docker images.", Answer = "What are OCI artifacts?" },

            new() { Category = "Container Registry", PointValue = 400, Question = "Your deployment pulls 'latest' and breaks unexpectedly. The correct fix is to use this.", Answer = "What is pinning a version tag or digest?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "You need automated builds when code changes. ACR can do this with ACR Tasks plus this trigger.", Answer = "What is a source control trigger (like Git commit/webhook)?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "A secure way to allow a workload to pull images is to assign it this permission.", Answer = "What is AcrPull role?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "This is the best reason to scan container images regularly.", Answer = "What is vulnerability detection?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "This is the advantage of keeping images in the same cloud region as your compute.", Answer = "What is reduced latency and egress risk?" },

            new() { Category = "Container Registry", PointValue = 500, Question = "A production incident happened because an image tag was overwritten. The strongest defense is to deploy by this.", Answer = "What is an image digest?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "Your service cannot pull from ACR after enabling private networking. The most likely missing piece is this.", Answer = "What is correct network access (like private endpoint/DNS)?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You want least privilege for a workload that only needs to download images. The correct role is this.", Answer = "What is AcrPull?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You need disaster tolerance and faster pulls worldwide. The ACR feature designed for that is this.", Answer = "What is geo-replication?" },
            new() { Category = "Container Registry", PointValue = 500, Question = "You must avoid embedding secrets for registry access in configs. The correct Azure-native approach is this.", Answer = "What is Managed Identity with RBAC?" },

            // ==================== APP CONFIGURATION ====================
            new() { Category = "App Configuration", PointValue = 100, Question = "This Azure service centralizes application settings as key-value pairs.", Answer = "What is Azure App Configuration?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This App Configuration feature lets you turn features on or off without redeploying.", Answer = "What are Feature Flags?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This concept helps separate settings for dev, test, and prod without different keys.", Answer = "What are Labels?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This is the primary benefit of centralizing config instead of hardcoding values.", Answer = "What is easier management and safer changes?" },
            new() { Category = "App Configuration", PointValue = 100, Question = "This is the common data shape stored in App Configuration.", Answer = "What are key-value settings?" },

            new() { Category = "App Configuration", PointValue = 200, Question = "This integration lets you reference secrets securely while keeping config non-secret.", Answer = "What are Key Vault references?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This feature allows apps to refresh configuration without restarting.", Answer = "What is dynamic configuration refresh?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is a best practice: store secrets in Key Vault and store non-secret settings here.", Answer = "What is App Configuration?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This is the purpose of Feature Flags in deployments.", Answer = "What is safer rollouts and experimentation?" },
            new() { Category = "App Configuration", PointValue = 200, Question = "This security approach grants access to config without storing credentials.", Answer = "What is Managed Identity?" },

            new() { Category = "App Configuration", PointValue = 300, Question = "This pattern uses a special key to trigger refresh checks efficiently.", Answer = "What is a Sentinel key?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is a practical example of a feature flag use: enable a new UI for 10% of users.", Answer = "What is gradual rollout?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This practice prevents config changes from breaking production unexpectedly.", Answer = "What is environment-specific labeling and testing?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the recommended way to keep config changes auditable and repeatable.", Answer = "What is configuration as code (versioned settings)?" },
            new() { Category = "App Configuration", PointValue = 300, Question = "This is the common reason to separate secrets from config values.", Answer = "What is security and least privilege?" },

            new() { Category = "App Configuration", PointValue = 400, Question = "Your app reads config at startup only. To support live updates, you need to implement this.", Answer = "What is configuration refresh (reload) logic?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You want the same key name but different values per environment. App Configuration solves this using this.", Answer = "What are Labels?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Feature flags reduce deployment risk by allowing you to do this.", Answer = "What is decouple release from deploy?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "You want to restrict who can change production flags. The correct control is this.", Answer = "What is RBAC (role-based access control)?" },
            new() { Category = "App Configuration", PointValue = 400, Question = "Your config is correct but users still see old behavior. The most likely missing piece is this.", Answer = "What is refresh not being triggered or cached values not updating?" },

            new() { Category = "App Configuration", PointValue = 500, Question = "A production feature must be disabled instantly without redeploying. The fastest tool for that is this.", Answer = "What are Feature Flags?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "A secret accidentally ended up in App Configuration. The correct remediation is this.", Answer = "What is rotate the secret and move it to Key Vault?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You need safe experiments with users while controlling blast radius. The best technique here is this.", Answer = "What is percentage-based feature flag rollout?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "Your app should authenticate to App Configuration without credentials stored anywhere. Use this.", Answer = "What is Managed Identity with RBAC?" },
            new() { Category = "App Configuration", PointValue = 500, Question = "You must ensure a configuration change is consistent across multiple services instantly. The architecture win of App Configuration is this.", Answer = "What is centralized configuration management?" },
            // ==================== POWERSHELL & CLI ====================
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell cmdlet lists files and folders in a directory.", Answer = "What is Get-ChildItem?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This symbol starts a variable name in PowerShell.", Answer = "What is the dollar sign ($)?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This operator sends the output of one command into another command.", Answer = "What is the pipe (|)?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This Azure CLI command signs you into Azure interactively.", Answer = "What is az login?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell cmdlet displays built-in documentation for commands.", Answer = "What is Get-Help?" },

            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This PowerShell switch simulates an action without making changes.", Answer = "What is -WhatIf?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This Azure CLI command lists all resource groups in the current subscription.", Answer = "What is az group list?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This PowerShell comparison operator means not equal.", Answer = "What is -ne?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This is the typical way to run a script in the current folder in PowerShell.", Answer = "What is .\\script.ps1?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This PowerShell concept controls whether scripts can run on a machine.", Answer = "What is Execution Policy?" },

            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell cmdlet processes each item flowing through the pipeline.", Answer = "What is ForEach-Object?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This Azure CLI command sets the active subscription for future commands.", Answer = "What is az account set?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell cmdlet converts an object into JSON text.", Answer = "What is ConvertTo-Json?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell variable contains the exit code of the last external program.", Answer = "What is $LASTEXITCODE?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This parameter forces many non-terminating errors to behave like terminating errors.", Answer = "What is -ErrorAction Stop?" },

            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This code structure provides structured exception handling and cleanup.", Answer = "What is try/catch/finally?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This Azure CLI command is used to call Azure REST endpoints directly.", Answer = "What is az rest?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This is the key difference: Write-Host prints to the screen, while this cmdlet outputs to the pipeline.", Answer = "What is Write-Output?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This phrase means running an automation twice should reach the same end state.", Answer = "What is idempotent?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This setting makes PowerShell treat errors as terminating by default.", Answer = "What is $ErrorActionPreference = 'Stop'?" },

            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This Azure CLI command deploys an ARM/Bicep template at the resource-group scope.", Answer = "What is az deployment group create?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This is the safest way to avoid hardcoding secrets in automation scripts.", Answer = "What is Managed Identity with Key Vault?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "If Azure CLI shows AuthorizationFailed, the first two things to verify are correct role assignment and this.", Answer = "What is the correct subscription context?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This automation principle means validating inputs early and stopping immediately on failure.", Answer = "What is fail fast?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This Azure feature helps automate cleanup and targeting by tagging resources consistently.", Answer = "What are tags?" },

            // ==================== AGILE & SCRUM ====================
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This time-boxed iteration in Scrum typically lasts one to four weeks.", Answer = "What is a Sprint?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This ordered list contains all desired product work items.", Answer = "What is the Product Backlog?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This daily 15-minute ceremony aligns the team on progress and blockers.", Answer = "What is the Daily Scrum (Standup)?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This role owns backlog priority and maximizes product value.", Answer = "What is the Product Owner?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This ceremony is the end-of-sprint demo and feedback session.", Answer = "What is the Sprint Review?" },

            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This ceremony defines the sprint goal and selects backlog items for the sprint.", Answer = "What is Sprint Planning?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This ceremony focuses on improving how the team works.", Answer = "What is the Retrospective?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This common user story format begins with these three words.", Answer = "What is 'As a'?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This measurement is used to estimate relative effort, complexity, and risk.", Answer = "What are Story Points?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This backlog contains the work the team commits to during the sprint.", Answer = "What is the Sprint Backlog?" },

            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This shared checklist defines when work is truly complete.", Answer = "What is the Definition of Done?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This chart shows remaining work over time in a sprint.", Answer = "What is a Burndown Chart?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This metric reflects how many story points a team completes per sprint on average.", Answer = "What is Velocity?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This activity clarifies, splits, and estimates upcoming backlog items.", Answer = "What is Backlog Refinement?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This standup anti-pattern turns the daily scrum into a manager status meeting.", Answer = "What is reporting to a manager instead of coordinating as a team?" },

            new() { Category = "Agile & Scrum", PointValue = 400, Question = "If the sprint goal is threatened mid-sprint, the best move is to renegotiate scope with this role.", Answer = "What is the Product Owner?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is uncontrolled work added mid-sprint that endangers commitments.", Answer = "What is scope creep?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is why sprint time-boxing matters: it forces prioritization and creates a predictable cadence.", Answer = "What is a time-box?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is a large body of work that is too big for a single sprint.", Answer = "What is an Epic?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This limit reduces multitasking by restricting how many items can be in progress.", Answer = "What is a WIP limit?" },

            new() { Category = "Agile & Scrum", PointValue = 500, Question = "If velocity is unstable, the first improvement is usually better story slicing and this.", Answer = "What is consistent estimation?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "This is the most valuable focus for a team: being busy or finishing usable work.", Answer = "What is finishing usable work (Done)?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "A team repeatedly misses sprint commitments; the most common root cause is this.", Answer = "What is over-committing work?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "This is the cleanest way to handle urgent new work during a sprint.", Answer = "What is swapping scope only with Product Owner agreement?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Retrospectives fail most often because action items have no owner and no follow-through, meaning no real this.", Answer = "What is accountability?" },

            // ==================== AZ-900 (AZURE FUNDAMENTALS) ====================
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This cloud service model provides virtual machines, networking, and storage as building blocks.", Answer = "What is Infrastructure as a Service (IaaS)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This Azure concept is a geographic area that contains one or more datacenters.", Answer = "What is an Azure Region?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This Azure feature groups resources together for lifecycle management and billing clarity.", Answer = "What is a Resource Group?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This term describes paying only for what you use in the cloud.", Answer = "What is pay-as-you-go?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This Microsoft tool is the web-based interface used to manage Azure resources.", Answer = "What is the Azure Portal?" },

            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This model splits security responsibilities between Microsoft and the customer.", Answer = "What is the Shared Responsibility Model?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This Azure governance service enforces rules like allowed regions or required tags.", Answer = "What is Azure Policy?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This is the Azure identity service used for authentication and authorization.", Answer = "What is Azure Active Directory (Microsoft Entra ID)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This feature provides datacenter-level redundancy inside a single region.", Answer = "What are Availability Zones?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This tool helps estimate Azure service costs before deployment.", Answer = "What is the Azure Pricing Calculator?" },

            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This control model assigns permissions using roles at scope like subscription or resource group.", Answer = "What is Role-Based Access Control (RBAC)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This compute style runs code without managing servers and scales automatically.", Answer = "What is serverless computing?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This Azure hierarchy level is primarily a billing and access boundary.", Answer = "What is a Subscription?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This tool provides cost optimization recommendations like rightsizing resources.", Answer = "What is Azure Advisor?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This is the practice of giving users and apps only the permissions they need.", Answer = "What is least privilege?" },

            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This monitoring service collects metrics, logs, and alerts across Azure resources.", Answer = "What is Azure Monitor?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This Azure service provides a managed relational database platform.", Answer = "What is Azure SQL Database?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This Azure service provides a global edge entry point and acceleration for web apps.", Answer = "What is Azure Front Door?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This infrastructure-as-code system defines resources in JSON templates.", Answer = "What is Azure Resource Manager (ARM) templates?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This service provides global DNS hosting and domain record management.", Answer = "What is Azure DNS?" },

            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This SLA percentage corresponds to about 8.76 hours of downtime per year.", Answer = "What is 99.9%?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This governance feature sits above subscriptions to organize and manage them at scale.", Answer = "What is a Management Group?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This storage replication option keeps data in a paired region for disaster recovery.", Answer = "What is geo-redundant storage (GRS)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "To keep PaaS resources private inside a VNet, you commonly use this feature.", Answer = "What is a Private Endpoint (Private Link)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This cost strategy is used for predictable long-running workloads to reduce price.", Answer = "What are Reserved Instances (or Savings Plans)?" },

            // ==================== AZ-204 (AZURE DEVELOPER) ====================
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This Azure service hosts web apps and APIs without managing virtual machines.", Answer = "What is Azure App Service?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This serverless service runs code when triggered by events.", Answer = "What is Azure Functions?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This Azure storage service is optimized for unstructured data like images and files.", Answer = "What is Azure Blob Storage?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This feature provides an identity for Azure resources without storing credentials in code.", Answer = "What is Managed Identity?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "These settings store environment-based configuration for an App Service app.", Answer = "What are App Settings?" },

            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This App Service feature lets you deploy to staging and swap into production.", Answer = "What are Deployment Slots?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This Azure messaging service is commonly used for reliable async work queues.", Answer = "What is Azure Service Bus?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This Azure service is the standard place to store secrets like connection strings.", Answer = "What is Azure Key Vault?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This is the recommended design approach for scalable services that can run on many instances.", Answer = "What is stateless design?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This collection of libraries is used to access Azure services from .NET code.", Answer = "What are Azure SDKs?" },

            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This Azure Functions feature supports long-running orchestrations with checkpoints and retries.", Answer = "What is Durable Functions?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This telemetry tool is used to trace requests, dependencies, and exceptions across services.", Answer = "What is Application Insights?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This is the best practice for production auth to Azure resources from code without secrets.", Answer = "What is Managed Identity?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This pattern improves resiliency by retrying transient failures with increasing delays.", Answer = "What is exponential backoff retry?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This is the recommended config pattern: config in app settings and secrets in this service.", Answer = "What is Azure Key Vault?" },

            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This Functions plan helps reduce cold starts and supports advanced scaling scenarios.", Answer = "What is the Premium plan?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This deployment technique allows near zero-downtime releases for App Service.", Answer = "What are deployment slots with swap?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This networking feature provides private access to Storage from inside a VNet.", Answer = "What is a Private Endpoint (Private Link)?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This is the key design principle for message handlers that might see the same message twice.", Answer = "What is idempotency?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This approach keeps secrets out of deployment pipelines by referencing them at runtime.", Answer = "What are Key Vault references?" },

            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "Your API sees spikes and needs async processing without blocking requests. The common architecture is this.", Answer = "What is queue-based load leveling?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "Telemetry is too noisy and costly. The first feature to reduce volume is this.", Answer = "What is sampling?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "A secret leaked in configuration. The correct response is to rotate it and move it into this service.", Answer = "What is Azure Key Vault?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "After a slot swap, production breaks due to swapped settings. The fix is to mark the right settings as this.", Answer = "What are slot settings (sticky settings)?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "You need exactly-once effect processing in a distributed system. The practical solution is to enforce this.", Answer = "What is idempotency with deduplication?" },

            // ==================== AI-900 (AZURE AI FUNDAMENTALS) ====================
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This machine learning task predicts categories like spam vs not spam.", Answer = "What is classification?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This machine learning task predicts a number like price or temperature.", Answer = "What is regression?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This term describes using a trained model to make predictions on new data.", Answer = "What is inference?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This acronym refers to techniques for understanding and generating human language.", Answer = "What is NLP (Natural Language Processing)?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This is the data used to teach a model patterns and relationships.", Answer = "What is training data?" },

            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This problem happens when a model memorizes training data and performs poorly on new data.", Answer = "What is overfitting?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This Azure AI service analyzes sentiment, entities, and key phrases in text.", Answer = "What is Azure AI Language?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This Azure AI service performs OCR and image analysis.", Answer = "What is Azure AI Vision?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This Responsible AI goal focuses on avoiding unfair outcomes across groups.", Answer = "What is fairness?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This is a common metric for classification that measures correct predictions over total predictions.", Answer = "What is accuracy?" },

            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This dataset split is used to tune the model without touching the final test set.", Answer = "What is a validation set?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This table shows true positives, false positives, true negatives, and false negatives.", Answer = "What is a confusion matrix?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This process creates useful inputs like normalized values or derived signals for a model.", Answer = "What is feature engineering?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This Azure feature supports building Q&A style bots from a knowledge base.", Answer = "What is Custom Question Answering?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This post-deployment issue occurs when real-world data changes over time and model accuracy drops.", Answer = "What is model drift?" },

            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This metric measures how many predicted positives were actually correct.", Answer = "What is precision?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This metric measures how many actual positives were successfully found.", Answer = "What is recall?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This is a major risk when training data reflects historical unfairness.", Answer = "What is bias?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This AI approach generates new content like text or images from prompts.", Answer = "What is generative AI?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This Responsible AI goal focuses on making model behavior understandable.", Answer = "What is transparency (explainability)?" },

            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "In medical screening, you want to minimize missed positives, so you optimize this metric.", Answer = "What is recall?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "This is why the holdout test set should not be used during training: it must represent real-world performance.", Answer = "What is unbiased evaluation?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "If a model causes harmful outputs, the first control to apply is policy enforcement and this.", Answer = "What are safety filters?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "A model performs well in training but fails in production due to changing patterns. The most likely cause is this.", Answer = "What is data or concept drift?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "Auditors require understanding why predictions were made. The key requirement is this.", Answer = "What is interpretability (explainability)?" },

            // ==================== VIRTUAL NETWORK ====================
            new() { Category = "Virtual Network", PointValue = 100, Question = "This Azure networking service provides an isolated private network in the cloud.", Answer = "What is a Virtual Network (VNet)?" },
            new() { Category = "Virtual Network", PointValue = 100, Question = "This is a smaller IP range inside a VNet used to segment resources.", Answer = "What is a Subnet?" },
            new() { Category = "Virtual Network", PointValue = 100, Question = "This component defines the IP address range for a VNet.", Answer = "What is an address space (CIDR range)?" },
            new() { Category = "Virtual Network", PointValue = 100, Question = "This Azure feature allows Azure resources to communicate privately without public internet exposure.", Answer = "What is private networking?" },
            new() { Category = "Virtual Network", PointValue = 100, Question = "This service converts names like myapp.internal into IP addresses inside Azure.", Answer = "What is DNS?" },

            new() { Category = "Virtual Network", PointValue = 200, Question = "This feature connects an App Service to a VNet for outbound access to private resources.", Answer = "What is VNet Integration?" },
            new() { Category = "Virtual Network", PointValue = 200, Question = "This feature provides private access to a PaaS service using a NIC in your subnet.", Answer = "What is a Private Endpoint (Private Link)?" },
            new() { Category = "Virtual Network", PointValue = 200, Question = "This VNet feature connects two VNets over the Microsoft backbone.", Answer = "What is VNet Peering?" },
            new() { Category = "Virtual Network", PointValue = 200, Question = "This device connects on-premises networks to Azure using an encrypted tunnel over the internet.", Answer = "What is a VPN Gateway?" },
            new() { Category = "Virtual Network", PointValue = 200, Question = "This routing concept determines where network traffic goes next.", Answer = "What is a route table?" },

            new() { Category = "Virtual Network", PointValue = 300, Question = "This security principle reduces blast radius by splitting workloads into separate subnets.", Answer = "What is network segmentation?" },
            new() { Category = "Virtual Network", PointValue = 300, Question = "This feature forces internet-bound traffic through a central inspection point.", Answer = "What is forced tunneling?" },
            new() { Category = "Virtual Network", PointValue = 300, Question = "This is the Azure service most commonly used as that central inspection point for traffic.", Answer = "What is Azure Firewall?" },
            new() { Category = "Virtual Network", PointValue = 300, Question = "This is the best reason to use private endpoints for PaaS services.", Answer = "What is keeping traffic off the public internet?" },
            new() { Category = "Virtual Network", PointValue = 300, Question = "This is a common cause of resources being unable to resolve names privately after adding private endpoints.", Answer = "What is DNS misconfiguration?" },

            new() { Category = "Virtual Network", PointValue = 400, Question = "If VNet peering is enabled, VNets can communicate as if they were one, but you must still control traffic with this.", Answer = "What is network security (NSGs/firewalls)?" },
            new() { Category = "Virtual Network", PointValue = 400, Question = "This feature provides inbound and outbound filtering at the subnet or NIC level.", Answer = "What is a Network Security Group (NSG)?" },
            new() { Category = "Virtual Network", PointValue = 400, Question = "This is the recommended approach to access a VM without exposing RDP/SSH to the internet.", Answer = "What is Azure Bastion?" },
            new() { Category = "Virtual Network", PointValue = 400, Question = "This term describes sending all outbound traffic through a firewall appliance.", Answer = "What is egress control?" },
            new() { Category = "Virtual Network", PointValue = 400, Question = "This is the most common reason two subnets cannot communicate even though they are in the same VNet.", Answer = "What is blocked rules in an NSG?" },

            new() { Category = "Virtual Network", PointValue = 500, Question = "You need private connectivity from on-prem to Azure with predictable performance that does not use the public internet.", Answer = "What is ExpressRoute?" },
            new() { Category = "Virtual Network", PointValue = 500, Question = "A private endpoint is created, but the app still hits the public endpoint. The likely missing piece is this.", Answer = "What is correct private DNS resolution?" },
            new() { Category = "Virtual Network", PointValue = 500, Question = "You must prevent data exfiltration and control outbound traffic at scale. The most direct network strategy is this.", Answer = "What is centralized egress via firewall and routing?" },
            new() { Category = "Virtual Network", PointValue = 500, Question = "Two VNets need to connect across regions quickly without gateways. The simplest built-in method is this.", Answer = "What is global VNet peering?" },
            new() { Category = "Virtual Network", PointValue = 500, Question = "A workload needs access to multiple private services and must avoid public internet entirely. The key building block is this.", Answer = "What are private endpoints?" },

            // ==================== NETWORK SECURITY GROUP ====================
            new() { Category = "Network Security Group", PointValue = 100, Question = "This Azure resource filters inbound and outbound traffic at the subnet or NIC level.", Answer = "What is a Network Security Group (NSG)?" },
            new() { Category = "Network Security Group", PointValue = 100, Question = "NSG rules are evaluated in order of this value.", Answer = "What is priority?" },
            new() { Category = "Network Security Group", PointValue = 100, Question = "This NSG rule action blocks traffic that matches the rule.", Answer = "What is Deny?" },
            new() { Category = "Network Security Group", PointValue = 100, Question = "This NSG rule action permits traffic that matches the rule.", Answer = "What is Allow?" },
            new() { Category = "Network Security Group", PointValue = 100, Question = "This traffic direction controls access into a subnet or VM.", Answer = "What is inbound?" },

            new() { Category = "Network Security Group", PointValue = 200, Question = "This traffic direction controls access leaving a subnet or VM.", Answer = "What is outbound?" },
            new() { Category = "Network Security Group", PointValue = 200, Question = "This is a best practice: open only required ports instead of this risky pattern.", Answer = "What is allowing all traffic?" },
            new() { Category = "Network Security Group", PointValue = 200, Question = "This is the NSG feature that groups IPs and services for simpler rules.", Answer = "What are service tags?" },
            new() { Category = "Network Security Group", PointValue = 200, Question = "This is the safest approach for admin access: avoid exposing this directly to the internet.", Answer = "What is RDP/SSH?" },
            new() { Category = "Network Security Group", PointValue = 200, Question = "If a VM is unreachable, the first network control to check is often this.", Answer = "What is the NSG?" },

            new() { Category = "Network Security Group", PointValue = 300, Question = "This is the default NSG behavior if no allow rule matches a packet.", Answer = "What is deny by default?" },
            new() { Category = "Network Security Group", PointValue = 300, Question = "This NSG scope attaches rules to all resources in a subnet.", Answer = "What is a subnet-level NSG?" },
            new() { Category = "Network Security Group", PointValue = 300, Question = "This NSG scope applies rules only to a single VM network interface.", Answer = "What is a NIC-level NSG?" },
            new() { Category = "Network Security Group", PointValue = 300, Question = "When two rules match, the one that wins is the one with the lowest this.", Answer = "What is priority number?" },
            new() { Category = "Network Security Group", PointValue = 300, Question = "This is the security principle of granting only required access in NSG rules.", Answer = "What is least privilege?" },

            new() { Category = "Network Security Group", PointValue = 400, Question = "You allowed a port in the NSG but still cannot connect; the next likely blocker is this OS-level control.", Answer = "What is the VM host firewall?" },
            new() { Category = "Network Security Group", PointValue = 400, Question = "This is a clean pattern for application tiers: web, app, and data separated into these.", Answer = "What are subnets?" },
            new() { Category = "Network Security Group", PointValue = 400, Question = "This is the safest way to allow traffic from only a specific source: restrict by this.", Answer = "What is source IP range (or service tag)?" },
            new() { Category = "Network Security Group", PointValue = 400, Question = "This is the main reason to keep NSG rules simple and consistent.", Answer = "What is easier auditing and troubleshooting?" },
            new() { Category = "Network Security Group", PointValue = 400, Question = "This is the risk of leaving wide inbound rules on a public VM.", Answer = "What is increased attack surface?" },

            new() { Category = "Network Security Group", PointValue = 500, Question = "A subnet has an NSG that allows traffic, but another NSG on the NIC blocks it. The effective result is this.", Answer = "What is blocked traffic (the most restrictive rule wins)?" },
            new() { Category = "Network Security Group", PointValue = 500, Question = "You need to lock down admin access without exposing ports publicly. The best Azure service to pair with NSGs is this.", Answer = "What is Azure Bastion?" },
            new() { Category = "Network Security Group", PointValue = 500, Question = "Your rules are correct but traffic still fails. The fastest Azure tool to confirm flow decisions is this.", Answer = "What is Network Watcher (IP flow verify)?" },
            new() { Category = "Network Security Group", PointValue = 500, Question = "To reduce maintenance, this NSG feature lets you reference Azure services without hardcoding IPs.", Answer = "What are service tags?" },
            new() { Category = "Network Security Group", PointValue = 500, Question = "The cleanest way to reduce blast radius across tiers is to segment networks and enforce this.", Answer = "What is least privilege with layered rules?" },

            // ==================== AZURE FIREWALL ====================
            new() { Category = "Azure Firewall", PointValue = 100, Question = "This Azure service provides centrally managed network traffic filtering.", Answer = "What is Azure Firewall?" },
            new() { Category = "Azure Firewall", PointValue = 100, Question = "This firewall direction controls traffic leaving your network to the internet.", Answer = "What is outbound (egress)?" },
            new() { Category = "Azure Firewall", PointValue = 100, Question = "This firewall direction controls traffic entering your network from outside.", Answer = "What is inbound (ingress)?" },
            new() { Category = "Azure Firewall", PointValue = 100, Question = "This firewall feature logs traffic events for auditing and troubleshooting.", Answer = "What is diagnostic logging?" },
            new() { Category = "Azure Firewall", PointValue = 100, Question = "This is the key benefit of a central firewall compared to scattered rules everywhere.", Answer = "What is centralized control?" },

            new() { Category = "Azure Firewall", PointValue = 200, Question = "This concept describes routing traffic through a firewall using route tables.", Answer = "What is forced tunneling (or UDR-based routing)?" },
            new() { Category = "Azure Firewall", PointValue = 200, Question = "This is the main reason to control egress in secure environments.", Answer = "What is preventing data exfiltration?" },
            new() { Category = "Azure Firewall", PointValue = 200, Question = "This firewall capability filters traffic based on fully qualified domain names.", Answer = "What is FQDN filtering?" },
            new() { Category = "Azure Firewall", PointValue = 200, Question = "This firewall feature can apply application-layer filtering for web traffic.", Answer = "What is web categories/application rules?" },
            new() { Category = "Azure Firewall", PointValue = 200, Question = "To send traffic from a subnet to Azure Firewall, you typically configure this.", Answer = "What is a route table (UDR)?" },

            new() { Category = "Azure Firewall", PointValue = 300, Question = "This is a common secure architecture pattern: all subnets route outbound traffic to a central this.", Answer = "What is firewall hub?" },
            new() { Category = "Azure Firewall", PointValue = 300, Question = "If outbound traffic suddenly stops, the first Azure networking item to verify is this.", Answer = "What is effective routes (UDRs)?" },
            new() { Category = "Azure Firewall", PointValue = 300, Question = "This describes controlling which destinations and ports are allowed out of the network.", Answer = "What is egress filtering?" },
            new() { Category = "Azure Firewall", PointValue = 300, Question = "This is the Azure monitoring destination commonly used to query firewall logs with KQL.", Answer = "What is Log Analytics?" },
            new() { Category = "Azure Firewall", PointValue = 300, Question = "This security approach reduces risk by adding multiple layers like NSGs plus this.", Answer = "What is defense in depth?" },

            new() { Category = "Azure Firewall", PointValue = 400, Question = "You need to allow access to external services without hardcoding IP addresses. A firewall rule can use this instead.", Answer = "What is FQDN filtering?" },
            new() { Category = "Azure Firewall", PointValue = 400, Question = "Traffic is allowed in NSG but still blocked. A likely central control doing it is this.", Answer = "What is Azure Firewall?" },
            new() { Category = "Azure Firewall", PointValue = 400, Question = "If you want consistent outbound rules across many VNets, the best design is a hub-and-spoke with a central this.", Answer = "What is Azure Firewall?" },
            new() { Category = "Azure Firewall", PointValue = 400, Question = "This is the key reason to log firewall traffic to a SIEM or Log Analytics.", Answer = "What is detection and auditing?" },
            new() { Category = "Azure Firewall", PointValue = 400, Question = "This concept describes blocking by default and only allowing required destinations.", Answer = "What is least privilege?" },

            new() { Category = "Azure Firewall", PointValue = 500, Question = "You want to stop any workload from reaching unknown destinations. The policy you implement is default deny with explicit this.", Answer = "What is allow rules?" },
            new() { Category = "Azure Firewall", PointValue = 500, Question = "A private endpoint is in use, but traffic still goes public. The most common missing link is this.", Answer = "What is correct DNS and routing?" },
            new() { Category = "Azure Firewall", PointValue = 500, Question = "You need enterprise-scale egress control, logging, and centralized policy. The Azure-native service for this is.", Answer = "What is Azure Firewall?" },
            new() { Category = "Azure Firewall", PointValue = 500, Question = "Outbound traffic must be inspected and logged before leaving Azure. The routing technique used is this.", Answer = "What is forced tunneling with UDRs?" },
            new() { Category = "Azure Firewall", PointValue = 500, Question = "Your environment requires strong audit trails of all network flows. The data source you enable is firewall this.", Answer = "What are diagnostic logs?" },

            // ==================== AZURE BASTION ====================
            new() { Category = "Azure Bastion", PointValue = 100, Question = "This Azure service provides secure RDP and SSH access to VMs through the Azure Portal.", Answer = "What is Azure Bastion?" },
            new() { Category = "Azure Bastion", PointValue = 100, Question = "Azure Bastion helps you avoid exposing this common admin port to the internet on Windows VMs.", Answer = "What is RDP (3389)?" },
            new() { Category = "Azure Bastion", PointValue = 100, Question = "Azure Bastion helps you avoid exposing this common admin port to the internet on Linux VMs.", Answer = "What is SSH (22)?" },
            new() { Category = "Azure Bastion", PointValue = 100, Question = "This is the main security benefit of Azure Bastion.", Answer = "What is no public IP needed for admin access?" },
            new() { Category = "Azure Bastion", PointValue = 100, Question = "This is where you typically launch a Bastion session to a VM.", Answer = "What is the Azure Portal?" },

            new() { Category = "Azure Bastion", PointValue = 200, Question = "This network requirement is common for Bastion: the target VM must be reachable inside the same this.", Answer = "What is Virtual Network (VNet)?" },
            new() { Category = "Azure Bastion", PointValue = 200, Question = "This type of access pattern is improved by Bastion: admin access without opening inbound NSG rules from the internet.", Answer = "What is secure remote management?" },
            new() { Category = "Azure Bastion", PointValue = 200, Question = "This is the biggest operational win of Bastion over managing jump boxes.", Answer = "What is managed service (no VM jump host)?" },
            new() { Category = "Azure Bastion", PointValue = 200, Question = "Bastion reduces attack surface by removing the need for this on VMs.", Answer = "What is a public IP address?" },
            new() { Category = "Azure Bastion", PointValue = 200, Question = "This security concept describes limiting externally reachable services and ports.", Answer = "What is reducing attack surface?" },

            new() { Category = "Azure Bastion", PointValue = 300, Question = "If a VM cannot be reached via Bastion, the first network thing to confirm is VM connectivity within the VNet and this.", Answer = "What is NSG rules allowing internal access?" },
            new() { Category = "Azure Bastion", PointValue = 300, Question = "This is the recommended approach for admin access in cloud networks: avoid internet-exposed management ports and use this.", Answer = "What is Azure Bastion?" },
            new() { Category = "Azure Bastion", PointValue = 300, Question = "This principle means you should grant admin access only to those who truly need it.", Answer = "What is least privilege?" },
            new() { Category = "Azure Bastion", PointValue = 300, Question = "This is the biggest reason opening RDP/SSH to the world is dangerous.", Answer = "What is brute force and exploit attempts?" },
            new() { Category = "Azure Bastion", PointValue = 300, Question = "This security design keeps admin access inside private networking rather than public endpoints.", Answer = "What is private access?" },

            new() { Category = "Azure Bastion", PointValue = 400, Question = "In a secure architecture, admin access should be separate from app traffic. Bastion supports this by isolating management access to this.", Answer = "What is the private network?" },
            new() { Category = "Azure Bastion", PointValue = 400, Question = "Your VM has no public IP and must still be administered safely. The simplest Azure-native option is.", Answer = "What is Azure Bastion?" },
            new() { Category = "Azure Bastion", PointValue = 400, Question = "This is a common alternative to Bastion that is riskier because it requires managing a VM as a jump host.", Answer = "What is a jump box?" },
            new() { Category = "Azure Bastion", PointValue = 400, Question = "This security benefit is achieved when you no longer need to allow inbound 3389/22 from the internet.", Answer = "What is reduced attack surface?" },
            new() { Category = "Azure Bastion", PointValue = 400, Question = "If compliance requires zero internet-exposed management ports, the control you implement is to use this and remove public rules.", Answer = "What is Azure Bastion?" },

            new() { Category = "Azure Bastion", PointValue = 500, Question = "A security audit flags open RDP on a public IP. The cleanest remediation is to remove the public IP and use this.", Answer = "What is Azure Bastion?" },
            new() { Category = "Azure Bastion", PointValue = 500, Question = "If you must administer VMs privately across many environments, the most scalable design is hub-and-spoke and centralized this.", Answer = "What is Bastion access (or a management network)?" },
            new() { Category = "Azure Bastion", PointValue = 500, Question = "Your NSG blocks inbound RDP from the internet, but admins still need access. The correct solution is to use this service.", Answer = "What is Azure Bastion?" },
            new() { Category = "Azure Bastion", PointValue = 500, Question = "This is the cloud security philosophy that says the identity and access layer is as important as the perimeter.", Answer = "What is Zero Trust?" },
            new() { Category = "Azure Bastion", PointValue = 500, Question = "To keep management secure, you should log and monitor Bastion sessions using this broad Azure capability.", Answer = "What is Azure Monitor (and logs)?" },

            // ==================== AZURE FRONT DOOR ====================
            new() { Category = "Azure Front Door", PointValue = 100, Question = "This Azure service provides global HTTP/HTTPS load balancing and acceleration at the edge.", Answer = "What is Azure Front Door?" },
            new() { Category = "Azure Front Door", PointValue = 100, Question = "This feature routes requests to the best backend based on latency and health.", Answer = "What is global load balancing?" },
            new() { Category = "Azure Front Door", PointValue = 100, Question = "This Front Door component defines where traffic can be sent, such as App Services or endpoints.", Answer = "What is a backend pool?" },
            new() { Category = "Azure Front Door", PointValue = 100, Question = "This Front Door feature checks whether a backend is healthy before sending traffic.", Answer = "What is health probing?" },
            new() { Category = "Azure Front Door", PointValue = 100, Question = "This is the most common reason to use Front Door: better performance for users worldwide.", Answer = "What is reduced latency?" },

            new() { Category = "Azure Front Door", PointValue = 200, Question = "This security capability helps protect web apps from common attacks like SQL injection and XSS.", Answer = "What is a Web Application Firewall (WAF)?" },
            new() { Category = "Azure Front Door", PointValue = 200, Question = "This feature enables secure HTTPS for a custom domain at the edge.", Answer = "What is TLS/SSL termination?" },
            new() { Category = "Azure Front Door", PointValue = 200, Question = "This routing feature sends requests based on URL paths like /api versus /images.", Answer = "What is path-based routing?" },
            new() { Category = "Azure Front Door", PointValue = 200, Question = "This caching-related concept improves performance by serving content from locations closer to users.", Answer = "What is edge caching?" },
            new() { Category = "Azure Front Door", PointValue = 200, Question = "This is the main advantage of Front Door compared to a regional load balancer.", Answer = "What is global distribution?" },

            new() { Category = "Azure Front Door", PointValue = 300, Question = "This Front Door scenario improves availability: route traffic away from unhealthy regions automatically.", Answer = "What is failover?" },
            new() { Category = "Azure Front Door", PointValue = 300, Question = "This feature helps defend against abuse by limiting request rates at the edge.", Answer = "What is rate limiting (WAF rules)?" },
            new() { Category = "Azure Front Door", PointValue = 300, Question = "This is the key reason to place Front Door in front of multiple regional backends.", Answer = "What is high availability?" },
            new() { Category = "Azure Front Door", PointValue = 300, Question = "This Front Door capability can accelerate dynamic content, not just static files.", Answer = "What is global application acceleration?" },
            new() { Category = "Azure Front Door", PointValue = 300, Question = "This is the typical way Front Door protects users: it routes to only healthy backends based on this.", Answer = "What are health probes?" },

            new() { Category = "Azure Front Door", PointValue = 400, Question = "For a web app with global users, TLS termination and WAF at the edge are best implemented using this.", Answer = "What is Azure Front Door?" },
            new() { Category = "Azure Front Door", PointValue = 400, Question = "This is the key difference: Front Door is global at the edge, while Application Gateway is typically this.", Answer = "What is regional?" },
            new() { Category = "Azure Front Door", PointValue = 400, Question = "If users in one region complain about slowness, a likely improvement is to use this global edge service.", Answer = "What is Azure Front Door?" },
            new() { Category = "Azure Front Door", PointValue = 400, Question = "This feature is crucial for secure web hosting: force redirect all HTTP requests to this.", Answer = "What is HTTPS?" },
            new() { Category = "Azure Front Door", PointValue = 400, Question = "When a backend returns many 5xx errors, Front Door should stop routing to it because of failing this.", Answer = "What are health checks (probes)?" },

            new() { Category = "Azure Front Door", PointValue = 500, Question = "You need global load balancing, WAF, and fast failover across regions. The single Azure service that fits is.", Answer = "What is Azure Front Door?" },
            new() { Category = "Azure Front Door", PointValue = 500, Question = "A security team wants to block OWASP-style web attacks at the edge. The Front Door add-on used is this.", Answer = "What is Web Application Firewall (WAF)?" },
            new() { Category = "Azure Front Door", PointValue = 500, Question = "Your app is deployed in multiple regions and one region goes down. The expected Front Door behavior is to do this.", Answer = "What is route traffic to healthy regions automatically?" },
            new() { Category = "Azure Front Door", PointValue = 500, Question = "Clients complain that only some paths are slow. The Front Door routing feature to optimize by endpoint type is this.", Answer = "What is path-based routing?" },
            new() { Category = "Azure Front Door", PointValue = 500, Question = "If the goal is both performance and protection, the best combined edge approach is Front Door plus this security layer.", Answer = "What is WAF?" },
            // ==================== POWERSHELL & CLI (Automation, scripting, Azure CLI) ====================

            // 100
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell cmdlet lists files and folders in the current directory.", Answer = "What is Get-ChildItem?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This operator sends the output of one command into the next command.", Answer = "What is the pipe (|)?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This Azure CLI command signs you in to Azure interactively.", Answer = "What is az login?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell cmdlet shows built-in help/documentation for a command.", Answer = "What is Get-Help?" },
            new() { Category = "PowerShell & CLI", PointValue = 100, Question = "This PowerShell switch simulates an action without making changes.", Answer = "What is -WhatIf?" },

            // 200
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This symbol starts a PowerShell variable name.", Answer = "What is the dollar sign ($)?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This Azure CLI command lists all resource groups in the active subscription.", Answer = "What is az group list?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This PowerShell comparison operator means “not equal.”", Answer = "What is -ne?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This PowerShell feature controls whether scripts can run (and under what rules).", Answer = "What is an Execution Policy?" },
            new() { Category = "PowerShell & CLI", PointValue = 200, Question = "This syntax runs a script located in the current folder in PowerShell.", Answer = "What is .\\script.ps1?" },

            // 300
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell command processes each item flowing through the pipeline.", Answer = "What is ForEach-Object?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This Azure CLI command sets the active subscription for future commands.", Answer = "What is az account set?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell cmdlet converts an object into JSON text.", Answer = "What is ConvertTo-Json?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This PowerShell variable holds the last external program’s exit code.", Answer = "What is $LASTEXITCODE?" },
            new() { Category = "PowerShell & CLI", PointValue = 300, Question = "This parameter forces non-terminating errors to become terminating errors.", Answer = "What is -ErrorAction Stop?" },

            // 400
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This code structure enables structured error handling and guaranteed cleanup.", Answer = "What is try/catch/finally?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This Azure CLI command calls Azure REST APIs directly.", Answer = "What is az rest?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "Unlike Write-Host, this cmdlet writes output to the pipeline.", Answer = "What is Write-Output?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "In deployments and automation, this means re-running yields the same end state.", Answer = "What is idempotent?" },
            new() { Category = "PowerShell & CLI", PointValue = 400, Question = "This global setting makes PowerShell stop immediately on errors.", Answer = "What is $ErrorActionPreference = 'Stop'?" },

            // 500
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This Azure CLI command deploys a Bicep/ARM template to a resource group.", Answer = "What is az deployment group create?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This is the safest pattern to avoid hardcoding secrets in scripts.", Answer = "What is Managed Identity + Key Vault?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "If Azure CLI returns AuthorizationFailed, the usual fix is to verify RBAC/subscription context and do this.", Answer = "What is re-authenticating (az login) after correcting role/subscription?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "This automation best practice means validating inputs early and stopping immediately on errors.", Answer = "What is fail fast?" },
            new() { Category = "PowerShell & CLI", PointValue = 500, Question = "Automation cleanup scripts often rely on this to filter and manage resources predictably.", Answer = "What are tags?" },


            // ==================== AGILE & SCRUM (Sprints, backlogs, ceremonies) ====================

            // 100
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This is the time-boxed iteration used in Scrum.", Answer = "What is a Sprint?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This is the ordered list of all desired product work.", Answer = "What is the Product Backlog?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This is the daily 15-minute ceremony for alignment and blockers.", Answer = "What is the Daily Scrum (Standup)?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This role owns backlog priority and maximizes product value.", Answer = "What is the Product Owner?" },
            new() { Category = "Agile & Scrum", PointValue = 100, Question = "This end-of-sprint ceremony is the demo + feedback session.", Answer = "What is the Sprint Review?" },

            // 200
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This ceremony defines the Sprint Goal and selects work for the sprint.", Answer = "What is Sprint Planning?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This meeting focuses on improving how the team works.", Answer = "What is the Retrospective?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This common user story format begins with these words: “As a…”.", Answer = "What is a user story?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This estimation method measures relative effort/complexity rather than hours.", Answer = "What are Story Points?" },
            new() { Category = "Agile & Scrum", PointValue = 200, Question = "This backlog contains the items selected for the sprint and the plan to deliver them.", Answer = "What is the Sprint Backlog?" },

            // 300
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This shared checklist defines when work is truly complete.", Answer = "What is the Definition of Done (DoD)?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This chart shows remaining work over time in a sprint.", Answer = "What is a Burndown Chart?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This metric reflects how many story points a team completes per sprint (trend).", Answer = "What is Velocity?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This activity clarifies, splits, estimates, and re-orders upcoming backlog items.", Answer = "What is Backlog Refinement?" },
            new() { Category = "Agile & Scrum", PointValue = 300, Question = "This standup anti-pattern turns the Daily Scrum into a manager status meeting.", Answer = "What is the status-meeting anti-pattern?" },

            // 400
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "If a Sprint Goal is at risk mid-sprint, the best Scrum move is to renegotiate scope with this role.", Answer = "What is the Product Owner?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is uncontrolled work added mid-sprint that endangers commitments.", Answer = "What is scope creep?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is why sprints are time-boxed: predictable cadence and forced prioritization.", Answer = "What is time-boxing?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This is the difference: an epic is large, while stories are small deliverable slices.", Answer = "What are Epics vs Stories?" },
            new() { Category = "Agile & Scrum", PointValue = 400, Question = "This limit reduces multitasking by restricting how many items can be in progress.", Answer = "What is a WIP limit?" },

            // 500
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "If velocity is unstable, the first improvement is usually better story slicing plus this.", Answer = "What is estimation consistency?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "The best outcome metric to optimize for is delivered value that meets DoD—aka this.", Answer = "What is Done?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "When teams keep missing sprint commitments, the most common root cause is this.", Answer = "What is over-commitment?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "The cleanest way to handle urgent new work mid-sprint is to swap scope only with agreement from this role.", Answer = "What is the Product Owner?" },
            new() { Category = "Agile & Scrum", PointValue = 500, Question = "Retrospectives fail most often because action items have no owner and no follow-through—meaning no real this.", Answer = "What is accountability?" },


            // ==================== AZ-900 (Azure Fundamentals) ====================

            // 100
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This cloud service model provides virtual machines, networking, and storage as building blocks.", Answer = "What is IaaS (Infrastructure as a Service)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This Azure compute service provides virtual machines.", Answer = "What is Azure Virtual Machines?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This Azure concept is a geographic area that contains one or more datacenters.", Answer = "What is an Azure Region?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "This is the logical container that holds related Azure resources for lifecycle and billing.", Answer = "What is a Resource Group?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 100, Question = "In cloud costs, this contrasts upfront capital spending with pay-as-you-go operating expense.", Answer = "What is CapEx vs OpEx?" },

            // 200
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This model splits security responsibilities between Microsoft and the customer.", Answer = "What is the Shared Responsibility Model?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This governance service enforces rules like allowed regions or required tags.", Answer = "What is Azure Policy?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This identity service is core to Azure sign-in and access management.", Answer = "What is Azure Active Directory (Microsoft Entra ID)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "This feature provides datacenter-level redundancy inside a single Azure region.", Answer = "What are Availability Zones?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 200, Question = "For many cloud services, this is a common pricing factor (compute, storage, bandwidth).", Answer = "What is usage?" },

            // 300
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This Azure service lets you manage and auto-scale a group of identical VMs.", Answer = "What is Azure Virtual Machine Scale Sets (VMSS)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This is the uptime commitment a provider makes for a cloud service.", Answer = "What is an SLA (Service Level Agreement)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This security principle means granting only the access needed.", Answer = "What is least privilege?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This controls who can do what to which Azure resources.", Answer = "What is RBAC (Role-Based Access Control)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 300, Question = "This compute approach means you manage code while the platform manages infrastructure.", Answer = "What is serverless?" },

            // 400
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "You choose this cloud model when you want less infrastructure management and more built-in platform features.", Answer = "What is PaaS?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "Azure Monitor Metrics are numeric time-series, while Azure Monitor Logs are this.", Answer = "What are detailed log records?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This Azure boundary is used for billing, access control, and limits.", Answer = "What is a Subscription?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "A common way to reduce cost for predictable, long-running workloads is this.", Answer = "What are Reserved Instances or Savings Plans?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 400, Question = "This is used for organization, cost tracking, and automation filtering in Azure.", Answer = "What are Tags?" },

            // 500
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This service provides global low-latency entry and TLS termination at the edge.", Answer = "What is Azure Front Door?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This feature provides private access to PaaS services from within a VNet.", Answer = "What are Private Endpoints (Private Link)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "To separate dev/test/prod billing cleanly, the best boundary is separate these.", Answer = "What are subscriptions?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "This storage concept provides resiliency across regions using geo-replication.", Answer = "What is Geo-Redundant Storage (GRS)?" },
            new() { Category = "AZ-900 (Azure Fundamentals)", PointValue = 500, Question = "To enforce “only allowed SKUs/regions” at scale, you use Azure Policy assignments plus these groupings.", Answer = "What are initiatives?" },


            // ==================== AZ-204 (Azure Developer) ====================

            // 100
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This Azure service hosts web apps and APIs without managing virtual machines.", Answer = "What is Azure App Service?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "In Azure Functions, this is what starts the function (an event).", Answer = "What is a trigger?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This Azure storage service is optimized for unstructured data like images and files.", Answer = "What is Azure Blob Storage?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "This provides an identity for Azure resources so apps can authenticate without secrets.", Answer = "What is Managed Identity?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 100, Question = "In App Service, these store environment configuration values.", Answer = "What are App Settings?" },

            // 200
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This App Service feature enables safe staging and swapping into production.", Answer = "What are Deployment Slots?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This Azure service queues messages for asynchronous processing.", Answer = "What is Azure Service Bus (or Storage Queue)?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This term means an app doesn’t store session state on a single instance, enabling easy scale-out.", Answer = "What is stateless?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "This Azure service is a standard secure place to store secrets like connection strings.", Answer = "What is Azure Key Vault?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 200, Question = "These libraries are used to access Azure services from code (like .NET).", Answer = "What are Azure SDKs?" },

            // 300
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "The best way to authenticate from code to Azure in production without storing a secret is this.", Answer = "What is Managed Identity?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "For HTTP APIs, this status code is commonly returned on successful creation.", Answer = "What is 201 Created?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "In Azure Functions, a trigger starts execution while this connects to input/output services.", Answer = "What is a binding?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "This tool helps trace requests across services using telemetry and correlation.", Answer = "What is Application Insights?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 300, Question = "Recommended pattern: config in environment/app settings, but secrets live in this service.", Answer = "What is Azure Key Vault?" },

            // 400
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This Azure Functions feature supports durable multi-step orchestrations.", Answer = "What is Durable Functions?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "To reduce cold starts and keep Functions warm, this plan is commonly used.", Answer = "What is the Premium plan?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "Blue/green deployments for App Service are commonly done using this feature.", Answer = "What are slots + swap?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "Private access to Storage typically uses networking plus this feature.", Answer = "What is a Private Endpoint (Private Link)?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 400, Question = "This resiliency pattern handles transient failures using retries with increasing delays.", Answer = "What is exponential backoff?" },

            // 500
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "For spiky load and event-driven processing, a common architecture is a queue plus this compute.", Answer = "What is Azure Functions?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "To reduce noisy/expensive telemetry in Application Insights, you enable this feature.", Answer = "What is sampling?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "To rotate secrets without redeploying, you use Key Vault references plus this concept.", Answer = "What is versioning?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "To get exactly-once *effects* in distributed systems, you rely on this plus deduplication keys.", Answer = "What is idempotency?" },
            new() { Category = "AZ-204 (Azure Developer)", PointValue = 500, Question = "High 5xx errors right after a slot swap often point to misconfigured this.", Answer = "What are slot-sticky settings (slot settings)?" },


            // ==================== AI-900 (Azure AI Fundamentals) ====================

            // 100
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This ML task predicts a label or category (like spam vs not spam).", Answer = "What is classification?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This ML task predicts a numeric value (like price or temperature).", Answer = "What is regression?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "These examples are used to teach a model patterns and relationships.", Answer = "What is training data?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This is using a trained model to make predictions on new data.", Answer = "What is inference?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 100, Question = "This acronym refers to techniques for understanding and generating human language.", Answer = "What is NLP (Natural Language Processing)?" },

            // 200
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This is a common metric for classification that measures correct predictions over total predictions.", Answer = "What is accuracy?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This problem happens when a model memorizes training data and performs poorly on new data.", Answer = "What is overfitting?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This Azure AI service analyzes text sentiment, entities, and key phrases.", Answer = "What is Azure AI Language?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This Azure AI service performs OCR and image analysis.", Answer = "What is Azure AI Vision?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 200, Question = "This area focuses on making AI fair, safe, transparent, and accountable.", Answer = "What is Responsible AI?" },

            // 300
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This dataset split is used to tune the model without touching the final test set.", Answer = "What is a validation set?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This table shows true positives, false positives, true negatives, and false negatives.", Answer = "What is a confusion matrix?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This process creates useful input signals (features) for a model.", Answer = "What is feature engineering?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "This Azure feature helps build Q&A style bots from a knowledge base.", Answer = "What is Custom Question Answering?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 300, Question = "After deployment, you monitor models for this because real-world data changes over time.", Answer = "What is drift?" },

            // 400
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This metric measures correct positives among predicted positives.", Answer = "What is precision?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This metric measures found positives among actual positives.", Answer = "What is recall?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This is when changing real-world patterns reduce model accuracy over time.", Answer = "What is model drift?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This AI approach generates new content like text, images, or code from prompts.", Answer = "What is generative AI?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 400, Question = "This is a major risk when training data reflects historical unfairness.", Answer = "What is bias?" },

            // 500
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "In medical screening, if you must minimize missed positives, you optimize this metric.", Answer = "What is recall (sensitivity)?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "Auditors require understanding why predictions were made; the key requirement is this.", Answer = "What is interpretability (explainability)?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "If an AI system outputs harmful content, the first control to apply is policy plus this.", Answer = "What are safety filters?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "A holdout test set is “sacred” because it provides this kind of evaluation.", Answer = "What is unbiased real-world performance estimation?" },
            new() { Category = "AI-900 (Azure AI Fundamentals)", PointValue = 500, Question = "If production accuracy drops after deployment, the most likely cause is this.", Answer = "What is data drift (concept drift)?" },


            // ==================== ALGORITHMS (C#) (Sorting, searching, recursion) ====================

            // 100
            new() { Category = "Algorithms (C#)", PointValue = 100, Question = "This is the Big-O time complexity of a linear search in an array.", Answer = "What is O(n)?" },
            new() { Category = "Algorithms (C#)", PointValue = 100, Question = "This is the Big-O time complexity of binary search on sorted data.", Answer = "What is O(log n)?" },
            new() { Category = "Algorithms (C#)", PointValue = 100, Question = "This data structure is LIFO (last in, first out).", Answer = "What is a Stack?" },
            new() { Category = "Algorithms (C#)", PointValue = 100, Question = "This data structure is FIFO (first in, first out).", Answer = "What is a Queue?" },
            new() { Category = "Algorithms (C#)", PointValue = 100, Question = "This is when a function calls itself.", Answer = "What is recursion?" },

            // 200
            new() { Category = "Algorithms (C#)", PointValue = 200, Question = "Binary search requires this property of the data.", Answer = "What is sorted order?" },
            new() { Category = "Algorithms (C#)", PointValue = 200, Question = "Average-case lookup time for Dictionary<TKey, TValue> is commonly this.", Answer = "What is O(1)?" },
            new() { Category = "Algorithms (C#)", PointValue = 200, Question = "Merge sort’s key step is combining these into one.", Answer = "What are two sorted halves?" },
            new() { Category = "Algorithms (C#)", PointValue = 200, Question = "In recursion, this stops the calls from continuing forever.", Answer = "What is a base case?" },
            new() { Category = "Algorithms (C#)", PointValue = 200, Question = "This kind of sort keeps equal items in their original order.", Answer = "What is a stable sort?" },

            // 300
            new() { Category = "Algorithms (C#)", PointValue = 300, Question = "This is a classic divide-and-conquer sorting algorithm.", Answer = "What is merge sort?" },
            new() { Category = "Algorithms (C#)", PointValue = 300, Question = "This is the worst-case Big-O time complexity of bubble sort.", Answer = "What is O(n²)?" },
            new() { Category = "Algorithms (C#)", PointValue = 300, Question = "Recursion can cause this when too many nested calls occur.", Answer = "What is a stack overflow?" },
            new() { Category = "Algorithms (C#)", PointValue = 300, Question = "This traversal order visits Left, Node, then Right.", Answer = "What is in-order traversal?" },
            new() { Category = "Algorithms (C#)", PointValue = 300, Question = "This data structure supports fast access to the minimum item.", Answer = "What is a min-heap (priority queue)?" },

            // 400
            new() { Category = "Algorithms (C#)", PointValue = 400, Question = "Quicksort’s worst case often happens with already-sorted input and this kind of pivot choice.", Answer = "What is a poor pivot?" },
            new() { Category = "Algorithms (C#)", PointValue = 400, Question = "To reduce quicksort worst-case risk, you use a randomized/median pivot and this technique.", Answer = "What is better partitioning (3-way partition)?" },
            new() { Category = "Algorithms (C#)", PointValue = 400, Question = "Breadth-first search (BFS) typically uses this data structure.", Answer = "What is a Queue?" },
            new() { Category = "Algorithms (C#)", PointValue = 400, Question = "Depth-first search (DFS) typically uses this data structure.", Answer = "What is a Stack?" },
            new() { Category = "Algorithms (C#)", PointValue = 400, Question = "For many contains-checks, this collection is typically faster than List<T>.", Answer = "What is HashSet<T>?" },

            // 500
            new() { Category = "Algorithms (C#)", PointValue = 500, Question = "If you need the top K items repeatedly, this structure is a common best choice.", Answer = "What is a heap (PriorityQueue)?" },
            new() { Category = "Algorithms (C#)", PointValue = 500, Question = "To dedupe while preserving order, a common approach is a HashSet for seen plus this for output.", Answer = "What is a List?" },
            new() { Category = "Algorithms (C#)", PointValue = 500, Question = "In production, this is usually easier to debug than recursion because the control flow is explicit.", Answer = "What is iteration?" },
            new() { Category = "Algorithms (C#)", PointValue = 500, Question = "This classic algorithm detects a cycle in a linked list using two pointers.", Answer = "What is Floyd’s tortoise and hare?" },
            new() { Category = "Algorithms (C#)", PointValue = 500, Question = "Fast key lookup plus ordered iteration is often achieved by combining a Dictionary with this.", Answer = "What is a sorted structure (like SortedDictionary/SortedSet)?" },


            // ==================== DP-3001 (Azure Data) (Migration, Azure SQL modernization) ====================

            // 100
            new() { Category = "DP-3001 (Azure Data)", PointValue = 100, Question = "This term means moving data and schema to a new platform.", Answer = "What is database migration?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 100, Question = "This is Microsoft’s managed relational database service.", Answer = "What is Azure SQL Database?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 100, Question = "In a migration, this is when users/apps switch to the new system.", Answer = "What is cutover?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 100, Question = "This is the time a service is unavailable.", Answer = "What is downtime?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 100, Question = "Before migration, you run this to find blockers and compatibility gaps.", Answer = "What is a compatibility assessment?" },

            // 200
            new() { Category = "DP-3001 (Azure Data)", PointValue = 200, Question = "This is why you run a pre-migration assessment: to identify feature gaps and performance risks.", Answer = "What is risk identification?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 200, Question = "Modernization commonly aims to reduce ops burden and improve this.", Answer = "What is scalability?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 200, Question = "This migration moves tables, views, procedures, and other DB objects.", Answer = "What is schema migration?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 200, Question = "After migration, you do this to confirm counts/rows/critical queries match expectations.", Answer = "What is data validation?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 200, Question = "This plan is required so you can recover safely if cutover fails.", Answer = "What is a rollback plan?" },

            // 300
            new() { Category = "DP-3001 (Azure Data)", PointValue = 300, Question = "For minimal risk, the safest strategy is usually a pilot, staged rollout, then this final step.", Answer = "What is cutover?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 300, Question = "A proof-of-concept migration is used to test assumptions on this.", Answer = "What is a real workload?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 300, Question = "To compare old vs new databases, you measure latency, throughput, errors, and this resource usage.", Answer = "What is CPU/DTU/vCore utilization?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 300, Question = "You typically update this last to prevent accidental writes to the wrong database.", Answer = "What are connection strings?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 300, Question = "This is watching performance and errors after go-live.", Answer = "What is post-migration monitoring?" },

            // 400
            new() { Category = "DP-3001 (Azure Data)", PointValue = 400, Question = "The biggest hidden migration risk is often app dependencies and this.", Answer = "What are unsupported features?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 400, Question = "You run read-only validation queries to verify behavior without changing this.", Answer = "What is data?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 400, Question = "A key benefit of Azure SQL managed services is automatic patching, backups, and this.", Answer = "What is high availability?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 400, Question = "Migrations often fail at authentication due to differences in identity and this.", Answer = "What are permissions?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 400, Question = "RPO and RTO refer to data loss tolerance and this other recovery target.", Answer = "What is recovery time?" },

            // 500
            new() { Category = "DP-3001 (Azure Data)", PointValue = 500, Question = "Near-zero downtime migrations often use continuous sync plus this short final window.", Answer = "What is a short cutover window?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 500, Question = "If the app is slow after migration, a top suspect is missing indexes or changed this.", Answer = "What are query plans?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 500, Question = "Data looks correct but the app errors; a common root cause is collation/encoding or this behavior difference.", Answer = "What is compatibility?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 500, Question = "For very large tables, you validate using row counts plus checksums, sampling, and these results.", Answer = "What are critical query results?" },
            new() { Category = "DP-3001 (Azure Data)", PointValue = 500, Question = "The most important non-technical deliverable in a migration is a runbook plus this.", Answer = "What is stakeholder communication?" },


            // ==================== DP-080 (Data Fundamentals) (Relational basics + T-SQL) ====================

            // 100
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 100, Question = "This SQL keyword retrieves data.", Answer = "What is SELECT?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 100, Question = "This SQL clause filters rows.", Answer = "What is WHERE?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 100, Question = "This SQL clause sorts results.", Answer = "What is ORDER BY?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 100, Question = "This SQL operation combines rows from related tables.", Answer = "What is a JOIN?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 100, Question = "This is the unique identifier for a table row.", Answer = "What is a primary key?" },

            // 200
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 200, Question = "This JOIN keeps all rows from the left table even when there’s no match.", Answer = "What is a LEFT JOIN?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 200, Question = "This clause enables aggregation per group.", Answer = "What is GROUP BY?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 200, Question = "This aggregate returns the number of rows.", Answer = "What is COUNT(*)?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 200, Question = "This database design process reduces duplication and anomalies.", Answer = "What is normalization?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 200, Question = "This constraint links child rows to a parent key.", Answer = "What is a foreign key?" },

            // 300
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 300, Question = "This clause filters aggregated results.", Answer = "What is HAVING?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 300, Question = "This improves query speed for lookups/sorts, with tradeoffs.", Answer = "What is an index?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 300, Question = "This is an all-or-nothing group of database operations.", Answer = "What is a transaction?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 300, Question = "In ACID, the 'A' stands for this.", Answer = "What is atomicity?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 300, Question = "This is a common risk of SELECT * in application code.", Answer = "What is unnecessary data transfer and breaking changes?" },

            // 400
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 400, Question = "Slow queries and table scans are a common symptom of missing this.", Answer = "What are indexes?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 400, Question = "This prevents SQL injection and improves plan reuse.", Answer = "What is parameterization?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 400, Question = "This is saved SQL logic executed on the server.", Answer = "What is a stored procedure?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 400, Question = "This is a saved query that behaves like a virtual table.", Answer = "What is a view?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 400, Question = "This refers to uniqueness and row counts that influence query plans.", Answer = "What is cardinality?" },

            // 500
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 500, Question = "If a query is slow only sometimes, a common cause is parameter sniffing or changing this.", Answer = "What is the execution plan?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 500, Question = "The best way to diagnose why a query is slow is to inspect this plus IO/time stats.", Answer = "What is the execution plan?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 500, Question = "Too many indexes increase storage and often slow this type of operation.", Answer = "What are writes (inserts/updates/deletes)?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 500, Question = "To get consistent reads during updates, you use a proper isolation level like this.", Answer = "What is snapshot isolation?" },
            new() { Category = "DP-080 (Data Fundamentals)", PointValue = 500, Question = "Even with ORMs, you still need SQL knowledge because bad queries and indexes still impact this.", Answer = "What is performance?" },


            // ==================== DP-3020 (Advanced Data) (Data-driven apps with Azure SQL) ====================

            // 100
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 100, Question = "This Azure service is primarily a managed relational data store.", Answer = "What is Azure SQL Database?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 100, Question = "This defines how an app connects to a database.", Answer = "What is a connection string?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 100, Question = "This security concept means granting minimal permissions needed for DB access.", Answer = "What is least privilege?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 100, Question = "Apps commonly use an API layer for database operations like this.", Answer = "What is CRUD?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 100, Question = "This relational constraint type enforces parent/child relationships.", Answer = "What is a foreign key constraint?" },

            // 200
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 200, Question = "This avoids stored credentials in code/config when accessing Azure SQL.", Answer = "What is Managed Identity?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 200, Question = "A common way to improve read performance is proper this.", Answer = "What is indexing?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 200, Question = "This operation inserts if missing, otherwise updates.", Answer = "What is an upsert?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 200, Question = "Retries are used to handle these kinds of failures in cloud DB access.", Answer = "What are transient failures?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 200, Question = "Separating read and write workloads helps with scaling and this.", Answer = "What is performance isolation?" },

            // 300
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 300, Question = "This reduces connection overhead by reusing established connections.", Answer = "What is connection pooling?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 300, Question = "This keeps schema changes version-controlled and repeatable.", Answer = "What are migrations?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 300, Question = "In APIs, repeating a request should not cause duplicate effects; that property is this.", Answer = "What is idempotency?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 300, Question = "The best defense against SQL injection is this type of query building.", Answer = "What are parameterized queries?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 300, Question = "A common secrets strategy in CI/CD is to use a vault plus these.", Answer = "What are pipeline secret variables?" },

            // 400
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 400, Question = "This concurrency approach detects conflicts without heavy locking.", Answer = "What is optimistic concurrency?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 400, Question = "This is a common concurrency token in SQL systems.", Answer = "What is RowVersion (Timestamp)?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 400, Question = "This scales read traffic separately from writes.", Answer = "What are read replicas?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 400, Question = "This resiliency pattern prevents hammering a failing database dependency.", Answer = "What is a circuit breaker?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 400, Question = "DB performance tuning is usually about these three: query, index, and this.", Answer = "What is schema design?" },

            // 500
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 500, Question = "If an Azure SQL app times out under load, the first fix is indexing hot queries plus this.", Answer = "What is scaling compute appropriately?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 500, Question = "Deploy succeeded but the app can’t connect; the most common causes are firewall/network rules or this.", Answer = "What are identity/permission issues?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 500, Question = "Slow bulk imports usually need batching plus this technique.", Answer = "What is bulk loading?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 500, Question = "If you need auditability, you add logging/auditing and ideally this property for logs.", Answer = "What is immutability?" },
            new() { Category = "DP-3020 (Advanced Data)", PointValue = 500, Question = "Zero-downtime schema changes rely on backward-compatible migrations plus this approach.", Answer = "What is phased rollout?" },


            // ==================== .NET CORE ====================

            // 100
            new() { Category = ".NET Core", PointValue = 100, Question = "This file defines a .NET project and its dependencies.", Answer = "What is a .csproj file?" },
            new() { Category = ".NET Core", PointValue = 100, Question = "This pattern provides dependencies to classes rather than creating them inside.", Answer = "What is dependency injection (DI)?" },
            new() { Category = ".NET Core", PointValue = 100, Question = "In ASP.NET Core, these are pipeline components that handle requests and responses.", Answer = "What is middleware?" },
            new() { Category = ".NET Core", PointValue = 100, Question = "This is the package manager ecosystem for .NET libraries.", Answer = "What is NuGet?" },
            new() { Category = ".NET Core", PointValue = 100, Question = "This CLI command builds and runs a .NET project.", Answer = "What is dotnet run?" },

            // 200
            new() { Category = ".NET Core", PointValue = 200, Question = "These configuration files commonly store settings, with environment overrides.", Answer = "What is appsettings.json?" },
            new() { Category = ".NET Core", PointValue = 200, Question = "This service is used to read configuration values in .NET apps.", Answer = "What is IConfiguration?" },
            new() { Category = ".NET Core", PointValue = 200, Question = "This interface is used for structured logging in .NET.", Answer = "What is ILogger<T>?" },
            new() { Category = ".NET Core", PointValue = 200, Question = "This command downloads NuGet dependencies for a project.", Answer = "What is dotnet restore?" },
            new() { Category = ".NET Core", PointValue = 200, Question = "This is the cross-platform web server used by ASP.NET Core.", Answer = "What is Kestrel?" },

            // 300
            new() { Category = ".NET Core", PointValue = 300, Question = "This binds configuration into a strongly-typed options class.", Answer = "What is IOptions<T>?" },
            new() { Category = ".NET Core", PointValue = 300, Question = "This endpoint style maps routes directly without controllers.", Answer = "What is Minimal APIs?" },
            new() { Category = ".NET Core", PointValue = 300, Question = "This DI lifetime creates one instance per request scope.", Answer = "What is scoped?" },
            new() { Category = ".NET Core", PointValue = 300, Question = "This DI lifetime creates one instance for the entire app lifetime.", Answer = "What is singleton?" },
            new() { Category = ".NET Core", PointValue = 300, Question = "This DI lifetime creates a new instance each time it’s requested.", Answer = "What is transient?" },

            // 400
            new() { Category = ".NET Core", PointValue = 400, Question = "Blocking calls in web apps can cause this under load.", Answer = "What is thread starvation?" },
            new() { Category = ".NET Core", PointValue = 400, Question = "This language feature helps with non-blocking I/O and scalability.", Answer = "What is async/await?" },
            new() { Category = ".NET Core", PointValue = 400, Question = "Configuration is layered primarily to support different settings per this.", Answer = "What is environment (dev/test/prod)?" },
            new() { Category = ".NET Core", PointValue = 400, Question = "AddControllers() uses MVC controllers, while this style maps endpoints directly.", Answer = "What are Minimal APIs?" },
            new() { Category = ".NET Core", PointValue = 400, Question = "This feature runs background tasks integrated with app lifecycle.", Answer = "What are hosted services?" },

            // 500
            new() { Category = ".NET Core", PointValue = 500, Question = "If an app works locally but fails in Azure, a common mistake is missing these.", Answer = "What are environment variables/connection strings?" },
            new() { Category = ".NET Core", PointValue = 500, Question = "For intermittent dependency failures, common resiliency tools are retry/backoff plus this.", Answer = "What is a circuit breaker?" },
            new() { Category = ".NET Core", PointValue = 500, Question = "Structured logs are better than plain strings because they support better search and this.", Answer = "What is correlation?" },
            new() { Category = ".NET Core", PointValue = 500, Question = "Request correlation across services is commonly done with this.", Answer = "What is distributed tracing?" },
            new() { Category = ".NET Core", PointValue = 500, Question = "The best way to keep secrets out of source control is using a secret store like this.", Answer = "What is Azure Key Vault?" },


            // ==================== GIT & VERSION CONTROL ====================

            // 100
            new() { Category = "Git & Version Control", PointValue = 100, Question = "This Git command saves a snapshot of staged changes with a message.", Answer = "What is git commit?" },
            new() { Category = "Git & Version Control", PointValue = 100, Question = "This Git command shows the current working tree and staging state.", Answer = "What is git status?" },
            new() { Category = "Git & Version Control", PointValue = 100, Question = "This Git command lists local branches.", Answer = "What is git branch?" },
            new() { Category = "Git & Version Control", PointValue = 100, Question = "This happens when Git can’t automatically combine changes.", Answer = "What is a merge conflict?" },
            new() { Category = "Git & Version Control", PointValue = 100, Question = "This Git command copies a remote repository locally.", Answer = "What is git clone?" },

            // 200
            new() { Category = "Git & Version Control", PointValue = 200, Question = "This command fetches remote changes and then integrates them into your branch.", Answer = "What is git pull?" },
            new() { Category = "Git & Version Control", PointValue = 200, Question = "This command downloads remote references without merging.", Answer = "What is git fetch?" },
            new() { Category = "Git & Version Control", PointValue = 200, Question = "This workflow is used to review and merge changes safely.", Answer = "What is a Pull Request (PR)?" },
            new() { Category = "Git & Version Control", PointValue = 200, Question = "This file tells Git which files to not track.", Answer = "What is .gitignore?" },
            new() { Category = "Git & Version Control", PointValue = 200, Question = "A commit is a local save, while this sends commits to the remote.", Answer = "What is git push?" },

            // 300
            new() { Category = "Git & Version Control", PointValue = 300, Question = "This rewrites history by replaying commits onto a new base commit.", Answer = "What is git rebase?" },
            new() { Category = "Git & Version Control", PointValue = 300, Question = "This is a named pointer to a remote repo URL (like origin).", Answer = "What is a remote?" },
            new() { Category = "Git & Version Control", PointValue = 300, Question = "Commit messages should be clear, imperative, and describe this.", Answer = "What is the change?" },
            new() { Category = "Git & Version Control", PointValue = 300, Question = "This state means you’re not on a branch tip.", Answer = "What is detached HEAD?" },
            new() { Category = "Git & Version Control", PointValue = 300, Question = "Branching is used to isolate this safely.", Answer = "What is work (features/fixes)?" },

            // 400
            new() { Category = "Git & Version Control", PointValue = 400, Question = "Merge is often preferred over rebase when you want to preserve this.", Answer = "What is branch history/context?" },
            new() { Category = "Git & Version Control", PointValue = 400, Question = "This merge type advances the branch pointer without creating a merge commit.", Answer = "What is a fast-forward merge?" },
            new() { Category = "Git & Version Control", PointValue = 400, Question = "This command applies a specific commit onto the current branch.", Answer = "What is git cherry-pick?" },
            new() { Category = "Git & Version Control", PointValue = 400, Question = "This merge strategy combines multiple commits into one when merging a PR.", Answer = "What is squash merge?" },
            new() { Category = "Git & Version Control", PointValue = 400, Question = "This marks a specific commit, often used for releases.", Answer = "What is a tag?" },

            // 500
            new() { Category = "Git & Version Control", PointValue = 500, Question = "If you accidentally commit a secret, the first response is to rotate it and do this carefully.", Answer = "What is removing it from history?" },
            new() { Category = "Git & Version Control", PointValue = 500, Question = "A PR shows huge unrelated changes most often because of this.", Answer = "What is the wrong base branch (or a dirty branch)?" },
            new() { Category = "Git & Version Control", PointValue = 500, Question = "To prevent repeated merge conflicts, keep branches small and integrate changes this often.", Answer = "What is frequently?" },
            new() { Category = "Git & Version Control", PointValue = 500, Question = "To undo the last commit but keep changes staged, use this command.", Answer = "What is git reset --soft HEAD~1?" },
            new() { Category = "Git & Version Control", PointValue = 500, Question = "To discard local changes completely, you use this command.", Answer = "What is git reset --hard?" },


            // ==================== CLOUD SECURITY ====================

            // 100
            new() { Category = "Cloud Security", PointValue = 100, Question = "This principle means granting only the minimum access required.", Answer = "What is least privilege?" },
            new() { Category = "Cloud Security", PointValue = 100, Question = "This adds a second factor to login beyond a password.", Answer = "What is MFA (Multi-Factor Authentication)?" },
            new() { Category = "Cloud Security", PointValue = 100, Question = "This means data is encrypted when stored.", Answer = "What is encryption at rest?" },
            new() { Category = "Cloud Security", PointValue = 100, Question = "This means data is encrypted while moving across a network.", Answer = "What is encryption in transit?" },
            new() { Category = "Cloud Security", PointValue = 100, Question = "This security approach uses multiple layers of controls.", Answer = "What is defense in depth?" },

            // 200
            new() { Category = "Cloud Security", PointValue = 200, Question = "This is a common identity risk in cloud environments.", Answer = "What is credential theft?" },
            new() { Category = "Cloud Security", PointValue = 200, Question = "This is a minimum secure configuration standard.", Answer = "What is a security baseline?" },
            new() { Category = "Cloud Security", PointValue = 200, Question = "This is a sensitive value like a password, token, or connection string.", Answer = "What is a secret?" },
            new() { Category = "Cloud Security", PointValue = 200, Question = "This is a common place to store secrets securely.", Answer = "What is a vault (Key Vault)?" },
            new() { Category = "Cloud Security", PointValue = 200, Question = "This permission model assigns access using roles at defined scopes.", Answer = "What is RBAC?" },

            // 300
            new() { Category = "Cloud Security", PointValue = 300, Question = "This security model assumes no implicit trust and requires continuous verification.", Answer = "What is Zero Trust?" },
            new() { Category = "Cloud Security", PointValue = 300, Question = "This groups identities for easier access control management.", Answer = "What is a security group?" },
            new() { Category = "Cloud Security", PointValue = 300, Question = "This is a common web app risk category (e.g., SQL injection, XSS).", Answer = "What is injection/XSS?" },
            new() { Category = "Cloud Security", PointValue = 300, Question = "This removes the need to manage credentials for Azure resources.", Answer = "What is Managed Identity?" },
            new() { Category = "Cloud Security", PointValue = 300, Question = "Security event logs are primarily used for detection, forensics, and this.", Answer = "What is compliance?" },

            // 400
            new() { Category = "Cloud Security", PointValue = 400, Question = "Perimeter-only security is weak in cloud because this is the new perimeter.", Answer = "What is identity?" },
            new() { Category = "Cloud Security", PointValue = 400, Question = "This practice regularly changes secrets/keys to reduce exposure window.", Answer = "What is key rotation?" },
            new() { Category = "Cloud Security", PointValue = 400, Question = "This network design limits blast radius by isolating workloads.", Answer = "What is segmentation?" },
            new() { Category = "Cloud Security", PointValue = 400, Question = "This protects web apps by filtering/blocking malicious HTTP traffic.", Answer = "What is a WAF (Web Application Firewall)?" },
            new() { Category = "Cloud Security", PointValue = 400, Question = "This is cloud security posture management—tools that find misconfigurations at scale.", Answer = "What is CSPM?" },

            // 500
            new() { Category = "Cloud Security", PointValue = 500, Question = "If a token leaks, the immediate response is to revoke/rotate it and do this next.", Answer = "What is investigating access logs?" },
            new() { Category = "Cloud Security", PointValue = 500, Question = "To prove who changed what and when, you rely on audit logs and this.", Answer = "What is immutable logging?" },
            new() { Category = "Cloud Security", PointValue = 500, Question = "The best pattern for secret access without human-managed keys is this.", Answer = "What is Managed Identity + Key Vault RBAC?" },
            new() { Category = "Cloud Security", PointValue = 500, Question = "To reduce blast radius of a compromised account, you combine least privilege with this and segmentation.", Answer = "What is conditional access?" },
            new() { Category = "Cloud Security", PointValue = 500, Question = "Security by checklist fails mainly because threats change—so you need monitoring and this.", Answer = "What is incident response?" },


            // ==================== DOCKER ====================

            // 100
            new() { Category = "Docker", PointValue = 100, Question = "This is a read-only template used to create containers.", Answer = "What is a Docker image?" },
            new() { Category = "Docker", PointValue = 100, Question = "This is a running instance of a Docker image.", Answer = "What is a container?" },
            new() { Category = "Docker", PointValue = 100, Question = "This file defines the steps to build a Docker image.", Answer = "What is a Dockerfile?" },
            new() { Category = "Docker", PointValue = 100, Question = "This command lists running containers.", Answer = "What is docker ps?" },
            new() { Category = "Docker", PointValue = 100, Question = "This command builds a Docker image from a Dockerfile.", Answer = "What is docker build?" },

            // 200
            new() { Category = "Docker", PointValue = 200, Question = "This identifies an image version label like :latest.", Answer = "What is a tag?" },
            new() { Category = "Docker", PointValue = 200, Question = "This downloads an image from a registry.", Answer = "What is docker pull?" },
            new() { Category = "Docker", PointValue = 200, Question = "This creates and starts a container from an image.", Answer = "What is docker run?" },
            new() { Category = "Docker", PointValue = 200, Question = "This maps a host port to a container port (example: -p 8080:80).", Answer = "What is port mapping?" },
            new() { Category = "Docker", PointValue = 200, Question = "This persists data outside the container lifecycle.", Answer = "What is a Docker volume?" },

            // 300
            new() { Category = "Docker", PointValue = 300, Question = "This build technique helps produce smaller final images by separating build and runtime stages.", Answer = "What is a multi-stage build?" },
            new() { Category = "Docker", PointValue = 300, Question = "This file prevents unnecessary files from being sent to the Docker build context.", Answer = "What is .dockerignore?" },
            new() { Category = "Docker", PointValue = 300, Question = "This defines the main process a container runs.", Answer = "What is an entrypoint?" },
            new() { Category = "Docker", PointValue = 300, Question = "A key best practice is to use slim base images and minimize this.", Answer = "What are layers?" },
            new() { Category = "Docker", PointValue = 300, Question = "Containers should not run as this for security reasons.", Answer = "What is root?" },

            // 400
            new() { Category = "Docker", PointValue = 400, Question = "If a container keeps restarting, the first thing to check is this output.", Answer = "What are container logs (docker logs)?" },
            new() { Category = "Docker", PointValue = 400, Question = "Pinning versions in Dockerfiles improves this.", Answer = "What is build reproducibility?" },
            new() { Category = "Docker", PointValue = 400, Question = "In containers, this means rebuild the image instead of patching running servers.", Answer = "What is immutable infrastructure?" },
            new() { Category = "Docker", PointValue = 400, Question = "ENV is runtime config, while this is build-time only.", Answer = "What is ARG?" },
            new() { Category = "Docker", PointValue = 400, Question = "The “one process per container” idea helps with scaling and this.", Answer = "What is failure isolation?" },

            // 500
            new() { Category = "Docker", PointValue = 500, Question = "Apps that work locally but fail in containers often bind to localhost instead of this address.", Answer = "What is 0.0.0.0?" },
            new() { Category = "Docker", PointValue = 500, Question = "Fast ways to shrink huge images include smaller bases and this build technique.", Answer = "What is multi-stage build?" },
            new() { Category = "Docker", PointValue = 500, Question = "Putting secrets in a Dockerfile is bad because they can end up in image layers and this.", Answer = "What is image history?" },
            new() { Category = "Docker", PointValue = 500, Question = "To find known CVEs in images, you run this type of process.", Answer = "What is vulnerability scanning?" },
            new() { Category = "Docker", PointValue = 500, Question = "Using :latest in production is risky because it breaks this.", Answer = "What is repeatability?" },


            // ==================== KUBERNETES ====================

            // 100
            new() { Category = "Kubernetes", PointValue = 100, Question = "This is the smallest deployable unit in Kubernetes.", Answer = "What is a Pod?" },
            new() { Category = "Kubernetes", PointValue = 100, Question = "This manages replicas and rolling updates for pods.", Answer = "What is a Deployment?" },
            new() { Category = "Kubernetes", PointValue = 100, Question = "This provides a stable network endpoint for a set of pods.", Answer = "What is a Service?" },
            new() { Category = "Kubernetes", PointValue = 100, Question = "This stores non-secret configuration data.", Answer = "What is a ConfigMap?" },
            new() { Category = "Kubernetes", PointValue = 100, Question = "This stores sensitive configuration data.", Answer = "What is a Secret?" },

            // 200
            new() { Category = "Kubernetes", PointValue = 200, Question = "This ensures the desired number of pod replicas are running.", Answer = "What is a ReplicaSet?" },
            new() { Category = "Kubernetes", PointValue = 200, Question = "This manages HTTP/HTTPS routing into a cluster.", Answer = "What is Ingress?" },
            new() { Category = "Kubernetes", PointValue = 200, Question = "This provides logical isolation inside a cluster.", Answer = "What is a namespace?" },
            new() { Category = "Kubernetes", PointValue = 200, Question = "This is the worker machine that runs pods.", Answer = "What is a node?" },
            new() { Category = "Kubernetes", PointValue = 200, Question = "This is the main Kubernetes command-line tool.", Answer = "What is kubectl?" },

            // 300
            new() { Category = "Kubernetes", PointValue = 300, Question = "This update strategy replaces pods gradually to minimize downtime.", Answer = "What is a rolling update?" },
            new() { Category = "Kubernetes", PointValue = 300, Question = "This probe restarts containers that are unhealthy.", Answer = "What is a liveness probe?" },
            new() { Category = "Kubernetes", PointValue = 300, Question = "This probe prevents traffic from going to a pod until it’s ready.", Answer = "What is a readiness probe?" },
            new() { Category = "Kubernetes", PointValue = 300, Question = "Labels and selectors are used to group resources and do this.", Answer = "What is targeting?" },
            new() { Category = "Kubernetes", PointValue = 300, Question = "This runs one pod per node, often for agents like logging.", Answer = "What is a DaemonSet?" },

            // 400
            new() { Category = "Kubernetes", PointValue = 400, Question = "This usually means a container is repeatedly crashing and restarting.", Answer = "What is CrashLoopBackOff?" },
            new() { Category = "Kubernetes", PointValue = 400, Question = "This autoscaler scales pod replicas based on metrics.", Answer = "What is the Horizontal Pod Autoscaler (HPA)?" },
            new() { Category = "Kubernetes", PointValue = 400, Question = "This tool packages and versions Kubernetes deployments.", Answer = "What is Helm?" },
            new() { Category = "Kubernetes", PointValue = 400, Question = "This is used for stateful apps needing stable identity and storage.", Answer = "What is a StatefulSet?" },
            new() { Category = "Kubernetes", PointValue = 400, Question = "This requests persistent storage from the cluster.", Answer = "What is a PersistentVolumeClaim (PVC)?" },

            // 500
            new() { Category = "Kubernetes", PointValue = 500, Question = "A Service is up but no traffic reaches pods; the first suspect is this mismatch.", Answer = "What is a selector/label mismatch?" },
            new() { Category = "Kubernetes", PointValue = 500, Question = "Pods can’t pull an image; a common cause is registry auth or wrong image this.", Answer = "What is the tag/name?" },
            new() { Category = "Kubernetes", PointValue = 500, Question = "Zero-downtime deploys rely heavily on correct readiness probes and this strategy.", Answer = "What is rolling update configuration?" },
            new() { Category = "Kubernetes", PointValue = 500, Question = "To reduce cluster cost, a first lever is right-sizing requests/limits plus this.", Answer = "What is autoscaling?" },
            new() { Category = "Kubernetes", PointValue = 500, Question = "If a secret leaks, you rotate it, restrict RBAC, and audit this.", Answer = "What is access?" },


            // ==================== AZURE BLOB STORAGE ====================

            // 100
            new() { Category = "Azure Blob Storage", PointValue = 100, Question = "In Blob Storage, this is like a folder/bucket for blobs.", Answer = "What is a container?" },
            new() { Category = "Azure Blob Storage", PointValue = 100, Question = "The three main blob types are Block, Append, and this.", Answer = "What is Page?" },
            new() { Category = "Azure Blob Storage", PointValue = 100, Question = "A common use of Blob Storage is storing files, images, and this.", Answer = "What are backups?" },
            new() { Category = "Azure Blob Storage", PointValue = 100, Question = "This is time-limited delegated access to storage.", Answer = "What is a SAS token?" },
            new() { Category = "Azure Blob Storage", PointValue = 100, Question = "This access tier is optimized for frequently accessed data.", Answer = "What is Hot?" },

            // 200
            new() { Category = "Azure Blob Storage", PointValue = 200, Question = "This access tier is optimized for infrequently accessed data at lower storage cost.", Answer = "What is Cool?" },
            new() { Category = "Azure Blob Storage", PointValue = 200, Question = "This access tier is lowest cost but slow to retrieve due to rehydration.", Answer = "What is Archive?" },
            new() { Category = "Azure Blob Storage", PointValue = 200, Question = "This is the parent Azure resource that contains Blob, Queue, Table, and File services.", Answer = "What is a Storage Account?" },
            new() { Category = "Azure Blob Storage", PointValue = 200, Question = "This feature lets you recover older versions of blobs.", Answer = "What is blob versioning?" },
            new() { Category = "Azure Blob Storage", PointValue = 200, Question = "This automatically moves/deletes blobs based on rules (like age/tier).", Answer = "What is lifecycle management?" },

            // 300
            new() { Category = "Azure Blob Storage", PointValue = 300, Question = "To give apps secure access without account keys, you use Managed Identity plus this.", Answer = "What is RBAC?" },
            new() { Category = "Azure Blob Storage", PointValue = 300, Question = "This provides private access from a VNet to Blob Storage.", Answer = "What is a Private Endpoint (Private Link)?" },
            new() { Category = "Azure Blob Storage", PointValue = 300, Question = "Checksums (like MD5) are used to verify this.", Answer = "What is data integrity?" },
            new() { Category = "Azure Blob Storage", PointValue = 300, Question = "Large uploads are commonly handled using chunked uploads to this blob type.", Answer = "What is a block blob?" },
            new() { Category = "Azure Blob Storage", PointValue = 300, Question = "This feature lets you recover deleted blobs within a retention window.", Answer = "What is soft delete?" },

            // 400
            new() { Category = "Azure Blob Storage", PointValue = 400, Question = "For immutable backups, you enable time-based retention or this control.", Answer = "What is a legal hold (immutable storage)?" },
            new() { Category = "Azure Blob Storage", PointValue = 400, Question = "A storage account key grants broad access, while this is scoped and time-limited.", Answer = "What is a SAS?" },
            new() { Category = "Azure Blob Storage", PointValue = 400, Question = "Public containers are risky because they can cause this.", Answer = "What is data exposure?" },
            new() { Category = "Azure Blob Storage", PointValue = 400, Question = "Logging on storage is used for auditing and this.", Answer = "What is troubleshooting?" },
            new() { Category = "Azure Blob Storage", PointValue = 400, Question = "A common place to query storage logs/metrics is Azure Monitor and this workspace.", Answer = "What is Log Analytics?" },

            // 500
            new() { Category = "Azure Blob Storage", PointValue = 500, Question = "A 403 when accessing a blob is often caused by missing permissions or this.", Answer = "What is an expired SAS?" },
            new() { Category = "Azure Blob Storage", PointValue = 500, Question = "To improve global read performance for static content, you commonly use this.", Answer = "What is a CDN?" },
            new() { Category = "Azure Blob Storage", PointValue = 500, Question = "Archive tier retrieval is slow primarily because it requires this.", Answer = "What is rehydration?" },
            new() { Category = "Azure Blob Storage", PointValue = 500, Question = "Best practice for sharing access to clients is short-lived SAS plus this.", Answer = "What is least privilege?" },
            new() { Category = "Azure Blob Storage", PointValue = 500, Question = "If storage cost spikes, first levers include lifecycle rules, tiering, and reducing this.", Answer = "What is egress?" },


            // ==================== KEY VAULT ====================

            // 100
            new() { Category = "Key Vault", PointValue = 100, Question = "Key Vault stores these three main things.", Answer = "What are secrets, keys, and certificates?" },
            new() { Category = "Key Vault", PointValue = 100, Question = "A connection string, password, or token is an example of this.", Answer = "What is a secret?" },
            new() { Category = "Key Vault", PointValue = 100, Question = "Keys are used mainly for encryption and this.", Answer = "What is signing?" },
            new() { Category = "Key Vault", PointValue = 100, Question = "Certificates are commonly used for TLS and this.", Answer = "What is identity?" },
            new() { Category = "Key Vault", PointValue = 100, Question = "This allows apps to access Key Vault without stored credentials.", Answer = "What is Managed Identity?" },

            // 200
            new() { Category = "Key Vault", PointValue = 200, Question = "Best practice is Key Vault access via Managed Identity and this permission model.", Answer = "What is RBAC?" },
            new() { Category = "Key Vault", PointValue = 200, Question = "You rotate secrets to reduce the damage window if this happens.", Answer = "What is a leak/compromise?" },
            new() { Category = "Key Vault", PointValue = 200, Question = "Putting secrets in appsettings.json is risky because they can be accidentally this.", Answer = "What is committed to source control?" },
            new() { Category = "Key Vault", PointValue = 200, Question = "This controls who can read/write secrets/keys/certs in Key Vault.", Answer = "What are access policies/RBAC assignments?" },
            new() { Category = "Key Vault", PointValue = 200, Question = "This feature lets you recover deleted Key Vault items.", Answer = "What is soft delete?" },

            // 300
            new() { Category = "Key Vault", PointValue = 300, Question = "The guiding principle for Key Vault permissions is this.", Answer = "What is least privilege?" },
            new() { Category = "Key Vault", PointValue = 300, Question = "In App Service, you can reference Key Vault secrets using this feature.", Answer = "What are Key Vault references?" },
            new() { Category = "Key Vault", PointValue = 300, Question = "Dev/test/prod should use separate vaults to control this.", Answer = "What is blast radius?" },
            new() { Category = "Key Vault", PointValue = 300, Question = "Key Vault auditing is commonly enabled via these.", Answer = "What are diagnostic logs?" },
            new() { Category = "Key Vault", PointValue = 300, Question = "This implies the key material is protected by hardware.", Answer = "What is HSM-backed?" },

            // 400
            new() { Category = "Key Vault", PointValue = 400, Question = "If Key Vault works locally but fails in Azure, the most likely issue is missing this.", Answer = "What are identity permissions?" },
            new() { Category = "Key Vault", PointValue = 400, Question = "Long-lived secrets are risky because they increase this.", Answer = "What is exposure?" },
            new() { Category = "Key Vault", PointValue = 400, Question = "The clean “no-secret” pattern for Azure resources is this approach.", Answer = "What is Managed Identity everywhere possible?" },
            new() { Category = "Key Vault", PointValue = 400, Question = "Key versioning enables safe rotation without breaking these.", Answer = "What are consumers?" },
            new() { Category = "Key Vault", PointValue = 400, Question = "Granting Get/List to too many identities increases risk of this.", Answer = "What is secret discovery?" },

            // 500
            new() { Category = "Key Vault", PointValue = 500, Question = "If a secret is compromised, the first step is to rotate/revoke it and update these.", Answer = "What are consumers/apps?" },
            new() { Category = "Key Vault", PointValue = 500, Question = "Rotation without downtime typically uses a new version, then updates apps, then retires this.", Answer = "What is the old version?" },
            new() { Category = "Key Vault", PointValue = 500, Question = "To prove Key Vault access history, you enable diagnostic logs to Log Analytics or this.", Answer = "What is a SIEM?" },
            new() { Category = "Key Vault", PointValue = 500, Question = "Restricting network access to Key Vault reduces attack surface and prevents this.", Answer = "What is exfiltration?" },
            new() { Category = "Key Vault", PointValue = 500, Question = "Worst practice: using Key Vault as a dumping ground with broad this access.", Answer = "What is admin?" },


            // ==================== APP SERVICE ====================

            // 100
            new() { Category = "App Service", PointValue = 100, Question = "App Service primarily hosts these workloads.", Answer = "What are web apps and APIs?" },
            new() { Category = "App Service", PointValue = 100, Question = "This defines the compute and pricing tier for App Service apps.", Answer = "What is an App Service Plan?" },
            new() { Category = "App Service", PointValue = 100, Question = "This provides a staging environment for safe releases.", Answer = "What is a deployment slot?" },
            new() { Category = "App Service", PointValue = 100, Question = "This means moving to a larger tier with more resources per instance.", Answer = "What is scale up?" },
            new() { Category = "App Service", PointValue = 100, Question = "This means adding more instances to handle load.", Answer = "What is scale out?" },

            // 200
            new() { Category = "App Service", PointValue = 200, Question = "In App Service, connection strings are stored in this configuration area.", Answer = "What are app settings/connection strings settings?" },
            new() { Category = "App Service", PointValue = 200, Question = "A common deployment method for App Service uses GitHub Actions or this.", Answer = "What is zip deploy?" },
            new() { Category = "App Service", PointValue = 200, Question = "This setting helps keep an app from going idle.", Answer = "What is Always On?" },
            new() { Category = "App Service", PointValue = 200, Question = "Slots are used so you can test in staging and then do this to go live.", Answer = "What is swap?" },
            new() { Category = "App Service", PointValue = 200, Question = "Custom domain + TLS provides a friendly URL and secure this.", Answer = "What is HTTPS?" },

            // 300
            new() { Category = "App Service", PointValue = 300, Question = "Best practice: store secrets using Key Vault and reference them via this feature.", Answer = "What are Key Vault references?" },
            new() { Category = "App Service", PointValue = 300, Question = "This kind of setting does not swap between slots.", Answer = "What is a slot setting (sticky setting)?" },
            new() { Category = "App Service", PointValue = 300, Question = "For diagnosing 500 errors quickly, you use log streaming plus this telemetry tool.", Answer = "What is Application Insights?" },
            new() { Category = "App Service", PointValue = 300, Question = "A common reason App Service startup fails is wrong runtime/config/port, or bad this.", Answer = "What is configuration?" },
            new() { Category = "App Service", PointValue = 300, Question = "This feature removes unhealthy instances from rotation based on an endpoint.", Answer = "What is Health Check?" },

            // 400
            new() { Category = "App Service", PointValue = 400, Question = "To access private resources in a VNet from App Service, you use this feature.", Answer = "What is VNet Integration?" },
            new() { Category = "App Service", PointValue = 400, Question = "To access App Service privately (no public internet), you can use this.", Answer = "What is Private Endpoint (Private Link)?" },
            new() { Category = "App Service", PointValue = 400, Question = "This enables secure auth from App Service to Azure resources without secrets.", Answer = "What is Managed Identity?" },
            new() { Category = "App Service", PointValue = 400, Question = "Storing files locally on App Service is risky because local state can be lost during this.", Answer = "What is scaling/restarts?" },
            new() { Category = "App Service", PointValue = 400, Question = "Stateless design is preferred in App Service because it improves scaling and this.", Answer = "What is reliability?" },

            // 500
            new() { Category = "App Service", PointValue = 500, Question = "Slot swap breakages are most commonly caused by non-sticky settings swapping unexpectedly.", Answer = "What are slot settings (sticky settings)?" },
            new() { Category = "App Service", PointValue = 500, Question = "If an app is slow under load, the first scaling decision is usually to do this.", Answer = "What is scale out?" },
            new() { Category = "App Service", PointValue = 500, Question = "Frequent 502/503 errors often require checking app health, dependency failures, and these platform signals.", Answer = "What are metrics?" },
            new() { Category = "App Service", PointValue = 500, Question = "For near-zero downtime releases, you use slots, warm-up, then this action.", Answer = "What is swap?" },
            new() { Category = "App Service", PointValue = 500, Question = "If you must restrict outbound calls, you plan for networking controls and dependency this.", Answer = "What are allow-lists?" },


            // ==================== FUNCTIONS ====================

            // 100
            new() { Category = "Functions", PointValue = 100, Question = "This is serverless, event-driven compute in Azure.", Answer = "What is Azure Functions?" },
            new() { Category = "Functions", PointValue = 100, Question = "This starts a function execution.", Answer = "What is a trigger?" },
            new() { Category = "Functions", PointValue = 100, Question = "These connect functions to input/output services without extra boilerplate.", Answer = "What are bindings?" },
            new() { Category = "Functions", PointValue = 100, Question = "HTTP-triggered functions return this type of result.", Answer = "What is an HTTP response?" },
            new() { Category = "Functions", PointValue = 100, Question = "This trigger runs on a schedule.", Answer = "What is a timer trigger?" },

            // 200
            new() { Category = "Functions", PointValue = 200, Question = "This plan auto-scales and charges per execution.", Answer = "What is the Consumption plan?" },
            new() { Category = "Functions", PointValue = 200, Question = "This is the startup delay when a function spins up from idle.", Answer = "What is a cold start?" },
            new() { Category = "Functions", PointValue = 200, Question = "This trigger is commonly used for async message processing.", Answer = "What is a queue trigger?" },
            new() { Category = "Functions", PointValue = 200, Question = "This trigger reacts to Azure events published via Event Grid.", Answer = "What is an Event Grid trigger?" },
            new() { Category = "Functions", PointValue = 200, Question = "A common MSSA runtime/language for Azure Functions is this.", Answer = "What is C# (.NET)?" },

            // 300
            new() { Category = "Functions", PointValue = 300, Question = "This feature is used for stateful orchestration and long-running workflows.", Answer = "What is Durable Functions?" },
            new() { Category = "Functions", PointValue = 300, Question = "HTTP functions are commonly secured using auth (keys/Entra ID) and this gateway.", Answer = "What is API Management?" },
            new() { Category = "Functions", PointValue = 300, Question = "Best place for secrets used by a function is Key Vault accessed via this.", Answer = "What is Managed Identity?" },
            new() { Category = "Functions", PointValue = 300, Question = "Retries for queues/events help improve this.", Answer = "What is resiliency?" },
            new() { Category = "Functions", PointValue = 300, Question = "A common logging/telemetry integration for Functions is this.", Answer = "What is Application Insights?" },

            // 400
            new() { Category = "Functions", PointValue = 400, Question = "This plan reduces cold starts and supports advanced scaling scenarios.", Answer = "What is the Premium plan?" },
            new() { Category = "Functions", PointValue = 400, Question = "This pattern fans out work in parallel then aggregates results.", Answer = "What is fan-out/fan-in?" },
            new() { Category = "Functions", PointValue = 400, Question = "Long-running work should avoid staying on HTTP paths due to timeouts and this.", Answer = "What is scaling constraints?" },
            new() { Category = "Functions", PointValue = 400, Question = "This property ensures duplicate messages don’t cause duplicate side effects.", Answer = "What is idempotency?" },
            new() { Category = "Functions", PointValue = 400, Question = "This is a message that repeatedly fails processing and may need special handling.", Answer = "What is a poison message?" },

            // 500
            new() { Category = "Functions", PointValue = 500, Question = "To defend against duplicates in queue processing, you use idempotency keys and this.", Answer = "What is deduplication storage?" },
            new() { Category = "Functions", PointValue = 500, Question = "If a function times out doing heavy compute, best fix is moving work off HTTP via queues or this.", Answer = "What is Durable Functions?" },
            new() { Category = "Functions", PointValue = 500, Question = "Spiky load plus strict latency often favors this Functions plan due to pre-warmed instances.", Answer = "What is Premium?" },
            new() { Category = "Functions", PointValue = 500, Question = "Output bindings are preferred over manual SDK calls because they reduce boilerplate and standardize this.", Answer = "What is configuration?" },
            new() { Category = "Functions", PointValue = 500, Question = "At scale, the best architecture is many small single-purpose functions with this approach.", Answer = "What is message-driven design?" },


            // ==================== BICEP ====================

            // 100
            new() { Category = "Bicep", PointValue = 100, Question = "This is an infrastructure-as-code language for Azure.", Answer = "What is Bicep?" },
            new() { Category = "Bicep", PointValue = 100, Question = "Bicep compiles into this format.", Answer = "What is ARM JSON?" },
            new() { Category = "Bicep", PointValue = 100, Question = "This is an input value you pass into a Bicep deployment.", Answer = "What is a parameter?" },
            new() { Category = "Bicep", PointValue = 100, Question = "This is a declared Azure resource in Bicep.", Answer = "What is a resource?" },
            new() { Category = "Bicep", PointValue = 100, Question = "Bicep/ARM deployments are designed to be this: reruns converge to desired state.", Answer = "What is idempotent?" },

            // 200
            new() { Category = "Bicep", PointValue = 200, Question = "This is a reusable component in Bicep.", Answer = "What is a module?" },
            new() { Category = "Bicep", PointValue = 200, Question = "This holds computed values used during deployment.", Answer = "What is a variable?" },
            new() { Category = "Bicep", PointValue = 200, Question = "This returns values after deployment (like resource IDs/URLs).", Answer = "What is an output?" },
            new() { Category = "Bicep", PointValue = 200, Question = "This defines where a deployment occurs (resource group, subscription, etc.).", Answer = "What is scope?" },
            new() { Category = "Bicep", PointValue = 200, Question = "This previews changes without applying them.", Answer = "What is What-If?" },

            // 300
            new() { Category = "Bicep", PointValue = 300, Question = "Modules are used for reuse, consistency, and these smaller files.", Answer = "What is maintainability?" },
            new() { Category = "Bicep", PointValue = 300, Question = "Best practice is to avoid hardcoding secrets by using secure parameters or this service.", Answer = "What is Key Vault?" },
            new() { Category = "Bicep", PointValue = 300, Question = "A common naming pattern uses parameters plus a uniqueness function like this.", Answer = "What is uniqueString()?" },
            new() { Category = "Bicep", PointValue = 300, Question = "Putting infra in version control enables repeatability and this.", Answer = "What is auditability?" },
            new() { Category = "Bicep", PointValue = 300, Question = "Bicep is declarative, meaning you describe this, not steps.", Answer = "What is the end state?" },

            // 400
            new() { Category = "Bicep", PointValue = 400, Question = "Deploying the same template twice should converge, not this.", Answer = "What is duplication?" },
            new() { Category = "Bicep", PointValue = 400, Question = "Separate environment parameter files help prevent accidental this.", Answer = "What is cross-environment drift?" },
            new() { Category = "Bicep", PointValue = 400, Question = "Manual portal changes are risky because they cause untracked this.", Answer = "What is configuration drift?" },
            new() { Category = "Bicep", PointValue = 400, Question = "Tags in Bicep help with cost management, governance, and this.", Answer = "What is automation?" },
            new() { Category = "Bicep", PointValue = 400, Question = "Outputs are commonly used in CI/CD to feed resource IDs/URLs to these later steps.", Answer = "What are downstream steps?" },

            // 500
            new() { Category = "Bicep", PointValue = 500, Question = "To enforce platform standards across many environments, you use modules, parameters, and this.", Answer = "What is policy?" },
            new() { Category = "Bicep", PointValue = 500, Question = "Safest rollout uses incremental deploys plus What-If and these.", Answer = "What are small PRs?" },
            new() { Category = "Bicep", PointValue = 500, Question = "“Resource already exists” errors are often caused by conflicting names or this.", Answer = "What is scope mismatch?" },
            new() { Category = "Bicep", PointValue = 500, Question = "Secrets should never appear in deployment logs due to this risk.", Answer = "What is leakage?" },
            new() { Category = "Bicep", PointValue = 500, Question = "Bicep’s key win for teams is fast, repeatable, reproducible this.", Answer = "What is infrastructure?" },


            // ==================== APPLICATION INSIGHTS ====================

            // 100
            new() { Category = "Application Insights", PointValue = 100, Question = "Application Insights collects requests, dependencies, exceptions, traces, and this general data type.", Answer = "What is telemetry?" },
            new() { Category = "Application Insights", PointValue = 100, Question = "This telemetry item represents an incoming HTTP operation.", Answer = "What is a request?" },
            new() { Category = "Application Insights", PointValue = 100, Question = "This telemetry item represents an outgoing call like SQL or HTTP.", Answer = "What is a dependency?" },
            new() { Category = "Application Insights", PointValue = 100, Question = "This telemetry item captures errors and stack traces.", Answer = "What is an exception?" },
            new() { Category = "Application Insights", PointValue = 100, Question = "This telemetry type is log-style messages with properties.", Answer = "What is a trace?" },

            // 200
            new() { Category = "Application Insights", PointValue = 200, Question = "This query language is used in Application Insights Logs.", Answer = "What is KQL (Kusto Query Language)?" },
            new() { Category = "Application Insights", PointValue = 200, Question = "This reduces telemetry volume and cost by keeping only a portion of events.", Answer = "What is sampling?" },
            new() { Category = "Application Insights", PointValue = 200, Question = "This links related telemetry together using operation IDs.", Answer = "What is correlation?" },
            new() { Category = "Application Insights", PointValue = 200, Question = "This view shows near-real-time app health.", Answer = "What is Live Metrics?" },
            new() { Category = "Application Insights", PointValue = 200, Question = "Custom events help track business actions like signup/purchase—this kind of telemetry.", Answer = "What is business telemetry?" },

            // 300
            new() { Category = "Application Insights", PointValue = 300, Question = "This traces a single request across multiple services using shared context.", Answer = "What is distributed tracing?" },
            new() { Category = "Application Insights", PointValue = 300, Question = "Dependency duration helps identify this kind of bottleneck.", Answer = "What is a slow external call?" },
            new() { Category = "Application Insights", PointValue = 300, Question = "Extra custom properties added to telemetry for filtering are called these.", Answer = "What are custom dimensions?" },
            new() { Category = "Application Insights", PointValue = 300, Question = "Best practice is to log with structured properties instead of only this.", Answer = "What are strings?" },
            new() { Category = "Application Insights", PointValue = 300, Question = "A good first alert is often based on failure rate or this metric.", Answer = "What is response time?" },

            // 400
            new() { Category = "Application Insights", PointValue = 400, Question = "Telemetry can be misleading without correlation and full instrumentation; sampling can also cause this.", Answer = "What is missing context?" },
            new() { Category = "Application Insights", PointValue = 400, Question = "A common cause of noisy telemetry is logging every request at high this.", Answer = "What is verbosity?" },
            new() { Category = "Application Insights", PointValue = 400, Question = "To separate dev vs prod telemetry cleanly, you use different resources or this.", Answer = "What is different configuration (connection strings)?" },
            new() { Category = "Application Insights", PointValue = 400, Question = "End-to-end transaction details are used to pinpoint the slowest this.", Answer = "What is hop/dependency?" },
            new() { Category = "Application Insights", PointValue = 400, Question = "Exception logging best practice is to log once per failure path with this included.", Answer = "What is context?" },

            // 500
            new() { Category = "Application Insights", PointValue = 500, Question = "High 500s with no exceptions often means failures outside app code—like platform or this.", Answer = "What is a dependency?" },
            new() { Category = "Application Insights", PointValue = 500, Question = "To find the slowest endpoint quickly, you query requests ordered by this descending.", Answer = "What is duration?" },
            new() { Category = "Application Insights", PointValue = 500, Question = "If telemetry cost exploded overnight, the first lever is sampling plus reducing this.", Answer = "What is logging noise/verbosity?" },
            new() { Category = "Application Insights", PointValue = 500, Question = "User/session IDs must be handled carefully due to privacy and this.", Answer = "What is compliance?" },
            new() { Category = "Application Insights", PointValue = 500, Question = "A solid root-cause workflow is failures → dependencies → traces, all tied together via this.", Answer = "What is correlation?" },
            // ==================== SERVICE BUS ====================
            new() { Category = "Service Bus", PointValue = 100, Question = "In Azure messaging, this is the basic unit that stores messages for processing.", Answer = "What is a queue?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This Service Bus feature lets multiple subscribers receive the same message independently.", Answer = "What is a topic with subscriptions?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This is the name of the “failed messages” holding area in Service Bus.", Answer = "What is the dead-letter queue (DLQ)?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This concept describes sending a message now and processing it later, decoupling services.", Answer = "What is asynchronous messaging?" },
            new() { Category = "Service Bus", PointValue = 100, Question = "This describes a system where producers and consumers don’t need to be online at the same time.", Answer = "What is message buffering?" },

            new() { Category = "Service Bus", PointValue = 200, Question = "This receive mode locks a message so other consumers can’t process it while you work on it.", Answer = "What is Peek-Lock?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This receive mode removes the message immediately when it’s read.", Answer = "What is Receive and Delete?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This is the action that permanently marks a Peek-Lock message as successfully processed.", Answer = "What is Complete?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This Service Bus feature prevents accidental re-processing of the same message ID within a time window.", Answer = "What is duplicate detection?" },
            new() { Category = "Service Bus", PointValue = 200, Question = "This is the maximum amount of time a message can live before expiring.", Answer = "What is TTL (time-to-live)?" },

            new() { Category = "Service Bus", PointValue = 300, Question = "This feature enables ordered, FIFO-like processing and stateful workflows by grouping related messages.", Answer = "What are sessions?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This capability lets you delay delivery until a specific time.", Answer = "What are scheduled messages?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This feature lets you temporarily set aside a message and retrieve it later by sequence number.", Answer = "What is message deferral?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "These rules on a subscription route only certain messages to that subscriber.", Answer = "What are filters and actions?" },
            new() { Category = "Service Bus", PointValue = 300, Question = "This concept means you can process messages in batches to reduce network calls and improve throughput.", Answer = "What is batch processing?" },

            new() { Category = "Service Bus", PointValue = 400, Question = "This Service Bus feature lets multiple operations succeed or fail together as one unit.", Answer = "What are transactions?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This pattern is required because messaging systems often deliver messages more than once.", Answer = "What is idempotent processing?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This is the best first place to look when messages “disappear” from the main queue unexpectedly.", Answer = "What is the dead-letter queue (DLQ)?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This feature lets you restrict who can send or receive using role assignments, avoiding shared keys.", Answer = "What is RBAC with managed identity (or Entra ID auth)?" },
            new() { Category = "Service Bus", PointValue = 400, Question = "This setting controls how long a Peek-Lock message stays locked before it can be delivered again.", Answer = "What is lock duration?" },

            new() { Category = "Service Bus", PointValue = 500, Question = "A consumer crashes after processing but before completing—your system should still be correct by using this approach.", Answer = "What is idempotency plus retry-safe design?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You need a pub/sub design where each downstream system can apply its own filter—best Service Bus construct?", Answer = "What is a topic with filtered subscriptions?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You want a resilient workflow: monitor failures, alert, and reprocess safely—what queue feature is central?", Answer = "What is the dead-letter queue (DLQ) with retry/replay?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "To reduce duplicate effects when a message is redelivered, you should store and check this value.", Answer = "What is a deduplication/idempotency key (often the MessageId)?" },
            new() { Category = "Service Bus", PointValue = 500, Question = "You need message processing that preserves order per customer while scaling across many customers—best feature?", Answer = "What are sessions (session-based processing)?" },

            // ==================== EVENT GRID ====================
            new() { Category = "Event Grid", PointValue = 100, Question = "This Azure service routes events from sources to handlers using a push model.", Answer = "What is Event Grid?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "In Event Grid, this is the destination that receives events (like Functions or Logic Apps).", Answer = "What is an event handler?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "In Event Grid, this is the configuration that tells where events should be delivered.", Answer = "What is an event subscription?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This common Event Grid source emits events when blobs are created or deleted.", Answer = "What is Azure Storage (Blob Storage)?" },
            new() { Category = "Event Grid", PointValue = 100, Question = "This describes reacting to something that happened, instead of polling for it.", Answer = "What is event-driven architecture?" },

            new() { Category = "Event Grid", PointValue = 200, Question = "This Event Grid feature lets you filter events by subject, type, or advanced rules.", Answer = "What is event filtering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This Event Grid option is used to ensure you can capture undeliverable events for later investigation.", Answer = "What is dead-lettering?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "These two handler types commonly process Event Grid events with minimal code.", Answer = "What are Azure Functions and Logic Apps?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This describes Event Grid delivery behavior where events may be delivered more than once.", Answer = "What is at-least-once delivery?" },
            new() { Category = "Event Grid", PointValue = 200, Question = "This Event Grid entity represents a publisher endpoint for custom events.", Answer = "What is a custom topic?" },

            new() { Category = "Event Grid", PointValue = 300, Question = "This kind of topic is automatically created/managed for Azure services as event sources.", Answer = "What is a system topic?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is the mechanism Event Grid uses to prove an endpoint really owns the subscription (handshake).", Answer = "What is subscription validation?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This feature groups related custom topics under a single management surface.", Answer = "What are event domains?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is a best practice because Event Grid can deliver duplicates and retries.", Answer = "What is idempotent event handling?" },
            new() { Category = "Event Grid", PointValue = 300, Question = "This is a common reason Event Grid deliveries fail to a webhook endpoint.", Answer = "What is endpoint authentication/authorization or network access blocking?" },

            new() { Category = "Event Grid", PointValue = 400, Question = "You need to keep events private inside Azure networks—this feature helps avoid public exposure.", Answer = "What are private endpoints (Private Link)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This is the recommended way to avoid processing the same business event twice.", Answer = "What is deduplication using an event ID plus idempotency storage?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This Event Grid capability lets you route only events matching specific metadata to different handlers.", Answer = "What are advanced filters (routing rules)?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "If an event handler is down temporarily, Event Grid does this automatically.", Answer = "What is retry with backoff?" },
            new() { Category = "Event Grid", PointValue = 400, Question = "This is the most common first troubleshooting step when events aren’t arriving at a Function.", Answer = "What is checking the event subscription configuration and delivery logs/dead-letter?" },

            new() { Category = "Event Grid", PointValue = 500, Question = "You want durable, replayable processing rather than pure push events—best pattern with Event Grid?", Answer = "What is Event Grid to Service Bus/Queue for durable buffering?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You must guarantee downstream consistency despite retries and duplicates—key handler design principle?", Answer = "What is idempotency with exactly-once effects?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "You need to fan out one event to many consumers, each with its own filtering—what do you use?", Answer = "What are multiple event subscriptions with filters?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "Events deliver but processing is inconsistent across services—what missing capability is most likely?", Answer = "What is correlation/trace context (end-to-end observability)?" },
            new() { Category = "Event Grid", PointValue = 500, Question = "Your webhook endpoint requires proof requests are from Azure—what should you validate?", Answer = "What is the Event Grid signature/validation handshake (and auth)?" },

            // ==================== EVENT HUB ====================
            new() { Category = "Event Hub", PointValue = 100, Question = "This Azure service ingests high-throughput streaming event data like logs and telemetry.", Answer = "What is Event Hubs?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "In Event Hubs, this is the unit that divides the stream for parallelism.", Answer = "What is a partition?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This Event Hubs concept represents a set of readers with independent checkpoints.", Answer = "What is a consumer group?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This describes reading events in order within a partition.", Answer = "What is ordered streaming per partition?" },
            new() { Category = "Event Hub", PointValue = 100, Question = "This common use case sends application logs and metrics into Event Hubs.", Answer = "What is telemetry ingestion?" },

            new() { Category = "Event Hub", PointValue = 200, Question = "This is how Event Hubs typically scales processing: multiple consumers reading different partitions.", Answer = "What is partition-based parallelism?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This value in event processing indicates “how far you’ve read” so you can resume after restarts.", Answer = "What is a checkpoint?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This Event Hubs feature writes incoming events directly to storage for batch analytics.", Answer = "What is Event Hubs Capture?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This describes Event Hubs best: it’s optimized for streaming ingestion, not point-to-point commands.", Answer = "What is an event stream (log)?" },
            new() { Category = "Event Hub", PointValue = 200, Question = "This is the general delivery reality for streaming systems: consumers must handle repeats safely.", Answer = "What is at-least-once processing?" },

            new() { Category = "Event Hub", PointValue = 300, Question = "You want all events for a specific user to stay in order—this key determines the partition.", Answer = "What is a partition key?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is the usual reason you can’t increase parallel consumption beyond a certain point.", Answer = "What is the number of partitions limiting parallelism?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is a common choice for real-time processing of Event Hubs streams.", Answer = "What is Azure Stream Analytics (or Functions)?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This is the primary reason you use consumer groups instead of creating multiple hubs.", Answer = "What is independent reads/checkpoints for different apps?" },
            new() { Category = "Event Hub", PointValue = 300, Question = "This statement is true about ordering: Event Hubs preserves order where?", Answer = "What is within a partition (not across partitions)?" },

            new() { Category = "Event Hub", PointValue = 400, Question = "Your processor restarts and re-reads old events—what’s the most likely missing piece?", Answer = "What is checkpointing (persisting offsets)?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "You need to reprocess a time window of events—what do you change?", Answer = "What is the starting position/offset (or timestamp)?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "A hot partition is causing lag—most likely cause?", Answer = "What is a skewed partition key creating uneven load?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "To secure access without sharing keys, Event Hubs commonly uses this identity approach.", Answer = "What is managed identity with RBAC (Entra ID)?" },
            new() { Category = "Event Hub", PointValue = 400, Question = "For compliance and cost control, this setting determines how long events are stored.", Answer = "What is retention period?" },

            new() { Category = "Event Hub", PointValue = 500, Question = "You must guarantee correctness when events replay—what design principle is non-negotiable?", Answer = "What is idempotent processing with deduplication keys?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Your analytics needs cheap long-term storage plus occasional replay—best built-in feature?", Answer = "What is Event Hubs Capture to Blob/Data Lake?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Events are delayed and backlog grows—first scaling lever to investigate?", Answer = "What is increasing partitions/throughput and scaling consumers?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Two apps need to read the same stream independently—what must you create?", Answer = "What is a separate consumer group (per app)?" },
            new() { Category = "Event Hub", PointValue = 500, Question = "Your processor is fast, but end-to-end is slow—most likely bottleneck after the hub?", Answer = "What is downstream dependency latency (storage/DB) requiring batching and backpressure control?" },
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
            new() { Category = "Azure Resource Manager", PointValue = 100, Question = "This is the logical container that holds related Azure resources for a solution.", Answer = "What is a Resource Group?" },
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
            new() { Category = "Ansible", PointValue = 100, Question = "Ansible is designed so re-running a playbook achieves the same end state; this property is called what?", Answer = "What is idempotency?" },
            new() { Category = "Ansible", PointValue = 100, Question = "This is the smallest unit of work in Ansible, like installing a package or copying a file.", Answer = "What is a task?" },

            new() { Category = "Ansible", PointValue = 200, Question = "This is the Ansible command-line tool that runs playbooks.", Answer = "What is ansible-playbook?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This mechanism organizes reusable automation into a standard structure.", Answer = "What is a role?" },
            new() { Category = "Ansible", PointValue = 200, Question = "These run only when notified, often used to restart a service after a config change.", Answer = "What are handlers?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This keyword is used to elevate privileges on the target host (sudo).", Answer = "What is become?" },
            new() { Category = "Ansible", PointValue = 200, Question = "This holds data you can reuse across tasks, like ports and environment names.", Answer = "What are variables?" },

            new() { Category = "Ansible", PointValue = 300, Question = "This feature gathers system information like OS, IP addresses, and disks.", Answer = "What are facts (setup)?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This allows selecting only certain tasks to run within a playbook.", Answer = "What are tags?" },
            new() { Category = "Ansible", PointValue = 300, Question = "This mode shows what would change without applying changes.", Answer = "What is check mode (--check)?" },
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
            new() { Category = "Container Registry", PointValue = 400, Question = "A common operational practice is to automatically delete old tags to control storage.", Answer = "What is retention policy / cleanup automation?" },
            new() { Category = "Container Registry", PointValue = 400, Question = "To deploy safely, production should pull only images that passed this stage.", Answer = "What is CI/CD promotion (build → scan → approve → deploy)?" },

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
            new() { Category = "App Configuration", PointValue = 200, Question = "This is the standard .NET concept used to bind configuration into strongly typed objects.", Answer = "What is IOptions<T> (options pattern)?" },
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
            new() { Category = "Container Apps", PointValue = 100, Question = "This Azure service runs containers without you managing Kubernetes directly.", Answer = "What is Azure Container Apps?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "In Container Apps, this is a deployed version of your app’s container configuration.", Answer = "What is a revision?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This describes automatically adding/removing instances based on load or events.", Answer = "What is autoscaling?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This term describes how Container Apps can route traffic between revisions.", Answer = "What is traffic splitting?" },
            new() { Category = "Container Apps", PointValue = 100, Question = "This is the typical source of container images deployed to Container Apps.", Answer = "What is a container registry (like ACR)?" },

            new() { Category = "Container Apps", PointValue = 200, Question = "This setting controls how many replicas your Container App should maintain at minimum.", Answer = "What is min replicas?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This setting controls the upper limit of replicas when scaling out.", Answer = "What is max replicas?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This concept means your container should not rely on local disk for important state.", Answer = "What is stateless design?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the safest way for a Container App to access Azure resources without secrets.", Answer = "What is managed identity?" },
            new() { Category = "Container Apps", PointValue = 200, Question = "This is the first place you check when your Container App won’t start.", Answer = "What are container logs (and revision status)?" },

            new() { Category = "Container Apps", PointValue = 300, Question = "This feature lets you do blue/green-like releases by shifting traffic gradually.", Answer = "What is revision traffic splitting?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is the common cause when the container runs locally but fails in Container Apps.", Answer = "What is binding to localhost instead of 0.0.0.0 (or wrong port)?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is how you typically keep secrets out of images and configs for Container Apps.", Answer = "What is Key Vault (plus managed identity)?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This principle is essential because scale-to-zero means instances can restart anytime.", Answer = "What is durable external state (DB/Storage) and idempotent startup?" },
            new() { Category = "Container Apps", PointValue = 300, Question = "This is the common event-driven pattern for spiky workloads with containers.", Answer = "What is queue-driven processing with autoscale?" },

            new() { Category = "Container Apps", PointValue = 400, Question = "You deployed a new revision but users still hit the old version—what did you forget?", Answer = "What is updating traffic routing to the new revision?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "Your app scales out but collapses under load; you likely violated this design rule.", Answer = "What is statelessness (or dependency bottlenecks)?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "A 502/503 appears intermittently; first suspect in containers is often this.", Answer = "What is readiness/health and downstream dependency failures?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "You must access a private resource in a VNet; the solution generally involves this.", Answer = "What is private networking/VNet integration patterns (private endpoints)?" },
            new() { Category = "Container Apps", PointValue = 400, Question = "Your container pulls fail from ACR; the most common missing permission is this role.", Answer = "What is AcrPull?" },

            new() { Category = "Container Apps", PointValue = 500, Question = "You need near-zero downtime upgrades—what’s the best Container Apps deployment technique?", Answer = "What is gradual traffic shift between revisions (canary/blue-green)?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your background worker must handle duplicate messages safely at scale—required design principle?", Answer = "What is idempotent processing?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "To prevent supply chain issues, production should deploy only images that passed this.", Answer = "What is CI scanning/signing and promotion gates?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your cost is spiking because of constant scale-out; the best first tuning lever is this.", Answer = "What is adjust scaling rules/thresholds and reduce noisy triggers?" },
            new() { Category = "Container Apps", PointValue = 500, Question = "Your container needs secrets and you must avoid long-lived credentials—best end-to-end pattern?", Answer = "What is managed identity + Key Vault + least privilege RBAC?" },

            // ==================== API MANAGEMENT ====================
            new() { Category = "API Management", PointValue = 100, Question = "This Azure service acts as a gateway to publish, secure, and manage APIs.", Answer = "What is Azure API Management (APIM)?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM concept represents the backend service your gateway forwards requests to.", Answer = "What is a backend?" },
            new() { Category = "API Management", PointValue = 100, Question = "This APIM feature provides interactive API docs based on OpenAPI.", Answer = "What is the developer portal (Swagger/OpenAPI docs)?" },
            new() { Category = "API Management", PointValue = 100, Question = "This is the APIM capability that enforces rules like rate limiting, JWT validation, or header rewrites.", Answer = "What are policies?" },
            new() { Category = "API Management", PointValue = 100, Question = "This describes putting a single entry point in front of multiple APIs.", Answer = "What is an API gateway?" },

            new() { Category = "API Management", PointValue = 200, Question = "This APIM control limits how many requests a client can make in a time window.", Answer = "What is rate limiting (throttling)?" },
            new() { Category = "API Management", PointValue = 200, Question = "This APIM concept groups APIs together for publishing and access control.", Answer = "What is a product?" },
            new() { Category = "API Management", PointValue = 200, Question = "This is commonly required for clients to call APIM-protected products.", Answer = "What is a subscription key?" },
            new() { Category = "API Management", PointValue = 200, Question = "This is the common auth method for securing APIs with tokens and claims.", Answer = "What is JWT (token) validation?" },
            new() { Category = "API Management", PointValue = 200, Question = "This helps reduce backend load by storing repeated responses temporarily.", Answer = "What is caching?" },

            new() { Category = "API Management", PointValue = 300, Question = "This APIM feature sends your API call logs/metrics to monitoring tools.", Answer = "What is diagnostics (logging) integration?" },
            new() { Category = "API Management", PointValue = 300, Question = "This policy can modify requests/responses, such as adding headers or transforming JSON.", Answer = "What is transformation with policies?" },
            new() { Category = "API Management", PointValue = 300, Question = "This is how you version APIs cleanly without breaking old clients.", Answer = "What is API versioning?" },
            new() { Category = "API Management", PointValue = 300, Question = "This is the principle for limiting who can call sensitive endpoints via APIM.", Answer = "What is least privilege access control?" },
            new() { Category = "API Management", PointValue = 300, Question = "When an API slows down under load, APIM’s first-line protection is usually this.", Answer = "What is throttling (rate limits/quotas)?" },

            new() { Category = "API Management", PointValue = 400, Question = "Clients get 401/403 through APIM but backend works directly—most likely cause?", Answer = "What is APIM auth/policy requirements (subscription key/JWT) not satisfied?" },
            new() { Category = "API Management", PointValue = 400, Question = "You need to protect backend services from being called directly; common approach?", Answer = "What is network restrictions (private endpoints/VNet) plus APIM as the only ingress?" },
            new() { Category = "API Management", PointValue = 400, Question = "Your API contract keeps drifting from reality; the best fix is to standardize on this.", Answer = "What is OpenAPI-first (contract-driven) development?" },
            new() { Category = "API Management", PointValue = 400, Question = "You want consistent security headers (CORS, HSTS, etc.) across many APIs; best place?", Answer = "What are APIM global/product policies?" },
            new() { Category = "API Management", PointValue = 400, Question = "A backend is flaky; APIM can help stabilize callers with this resiliency concept.", Answer = "What is retry/circuit-breaker style policy patterns (where appropriate)?" },

            new() { Category = "API Management", PointValue = 500, Question = "To roll out breaking changes safely, the best strategy is to run this in parallel.", Answer = "What is versioned APIs with staged migration (v1 + v2)?" },
            new() { Category = "API Management", PointValue = 500, Question = "You must audit who called what endpoint and when; APIM must enable this.", Answer = "What is diagnostic logging with correlation IDs?" },
            new() { Category = "API Management", PointValue = 500, Question = "Traffic spikes threaten backend stability; best combined APIM controls?", Answer = "What are rate limits/quotas plus caching (and possibly queueing)?" },
            new() { Category = "API Management", PointValue = 500, Question = "Your security posture demands zero trust—what should be enforced at the gateway?", Answer = "What is strong authentication/authorization (JWT/OAuth), TLS, and least privilege policies?" },
            new() { Category = "API Management", PointValue = 500, Question = "To prevent data leaks, the gateway should enforce this principle for outbound responses.", Answer = "What is response filtering/minimization (only necessary data) with policy controls?" },
            // ==================== LOGIC APPS ====================
            new() { Category = "Logic Apps", PointValue = 100, Question = "This Azure service builds workflows using a visual designer and connectors.", Answer = "What is Azure Logic Apps?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "In Logic Apps, this starts a workflow when an event occurs or on a schedule.", Answer = "What is a trigger?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "In Logic Apps, these are the steps that run after the trigger.", Answer = "What are actions?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "Logic Apps integrates with services using these prebuilt integrations.", Answer = "What are connectors?" },
            new() { Category = "Logic Apps", PointValue = 100, Question = "This describes automating work across services without writing much code.", Answer = "What is workflow automation?" },

            new() { Category = "Logic Apps", PointValue = 200, Question = "This Logic Apps feature branches execution based on true/false logic.", Answer = "What is a condition?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This Logic Apps feature repeats actions over an array of items.", Answer = "What is a For each loop?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This built-in reliability concept lets Logic Apps retry transient failures automatically.", Answer = "What is retry policy?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This is a common best practice for secrets when a workflow needs credentials.", Answer = "What is Azure Key Vault?" },
            new() { Category = "Logic Apps", PointValue = 200, Question = "This is the most common use case for Logic Apps in enterprise systems.", Answer = "What is system integration (iPaaS-style)?" },

            new() { Category = "Logic Apps", PointValue = 300, Question = "This Logic Apps feature lets you run different steps depending on whether a prior step succeeded or failed.", Answer = "What is run-after configuration?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This pattern is used to handle failures without losing messages, often by routing to another action/path.", Answer = "What is dead-letter or failure handling path (compensation)?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the common way to decouple workflows from spikes in load.", Answer = "What is using a queue (Service Bus/Queue Storage)?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the best practice to make workflow steps safe when retries happen.", Answer = "What is idempotency?" },
            new() { Category = "Logic Apps", PointValue = 300, Question = "This is the main reason workflows break when moved from dev to prod.", Answer = "What are environment-specific connections/settings (auth endpoints, IDs, secrets)?" },

            new() { Category = "Logic Apps", PointValue = 400, Question = "Your workflow runs twice for the same event—most likely explanation?", Answer = "What is at-least-once delivery and retries causing duplicates?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "A connector call returns 401/403—first thing to verify?", Answer = "What is the connection’s authentication and permissions?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "You need to trigger a workflow from an HTTP call and validate the caller identity—what do you add?", Answer = "What is authentication/authorization (Entra ID/JWT/API key) on the HTTP trigger or via APIM?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "Workflow timeouts happen under heavy load; the best redesign is often to do this.", Answer = "What is move long work async using queues and smaller actions?" },
            new() { Category = "Logic Apps", PointValue = 400, Question = "To troubleshoot workflow failures, the most useful built-in view is this.", Answer = "What is run history (workflow run details)?" },

            new() { Category = "Logic Apps", PointValue = 500, Question = "You must guarantee a business action happens exactly once despite retries—what design approach?", Answer = "What is idempotent actions with deduplication keys/state storage?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "A workflow integrates many systems; the biggest reliability risk is usually what?", Answer = "What is dependency failures and lack of compensation/error handling?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "You need strong governance for who can change production workflows—best control?", Answer = "What is RBAC + approvals + IaC deployments?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "For sensitive data, you must reduce accidental exposure in monitoring—what principle?", Answer = "What is minimize/secure logging and protect secrets/PII?" },
            new() { Category = "Logic Apps", PointValue = 500, Question = "Your workflow needs to run inside a private network boundary—what capability matters?", Answer = "What is VNet integration/private endpoints (private connectivity)?" },

            // ==================== DATA FACTORY ====================
            new() { Category = "Data Factory", PointValue = 100, Question = "This Azure service builds ETL/ELT pipelines to move and transform data.", Answer = "What is Azure Data Factory (ADF)?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "In ADF, this is a workflow made of activities that move/transform data.", Answer = "What is a pipeline?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "In ADF, this defines a connection to a data source or destination.", Answer = "What is a linked service?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "In ADF, this represents the structure/shape of data being used.", Answer = "What is a dataset?" },
            new() { Category = "Data Factory", PointValue = 100, Question = "This ADF activity copies data from a source to a sink.", Answer = "What is Copy activity?" },

            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF feature runs a pipeline on a schedule or when an event occurs.", Answer = "What is a trigger?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF component executes activities and manages compute for pipeline runs.", Answer = "What is an integration runtime?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This ADF compute option runs inside your network to reach on-prem data sources.", Answer = "What is self-hosted integration runtime?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This describes moving raw data first, then transforming it in the destination system.", Answer = "What is ELT?" },
            new() { Category = "Data Factory", PointValue = 200, Question = "This describes transforming data during movement through the pipeline.", Answer = "What is ETL?" },

            new() { Category = "Data Factory", PointValue = 300, Question = "This is the best practice to keep pipelines flexible across dev/test/prod.", Answer = "What are parameters (and environment-specific linked services)?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This ADF concept groups repeated steps into a reusable component.", Answer = "What is a pipeline (or reusable activity pattern) with parameters?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "When a pipeline fails intermittently due to transient issues, you should rely on this.", Answer = "What is retry policy?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This is the first thing to check when copied row counts don’t match expectations.", Answer = "What are source filters/queries and sink mappings?" },
            new() { Category = "Data Factory", PointValue = 300, Question = "This monitoring area shows pipeline runs, activity runs, and failure details.", Answer = "What is the ADF Monitor view?" },

            new() { Category = "Data Factory", PointValue = 400, Question = "Your pipeline can’t reach an on-prem database; most likely missing piece?", Answer = "What is a self-hosted integration runtime and network connectivity?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "A big cost spike happened after a change; first suspect in ADF?", Answer = "What is increased activity runs/data movement/compute usage (integration runtime)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Your data load is slow; the best first performance lever is usually to adjust this.", Answer = "What is parallelism (DIUs/partitions) and batch sizing?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "You need to ensure data quality after ingestion; best practice is to add this step.", Answer = "What is validation checks (row counts, checksums, critical query tests)?" },
            new() { Category = "Data Factory", PointValue = 400, Question = "Security best practice: credentials in linked services should be sourced from where?", Answer = "What is Azure Key Vault?" },

            new() { Category = "Data Factory", PointValue = 500, Question = "You must load incrementally rather than full refresh; the key pattern is what?", Answer = "What is watermarking (incremental loads based on a high-water mark)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "A pipeline re-run must not duplicate data; what property must the load have?", Answer = "What is idempotency (dedupe/upsert strategy)?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "After a migration, reports are wrong even though loads succeed; the most likely cause?", Answer = "What is transformation logic/mapping differences or data type/collation issues?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "You need enterprise governance: promote pipelines through environments with approvals—best approach?", Answer = "What is CI/CD for ADF with source control and release gates?" },
            new() { Category = "Data Factory", PointValue = 500, Question = "Best way to investigate a pipeline that fails only sometimes?", Answer = "What is inspect activity run details, dependency errors, and add retries/alerts?" },

            // ==================== SYNAPSE ANALYTICS ====================
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Azure service combines data warehousing and big data analytics in one workspace.", Answer = "What is Azure Synapse Analytics?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "In Synapse, this is a logical place where analytics tools and resources are organized.", Answer = "What is a Synapse workspace?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This describes large-scale query and reporting optimized for analytics rather than transactions.", Answer = "What is a data warehouse?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse compute option is commonly used for big data processing with Spark.", Answer = "What is a Spark pool?" },
            new() { Category = "Synapse Analytics", PointValue = 100, Question = "This Synapse component provides SQL querying capabilities over data.", Answer = "What is a SQL pool (dedicated or serverless)?" },

            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This Synapse option queries data in storage without provisioning dedicated data warehouse compute.", Answer = "What is serverless SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This Synapse option provides provisioned, scalable data warehouse compute.", Answer = "What is dedicated SQL pool?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "A common storage foundation for Synapse analytics is this Azure service.", Answer = "What is Azure Data Lake Storage (ADLS)?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This describes consolidating data from multiple sources for analytics.", Answer = "What is data integration?" },
            new() { Category = "Synapse Analytics", PointValue = 200, Question = "This describes a schema design optimized for analytics with facts and dimensions.", Answer = "What is a star schema?" },

            new() { Category = "Synapse Analytics", PointValue = 300, Question = "This is the main difference: OLTP is transactions; Synapse is commonly used for this.", Answer = "What is OLAP (analytics)?" },
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
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "You must support audits on who queried sensitive data—what must be enabled?", Answer = "What is auditing/diagnostic logs to Log Analytics/SIEM?" },
            new() { Category = "Synapse Analytics", PointValue = 500, Question = "A query is correct but too slow at scale; what’s the most common high-impact fix?", Answer = "What is partitioning, columnar formats, and reducing scanned data (filter early)?" },
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
            new() { Category = "Data Analytics", PointValue = 500, Question = "A model performs great in test but fails in production; the usual culprit is this.", Answer = "What is data drift or concept drift?" },
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
            new() { Category = "Cosmos DB", PointValue = 400, Question = "A container design that forces full scans is usually caused by this.", Answer = "What is missing filters/indexing or poorly chosen partition key and query patterns?" },

            new() { Category = "Cosmos DB", PointValue = 500, Question = "Your RU cost exploded after a feature release—most likely root cause?", Answer = "What is more queries per request or inefficient cross-partition queries/indexing changes?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You must guarantee ‘exactly-once effect’ when reacting to Change Feed—key design rule?", Answer = "What is idempotent processing with deduplication/state tracking?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "A partition key mistake is the hardest to fix later because it impacts this.", Answer = "What is physical data distribution and scalability?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "You need consistent global data with minimal anomalies; the tradeoff you’re balancing is this.", Answer = "What is consistency vs latency/availability?" },
            new() { Category = "Cosmos DB", PointValue = 500, Question = "For high-scale Cosmos DB apps, the #1 reliability guardrail is watching and alerting on this.", Answer = "What is throttling (429s) and RU saturation metrics?" },
            // ==================== CONTAINERS & KUBERNETES ====================
            // 100
            new() { Category = "Containers & Kubernetes", PointValue = 100, Question = "A lightweight package that includes an app and its dependencies while sharing the host OS kernel.", Answer = "What is a container?" },
            new() { Category = "Containers & Kubernetes", PointValue = 100, Question = "A read-only template used to create containers.", Answer = "What is a container image?" },
            new() { Category = "Containers & Kubernetes", PointValue = 100, Question = "The file that defines the steps to build a Docker image.", Answer = "What is a Dockerfile?" },
            new() { Category = "Containers & Kubernetes", PointValue = 100, Question = "The smallest deployable unit in Kubernetes that can contain one or more containers.", Answer = "What is a Pod?" },
            new() { Category = "Containers & Kubernetes", PointValue = 100, Question = "The command-line tool used to interact with a Kubernetes cluster.", Answer = "What is kubectl?" },

            // 200
            new() { Category = "Containers & Kubernetes", PointValue = 200, Question = "A Kubernetes object that ensures a set of Pods stays running and supports rolling updates.", Answer = "What is a Deployment?" },
            new() { Category = "Containers & Kubernetes", PointValue = 200, Question = "A stable endpoint that exposes Pods using ClusterIP, NodePort, or LoadBalancer.", Answer = "What is a Service?" },
            new() { Category = "Containers & Kubernetes", PointValue = 200, Question = "A logical way to group Kubernetes resources to separate environments like dev and prod.", Answer = "What is a Namespace?" },
            new() { Category = "Containers & Kubernetes", PointValue = 200, Question = "A repository for storing and versioning container images.", Answer = "What is a container registry?" },
            new() { Category = "Containers & Kubernetes", PointValue = 200, Question = "The common file format used to define Kubernetes resources like Deployments and Services.", Answer = "What is YAML?" },

            // 300
            new() { Category = "Containers & Kubernetes", PointValue = 300, Question = "A Kubernetes object that stores non-sensitive configuration as key/value pairs.", Answer = "What is a ConfigMap?" },
            new() { Category = "Containers & Kubernetes", PointValue = 300, Question = "A Kubernetes object designed to store sensitive data like passwords or tokens.", Answer = "What is a Secret?" },
            new() { Category = "Containers & Kubernetes", PointValue = 300, Question = "A Kubernetes resource that routes external HTTP/HTTPS traffic to Services.", Answer = "What is an Ingress?" },
            new() { Category = "Containers & Kubernetes", PointValue = 300, Question = "A request for storage that a Pod can mount, abstracting the underlying storage.", Answer = "What is a PersistentVolumeClaim (PVC)?" },
            new() { Category = "Containers & Kubernetes", PointValue = 300, Question = "Kubernetes’ popular package manager that installs apps using Charts.", Answer = "What is Helm?" },

            // 400
            new() { Category = "Containers & Kubernetes", PointValue = 400, Question = "A health check that restarts a container if the app is stuck or dead.", Answer = "What is a liveness probe?" },
            new() { Category = "Containers & Kubernetes", PointValue = 400, Question = "A health check that controls whether a Pod should receive traffic.", Answer = "What is a readiness probe?" },
            new() { Category = "Containers & Kubernetes", PointValue = 400, Question = "Kubernetes scaling that automatically adjusts the number of Pods based on metrics.", Answer = "What is a Horizontal Pod Autoscaler (HPA)?" },
            new() { Category = "Containers & Kubernetes", PointValue = 400, Question = "The update method that replaces Pods gradually to avoid downtime.", Answer = "What is a rolling update?" },
            new() { Category = "Containers & Kubernetes", PointValue = 400, Question = "A group of worker machines in Kubernetes that run your Pods.", Answer = "What are nodes?" },

            // 500
            new() { Category = "Containers & Kubernetes", PointValue = 500, Question = "Kubernetes’ key-value store that holds cluster state and configuration.", Answer = "What is etcd?" },
            new() { Category = "Containers & Kubernetes", PointValue = 500, Question = "A Kubernetes pattern/controller used to manage complex apps as custom resources.", Answer = "What is an Operator?" },
            new() { Category = "Containers & Kubernetes", PointValue = 500, Question = "Kubernetes rules that control which Pods can communicate with which other Pods.", Answer = "What is a NetworkPolicy?" },
            new() { Category = "Containers & Kubernetes", PointValue = 500, Question = "A layer that provides service-to-service security, observability, and traffic control (often via sidecars).", Answer = "What is a service mesh?" },
            new() { Category = "Containers & Kubernetes", PointValue = 500, Question = "A Kubernetes concept where workloads are spread across failure domains to improve availability.", Answer = "What is fault tolerance via multi-zone or multi-node distribution?" },

        ];
    }
}
