docker build -t poll .
docker tag poll registry.www.areya.fr/poll:v1
docker push registry.www.areya.fr/poll:v1