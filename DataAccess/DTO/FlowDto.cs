namespace DataAccess.DTO;

public class FlowDto
{
    public Guid? FlowId { get; set; }
    public string? RoleStart { get; set; }
    public string? RoleEnd { get; set; }
    public bool? IsFallbackFlow { get; set; }
    public List<StepDto>? Steps { get; set; }
}