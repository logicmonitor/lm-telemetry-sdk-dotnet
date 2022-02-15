echo """
#!/bin/bash

set -x
cd /lm-data-sdk-dotnet/
apt-get update && apt-get -y install build-essential
apt-get update && apt-get -y install cmake && apt-get -y 
apt-get install clang -y

dotnet tool restore
rm -rf opentelemetry-dotnet-instrumentation/bin
cd opentelemetry-dotnet-instrumentation
dotnet nuke
ls
cd ..
dotnet publish -f netcoreapp3.1
cp -R ./opentelemetry-dotnet-instrumentation/bin .
rm -rf bin/tracer-home/netcoreapp3.1/LMTelemetry
mkdir bin/tracer-home/netcoreapp3.1/LMTelemetry
cp LMStartupHook/bin/Debug/netcoreapp3.1/publish/LMStartupHook.dll bin/tracer-home/netcoreapp3.1
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/LMTelemetrySDK.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/AWSSDK.Core.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/AWSSDK.EC2.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/OpenTelemetry.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
""" > build-2.sh