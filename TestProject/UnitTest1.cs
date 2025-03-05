using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using BusinessObject;
using Microsoft.AspNetCore.Http;
using Moq;
using Repository;
using Service;
using Service.Impl;
using Xunit.Abstractions;
using Xunit.Sdk;
using Task = System.Threading.Tasks.Task;

namespace TestProject;

public class UnitTest1
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IUserService _userService;
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _userService = new UserService(_mapperMock.Object, _unitOfWorkMock.Object);
        _output = output;
    }

    [Fact]
    public async Task ReadUsersFromExcelFile_ValidExcelFile_MatchesJsonData()
    {
        // Arrange
        string projectPath = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        string excelFilePath = Path.Combine(projectPath, "TestData", "user_info_capstone.xlsx");
        string jsonFilePath = Path.Combine(projectPath, "TestData", "user_info_capstone.json");

        Assert.True(File.Exists(excelFilePath), $"Excel file not found: {excelFilePath}");
        Assert.True(File.Exists(jsonFilePath), $"JSON file not found: {jsonFilePath}");

        var jsonContent = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
        var usersFromJson = JsonSerializer.Deserialize<List<User>>(jsonContent, options);

        // Act
        var usersFromExcel = await _userService.ReadUsersFromExcelFile(excelFilePath);

        // Assert
        Assert.NotNull(usersFromExcel);
        _output.WriteLine($"JSON Count: {usersFromJson.Count}");
        _output.WriteLine($"Excel Count: {usersFromExcel.Count}");
        Assert.Equal(usersFromJson.Count, usersFromExcel.Count);

        for (int i = 0; i < usersFromJson.Count; i++)
        {
            try
            {
                _output.WriteLine($"JSON User[{i}]: {JsonSerializer.Serialize(usersFromJson[i])}");
                _output.WriteLine($"Excel User[{i}]: {JsonSerializer.Serialize(usersFromExcel[i])}");
                Assert.Equal(usersFromJson[i].UserName, usersFromExcel[i].UserName);
                Assert.Equal(usersFromJson[i].Email, usersFromExcel[i].Email);
                Assert.Equal(usersFromJson[i].FullName, usersFromExcel[i].FullName);
                Assert.Equal(usersFromJson[i].PhoneNumber, usersFromExcel[i].PhoneNumber);
                Assert.Equal(usersFromJson[i].Address, usersFromExcel[i].Address);
                Assert.Equal(usersFromJson[i].Gender, usersFromExcel[i].Gender);
                Assert.Equal(usersFromJson[i].Position, usersFromExcel[i].Position);
                Assert.False(usersFromExcel[i].IsDeleted);
                Assert.False(usersFromExcel[i].IsEnable);
            }
            catch (EqualException ex)
            {
                _output.WriteLine($"Assertion failed at index {i}: {ex.Message}");
                throw;
            }
        }
    }
}