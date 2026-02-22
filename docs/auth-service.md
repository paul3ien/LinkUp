# 🔐 Documentation Auth Service - LinkUp

## Vue d'ensemble

**AuthService** gère l'authentification complète : registration, login, JWT token generation.

### Architecture

```
AuthService
├── Models/
│   └── User                          ← Entity (T020)
├── Data/
│   └── AuthDbContext                ← EF Core DbContext (T020)
├── Services/
│   ├── IAuthService / AuthenticationService   ← Registration (T021)
│   └── IJwtService / JwtService               ← JWT Generation (T022)
└── Controllers/
    └── AuthController               ← Endpoints (T021, T022)
```

---

## T021 - Registration (Inscription)

### Endpoint

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "secure_password_123"
}
```

### Réponse (201 Created)

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "message": "User registered successfully"
}
```

### Codes de réponse

| Code | Description |
|------|-------------|
| 201 | User créé avec succès |
| 400 | Email ou password manquants/invalides |
| 409 | Email existe déjà |

### Implémentation

**AuthenticationService.Register(email, password)** :

1. **Validation**
   - Email non vide
   - Password >= 6 caractères

2. **Vérification unicité**
   - Query `SELECT * FROM Users WHERE Email = @email`
   - Si existe → Throw InvalidOperationException

3. **Hachage password**
   ```csharp
   var passwordHash = BCrypt.HashPassword(password);
   // Exemple: $2a$11$Sv3...long...hash
   ```

4. **Persistence**
   - Créer User entity
   - `dbContext.Users.Add(user)`
   - `await dbContext.SaveChangesAsync()`

### Sécurité

✅ Password hashé avec **BCrypt** (irreversible)  
✅ Hash stocké en DB, jamais le plaintext  
✅ Email validé comme unique  
✅ Validation côté serveur (jamais faire confiance au client)  

### Test via Postman

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"mypassword123"}'
```

---

## T022 - Login & JWT Token

### Endpoint

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "secure_password_123"
}
```

### Réponse (200 OK)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

### Codes de réponse

| Code | Description |
|------|-------------|
| 200 | Login réussi, token retourné |
| 400 | Email ou password manquants |
| 401 | Credentials invalides |
| 500 | Erreur serveur |

### Implémentation

#### JwtService.GenerateToken(userId, email, role)

**Configuration** (appsettings.json) :

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-...,",
    "Issuer": "LinkUp",
    "Audience": "LinkUpClients",
    "ExpirationMinutes": 60
  }
}
```

**Génération** :

```csharp
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // sub
    new Claim(ClaimTypes.Email, email),                       // email
    new Claim(ClaimTypes.Role, role)                          // role
};

var token = new JwtSecurityToken(
    issuer: _issuer,
    audience: _audience,
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(60),  // 1h expiration
    signingCredentials: credentials
);

return new JwtSecurityTokenHandler().WriteToken(token);
```

### JWT Token Structure

**Header** :
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** (claims) :
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "role": "user",
  "iss": "LinkUp",
  "aud": "LinkUpClients",
  "exp": 1708712400,
  "iat": 1708708800
}
```

**Signature** :
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secretKey
)
```

### Sécurité

✅ **SymmetricSecurityKey** (HMAC-SHA256)  
✅ **Expiration 1h** (renouvellable via refresh token - future)  
✅ **Claims signés** (non modifiables sans clé privée)  
✅ **SecretKey** changé en production  

⚠️ **À implémenter en production** :
- Stocker SecretKey en environment variable ou secret manager
- Utiliser asymmetric encryption (RSA) pour microservices
- Implémenter refresh tokens
- Rotation des clés

### Test via Postman

```bash
# 1. Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"mypassword123"}'

# 2. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"mypassword123"}'

# Response: { "token": "eyJ...", "expiresIn": 3600 }

# 3. Utiliser le token
curl http://localhost:5000/api/channels \
  -H "Authorization: Bearer eyJ..."
```

---

## 📚 Références

- [BCrypt.Net Official](https://github.com/BcryptNet/bcrypt.net)
- [JWT Introduction](https://jwt.io/)
- [HS256 Algorithm](https://tools.ietf.org/html/rfc7518#section-3.2)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

**Dernière mise à jour** : 22 février 2026  
**Version** : T021, T022 ✅ COMPLÈTE
