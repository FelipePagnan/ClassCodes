# EduPlatform API

API em **C# / .NET 8** (ASP.NET Core Web API + Entity Framework Core) implementada a partir do desafio em
`Desafio_Abril_2024.pdf`. Cobre os itens I, II e III do desafio.

## Como rodar

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd EduPlatform.Api
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5080` e o Swagger abre automaticamente em `/swagger`.
O banco de dados é **SQLite** (`eduplatform.db`, criado automaticamente na primeira execução) — não
precisa instalar SQL Server nem Docker para testar localmente.

Se quiser usar SQL Server em vez de SQLite, troque `UseSqlite` por `UseSqlServer` em `Program.cs` e ajuste
a `ConnectionStrings:DefaultConnection` em `appsettings.json`.

## Estrutura

```
EduPlatform.Api/
├── Models/            Entidades (Usuario, Curso, Modulo, Aula, Assinatura, UsuarioAssinatura,
│                       PagamentoAssinatura, UsuarioAulaConcluida) + Enums
├── Data/               AppDbContext (EF Core)
├── DTOs/                Objetos de entrada/saída dos endpoints
├── Controllers/
│   ├── UsuariosController       CRUD de Usuário
│   ├── CursosController          CRUD de Curso, Módulo e Aula
│   ├── AssinaturasController     CRUD de Assinatura + conectar Usuário↔Assinatura
│   ├── ProgressoController       Concluir aula (com nota) + status de curso por usuário
│   └── InscricaoController       Inscrição em assinatura via Asaas + Webhook de cobrança
└── Services/
    └── AsaasService               Cliente HTTP para a API do Asaas (clientes e cobranças)
```

## Mapeamento com o desafio

### I — CRUDs e vínculo Usuário ↔ Assinatura
- `GET/POST/PUT/DELETE /api/usuarios`
- `GET/POST/PUT/DELETE /api/cursos`
- `GET/POST/PUT/DELETE /api/modulos`
- `GET/POST/PUT/DELETE /api/aulas`
- `GET/POST/PUT/DELETE /api/assinaturas`
- `POST /api/assinaturas/conectar-usuario` — cria o registro `UsuarioAssinatura`

### II — Progresso do aluno
- `POST /api/progresso/concluir-aula` — grava `UsuarioAulaConcluida` com a nota
- `GET /api/progresso/status-curso/{idCurso}/usuario/{idUsuario}` — retorna tempo total,
  tempo concluído, tempo restante e percentual de conclusão do curso para o usuário

### III — Assinatura via Asaas
- `POST /api/inscricao/assinatura` — cria/reaproveita o Cliente no Asaas, gera a Cobrança,
  salva o `IdExternoPagamento` e o `LinkPagamento` em `PagamentoAssinatura`
- `POST /api/webhooks/asaas` — recebe as notificações de cobrança (configurar essa URL no
  painel do Asaas em Integrações → Webhooks) e atualiza o status do pagamento/assinatura

Configure sua chave da API do Asaas em `appsettings.json` → `Asaas:ApiKey` (use o ambiente
sandbox: `https://sandbox.asaas.com/api/v3/` durante os testes).

### IV
Item apenas de estudo (Hotmart/Webhooks) — sem entrega de código correspondente no desafio.

## Observações de design
- Senhas são armazenadas com hash **BCrypt**, nunca em texto puro.
- `Usuario.Ativo` é usado como *soft delete* no DELETE de usuário, preservando o histórico de
  aulas concluídas e assinaturas.
- `Duracao` em `Aula` está em **segundos**; em `Assinatura`, em **dias** — usados para calcular
  tempo restante do curso e data de expiração da assinatura, respectivamente.
- O webhook responde sempre `200 OK` mesmo quando não encontra o pagamento, para evitar reenvios
  indefinidos por parte do Asaas (apenas loga um aviso).
