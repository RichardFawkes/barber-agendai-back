using System.Text.RegularExpressions;

namespace BarbeariaSaaS.Application.Validators;

public static class TenantValidationService
{
    private static readonly List<string> ReservedSubdomains = new()
    {
        "api", "www", "admin", "app", "mail", "ftp", "support", "help", "blog", "news",
        "cdn", "static", "assets", "files", "docs", "dev", "test", "staging", "prod"
    };

    public static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        // Verificar se não é reservado
        if (ReservedSubdomains.Contains(subdomain.ToLower()))
            return false;

        // Verificar formato: alfanumérico + hífen, não pode começar ou terminar com hífen
        var pattern = @"^[a-z0-9][a-z0-9-]*[a-z0-9]$";
        if (subdomain.Length < 3)
            pattern = @"^[a-z0-9]+$"; // Para subdomains de 3 caracteres, não pode ter hífen

        return Regex.IsMatch(subdomain, pattern) && 
               subdomain.Length >= 3 && 
               subdomain.Length <= 50 &&
               !subdomain.Contains("--"); // Não pode ter hífen duplo
    }

    public static bool IsValidBrazilianPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Formatos aceitos:
        // (11) 99999-9999 (celular)
        // (11) 9999-9999 (fixo)
        // 11999999999 (só números - celular)
        // 1199999999 (só números - fixo)

        // Remover espaços e caracteres especiais para validação alternativa
        var numbersOnly = Regex.Replace(phone, @"[^\d]", "");

        // Validar formato com máscara
        var maskedPattern = @"^\(\d{2}\)\s\d{4,5}-\d{4}$";
        
        // Validar formato só números
        var numbersPattern = @"^\d{10,11}$";

        if (Regex.IsMatch(phone, maskedPattern))
        {
            // Verificar se o DDD é válido (11-99)
            var ddd = int.Parse(phone.Substring(1, 2));
            return ddd >= 11 && ddd <= 99;
        }

        if (Regex.IsMatch(numbersOnly, numbersPattern))
        {
            // Verificar se o DDD é válido (11-99)
            var ddd = int.Parse(numbersOnly.Substring(0, 2));
            return ddd >= 11 && ddd <= 99;
        }

        return false;
    }

    public static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        var pattern = @"^#[0-9A-Fa-f]{6}$";
        return Regex.IsMatch(color, pattern);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true; // URL é opcional

        // Normalizar URL adicionando https:// se não tiver protocolo
        var normalizedUrl = url.Trim();
        if (!normalizedUrl.StartsWith("http://") && !normalizedUrl.StartsWith("https://") && !normalizedUrl.StartsWith("ftp://"))
        {
            normalizedUrl = "https://" + normalizedUrl;
        }

        return Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeFtp);
    }

    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        var normalizedUrl = url.Trim();
        if (!normalizedUrl.StartsWith("http://") && !normalizedUrl.StartsWith("https://") && !normalizedUrl.StartsWith("ftp://"))
        {
            normalizedUrl = "https://" + normalizedUrl;
        }

        return normalizedUrl;
    }

    public static List<string> GetValidationErrors(string fieldName, string value, string fieldType)
    {
        var errors = new List<string>();

        switch (fieldType.ToLower())
        {
            case "subdomain":
                if (!IsValidSubdomain(value))
                {
                    if (!string.IsNullOrEmpty(value) && ReservedSubdomains.Contains(value.ToLower()))
                        errors.Add("Subdomínio não é permitido");
                    else if (value?.Length < 3)
                        errors.Add("Subdomínio deve ter pelo menos 3 caracteres");
                    else if (value?.Length > 50)
                        errors.Add("Subdomínio deve ter no máximo 50 caracteres");
                    else
                        errors.Add("Subdomínio deve conter apenas letras minúsculas, números e hífen");
                }
                break;

            case "email":
                if (!IsValidEmail(value))
                    errors.Add("Formato de email inválido");
                break;

            case "hexcolor":
                if (!IsValidHexColor(value))
                    errors.Add("Deve ser uma cor hexadecimal válida (ex: #ffffff)");
                break;

            case "url":
                if (!IsValidUrl(value))
                    errors.Add("URL inválida");
                break;
        }

        return errors;
    }
} 