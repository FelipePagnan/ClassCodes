# Sistema de Banco de Dados de Doação de Sangue

Projeto desenvolvido em **.NET 8 / C#**, entregue em 4 níveis de evolução, do console
até Arquitetura Limpa com CQRS.

```
01-Basico-I-Console/     Aplicação Console
02-Basico-II-API/        ASP.NET Core API + EF Core (banco em memória)
03-Intermediario-API/    FluentValidation, Repository, Middleware, DTOs, SQLite, ViaCEP
04-Avancado-CleanArch/   Clean Architecture, CQRS, Unit of Work, Domain Events,
                         HostedService e testes unitários
```

## Como rodar

Requer o [SDK do .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) e conexão
com a internet na primeira execução (para restaurar os pacotes NuGet).

```bash
# Nível Básico I - Console
cd 01-Basico-I-Console
dotnet run --project src/BancoSangue.Console

# Nível Básico II - API com banco em memória
cd 02-Basico-II-API
dotnet run --project src/BancoSangue.Api
# Swagger: https://localhost:7xxx/swagger

# Nível Intermediário - API com SQLite
cd 03-Intermediario-API
dotnet run --project src/BancoSangue.Intermediario

# Nível Avançado - Clean Architecture
cd 04-Avancado-CleanArch
dotnet run --project src/BancoSangue.Api
dotnet test                     # roda os testes unitários
```

O endereço exato aparece no console ao subir a aplicação. Nos níveis II, III e IV a
raiz (`/`) e a rota `/swagger` abrem a documentação interativa da API.

> Os níveis III e IV criam o arquivo SQLite automaticamente na primeira execução
> (`EnsureCreated`), já com as 8 combinações de tipo sanguíneo semeadas no estoque.

## Funcionalidades

| Funcionalidade | Básico I | Básico II | Intermediário | Avançado |
|---|---|---|---|---|
| Cadastro de doadores com validação | ✅ | ✅ | ✅ | ✅ |
| Consulta de CEP via API externa (ViaCEP) | ✅ | — | ✅ | ✅ |
| Controle de estoque de sangue | ✅ | ✅ | ✅ | ✅ |
| Aviso de estoque abaixo do mínimo | ✅ | ✅ | ✅ | ✅ |
| Registro de doações atualizando o estoque | ✅ | ✅ | ✅ | ✅ |
| Histórico de doações por doador | ✅ | ✅ | ✅ | ✅ |
| Relatório de total por tipo sanguíneo | ✅ | ✅ | ✅ | ✅ |
| Relatório de doações dos últimos 30 dias | ✅ | ✅ | ✅ | ✅ |
| Monitoramento periódico do estoque | — | — | — | ✅ |

## Regras de negócio implementadas

| Regra | Onde fica |
|---|---|
| Não permitir dois doadores com o mesmo e-mail | Validação na aplicação + índice único no banco |
| Menor de idade pode se cadastrar, mas não pode doar | Cadastro aceita; `GarantirQuePodeDoar` bloqueia a doação |
| Peso mínimo de 50 kg para doar | `Doador.PesoMinimoKg` |
| Mulheres podem doar a cada 90 dias | `Doador.IntervaloDiasFeminino` |
| Homens podem doar a cada 60 dias | `Doador.IntervaloDiasMasculino` |
| Doação entre 420 ml e 470 ml | Construtor de `Doacao` |
| Estoque mínimo de 2.000 ml por tipo | `EstoqueSangue.QuantidadeMinimaML` |

Nos níveis Intermediário e Avançado as regras vivem **dentro das entidades de
domínio**, não nos controllers: a entidade nunca entra em estado inválido.

## Entidades

```
Doador            Id, NomeCompleto, Email, DataNascimento, Genero, Peso,
                  TipoSanguineo, FatorRh, Doacoes, Endereco
Endereco          Id, Logradouro, Cidade, Estado, CEP, Doador
Doacao            Id, DoadorId, DataDoacao, QuantidadeML, Doador
EstoqueSangue     Id, TipoSanguineo, FatorRh, QuantidadeML
```

`Genero`, `TipoSanguineo` e `FatorRh` são enums, persistidos como texto no banco
(`HasConversion<string>()`) para que as tabelas fiquem legíveis.

## Endpoints (níveis II, III e IV)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/doadores` | Lista os doadores |
| GET | `/api/doadores/{id}` | Detalhe do doador com histórico |
| GET | `/api/doadores/{id}/doacoes` | Histórico de doações do doador |
| POST | `/api/doadores` | Cadastra um doador (informe `cep` para buscar o endereço) |
| GET | `/api/doacoes` | Lista as doações |
| POST | `/api/doacoes` | Registra uma doação e atualiza o estoque |
| GET | `/api/estoque` | Estoque atual por tipo sanguíneo |
| GET | `/api/estoque/alertas` | Tipos com estoque abaixo do mínimo |
| GET | `/api/relatorios/estoque-por-tipo` | Total de sangue por tipo |
| GET | `/api/relatorios/doacoes-ultimos-30-dias` | Doações dos últimos 30 dias |

No nível Básico II a rota de alertas é `/api/estoque/baixo`.

## Arquitetura do nível Avançado

```
BancoSangue.Domain          Entidades, regras de negócio, eventos de domínio,
                            Value Objects e as interfaces dos repositórios.
                            Não referencia nenhum outro projeto nem biblioteca externa.

BancoSangue.Application     Commands e Queries (CQRS com MediatR), handlers dos
                            eventos de domínio, validadores e pipeline behaviors.
                            Referencia apenas o Domain.

BancoSangue.Infrastructure  EF Core, IEntityTypeConfiguration, repositórios,
                            Unit of Work, integração ViaCEP e o HostedService.

BancoSangue.Api             Controllers finos, middleware de exceções e Swagger.
```

As dependências apontam sempre para dentro: a API conhece a infraestrutura, a
infraestrutura conhece a aplicação, e todas conhecem o domínio — mas o domínio não
conhece ninguém.

### Fluxo de uma doação

```
POST /api/doacoes
  └─ RegistrarDoacaoCommand
       └─ ValidationBehavior            valida o command (FluentValidation)
       └─ RegistrarDoacaoCommandHandler
            └─ doador.RegistrarDoacao() aplica idade, peso, intervalo e quantidade
                 └─ dispara DoacaoRegistradaEvent
            └─ UnitOfWork.CommitAsync() salva e despacha os eventos
                 └─ DoacaoRegistradaEventHandler    credita o estoque
                      └─ estoque.Adicionar()
                           └─ dispara EstoqueAbaixoDoMinimoEvent (se aplicável)
                                └─ EstoqueAbaixoDoMinimoEventHandler  emite o alerta
```

O command de doação não sabe que existe estoque, e o estoque não sabe que existe
doação: a ligação é feita pelos eventos de domínio.

### Testes

```bash
cd 04-Avancado-CleanArch
dotnet test
```

Cobrem as regras de negócio no domínio (idade, peso, intervalo por gênero,
quantidade em ml, alerta de estoque) e os handlers de CQRS com mocks dos
repositórios, da Unit of Work e da API de CEP.

## Exemplos de requisição

Cadastrar um doador com busca de endereço pelo CEP:

```bash
curl -X POST https://localhost:7001/api/doadores \
  -H "Content-Type: application/json" \
  -d '{
    "nomeCompleto": "Maria Oliveira",
    "email": "maria.oliveira@example.com",
    "dataNascimento": "1995-03-22",
    "genero": "Feminino",
    "peso": 62,
    "tipoSanguineo": "A",
    "fatorRh": "Positivo",
    "cep": "01001-000",
    "complemento": "apto 42"
  }'
```

Registrar uma doação:

```bash
curl -X POST https://localhost:7001/api/doacoes \
  -H "Content-Type: application/json" \
  -d '{ "doadorId": 1, "quantidadeML": 450 }'
```

Resposta quando uma regra é violada:

```json
{
  "titulo": "Regra de negócio violada",
  "status": 400,
  "caminho": "/api/doacoes",
  "erros": [
    "'Maria Oliveira' doou em 15/10/2026. Faltam 66 dia(s) para completar o intervalo mínimo de 90 dias (mulheres)."
  ]
}
```

## Stack

- .NET 8 / C# 12
- ASP.NET Core Web API + Swagger (Swashbuckle)
- Entity Framework Core 8 (InMemory no nível II, SQLite nos níveis III e IV)
- FluentValidation
- MediatR (CQRS e eventos de domínio)
- xUnit, FluentAssertions e Moq
- API externa ViaCEP para consulta de endereço
