docker compose down
sleep 2
docker compose build

docker compose up -d rabbitmq
sleep 1
docker compose up -d consumer1
sleep 1
docker compose up -d producer


sleep .8

docker compose up -d loadbalancer

