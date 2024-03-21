using Aspire.Hosting;
using Aspire.Hosting.Dapr;
using System.Collections.Immutable;

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

//add mosquitto
//var mqtt = builder.AddContainer("mosquitto", "eclipse-mosquitto").WithBindMount("../mosquitto", "/mosquitto/config/")
//    .WithEndpoint(containerPort: 9001,hostPort:9001,  scheme: "ws", name: "ws")
//    .WithEndpoint(containerPort: 1883, name: "mqtt");

//var mosquitto = mqtt.GetEndpoint("mqtt");

//add prometheus
var prometheus = builder.AddContainer("prometheus", "prom/prometheus").WithBindMount("../prometheus", "/etc/prometheus/")
    .WithEndpoint(9090, hostPort: 9090);

//add rabbitmq
var rabbit = builder.AddContainer("rabbitmq", "rabbitmq", "management-alpine").WithEndpoint(5672, hostPort: 5672, name: "amqp")
    .WithEndpoint(15672, hostPort: 15672, name: "management");

builder.AddDapr(config =>
{
    config.EnableTelemetry = true;
});


var trafficcache = builder.AddRedis("trafficcache").WithRedisCommander();
var tcs = builder.AddProject<Projects.AspireDTC_TrafficControlService>("trafficcontrolservice")
    .WithReference(trafficcache).WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "trafficcontrolservice",
        AppPort = 5258,
        ResourcesPaths = ImmutableHashSet.Create("./DaprComponents"),
        Config = "./DaprComponents/config.yaml",
        DaprHttpPort = 3500,
        MetricsPort = 9095
    });


builder.AddProject<Projects.AspireDtC_TrafficSimulationWorker>("trafficsimulationworker")
     //.WithEnvironment("MQTT_HOST", mosquitto);
     .WithReference(tcs);

builder.AddProject<Projects.AspireDTC_VisualSimulation>("visualsimulation")
     .WithReference(tcs);






builder.Build().Run();
