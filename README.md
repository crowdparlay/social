# Crowd Parlay's *social* microservice [![Test](https://github.com/crowdparlay/social/actions/workflows/test.yml/badge.svg)](https://github.com/crowdparlay/social/actions/workflows/test.yml)

A high-performance, cloud-native microservice enabling discussions, threaded comments, emoji/text reactions, and real-time event publishing via message broker. Designed with a clean onion architecture and test-driven development, it integrates seamlessly with other services through resilient, cache-optimized API calls. Built on a NoSQL store with embedded documents and strategic indexing for fast reads. Basic telemetry and observability are integrated, with support for future expansion. Fully automated CI/CD pipelines enable one-click production deployments.

- **Stack:** <kbd>C# 13</kbd> <kbd>.NET 9</kbd> <kbd>ASP.NET Core</kbd> <kbd>MongoDb.Driver</kbd> <kbd>MassTransit</kbd> <kbd>OpenTelemetry</kbd> <kbd>SignalR</kbd> <kbd>FluentValidation</kbd> <kbd>Mapster</kbd> <kbd>Swashbuckle/OpenAPI</kbd> <kbd>Testcontainers</kbd>
- **Environment:** <kbd>MongoDB</kbd> <kbd>Redis</kbd> <kbd>RabbitMQ</kbd> <kbd>Elastic APM</kbd> [<kbd>crowdparlay/users</kbd>](https://github.com/crowdparlay/users)  
- **Adopted but retired:** <kbd>Neo4j</kbd> <kbd>MediatR</kbd>

> [!NOTE]
> This Git repository contains Submodules. Don’t forget to clone with `--recurse-submodules`
