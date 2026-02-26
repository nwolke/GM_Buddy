var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database (matching docker-compose: postgres:17-alpine on port 15432)
var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password");

var postgres = builder.AddPostgres("postgres", userName: postgresUsername, password: postgresPassword, port: 15432)
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("gm_buddy_postgres_data")
    .WithBindMount("./init.sql/init.sql", "/docker-entrypoint-initdb.d/init.sql", isReadOnly: true)
    .WithEnvironment("POSTGRES_DB", builder.Configuration["POSTGRES_DB"] ?? "gm_buddy");

var db = postgres.AddDatabase("gm-buddy-db", "gm_buddy");

// pgAdmin (Database Management Tool on port 15435)
var pgadmin = builder.AddContainer("pgadmin", "dpage/pgadmin4", "latest")
    .WithHttpEndpoint(targetPort: 80, port: 15435, name: "pgadminport")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", builder.Configuration["PGADMIN_DEFAULT_EMAIL"] ?? "admin@example.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", builder.Configuration["PGADMIN_DEFAULT_PASSWORD"] ?? "admin")
    .WithBindMount("./servers.json", "/pgadmin4/servers.json", isReadOnly: true)
    .WithVolume("gm_buddy_pgadmin_data", "/var/lib/pgadmin")
    .WaitFor(postgres);

// ASP.NET Core Backend Server (on port 8080)
var server = builder.AddProject<Projects.GM_Buddy_Server>("gm-buddy-server")
    .WithReference(db)  // This injects ConnectionStrings__gm-buddy-db
    .WithHttpEndpoint(port: 8080, name: "serverPort")  // Explicitly set to port 8080 for React to connect
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development")
    .WithEnvironment("Cognito__Region", builder.Configuration["COGNITO_REGION"] ?? "")
    .WithEnvironment("Cognito__UserPoolId", builder.Configuration["COGNITO_USER_POOL_ID"] ?? "")
    .WithEnvironment("Cognito__ClientId", builder.Configuration["COGNITO_CLIENT_ID"] ?? "")
    .WithEnvironment("Cognito__Domain", builder.Configuration["COGNITO_DOMAIN"] ?? "")
    .WaitFor(postgres);

// Use AddNpmApp
var web = builder.AddNpmApp("gm-buddy-react", "../GM_Buddy.React", "dev")
    .WithReference(server)
    //.WithHttpEndpoint(port: 3000, name: "http") -- Vite dev server runs on 3000, but explicitly setting here will conflict.
    .WithEnvironment("VITE_USE_COGNITO", builder.Configuration["VITE_USE_COGNITO"] ?? "false")
    .WithEnvironment("VITE_COGNITO_DOMAIN", builder.Configuration["VITE_COGNITO_DOMAIN"] ?? "")
    .WithEnvironment("VITE_COGNITO_CLIENT_ID", builder.Configuration["VITE_COGNITO_CLIENT_ID"] ?? "")
    .WithEnvironment("VITE_COGNITO_REDIRECT_URI", builder.Configuration["VITE_COGNITO_REDIRECT_URI"] ?? "http://localhost:3000/callback")
    .WithEnvironment("VITE_COGNITO_LOGOUT_URI", builder.Configuration["VITE_COGNITO_LOGOUT_URI"] ?? "http://localhost:3000")
    .WaitFor(server);

builder.Build().Run();