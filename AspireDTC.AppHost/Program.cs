var builder = DistributedApplication.CreateBuilder(args);


//add zipkin
var zipkin = builder.AddContainer("zipkin", "openzipkin/zipkin").WithEndpoint(9411, hostPort: 9412);

//grafana
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "grafana")
                     .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
                     .WithEndpoint(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

//add prometheus
var prometheus = builder.AddContainer("prometheus", "prom/prometheus").WithBindMount("../prometheus", "/etc/prometheus/")
    .WithEndpoint(9090, hostPort: 9090);

var trafficcache = builder.AddRedis("trafficcache").WithRedisCommander();
var finequeue = builder.AddRabbitMQ("fine-messaging");

var tcs = builder.AddProject<Projects.AspireDTC_TrafficControlService>("trafficcontrolservice")
    .WithReference(trafficcache)
    .WithReference(finequeue)
    .WithReplicas(2);


builder.AddProject<Projects.AspireDtC_TrafficSimulationWorker>("trafficsimulationworker")
     .WithReference(tcs);

//builder.AddProject<Projects.AspireDTC_VisualSimulation>("visualsimulation")
//     .WithReference(tcs);

var vrs = builder.AddProject<Projects.AspireDTC_VehicleRegistrationService>("vehicleregistrationservice");

builder.AddProject<Projects.AspireDTC_FineCollectionWorker>("finecollectionworker")
    .WithReference(tcs)
    .WithReference(vrs)
    .WithReference(finequeue);



builder.Build().Run();
