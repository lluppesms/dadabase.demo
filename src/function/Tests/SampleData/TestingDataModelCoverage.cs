//-----------------------------------------------------------------------
// <copyright file="TestingDataModelCoverage.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Testing Data Data Modal Coverage
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data;

namespace DadABase.SampleData;

/// <summary>
/// Testing Data Manager
/// </summary>
public partial class TestingData
{
    /// <summary>
    /// Add Model Test Coverage
    /// </summary>
    private static void AddDataModelCodeCoverage()
    {
        _ = new JokeCategory() { JokeCategoryId = 1, JokeCategoryTxt = "Test" };
        _ = new Joke() { JokeId = 1, Categories = "Test", JokeTxt = "Test", ImageTxt = "Picture" };

        _ = new CategoryBasic();
        _ = new CategoryBasic().Category = "newCategory";
        _ = new CategoryBasic().Category;
        _ = new JokeBasic();
        _ = new JokeBasic("joke text", "Chickens");
        _ = new JokeBasicList();
        _ = new JokeBasicList().Jokes;
        _ = new JokeList();
        _ = new JokeList().Jokes;
        _ = new JokeList().Jokes = new List<Joke>();
        _ = new ValueMessage().Value;
        _ = new ValueMessage().Message;
        _ = new ValueMessage("test");
        _ = new ValueMessage("Error").Value;
        _ = new ValueMessage("TimeOut").Value;
        _ = new ValueMessage("test", 1);

        _ = new BuildInfo { BuildDate = "2026-01-01", BuildNumber = "1.0.0", BuildId = "1", BranchName = "main", CommitHash = "abc" };
        _ = new BackupData();

        _ = new JokeBasicPlus();
        _ = new JokeBasicPlus("joke text", "Chickens");
        _ = new JokeBasicPlus("joke text", "Chickens", "Author");
        _ = new JokeBasicPlus("joke text", "Chickens", string.Empty);
        var testJoke = new Joke { JokeTxt = "joke text", Categories = "Chickens", Attribution = "Author" };
        _ = new JokeBasicPlus(testJoke);
        testJoke.Attribution = string.Empty;
        _ = new JokeBasicPlus(testJoke);

        Constants.Initialize(new AppSettings { AppTitle = "Test", SuperUserFirstName = "Admin", SuperUserLastName = "User" });
    }
}
