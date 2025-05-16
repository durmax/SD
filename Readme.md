# Containerising:

in SD folder:

- to build the image:\
 docker build -f Server/Dockerfile -t sdapi .

- to run the image:\
docker run --name sdapi-container -p 4444:8080 sdapi

------------------------