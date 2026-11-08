# CRMDev — Documentação do Projeto

Documento de referência que consolida o escopo original (PDF enviado por
Felipe Pagnan) e a implementação feita em C# / .NET 8.

## 1. Visão Geral

CRMDev é um CRM (Customer Relationship Management) simples, pensado como
projeto de estudo/portfólio, cujo objetivo é gerenciar **Contatos** e
**Oportunidades** de negócio ao longo de um funil de vendas com **etapas**.

Autor do escopo: Felipe Pagnan.

## 2. Conceitos de Domínio (conforme o PDF)

### Contato
Pessoa ou empresa com quem há relacionamento comercial.

| Campo | Observação |
|---|---|
| Nome | obrigatório |
| Email | obrigatório, único |
| Telefone / Celular | — |
| Área de Atuação | Enum ou cadastro — **implementado já como entidade** (`AreaAtuacao`), conforme indicado no PDF |
| Cargo | — |
| Documento? | opcional (CPF/CNPJ) |
| Endereço? | opcional |
| Notas | texto livre |

### Oportunidade
Negócio possível associado a um Contato.

| Campo | Observação |
|---|---|
| Título | — |
| Descrição | — |
| Prazo | — |
| Orçamento | — |
| Escopo | — |
| Suporte Incluso | booleano |
| Status | Em Aberto, Ganho, Perdido |
| Razão da Perda | Enum (Dinheiro, Desinteresse, ...), exigida quando perdida |
| Etapa | ver abaixo |
| Id Contato | referência ao Contato dono da oportunidade |

Regra do PDF: **se a oportunidade for Ganha ou Perdida, ela vai para a etapa
Fechado** — implementado em `Oportunidade.MarcarGanho()` /
`Oportunidade.MarcarPerdido(razao)`.

### Etapas da Oportunidade
- **V1 (implementada)**: enum fixo — `ContatoFeito → PreQualificacao →
  Conexao → ReuniaoAgendada → PropostaEnviada → AguardandoAssinatura →
  Fechado`.
- **V3 (futuro)**: etapas personalizáveis, viram uma entidade própria para o
  usuário definir quais etapas existem e em que sequência.

> Nota: o PDF lista duas numerações de etapas em pontos diferentes — uma
> versão mais "funcional" (Contato Feito, Pré Qualificação, Agendamento
> Realizado, Proposta Apresentada, Proposta Assinada) e uma versão mais
> "técnica"/de implementação (ContatoFeito, PreQualificacao, Conexao,
> ReuniaoAgendada, PropostaEnviada, AguardandoAssinatura, Fechado). A segunda
> foi a adotada no código, por ser a lista mais detalhada e já nomeada no
> formato de enum.

## 3. Funcionalidades por versão (conforme o PDF)

### V1 — implementada neste projeto
- Autenticação e Autorização
- CRUD de Contato
- CRUD de Oportunidade
- Alterar Etapa da Oportunidade
- Marcar Perda ou Ganho da Oportunidade
- Gestão de Tags para Contato

### V2 — não implementada ainda
- Tarefas por Etapa
- Indicadores
- E-mail de notificação de ganho

### V3 — não implementada ainda
- Tarefas de tipos complexos
- Envio de e-mail através de tarefa (modelo pré-pronto)
- Agendamento com integração ao Google Calendário
- Geração de link de pagamento
- Webhooks para ferramentas externas (MailChimp, RD Station, Active Campaign)
- Etapas personalizáveis como entidade

### Pré-requisito citado no PDF
"Se fez o Curso de Blazor" é tratado como **obrigatório** e "curso focado em
APIs" como **opcional, para depois** — ambos relativos ao percurso de
aprendizado do autor, não à arquitetura da API em si (a API atual já é REST
puro, consumível por qualquer front-end, incluindo um futuro front em
Blazor).

## 4. Decisões de Implementação

O escopo do PDF é de domínio/produto; as decisões técnicas abaixo foram
feitas durante a implementação, buscando um projeto didático e de padrão de
mercado:

- **Arquitetura em camadas** (Domain / Application / Infrastructure / Api),
  inspirada em Clean Architecture, para isolar regras de negócio de EF Core
  e ASP.NET.
- **Autenticação**: ASP.NET Identity (gestão de usuários/senhas) + JWT
  Bearer Token (stateless, usado nas chamadas à API).
- **Banco de dados**: SQLite por padrão, para rodar sem nenhuma instalação
  extra; troca simples para SQL Server quando necessário (documentado no
  `README.md`).
- **Tags**: modeladas como entidade própria + tabela de associação N:N
  (`ContatoTag`), permitindo reutilizar tags entre vários contatos.
- **Área de Atuação**: implementada já como entidade (`AreaAtuacao`) com
  endpoints próprios de listagem/criação, conforme sugerido no PDF ("Enum /
  Cadastro indicado, já começando como entidade").
- **Regra de etapa fechada**: a API impede alterar manualmente a etapa de
  uma oportunidade já marcada como Ganha ou Perdida (`Oportunidade.
  AlterarEtapa` lança exceção nesse caso), evitando inconsistência entre
  Status e Etapa.

## 5. Estrutura de Pastas

```
CRMDev.sln
src/
  CRMDev.Domain/          Entidades, enums — sem dependências externas
    Entities/              Contato, Oportunidade, Tag, ContatoTag, AreaAtuacao, BaseEntity
    Enums/                 EtapaOportunidade, StatusOportunidade, RazaoPerda
  CRMDev.Application/     Regras de negócio e contratos
    DTOs/                  Contato*, Oportunidade*, Auth*
    Interfaces/            IContatoRepository, IOportunidadeRepository, IJwtTokenService
    Services/              ContatoService, OportunidadeService
  CRMDev.Infrastructure/  Implementações técnicas
    Data/                  CRMDevDbContext (EF Core + Identity)
    Identity/              ApplicationUser
    Repositories/          Contato/Oportunidade/AreaAtuacao Repository
    Security/              JwtSettings, JwtTokenService
    DependencyInjection.cs Registro de DbContext, Identity, JWT, repositórios e services
  CRMDev.Api/             Camada de apresentação
    Controllers/           Auth, Contatos, Oportunidades, AreasAtuacao
    Program.cs             Pipeline, Swagger, CORS, auto-migrate
```

## 6. Endpoints Implementados

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/auth/registrar` | não | Cria usuário |
| POST | `/api/auth/login` | não | Login, retorna JWT |
| GET  | `/api/contatos?busca=` | sim | Lista contatos (busca por nome/email) |
| GET  | `/api/contatos/{id}` | sim | Detalhe do contato |
| POST | `/api/contatos` | sim | Cria contato |
| PUT  | `/api/contatos/{id}` | sim | Atualiza contato |
| DELETE | `/api/contatos/{id}` | sim | Remove contato |
| POST | `/api/contatos/{id}/tags` | sim | Adiciona tag ao contato |
| DELETE | `/api/contatos/{id}/tags/{nomeTag}` | sim | Remove tag do contato |
| GET  | `/api/areasatuacao` | sim | Lista áreas de atuação |
| POST | `/api/areasatuacao` | sim | Cria área de atuação |
| GET  | `/api/oportunidades?etapa=&status=&contatoId=` | sim | Lista oportunidades (com filtros) |
| GET  | `/api/oportunidades/{id}` | sim | Detalhe da oportunidade |
| POST | `/api/oportunidades` | sim | Cria oportunidade |
| PUT  | `/api/oportunidades/{id}` | sim | Atualiza oportunidade |
| DELETE | `/api/oportunidades/{id}` | sim | Remove oportunidade |
| PATCH | `/api/oportunidades/{id}/etapa` | sim | Altera etapa |
| PATCH | `/api/oportunidades/{id}/ganhar` | sim | Marca como ganha (→ Fechado) |
| PATCH | `/api/oportunidades/{id}/perder` | sim | Marca como perdida, com razão (→ Fechado) |

## 7. Como Rodar

Ver instruções completas em `README.md`. Resumo:

```bash
cd CRMDev
dotnet restore
dotnet run --project src/CRMDev.Api
```

Swagger disponível em `/swagger` após subir a aplicação.

## 8. Roadmap (próximos passos sugeridos)

1. **V2**: criar entidade `Tarefa` vinculada a uma etapa de oportunidade;
   endpoint/dashboard de indicadores (ex.: taxa de conversão por etapa,
   ticket médio, tempo médio por etapa); disparo de e-mail ao marcar ganho
   (ex.: via `IEmailSender` + serviço SMTP ou SendGrid).
2. **V3**: transformar `EtapaOportunidade` de enum em entidade
   configurável por usuário/organização; tipos de tarefa mais ricos
   (e-mail com template, agendamento via Google Calendar API, geração de
   link de pagamento via gateway); endpoints de webhook para receber leads
   de MailChimp/RD Station/Active Campaign.
3. Testes automatizados (unitários para os `Services`, integração para os
   `Controllers`).
4. Front-end (ex.: Blazor, conforme mencionado no PDF como pré-requisito de
   aprendizado do autor) consumindo esta API.