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
- Docker & Docker Compose
- PostgreSQL 16+ (via Docker)

### Installation

```bash
# Cloner le projet
git clone <repo> && cd LinkUp

# Lancer PostgreSQL
docker-compose up -d

# Compiler la solution
dotnet build

# Migrations (à faire après création des DbContext)
dotnet ef database update --project src/AuthService
dotnet ef database update --project src/BusinessService

# Lancer les services
dotnet run --project src/AuthService
dotnet run --project src/BusinessService
dotnet run --project src/NotificationService
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

---

**Dernière mise à jour** : 22 février 2026  
**Phase actuelle** : 0_Setup ✅ | 1_Database 🚀
