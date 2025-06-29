using MediatR;
using Microsoft.Extensions.Logging;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Application.Validators;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Tenants.Commands;

public class CreateTenantWithAdminCommandHandler : IRequestHandler<CreateTenantWithAdminCommand, CreateTenantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<CreateTenantWithAdminCommandHandler> _logger;

    public CreateTenantWithAdminCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthenticationService authService,
        ILogger<CreateTenantWithAdminCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<CreateTenantResponseDto> Handle(CreateTenantWithAdminCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting tenant creation process for subdomain: {Subdomain}", 
                request.Request.Tenant.Subdomain);

            // 1. Validações adicionais
            var validationErrors = await ValidateRequestAsync(request.Request);
            if (validationErrors.Any())
            {
                return CreateErrorResponse("Dados inválidos", validationErrors);
            }

            // 2. Verificar unicidade
            var isSubdomainAvailable = await _unitOfWork.Tenants.IsSubdomainAvailableAsync(
                request.Request.Tenant.Subdomain.ToLower());
            
            if (!isSubdomainAvailable)
            {
                return CreateErrorResponse("Conflito", new Dictionary<string, List<string>>
                {
                    ["tenant.subdomain"] = new() { "Subdomínio já está em uso" }
                });
            }

            var isEmailAvailable = await _unitOfWork.Users.IsEmailAvailableAsync(request.Request.Admin.Email);
            if (!isEmailAvailable)
            {
                return CreateErrorResponse("Conflito", new Dictionary<string, List<string>>
                {
                    ["admin.email"] = new() { "Email já está cadastrado" }
                });
            }

            // 3. Criar tenant
            var tenant = CreateTenant(request.Request.Tenant);
            await _unitOfWork.Tenants.AddAsync(tenant);

            // 4. Criar usuário admin
            var admin = CreateAdminUser(request.Request.Admin, tenant.Id);
            await _unitOfWork.Users.AddAsync(admin);

            // 5. Criar configurações padrão
            CreateDefaultConfigurations(tenant);

            // 6. Salvar todas as mudanças
            await _unitOfWork.SaveChangesAsync();

            // 7. Gerar JWT token
            var token = await _authService.GenerateJwtTokenAsync(admin);

            // 8. Criar resposta de sucesso
            var response = CreateSuccessResponse(tenant, admin, token);

            _logger.LogInformation("Tenant created successfully: {TenantId} for subdomain: {Subdomain}", 
                tenant.Id, tenant.Subdomain);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant for subdomain: {Subdomain}", 
                request.Request.Tenant.Subdomain);
            
            return CreateErrorResponse("Erro interno do servidor", null);
        }
    }

    private Task<Dictionary<string, List<string>>> ValidateRequestAsync(CreateTenantWithAdminDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validar subdomain
        var subdomainErrors = TenantValidationService.GetValidationErrors("subdomain", request.Tenant.Subdomain, "subdomain");
        if (subdomainErrors.Any())
            errors["tenant.subdomain"] = subdomainErrors;

        // Validar emails
        var tenantEmailErrors = TenantValidationService.GetValidationErrors("email", request.Tenant.Email, "email");
        if (tenantEmailErrors.Any())
            errors["tenant.email"] = tenantEmailErrors;

        var adminEmailErrors = TenantValidationService.GetValidationErrors("email", request.Admin.Email, "email");
        if (adminEmailErrors.Any())
            errors["admin.email"] = adminEmailErrors;

        // Validar website (se fornecido)
        if (!string.IsNullOrEmpty(request.Tenant.Website))
        {
            var websiteErrors = TenantValidationService.GetValidationErrors("website", request.Tenant.Website, "url");
            if (websiteErrors.Any())
                errors["tenant.website"] = websiteErrors;
        }

        // Validar cores de branding (se fornecido)
        if (request.Tenant.Branding?.Colors != null)
        {
            var primaryColorErrors = TenantValidationService.GetValidationErrors("primary", request.Tenant.Branding.Colors.Primary, "hexcolor");
            if (primaryColorErrors.Any())
                errors["tenant.branding.colors.primary"] = primaryColorErrors;

            var accentColorErrors = TenantValidationService.GetValidationErrors("accent", request.Tenant.Branding.Colors.Accent, "hexcolor");
            if (accentColorErrors.Any())
                errors["tenant.branding.colors.accent"] = accentColorErrors;
        }

        return Task.FromResult(errors);
    }

    private Tenant CreateTenant(TenantDataDto tenantData)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = tenantData.Name,
            Description = tenantData.Description,
            Subdomain = tenantData.Subdomain.ToLower(),
            Phone = tenantData.Phone,
            Email = tenantData.Email,
            Address = tenantData.Address,
            Website = !string.IsNullOrEmpty(tenantData.Website) ? TenantValidationService.NormalizeUrl(tenantData.Website) : null,
            Status = TenantStatus.Active,
            Plan = SubscriptionPlan.Basic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Configurar branding
        if (tenantData.Branding != null)
        {
            tenant.Branding = new TenantBranding
            {
                Colors = new BrandingColors
                {
                    Primary = tenantData.Branding.Colors.Primary,
                    Accent = tenantData.Branding.Colors.Accent
                }
            };

            if (tenantData.Logo != null)
            {
                tenant.Branding.Logo = new BrandingLogo
                {
                    Url = tenantData.Logo.Url ?? string.Empty,
                    Alt = tenantData.Logo.FileName ?? string.Empty
                };
            }
        }
        else
        {
            tenant.Branding = new TenantBranding
            {
                Colors = new BrandingColors
                {
                    Primary = "#0f172a",
                    Accent = "#fbbf24"
                }
            };
        }

        return tenant;
    }

    private User CreateAdminUser(AdminDataDto adminData, Guid tenantId)
    {
        var passwordHash = _authService.HashPassword(adminData.Password);

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = adminData.Name,
            Email = adminData.Email,
            Phone = adminData.Phone,
            PasswordHash = passwordHash,
            Role = UserRole.TenantAdmin,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return admin;
    }

    private void CreateDefaultConfigurations(Tenant tenant)
    {
        // Criar horários de funcionamento padrão
        var defaultBusinessHours = CreateDefaultBusinessHours(tenant.Id);
        foreach (var businessHour in defaultBusinessHours)
        {
            tenant.BusinessHours.Add(businessHour);
        }

        // Criar serviços padrão
        var defaultServices = CreateDefaultServices(tenant.Id);
        foreach (var service in defaultServices)
        {
            tenant.Services.Add(service);
        }
    }

    private List<BusinessHour> CreateDefaultBusinessHours(Guid tenantId)
    {
        var businessHours = new List<BusinessHour>();

        // Segunda a Sexta: 08:00 - 18:00
        for (int day = 1; day <= 5; day++)
        {
            businessHours.Add(new BusinessHour
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DayOfWeek = day,
                IsOpen = true,
                OpenTime = TimeSpan.Parse("08:00"),
                CloseTime = TimeSpan.Parse("18:00")
            });
        }

        // Sábado: 08:00 - 16:00
        businessHours.Add(new BusinessHour
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DayOfWeek = 6,
            IsOpen = true,
            OpenTime = TimeSpan.Parse("08:00"),
            CloseTime = TimeSpan.Parse("16:00")
        });

        // Domingo: Fechado
        businessHours.Add(new BusinessHour
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DayOfWeek = 0,
            IsOpen = false,
            OpenTime = TimeSpan.Zero,
            CloseTime = TimeSpan.Zero
        });

        return businessHours;
    }

    private List<Service> CreateDefaultServices(Guid tenantId)
    {
        return new List<Service>
        {
            new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Corte Tradicional",
                Description = "Corte clássico masculino",
                Price = 25.00m,
                DurationMinutes = 30,
                IsActive = true,
                Color = "#3B82F6",
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Barba",
                Description = "Aparar e modelar barba",
                Price = 15.00m,
                DurationMinutes = 20,
                IsActive = true,
                Color = "#10B981",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private CreateTenantResponseDto CreateSuccessResponse(Tenant tenant, User admin, string token)
    {
        return new CreateTenantResponseDto
        {
            Success = true,
            Message = "Barbearia criada com sucesso!",
            Data = new CreateTenantDataDto
            {
                Tenant = new TenantResponseDto
                {
                    Id = tenant.Id.ToString(),
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    Email = tenant.Email ?? string.Empty,
                    Phone = tenant.Phone ?? string.Empty,
                    Address = tenant.Address ?? string.Empty,
                    Website = tenant.Website,
                    Branding = new TenantBrandingResponseDto
                    {
                        Colors = new BrandingColorsResponseDto
                        {
                            Primary = tenant.Branding?.Colors?.Primary ?? "#0f172a",
                            Accent = tenant.Branding?.Colors?.Accent ?? "#fbbf24"
                        },
                        Logo = tenant.Branding?.Logo != null ? new BrandingLogoResponseDto
                        {
                            Url = tenant.Branding.Logo.Url,
                            FileName = tenant.Branding.Logo.Alt
                        } : null
                    },
                    Status = tenant.Status.ToString().ToLower(),
                    CreatedAt = tenant.CreatedAt
                },
                Admin = new AdminResponseDto
                {
                    Id = admin.Id.ToString(),
                    Name = admin.Name,
                    Email = admin.Email,
                    Role = "tenant_admin"
                },
                Auth = new AuthResponseDto
                {
                    Token = token,
                    ExpiresIn = "24h"
                },
                Urls = new UrlsResponseDto
                {
                    Public = $"https://{tenant.Subdomain}.barbearia.app",
                    Admin = "https://app.barbearia.com/dashboard"
                }
            }
        };
    }

    private CreateTenantResponseDto CreateErrorResponse(string message, Dictionary<string, List<string>>? errors)
    {
        return new CreateTenantResponseDto
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
} 