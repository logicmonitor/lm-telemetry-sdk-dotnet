#!/bin/bash
set -x
apt-get update && apt-get -y install build-essential
apt-get update && apt-get -y install cmake && apt-get -y
apt-get install clang -y
cd /lm-data-sdk-dotnet/opentelemetry-dotnet-instrumentation
git submodule init
git submodule update
git log --format="%H" -n 1 >>opentelemetry_version.txt
dotnet tool restore
dotnet nuke
cd ..
dotnet publish -f netcoreapp3.1
cp -R ./opentelemetry-dotnet-instrumentation/bin .
cp -R ./opentelemetry-dotnet-instrumentation/opentelemetry_version.txt bin
rm -rf bin/tracer-home/netcoreapp3.1/LMTelemetry
mkdir bin/tracer-home/netcoreapp3.1/LMTelemetry
cp LMStartupHook/bin/Debug/netcoreapp3.1/publish/LMStartupHook.dll bin/tracer-home/netcoreapp3.1
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/LMTelemetrySDK.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/OpenTelemetry.Api.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/AWSSDK.Core.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/AWSSDK.EC2.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
cp otel-resource/src/main/bin/Debug/netcoreapp3.1/publish/OpenTelemetry.dll bin/tracer-home/netcoreapp3.1/LMTelemetry
chown -R 2007:2007 /lm-data-sdk-dotnet/