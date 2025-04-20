using Microsoft.AspNetCore.Http;

namespace DataAccess.DTO.Request;

public class TestDto
{
    public string Name {get; set;}
    public IFormFile Ima {get; set;}
}