# Caminho onde está o YAML do Dashboard
$yamlPath = "$PSScriptRoot\recommended.yaml"
$tokenFile = "$PSScriptRoot\token.txt"
$portaCustomizada = 8500

# Aplica o YAML de instalação do Dashboard
Write-Host "Aplicando recommended.yaml para instalar o Kubernetes Dashboard..."
kubectl apply -f $yamlPath

# Aguarda pods subirem
Write-Host "Aguardando 10 segundos para os pods iniciarem..."
Start-Sleep -Seconds 10

# Cria ServiceAccount e permissões administrativas
Write-Host "Criando ServiceAccount e permissões..."
kubectl create serviceaccount dashboard-admin-sa -n kubernetes-dashboard -o yaml --dry-run=client | kubectl apply -f -
kubectl create clusterrolebinding dashboard-admin-sa `
  --clusterrole=cluster-admin `
  --serviceaccount=kubernetes-dashboard:dashboard-admin-sa `
  -o yaml --dry-run=client | kubectl apply -f -

Start-Sleep -Seconds 2

# Gera o token
Write-Host "Gerando token de acesso..."
$token = kubectl -n kubernetes-dashboard create token dashboard-admin-sa
$token | Out-File -Encoding ASCII -FilePath $tokenFile
Write-Host "Token salvo em: $tokenFile"

# Inicia proxy na porta personalizada
Write-Host "Iniciando proxy na porta $portaCustomizada..."
Start-Process powershell -ArgumentList "kubectl proxy --port=$portaCustomizada"

Start-Sleep -Seconds 3

# Abre o dashboard no navegador
$url = "http://localhost:$portaCustomizada/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/"
Start-Process $url

Write-Host "`n Acesse o Dashboard na URL acima e cole o token gerado."
Write-Host "Token (também salvo em token.txt):`n"
Write-Host $token
