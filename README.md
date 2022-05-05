# lm-telemetry-sdk-dotnet
LogicMonitor Telemetry SDK

## AUTO INSTRUMENTATION

To auto-instrument applications, the instrumentation:

1. Injects and configures the [OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry/README.md#opentelemetry-net-sdk) into the application.
2. Adds [OpenTelemetry Instrumentation](https://opentelemetry.io/docs/concepts/instrumenting/) to key packages and APIs used by the application.

The auto-instrumentation is capable of injecting instrumentations at runtime, a technique known as monkey-patching. This allows to instrument specific packages or APIs that don't provide the necessary hooks to generate .NET instrumentation packages.

### Mimimun Requirement

`System.Diagnostics.DiagnosticSource version 6.0.0`

Dynamically loading System.Diagnostics.DiagnosticSource.dll with a higher version than the one from framework/package will crash the process.
Linked issue - [`Diagnostic Source assembly versioning Issue`](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/issues/260).
Hence it is recommended to uses System.Diagnostics.DiagnosticSource version 6.0.0 or higher.

### Instrument a .NET application

Before executing the application,first build the application and then set the following environment variables:

```env
DOTNET_ADDITIONAL_DEPS=./%InstallationLocation%/AdditionalDeps
DOTNET_SHARED_STORE=%InstallationLocation%/store
DOTNET_STARTUP_HOOKS=%InstallationLocation%/netcoreapp3.1/LMStartupHook.dll
OTEL_DOTNET_AUTO_ENABLED_INSTRUMENTATIONS=AspNet,HttpClient,SqlClient
OTEL_TRACES_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=<YOUR_OTLP_ENDPOINT>
OTEL_SERVICE_NAME=<YOUR_SERVICE_NAME>
```
** InstallationLocation refers to /bin/tracer-home **

## ENVIRONMENT VARIABLES

To Auto instrument .Net application using LMStartupHook, the following environment variables need to be set:

### LMStartupHook

To initialise LMStartupHook which will then invoke opentelemetry startuphook.
| Environment variable | Value |
|-|-|
| `DOTNET_STARTUP_HOOKS` | `%InstallationLocation%/netcoreapp3.1/LMStartupHook.dll` |


### .NET Runtime Additional-Deps and Package Store

To resolve assembly version conflicts in .NET Core,
set the
[`DOTNET_ADDITIONAL_DEPS`](https://github.com/dotnet/runtime/blob/main/docs/design/features/additional-deps.md)
and [`DOTNET_SHARED_STORE`](https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-store)
environment variables to the following values:

| Environment variable | Value |
|-|-|
| `DOTNET_ADDITIONAL_DEPS` | `./%InstallationLocation%/AdditionalDeps` |
| `DOTNET_SHARED_STORE` | `%InstallationLocation%/store` |

### Resources

| Environment variable | Description | Default |
|-|-|-|
| `OTEL_RESOURCE_ATTRIBUTES` | Key-value pairs to be used as resource attributes. See [Resource SDK](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/sdk.md#specifying-resource-information-via-an-environment-variable) for more details. For example, OTEL_RESOURCE_ATTRIBUTES="key1=value1,key2=value2"| See [Resource semantic conventions](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md#semantic-attributes-with-sdk-provided-default-value) for details. |
| `OTEL_SERVICE_NAME` | Sets the value of the [`service.name`](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md#service) resource attribute. If `service.name` is also provided in `OTEL_RESOURCE_ATTRIBUTES`, then `OTEL_SERVICE_NAME` takes precedence. | `unknown_service:%ProcessName%` |

The LMStartupHook invokes Lm-telemetry-dotnet resource detector to automatically detect resource attributes.Currently lm-telemetry-dotnet resource detector can only detect AWS EC2 environment variables along with service name.
To disable lm auto resource detection, set the `DISABLE_RESOURCE_DETECTION` variable to `1`. Default value is `0`.

### Instumented libraries and framework
To use specific instrumentations, set the `OTEL_DOTNET_AUTO_ENABLED_INSTRUMENTATIONS` environment variable. Following are currently supported instrumentation libraries and frameworks.

| ID | Library | Instrumentation type |
|-|-|-|
| `AspNet` | ASP.NET and ASP.NET Core | source |
| `GraphQL` | [GraphQL](https://www.nuget.org/packages/GraphQL/) | bytecode |
| `HttpClient` | [System.Net.Http.HttpClient](https://docs.microsoft.com/dotnet/api/system.net.http.httpclient) and [System.Net.HttpWebRequest](https://docs.microsoft.com/dotnet/api/system.net.httpwebrequest) | source |
| `MongoDb` | [MongoDB.Driver.Core](https://www.nuget.org/packages/MongoDB.Driver.Core/) | bytecode |
| `SqlClient` | [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient) and [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient) | source |

### ASP.NET (.NET Framework) Instrumentation

ASP.NET instrumentation on .NET Framework requires installing the
[`OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule`](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule/)
NuGet package on the instrumented project.
See [the WebConfig section](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Instrumentation.AspNet#step-2-modify-webconfig) for more information.

### Exporters

The exporter is used to output the telemetry.
| Environment variable | Description | Default |
|-|-|-|
| `OTEL_TRACES_EXPORTER` | The traces exporter to be used. Available values are: `jeager`, `otlp`, `none`. | `otlp` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Target endpoint for OTLP exporter. More details [here](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/protocol/exporter.md). | `http://localhost:4318` for `http/protobuf` protocol, `http://localhost:4317` for `grpc` protocol |
| `OTEL_EXPORTER_OTLP_HEADERS` | Key-value pairs to be used as headers associated with gRPC or HTTP requests. More details [here](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/protocol/exporter.md). | |
| `OTEL_EXPORTER_OTLP_TIMEOUT` | Maximum time the OTLP exporter will wait for each batch export. | `1000` (ms) |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | The OTLP expoter transport protocol. Supported values: `grpc`, `http/protobuf`. [1] | `http/protobuf` |

### Environment Change Log

| Old Environment variable | New Environment variable | Description | Default |
|-|-|-|-|
| `OTEL_DOTNET_TRACER_INSTRUMENTATIONS` | `OTEL_DOTNET_AUTO_ENABLED_INSTRUMENTATIONS` | Comma-separated list of source instrumentations you want to enable.| |
| `OTEL_DOTNET_TRACER_CONSOLE_EXPORTER_ENABLED` | `OTEL_DOTNET_AUTO_CONSOLE_EXPORTER_ENABLED` | Whether the console exporter is enabled or not.| `false` |
| `OTEL_DOTNET_TRACER_LOAD_AT_STARTUP` | `OTEL_DOTNET_AUTO_LOAD_AT_STARTUP` | Whether the tracer is created by the automatic instrumentation library or not. Set to false when the application initializes the OpenTelemetry .NET SDK Tracer on its own. This configuration can be used, for example, to retrieve the bytecode instrumentations.| `true` |
| `OTEL_DOTNET_TRACER_ADDITIONAL_SOURCES` | `OTEL_DOTNET_AUTO_ADDITIONAL_SOURCES` |Comma-separated list of additional ActivitySource names to be added to the tracer at the startup. Use it to capture manually instrumented spans.||å
| `OTEL_DOTNET_TRACER_INSTRUMENTATION_PLUGINS` | `OTEL_DOTNET_AUTO_INSTRUMENTATION_PLUGINS` | Colon-separated list of OTel SDK instrumentation plugins represented by System.Type.AssemblyQualifiedName.||
## UPDATING OPENTELEMETRY-DOTNET-INSTRUMENTATION SUBMODULE

 - To initialise submodule: 
   -  git submodule init
   -  git submodule update
 - To checkout specific commit of [opentelemetry-dotnet-instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation) repository 
   -  git checkout <commit-id> 
