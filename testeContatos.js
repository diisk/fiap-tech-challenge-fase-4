import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 1000, // Número de usuários simultâneos
  duration: '30s', // Duração total do teste
};

export default function () {
  // 1. Autenticar e obter o token
  const loginPayload = JSON.stringify({
    login: 'admin',
    senha: 'admin',
  });

  const loginHeaders = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const loginRes = http.post('http://localhost:30007/api/auth/logar', loginPayload, loginHeaders);
  check(loginRes, {
    'login status 200': (res) => res.status === 200,
  });

  const token = loginRes.json('data.token');

  // 2. Usar o token no header Authorization
  const contatosHeaders = {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  };

  const contatosRes = http.get('http://localhost:30009/api/contatos?codigoArea=31', contatosHeaders);
  check(contatosRes, {
    'contatos status 200': (res) => res.status === 200,
  });

  // opcional: log de resposta para debug
  // console.log(contatosRes.body);

  sleep(1); // espera 1s entre as execuções
}
