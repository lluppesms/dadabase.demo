// namespace DadABase.Tests;
// //-----------------------------------------------------------------------
// // <copyright file="Category_API_Tests.cs" company="Luppes Consulting, Inc.">
// // Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// // </copyright>
// // <summary>
// // GENERATED - Category API Tests
// // </summary>
// //-----------------------------------------------------------------------

// [ExcludeFromCodeCoverage]
// public class Category_API_Tests : BaseTest
// {
// 	private readonly IJokeRepository repo;
// 	private readonly CategoryController apiController;

// 	public Category_API_Tests(ITestOutputHelper output)
// 	{
// 		Task.Run(() => SetupInitialize(output)).Wait();

// 		var mockContext = GetMockHttpContext(testData.UserName);
// 		repo = new MockJokeRepository();
// 		apiController = new CategoryController(appSettings, mockContext, repo);
// 	}

// 	[Fact]
// 	public void Api_Category_List_Works()
// 	{
// 		// Arrange

// 		// Act
// 		var categoryList = apiController.List();

// 		// Assert
// 		Assert.NotNull(categoryList);
// 		Assert.True(categoryList.Count > 0, "Found no categories!");
// 		output.WriteLine($"Found {categoryList.Count} Categories!");
// 		foreach (var category in categoryList)
// 		{
// 			output.WriteLine($"Category: {category}");
// 		}
// 	}
// }

// // Mock repository for testing
// public class MockJokeRepository : IJokeRepository
// {
// 	public IQueryable<Joke> ListAll(string activeInd = "Y", string requestingUserName = "ANON")
// 	{
// 		throw new NotImplementedException();
// 	}

// 	public Joke GetOne(int id, string requestingUserName = "ANON")
// 	{
// 		throw new NotImplementedException();
// 	}

// 	public Joke GetRandomJoke(string requestingUserName = "ANON")
// 	{
// 		throw new NotImplementedException();
// 	}

// 	public IQueryable<string> GetJokeCategories(string activeInd = "Y", string requestingUserName = "ANON")
// 	{
// 		return new List<string> { "Category1", "Category2", "Category3" }.AsQueryable();
// 	}

// 	public IQueryable<Joke> SearchJokes(string searchTxt, string jokeCategoryTxt, string requestingUserName = "ANON")
// 	{
// 		throw new NotImplementedException();
// 	}

// 	public void Dispose()
// 	{
// 		// Dispose resources if any
// 	}
// }