docker build -t poll .
docker tag poll registry.w.thera-engineering.com/poll:v1
docker push registry.w.thera-engineering.com/poll:v1