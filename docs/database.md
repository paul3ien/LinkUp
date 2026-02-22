# 🗄️ Documentation EF Core & Migrations - LinkUp

## Vue d'ensemble

**Approche** : EF Core Code-First avec PostgreSQL via Npgsql  
**Stratégie** : Migrations générées manuellement (structure boilerplate standard EF Core)

---

## Structure des Migrations

### AuthService

**DbContext** : [AuthDbContext.cs](../src/AuthService/Data/AuthDbContext.cs)  
**Entity** : [User.cs](../src/AuthService/Models/User.cs)  
**Migrations** : [src/AuthService/Migrations/](../src/AuthService/Migrations/)

| Fichier | Rôle |
|---------|------|
| `20260222000000_InitialCreate.cs` | Up/Down methods pour créer table Users |
| `20260222000000_InitialCreate.Designer.cs` | Metadata de la migration |
| `AuthDbContextModelSnapshot.cs` | Snapshot du modèle EF |

**Table Users** :
```sql
CREATE TABLE "Users" (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL
);
```

### BusinessService

**DbContext** : [BusinessDbContext.cs](../src/BusinessService/Data/BusinessDbContext.cs)  
**Entities** : 
- [Channel.cs](../src/BusinessService/Models/Channel.cs)
- [Message.cs](../src/BusinessService/Models/Message.cs)

**Migrations** : [src/BusinessService/Migrations/](../src/BusinessService/Migrations/)

**Tables** :
```sql
CREATE TABLE "Channels" (
    Id UUID PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL
);

CREATE TABLE "Messages" (
    Id UUID PRIMARY KEY,
    ChannelId UUID NOT NULL REFERENCES "Channels"(Id) ON DELETE CASCADE,
    UserId TEXT NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL
);

CREATE INDEX IX_Messages_ChannelId ON "Messages"(ChannelId);
```

---

## 📋 Appliquer les Migrations

### Une seule fois (initialisation)

```bash
cd LinkUp

# Lancer PostgreSQL
docker compose up -d

# Patientar que Postgres soit ready (health check)
# ...

# Appliquer migrations AuthService
dotnet ef database update --project src/AuthService

# Appliquer migrations BusinessService
dotnet ef database update --project src/BusinessService

# Vérifier
psql -U linkup_user -d linkup_auth_db -h localhost -c "\dt"
psql -U linkup_user -d linkup_business_db -h localhost -c "\dt"
```

### À chaque changement de modèle

**Workflow** :
1. Modifier entity (ex: ajouter propriété dans User)
2. Générersz migration (voir section suivante)
3. Appliquer migration (dotnet ef database update)

---

## 🔧 Générer nouvelles migrations

### Méthode 1 : dotnet-ef CLI (recommandée en prod)

```bash
# Installer le tool
dotnet tool install --global dotnet-ef --version 8.0.0

# Générer migration
dotnet ef migrations add AddBirthDateToUser --project src/AuthService

# Cela crée dans AuthService/Migrations/:
# - 20260222HHMMSS_AddBirthDateToUser.cs
# - 20260222HHMMSS_AddBirthDateToUser.Designer.cs
# - Met à jour ModelSnapshot.cs

# Appliquer
dotnet ef database update --project src/AuthService
```

### Méthode 2 : Créer manuellement (comme fait pour InitialCreate)

Structure minimale (voir migration InitialCreate comme référence) :

```csharp
// 20260222HHMMSS_MigrationName.cs
public partial class MigrationName : Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        // Instructions SQL pour appliquer le changement
        mb.CreateTable(...);
    }

    protected override void Down(MigrationBuilder mb)
    {
        // Instructions SQL pour revert
        mb.DropTable(...);
    }
}
```

---

## 📝 Convention de nommage

**Format timestamp** : `YYYYMMDDHHmmss_DescriptionEnPascalCase`

**Exemples** :
- `20260222120000_InitialCreate`
- `20260225143002_AddUniqueEmailIndex`
- `20260301090530_AddMessageTable`

---

## ⚠️ Points importants

### Foreign Keys
- **Cascade delete** : Tables liées (ex: Message → Channel)  
- Configuration : `HasForeignKey().OnDelete(DeleteBehavior.Cascade)`

### Indexes
- Index unique : Email (Users table)
- Index standard : ChannelId (Messages table)

### Timestampz
- Utiliser `timestamp with time zone` pour PostgreSQL
- En C# : `DateTime` (UTC recommended)

### Nullabilité
- `string Email` = NOT NULL (model avec `[Required]`)
- `string? OptionalField` = NULL  
- Configuration : `.IsRequired()` / `.IsRequired(false)`

---

## 🐛 Troubleshooting

### Migration ne s'applique pas
```bash
# Vérifier status
dotnet ef migrations list --project src/AuthService

# Voir erreur détaillée
dotnet ef database update --project src/AuthService --verbose
```

### Reverting une migration
```bash
# Revert dernière migration (appliquée mais non comittée)
dotnet ef database update PreviousMigrationName --project src/AuthService

# Ex: revert InitialCreate (revient à zéro)
dotnet ef database update 0 --project src/AuthService
```

### PostgreSQL not accessible
```bash
# Vérifier container
docker ps

# Vérifier logs
docker logs linkup-postgres

# Tester connexion
psql -U linkup_user -h localhost -d linkup_auth_db
```

---

## 📚 Références

- [EF Core Migrations (MS Docs)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations)
- [Npgsql EF Core (GitHub)](https://github.com/npgsql/efcore.pg)
- [PostgreSQL Data Types](https://www.postgresql.org/docs/16/datatype.html)

---

**Dernière mise à jour** : 22 février 2026  
**Version EF Core** : 8.0.0  
**PostgreSQL** : 16+
