namespace VMWorkflow.Application.DTOs;

public class DropdownOptionDto
{
    public Guid? DropdownOptionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
