# Containerising:

in SD folder:

Alte Container stoppen (ohne DB zu löschen):

docker compose --env-file env/.env.production down --remove-orphans

-------------------------

Container neu bauen und starten:

docker compose --env-file env/.env.development up --build -d

docker compose --env-file env/.env.production up --build -d