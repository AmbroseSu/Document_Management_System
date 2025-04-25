namespace DataAccess.DTO.Response;

public class FlowResponse
{
    public Guid? FlowId { get; set; }
    public string? RoleStart { get; set; }
    public string? RoleEnd { get; set; }
    public List<StepResponse>? StepResponses { get; set; }
}