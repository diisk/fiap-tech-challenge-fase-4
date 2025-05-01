FROM node:18

RUN apt-get update && \
    apt-get install -y wget apt-transport-https && \
    wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0

# Instala Azure Functions Core Tools
RUN npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Define diretório de trabalho
WORKDIR /app

# Copia o código da função
COPY . .

# Exponha a porta padrão
EXPOSE 7071

# Comando para iniciar a function app
CMD ["func", "start", "--verbose"]