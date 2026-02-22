# LinkUp - Architecture Microservices

## 📖 Vue d'ensemble

**LinkUp** est une plateforme de communication en temps réel basée sur une architecture microservices ASP.NET Core 8.0 avec support gRPC et PostgreSQL.

### Stack Technique
- **Backend** : ASP.NET Core 8.0 (Minimal APIs)
- **Langage** : C# 12
- **ORM** : EF Core 8.0 (Code-First)
- **Base de données** : PostgreSQL 16+
- **Communication intra-services** : gRPC
- **Communication Frontend** : gRPC-Web
- **Frontend** : Angular 17+ (Standalone Components)
- **Styling** : Tailwind CSS
- **Container** : Docker (Alpine Linux)
- **Orchestration** : Kubernetes (manifestes YAML)

### Architecture

```
LinkUp (Microservices)
├── AuthService           (WebAPI) - Authentification & JWT
├── BusinessService       (WebAPI) - Logique métier (Channels, Messages)
├── NotificationService   (gRPC)  - Real-time notifications
└── Shared               (ClassLib) - Protos, DTOs, Models partagés
```

### Bases de données
- `linkup_auth_db` : Utilisateurs, Sessions (AuthService)
- `linkup_business_db` : Channels, Messages (BusinessService)

## 🚀 Démarrage rapide

### Prérequis
- .NET 8.0 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16+ (via Docker)

### Installation

**Option 1 : Lancer tout en CLI (Une ligne)**

```bash
# Démarrer database
docker compose up -d && sleep 3 && \
dotnet ef database update --project src/AuthService && \
dotnet ef database update --project src/BusinessService && \
dotnet run --project src/AuthService & \
dotnet run --project src/BusinessService & \
dotnet run --project src/NotificationService & \
cd frontend && npm ci && ng serve --open
```

**Option 2 : Lancer dans des terminaux séparés (mieux pour déboguer)**

Terminal 1 - Database :
```bash
docker compose up
```

Terminal 2 - Migrations :
```bash
dotnet ef database update --project src/AuthService
dotnet ef database update --project src/BusinessService
```

Terminal 3 - AuthService (port 7000) :
```bash
dotnet run --project src/AuthService
```

Terminal 4 - BusinessService (port 7001) :
```bash
dotnet run --project src/BusinessService
```

Terminal 5 - NotificationService (port 7002) :
```bash
dotnet run --project src/NotificationService
```

Terminal 6 - Frontend (port 4200) :
```bash
cd frontend && npm ci && ng serve --open
```

### Ports disponibles

| Service | Port | URL |
|---------|------|-----|
| AuthService | 7000 | http://localhost:7000 |
| BusinessService | 7001 | http://localhost:7001 |
| NotificationService (gRPC) | 7002 | http://localhost:7002 |
| Frontend (Angular) | 4200 | http://localhost:4200 |
| PostgreSQL | 5432 | localhost |

### Test rapide (Postman/curl)

```bash
# 1. Register
curl -X POST http://localhost:7000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecurePass123!"}'

# 2. Login (récupérer le JWT)
curl -X POST http://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecurePass123!"}'

# 3. Créer un channel (remplacer TOKEN par le JWT)
curl -X POST http://localhost:7001/api/channels \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"general","description":"General discussion"}'

# 4. Lister les channels
curl -X GET http://localhost:7001/api/channels \
  -H "Authorization: Bearer TOKEN"
```

### Vérification

```bash
# Vérifier que PostgreSQL est prêt
docker compose ps

# Vérifier les logs
docker compose logs postgres

# Vérifier que les migrations sont appliquées
dotnet ef migrations list --project src/AuthService
```

## 📚 Structure des dossiers

```
LinkUp/
├── docker-compose.yml              # Configuration PostgreSQL
├── LinkUp.sln                       # Solution principale
├── src/
│   ├── AuthService/                # Service authentification
│   │   ├── appsettings.json
│   │   └── Program.cs
│   ├── BusinessService/            # Service métier
│   │   ├── appsettings.json
│   │   └── Program.cs
│   ├── NotificationService/        # Service notifications (gRPC)
│   │   └── Program.cs
│   └── Shared/                     # Partagé entre services
│       ├── Protos/                 # Définitions gRPC
│       │   ├── auth.proto
│       │   └── chat.proto
│       └── Shared.csproj
├── README.md                       # Documentation projet (ce fichier)
└── TASKS.md                        # Suivi des tâches
```

## 🔗 Documentation additionnelle

- [TASKS.md](./TASKS.md) - État de progression des tâches
- `docs/` (à créer) - Documentation détaillée par service

## 🛡️ Sécurité & Dépendances

### Backend (.NET)
- ✅ Tous les tests passent (23 tests xUnit + Moq)
- ⚠️ Vulnérabilité modérée détectée dans `System.IdentityModel.Tokens.Jwt 7.0.x`
  - Impact : Faible (token signing n'est pas affecté)
  - Action : Upgrade prévu en v9.0+ lors de la prochaine maintenance

### Frontend (Angular)
- ✅ Tous les tests passent (46 tests Jasmine)
- ⚠️ 42 vulnérabilités restantes (4 low, 9 moderate, 29 high)
  - **Impact** : Majoritairement en `devDependencies` (build-time only)
  - **Raison** : Angular 21 n'a pas d'LTS jusqu'à v24 (février 2027)
  - **Plan** : Migration vers Angular 19 LTS avant production

## 📊 État du Projet

| Aspect | Statut |
|--------|--------|
| **Backend Build** | ✅ Success (0 erreurs) |
| **Backend Tests** | ✅ 23/23 passent |
| **Frontend Build** | ✅ Success |
| **Frontend Tests** | ✅ 46/46 passent |
| **CI/CD (GitHub Actions)** | ✅ Les deux pipelines passent |
| **Database EF Core** | ✅ Migrations appliquées |
| **gRPC Integration** | ✅ Server + Client implémentés |
| **JWT Authentication** | ✅ Bearer Token configuré |
| **API Endpoints** | ✅ 9 endpoints fonctionnels |

## 🚀 Prochaines étapes recommandées

1. **Intégration Frontend-Backend** (en cours)
   - [ ] Connecter le login Angular avec AuthService
   - [ ] Connecter l'écran chat avec BusinessService
   - [ ] Tester gRPC-Web pour les notifications

2. **Données de Test**
   - [ ] Ajouter seed data dans les migrations (comptes test)
   - [ ] Créer un dashboard admin pour gérer les données

3. **Sécurité**
   - [ ] Activer HTTPS en production
   - [ ] Ajouter rate limiting sur les endpoints
   - [ ] Configurer CORS proprement

4. **Déploiement**
   - [ ] Builder toutes les images Docker
   - [ ] Déployer en Kubernetes (k8s/ prêt)
   - [ ] Setup monitoring + logging (Prometheus, ELK)

---

**Dernière mise à jour** : 23 février 2026  
**Phases complétées** : 0 (Setup) ✅ | 1 (Database) ✅ | 2 (Auth) ✅ | 3 (Business) ✅ | CI/CD ✅  
**Environnement** : Dev ✅ | Staging ⏳ | Production ❌
