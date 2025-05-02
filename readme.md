# Documentação do Projeto

## Pré-requisitos

Antes de iniciar, certifique-se de ter:

- Docker instalado em sua máquina
- Um cluster Kubernetes configurado (preferencialmente o Docker Desktop, pois o projeto foi desenvolvido com ele)
- As seguintes portas disponíveis em seu sistema:
  - 8080 (API)
  - 30090 (Prometheus)
  - 3000 (Grafana)
  - 31572 e 31672 (RabbitMQ)
  - 30009 (Contatos API)
  - 30007 (Auth API)
  - 30008 (Areas API)

## Observações sobre as portas

- As portas expostas acima de 30000 (RabbitMQ, Prometheus, APIs) são configuradas como serviços do tipo NodePort. Em um ambiente de produção, essas portas não estariam expostas diretamente.

## Iniciando o Projeto

1. Certifique-se de que o cluster Kubernetes está ativo.

2. No diretório `kubernetes`, execute o script `install-metrics-server.sh` no Git Bash para instalar o Metrics Server, ou execute os comandos do script manualmente no PowerShell.

3. No diretório raiz do projeto, execute o seguinte comando para construir as imagens Docker que serão usadas no cluster:

```bash
docker compose build
```

4. Após construir as imagens, aplique as configurações do Kubernetes com o comando:

```bash
kubectl apply -f ./kubernetes --recursive
```

Este comando irá aplicar todas as configurações do Kubernetes definidas na pasta `kubernetes`.

## Configurando o Grafana

1. Acesse o Grafana através do navegador:
```
http://localhost:3000
```

2. Faça login utilizando as seguintes credenciais:
```
Usuário: admin
Senha: admin
```

3. Configure o Prometheus como fonte de dados:
   - Acesse as configurações de Data Sources
   - Clique em "Add new data source"
   - Selecione "Prometheus"
   - No campo URL, insira: `http://prometheus-svc:9090`
   - Clique em "Save & Test"

4. Importe o dashboard pré-configurado:
   - Localize o arquivo `grafana-dashboard-config.json` na pasta raiz do projeto
   - No Grafana, vá até a seção de importação de dashboards
   - Carregue o arquivo de configuração
   - Confirme a importação

5. Após a importação, você poderá visualizar o dashboard com as métricas do projeto.

## GitHub Actions

Para testar o workflow do GitHub Actions, você tem duas opções:

1. Realize um commit em qualquer branch do projeto
2. Ou acesse o repositório no GitHub:
   - Vá até a aba "Actions"
   - Localize o workflow principal
   - Clique em "Run workflow"

O workflow será executado automaticamente, e você poderá acompanhar o progresso na aba Actions.