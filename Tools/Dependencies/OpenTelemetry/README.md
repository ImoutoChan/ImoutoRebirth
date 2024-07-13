### Run dependencies:

```bash
docker compose up
```

[Jaeger UI](http://localhost:16686/)
[Prometheus UI](http://localhost:9090/)

### Sample metric:

```
irate(db_client_commands_bytes_read_total{application="lilin",pool_name=~".*LilinProd.*"} [5m])
```
