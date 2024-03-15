# AspireDTC

This project is to create a .NET Aspire preview3 version of the the original Dapr Traffic Control Sample. The original sample can be found [here](https://github.com/EdwinVW/dapr-traffic-control).

## Overall Architecture

The overall architecture of the system is shown below:

```mermaid
graph LR
    subgraph subTcs[Traffic Control Service]
       ds1[Dapr Sidecar] -. input binding /entrycam .-> tcs[TrafficControlService]
       ds1 -. input binding /exitcam .-> tcs
       tcs -. pub/sub,state management .-> ds1
    end
     subgraph subFcs[Fine Collection Service]
       ds2[Dapr Sidecar] -. input binding /entrycam .-> fcs[FineCollectionService]
       ds2 -. pub/sub /collectfine .-> fcs
       fcs -. service invokation,output binding,secrets management .-> ds2
    end
    subgraph subVhs[Vehicle Registration Service]
       ds3[Dapr Sidecar] -. input binding /entrycam .-> vhs[VehicleRegistrationService]
       ds3 -. service invokation /vehicleinfo/license-number .-> vhs
    end
    subgraph subHlp[Infrastrcture]
        MQTT[MQTT]
        REDIS[Redis]
        MQ[RAbbitMQ]
        SMTP[SMTP]
     end
    CS[CameraSimulation] -. input binding .-> MQTT
    MQTT -. input binding .-> ds1
    ds1 -. state management .-> REDIS
    ds1 -. pub/sub .-> MQ
    MQ -. pub/sub .-> ds2
    ds2 -. output binding .-> SMTP
    ds2 -. service invokation .-> ds3
    
   linkStyle default stroke-width:2px 
   style CS fill:blue,stroke:#333,stroke-width:2px;
   style tcs fill:blue,stroke:#333,stroke-width:2px;
   style fcs fill:blue,stroke:#333,stroke-width:2px;
   style vhs fill:blue,stroke:#333,stroke-width:2px;
   style ds1 fill:darkBlue,stroke:#333,stroke-width:2px;
   style ds2 fill:darkBlue,stroke:#333,stroke-width:2px;
   style ds3 fill:darkBlue,stroke:#333,stroke-width:2px;
   style subTcs fill:lightGrey,opacity:0.5;
   style subVhs fill:lightGrey,opacity:0.5;
   style MQTT fill:green,stroke:#333,stroke-width:2px;
   style REDIS fill:green,stroke:#333,stroke-width:2px;
   style SMTP fill:green,stroke:#333,stroke-width:2px;
   style MQ fill:green,stroke:#333,stroke-width:2px;
   style subFcs fill:lightGrey,opacity:0.5;
   style subHlp fill:lightGrey,opacity:0.5;
```