# CITL — Development Links

> Quick reference for all local development endpoints.
> Start everything with `.\scripts\start.ps1` (Windows) or `./scripts/start.sh` (Linux/macOS).

---

## Application

| Service        | URL                                              |
|----------------|--------------------------------------------------|
| Swagger UI     | https://localhost:7001/swagger/index.html         |
| Scalar Docs    | https://localhost:7001/scalar                     |
| Health Check   | https://localhost:7001/health                     |
| OpenAPI JSON   | https://localhost:7001/openapi/v1.json            |

## Observability (Grafana LGTM Stack)

| Service        | URL                                              | Credentials                  |
|----------------|--------------------------------------------------|------------------------------|
| Grafana        | http://localhost:3000                             | admin / admin                |
| OTLP gRPC      | http://localhost:4317                             | —                            |
| OTLP HTTP      | http://localhost:4318                             | —                            |

### Pre-Provisioned Dashboards (Grafana → CITL folder)

| Dashboard              | Description                                                          |
|------------------------|----------------------------------------------------------------------|
| Application Overview   | Request rate, latency percentiles (p50/p95/p99), errors, status codes |
| SQL Performance        | Query rate, duration percentiles, top stored procedures, DB logs       |

## Docker

```bash
docker compose up -d          # Start Grafana LGTM stack
docker compose down           # Stop Grafana LGTM stack
docker compose logs grafana   # View Grafana container logs
```
