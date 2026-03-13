# pass-facile-api

API **.NET 10** construite selon les principes **Clean Architecture** (Domain / Application / Infrastructure / Api), avec **JWT**, **Swagger/OpenAPI** (en Development), **Serilog** et **PostgreSQL**.

## Architecture

Le flux principal est : **HTTP → Controllers → Application (use-cases via MediatR) → Domain → Infrastructure (EF Core, services externes)**.

```mermaid
flowchart LR
  Client[Client HTTP] --> Api[Api (ASP.NET Core)\nControllers + Middlewares]
  Api -->|IMediator.Send| App[Application\nCommands/Queries + DTOs + Validation]
  App --> Domain[Domain\nAggregates + ValueObjects + DomainEvents]
  App --> Infra[Infrastructure\nEF Core + Identity + Services externes]
  Infra --> Db[(PostgreSQL)]

  Infra -.-> Domain
  Infra -.-> App
```

### Projets (dossier `src/`)

- **`src/Api`**: couche présentation (controllers, pipeline HTTP, Swagger, auth JWT)
- **`src/Application`**: cas d’usage (commands/queries), DTOs, interfaces
- **`src/Domain`**: modèle métier (aggregates, value objects, domain events)
- **`src/Infrastructure`**: persistance (EF Core), Identity, implémentations de services

## Prérequis

- **.NET SDK**: .NET 10
- **Docker Desktop**: recommandé pour lancer PostgreSQL en local via Compose

## Démarrage rapide (local)

1) Lancer PostgreSQL + pgAdmin :

```bash
docker compose up -d
```

2) Configurer les secrets (au minimum JWT) :

```bash
dotnet user-secrets --project src/Api set "JwtSettings:SecretKey" "REMPLACER_PAR_UN_SECRET_LONG_32+_CARACTERES"
```

3) Lancer l’API :

```bash
dotnet run --project src/Api
```

## Endpoints (Development)

- **Swagger UI**: disponible à la racine (`/`) quand `ASPNETCORE_ENVIRONMENT=Development`
- **Health check**: `GET /health`

## Configuration

La configuration est lue depuis `src/Api/appsettings.json`, plus les surcharges par environnement, variables d’environnement et **User Secrets** (recommandé en local).

### Auth JWT

Section `JwtSettings` :

- `JwtSettings:SecretKey` (**à stocker en User Secrets / variables d’environnement**)
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:ExpiryMinutes`

## PostgreSQL en local (pas-à-pas)

### 1) Lancer la base via Docker Compose

```bash
docker compose up -d postgres pgadmin
docker compose ps
```

Valeurs par défaut (voir `docker-compose.yml`) :

- PostgreSQL: `localhost:5432`
- Utilisateur: `pf_user`
- Mot de passe: `pf_pwd`
- Base: `pf`
- pgAdmin: `http://localhost:5050` (login par défaut défini dans Compose)

### 2) Vérifier / surcharger les connection strings

L’API utilise deux connexions PostgreSQL :

- `ConnectionStrings:DefaultConnection` → contexte applicatif (`AppDbContext`)
- `ConnectionStrings:IdentityConnection` → contexte Identity (`IDbContext`)

Important : dans `appsettings.json`, le `Host` pointe vers `postgres` (nom du service Docker).  
Si vous exécutez l’API **en dehors** de Docker (via `dotnet run`), vous devez surcharger le host en `localhost`.

Exemples via **User Secrets** :

```bash
dotnet user-secrets --project src/Api set "ConnectionStrings:DefaultConnection"  "Host=localhost;Port=5432;Database=pf;Username=pf_user;Password=pf_pwd"
dotnet user-secrets --project src/Api set "ConnectionStrings:IdentityConnection" "Host=localhost;Port=5432;Database=pf_identity;Username=pf_user;Password=pf_pwd"
```

### 3) Appliquer les migrations EF Core

Les migrations sont versionnées dans `src/Infrastructure/Migrations/`.
Deux DbContexts sont utilisés :

- `Infrastructure.Persistences.AppDbContext` (données métier)
- `Infrastructure.Persistences.IDbContext` (Identity)

Installer l’outil EF (si nécessaire) :

```bash
dotnet tool install --global dotnet-ef
```

Appliquer les migrations :

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api --context AppDbContext
dotnet ef database update --project src/Infrastructure --startup-project src/Api --context IDbContext
```

## Endpoints API (définis dans `src/Api/Controllers`)

Base path typique : `api/<controller>`.

### Auth (`AuthController`)

- `POST /api/auth/send-otp` (**anonyme**): demander un OTP
- `POST /api/auth/verify-otp` (**anonyme**): vérifier un OTP
- `POST /api/auth/refresh` (**auth requis**): rafraîchir un token à partir d’un refresh token

### Users (`UsersController`)

- `PUT /api/users/me/profile` (**auth requis**): mettre à jour le profil de l’utilisateur courant

### Autres contrôleurs présents (endpoints à compléter)

Les contrôleurs suivants existent mais ne déclarent pas encore d’actions HTTP dans le code actuel :

- `CategoriesController`
- `EventsController`
- `MediaController`
- `OrganizersController`
- `OrdersController`
- `PaymentsController`
- `ScanController`
- `TicketsController`

## Commandes utiles

Build :

```bash
dotnet build
```

Tests :

```bash
dotnet test
```

Exécuter l’API :

```bash
dotnet run --project src/Api
```

## License

Voir `LICENSE.txt`.