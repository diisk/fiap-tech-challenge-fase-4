#!/bin/sh
echo "🔄 Aguardando Kong estar pronto..."
until $(curl --output /dev/null --silent --head --fail http://kong-svc:8001); do
    sleep 2
done

echo "✅ Kong está pronto! Iniciando configuração..."

servico_existe() {
    curl -s http://kong-svc:8001/services/$1 | grep -q '"id"'
}

rate_limit_existe() {
    curl -s http://kong-svc:8001/services/$1/plugins | grep -q '"name":"rate-limiting"'
}


criar_servico_e_limite() {
    SERVICO=$1
    URL=$2
    ROTA=$3

    if ! servico_existe $SERVICO; then
        echo "➕ Criando serviço $SERVICO..."
        curl -i -X POST http://kong-svc:8001/services \
          --data "name=$SERVICO" \
          --data "url=$URL"

        curl -i -X POST http://kong-svc:8001/services/$SERVICO/routes \
          --data "name=rota-$SERVICO" \
          --data "paths[]=/$ROTA"
    fi

    if ! rate_limit_existe $SERVICO; then
        echo "➕ Aplicando Rate Limiting global ao serviço $SERVICO..."
        curl -i -X POST http://kong-svc:8001/services/$SERVICO/plugins \
          --data "name=rate-limiting" \
          --data "config.second=2" \
          --data "config.minute=30" \
          --data "config.policy=local"
    fi
}


criar_servico_e_limite "auth" "http://auth-api-svc:8080/api/auth" "api/auth"
criar_servico_e_limite "areas" "http://areas-api-svc:8080/api/areas" "api/areas"
criar_servico_e_limite "contatos" "http://contatos-api-svc:8080/api/contatos" "api/contatos"


echo "✅ Configuração do Kong finalizada!"
