using System.Net;
using System.Text;
using DataAccess.DTO;
using Newtonsoft.Json;

namespace Service.Utilities;

public class ResponseUtil
{
    // Phương thức trả về một đối tượng (response)
    public static ResponseDto GetObject(object result, string message, HttpStatusCode status, int size)
    {
        return new ResponseDto
        {
            Content = result,
            Message = message,
            Size = size,
            StatusCode = (int)status,
            MeatadataDto = null // for a single object, metadata is not needed
        };
    }

    // Phương thức trả về một tập hợp đối tượng (collection)
    public static ResponseDto GetCollection(object result, string message, HttpStatusCode status, int size, int page, int limit, long count)
    {
        return new ResponseDto
        {
            Content = result,
            Message = message,
            Size = size,
            StatusCode = (int)status,
            MeatadataDto = GetMeatadata(page, limit, count)
        };
    }

    // Phương thức trả về lỗi dạng chuỗi
    public static ResponseDto Error(string error, string message, HttpStatusCode status)
    {
        return new ResponseDto
        {
            Content = error,
            Message = message,
            Size = 0,
            StatusCode = (int)status,
            MeatadataDto = null
        };
    }

    // Phương thức trả về lỗi dạng danh sách
    public static ResponseDto ErrorList(List<string> errors, string message, HttpStatusCode status)
    {
        return new ResponseDto
        {
            Message = message,
            Size = 0,
            StatusCode = (int)status,
            MeatadataDto = null
        };
    }
    
    private static MeatadataDto GetMeatadata(int page, int limit, long count)
    {
        
        var totalPages = (int)Math.Ceiling((double)count / limit);

        return new MeatadataDto
        {
            page = page,
            total = totalPages,
            limit = limit,
            hasNextPage = page < totalPages,
            hasPrevPage = page > 1
        };
    }
}