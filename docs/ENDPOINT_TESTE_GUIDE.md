# 🧪 Guia de Teste: Endpoint de Criação de Tenant

## ✅ Problema da URL Resolvido!

O erro que você estava recebendo foi **CORRIGIDO**:

```json
{
  "errors": {
    "Tenant.Website": [
      "The Website field is not a valid fully-qualified http, https, or ftp URL."
    ]
  }
}
```

## 🔧 Correções Implementadas

1. ✅ **Removida validação `[Url]` restritiva** do DTO
2. ✅ **Validação customizada inteligente** 
3. ✅ **Normalização automática de URLs**
4. ✅ **Campo Website adicionado** à entidade Tenant

## 📝 Teste com URL sem protocolo (AGORA FUNCIONA!)

```bash
curl -X POST http://localhost:5000/api/tenant/create \
  -H "Content-Type: application/json" \
  -d '{
    "tenant": {
      "name": "Barbearia Teste",
      "description": "Barbearia para teste do endpoint corrigido",
      "subdomain": "teste",
      "phone": "(11) 99999-9999", 
      "email": "teste@barbearia.com",
      "address": "Rua de Teste, 123 - Centro, São Paulo - SP",
      "website": "www.barbeariateste.com.br"
    },
    "admin": {
      "name": "Admin Teste",
      "email": "admin@barbeariateste.com",
      "phone": "(11) 98888-8888",
      "password": "teste123"
    }
  }'
```

## 🔍 URLs Aceitas Agora

| Entrada | URL Armazenada |
|---------|----------------|
| `www.site.com` | `https://www.site.com` |
| `site.com.br` | `https://site.com.br` |
| `https://site.com` | `https://site.com` |

## 🚀 Para Testar

```bash
cd src/BarbeariaSaaS.API
dotnet run
```

Endpoint: `POST http://localhost:5000/api/tenant/create`

**Problema resolvido! Agora aceita URLs com ou sem protocolo! 🎉** 