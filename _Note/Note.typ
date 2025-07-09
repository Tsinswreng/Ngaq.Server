=
[2025-06-26T15:07:01.733+08:00_W26-4]
```bash
docker pull postgres
docker volume create pgdata
docker run --name my-postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres -v pgdata:/var/lib/postgresql/data -d postgres
```

