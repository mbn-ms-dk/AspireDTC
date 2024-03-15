using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


//add zipkin
var zipkin = builder.AddContainer("zipkin", "openzipkin/zipkin").WithEndpoint(9411, hostPort: 9412).WithDaprSidecar(appId: "zip");

//grafana
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "grafana")
                     .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
                     .WithEndpoint(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

//add mosquitto
var mqtt = builder.AddContainer("mosquitto", "eclipse-mosquitto").WithBindMount("../mosquitto", "/mosquitto/config/")
    .WithEndpoint(containerPort: 9001,hostPort:9001,  scheme: "ws", name: "ws")
    .WithEndpoint(containerPort: 1883, name: "mqtt");

var mosquitto = mqtt.GetEndpoint("mqtt");

//add prometheus
var prometheus = builder.AddContainer("prometheus", "prom/prometheus").WithBindMount("../prometheus", "/etc/prometheus/")
    .WithEndpoint(9090, hostPort: 9090);


builder.AddProject<Projects.AspireDtC_TrafficSimulationWorker>("trafficsimulationworker")
     .WithEnvironment("MQTT_HOST", mosquitto);

builder.AddProject<Projects.AspireDTC_VisualSimulation>("visualsimulation");



builder.Build().Run();
