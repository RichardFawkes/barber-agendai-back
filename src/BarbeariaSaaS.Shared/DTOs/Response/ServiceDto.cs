namespace BarbeariaSaaS.Shared.DTOs.Response;

public class ServiceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; } // em minutos
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public ServiceCategoryDto? Category { get; set; }
}

public class ServiceCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
} 