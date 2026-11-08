# CRMDev — API (.NET 8)

API para o projeto CRMDev (escopo V1), baseada no documento enviado: cadastro
de **Contatos** e **Oportunidades**, gestão de **etapas** do funil de vendas,
marcação de **ganho/perda**, **tags** de contato e **autenticação/autorização**
com ASP.NET Identity + JWT.

## Estrutura (Clean Architecture simplificada)

```
CRMDev.sln
src/
  CRMDev.Domain/          Entidades e enums (sem dependências externas)
  CRMDev.Application/     DTOs, interfaces e regras de negócio (Services)
  CRMDev.Infrastructure/  EF Core, Identity, JWT, Repositórios
  CRMDev.Api/             Controllers, Program.cs, Swagger
```

### Entidades principais

- **Contato**: Nome, Email, Telefone/Celular, Área de Atuação (entidade),
  Cargo, Documento, Endereço, Notas, Tags (N:N).
- **Oportunidade**: Título, Descrição, Prazo, Orçamento, Escopo, Suporte
  Incluso, Status (EmAberto/Ganho/Perdido), Razão de Perda, Etapa, vínculo
  com Contato.
- **Etapas (V1, fixas)**: ContatoFeito → PreQualificacao → Conexao →
  ReuniaoAgendada → PropostaEnviada → AguardandoAssinatura → Fechado.
- **AreaAtuacao** e **Tag**: já nascem como entidades/cadastros, como
  indicado no documento de escopo.

### Regras de negócio implementadas

- Marcar Ganho ou Perda move a oportunidade automaticamente para a etapa
  **Fechado** (`Oportunidade.MarcarGanho/MarcarPerdido`).
- Perda exige uma `RazaoPerda` (Dinheiro, Desinteresse, etc.).
- Não é possível alterar manualmente a etapa de uma oportunidade já fechada.
- E-mail de contato é único.

## Como rodar localmente

Pré-requisitos: **.NET 8 SDK**.

```bash
cd CRMDev
dotnet restore
dotnet build
dotnet run --project src/CRMDev.Api
```

O banco usado por padrão é **SQLite** (`crmdev.db`, criado automaticamente
na pasta de execução) e as migrations rodam sozinhas no startup
(`db.Database.Migrate()` em `Program.cs`). Não é necessário instalar nada
além do SDK para subir o projeto pela primeira vez.

Ao rodar, a API sobe (por padrão) em algo como `https://localhost:5xxx` e o
Swagger fica disponível em `/swagger`.

### Gerando a primeira migration

Como as migrations ainda não foram geradas neste ambiente (sem acesso à
internet para restaurar pacotes), rode uma vez, na sua máquina:

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver
cd src/CRMDev.Infrastructure
dotnet ef migrations add InicialV1 --startup-project ../CRMDev.Api --output-dir Data/Migrations
```

Depois disso, `dotnet run` já aplica a migration automaticamente.

### Trocando para SQL Server (opcional)

Em `CRMDev.Infrastructure/DependencyInjection.cs`, troque:

```csharp
options.UseSqlite(connectionString)
```

por:

```csharp
options.UseSqlServer(connectionString)
```

e ajuste o pacote `Microsoft.EntityFrameworkCore.SqlServer` (já referenciado
no `.csproj`) e a `ConnectionStrings:DefaultConnection` em
`appsettings.json`.

## Autenticação

1. `POST /api/auth/registrar` — cria usuário (Nome, Email, Senha).
2. `POST /api/auth/login` — retorna um token JWT.
3. Envie `Authorization: Bearer {token}` nas demais rotas (todas protegidas
   com `[Authorize]`).

A chave JWT em `appsettings.json` (`Jwt:Secret`) é só um valor de exemplo —
**troque por uma chave forte antes de qualquer uso real**, idealmente via
`dotnet user-secrets` ou variável de ambiente.

## Principais endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | /api/auth/registrar | Cria usuário |
| POST | /api/auth/login | Login, retorna JWT |
| GET  | /api/contatos?busca= | Lista contatos |
| GET  | /api/contatos/{id} | Detalhe do contato |
| POST | /api/contatos | Cria contato |
| PUT  | /api/contatos/{id} | Atualiza contato |
| DELETE | /api/contatos/{id} | Remove contato |
| POST | /api/contatos/{id}/tags | Adiciona tag |
| DELETE | /api/contatos/{id}/tags/{nomeTag} | Remove tag |
| GET  | /api/areasatuacao | Lista áreas de atuação |
| POST | /api/areasatuacao | Cria área de atuação |
| GET  | /api/oportunidades?etapa=&status=&contatoId= | Lista oportunidades |
| GET  | /api/oportunidades/{id} | Detalhe da oportunidade |
| POST | /api/oportunidades | Cria oportunidade |
| PUT  | /api/oportunidades/{id} | Atualiza oportunidade |
| DELETE | /api/oportunidades/{id} | Remove oportunidade |
| PATCH | /api/oportunidades/{id}/etapa | Altera etapa |
| PATCH | /api/oportunidades/{id}/ganhar | Marca como ganha |
| PATCH | /api/oportunidades/{id}/perder | Marca como perdida (com razão) |

## Próximos passos sugeridos (V2/V3, conforme o escopo)

- V2: Tarefas por Etapa, Indicadores, e-mail de notificação de ganho.
- V3: Etapas personalizáveis (entidade), Tarefas complexas, integração com
  Google Calendário, geração de link de pagamento, webhooks para
  MailChimp/RD Station/Active Campaign.

## ⚠️ Observação sobre este ambiente

Este código foi gerado em um ambiente sandbox **sem acesso ao nuget.org**,
então não foi possível rodar `dotnet restore`/`build`/gerar migrations aqui
para validar a compilação. O código segue os padrões usuais do ASP.NET
Core 8 + EF Core + Identity, mas rode `dotnet build` na sua máquina assim
que receber os arquivos para confirmar que compila limpo — e me avise se
aparecer algum erro, que eu corrijo.
