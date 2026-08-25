var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite("sqlitedb",
    databasePath: Path.Combine(builder.AppHostDirectory, "data"),
    databaseFileName: "app.db");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

builder.AddProject<Projects.AKSCS14App_Api>("api")
    .WithReference(sqlite)
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();