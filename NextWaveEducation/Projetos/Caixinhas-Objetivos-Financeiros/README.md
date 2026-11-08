# Gerenciador de Objetivos Financeiros

Sistema de gerenciamento de objetivos financeiros inspirado nas **caixinhas do
Nubank**, desenvolvido em **.NET 8 / C#** e entregue em 4 níveis de evolução, do
console até Arquitetura Limpa com CQRS.

```
01-Basico-I-Console/     Aplicação Console
02-Basico-II-API/        ASP.NET Core API + EF Core (banco em memória)
03-Intermediario-API/    FluentValidation, Repository, Middleware, DTOs, SQLite,
                         soft delete por query filter, simulação e upload de capa
04-Avancado-CleanArch/   Clean Architecture, CQRS, Unit of Work com transação de
                         banco, Domain Events, HostedService e testes unitários
```

## Como rodar

Requer o [SDK do .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) e conexão
com a internet na primeira execução (para restaurar os pacotes NuGet).

```bash
# Nível Básico I - Console
cd 01-Basico-I-Console
dotnet run --project src/Caixinhas.Console

# Nível Básico II - API com banco em memória
cd 02-Basico-II-API
dotnet run --project src/Caixinhas.Api

# Nível Intermediário - API com SQLite
cd 03-Intermediario-API
dotnet run --project src/Caixinhas.Intermediario

# Nível Avançado - Clean Architecture
cd 04-Avancado-CleanArch
dotnet run --project src/Caixinhas.Api
dotnet test                     # roda os testes unitários
```

O endereço aparece no console ao subir a aplicação. Nos níveis II, III e IV a raiz
(`/`) redireciona para o Swagger, onde dá para testar tudo pelo navegador.

> Os níveis III e IV criam o arquivo SQLite automaticamente na primeira execução
> (`EnsureCreated`) e já semeiam 3 caixinhas com transações de exemplo.

## Funcionalidades

| Funcionalidade | Básico I | Básico II | Intermediário | Avançado |
|---|---|---|---|---|
| CRUD de caixinhas com validação | ✅ | ✅ | ✅ | ✅ |
| CRUD de transações com validação | ✅ | ✅ | ✅ | ✅ |
| `ValorTotal` calculado no cadastro da transação | ✅ | ✅ | ✅ | ✅ |
| Remoção lógica (`EstaDeletado`) | ✅ | ✅ | ✅ | ✅ |
| Pausar / retomar / cancelar objetivo | ✅ | ✅ | ✅ | ✅ |
| Extrato com saldo corrido | ✅ | — | ✅ | ✅ |
| Simulação de evolução com aportes e juros | ✅ | — | ✅ | ✅ |
| Upload de imagem de capa | ✅ | — | ✅ | ✅ |
| Relatório de evolução das caixinhas | ✅ | — | ✅ | ✅ |
| Conferência periódica dos saldos | — | — | — | ✅ |
| Aviso de prazo vencido | — | — | — | ✅ |

## Regras de negócio implementadas

| Regra | Onde fica |
|---|---|
| Transação com até 2 casas decimais de precisão | Value Object `Dinheiro` (níveis III e IV); validação estática nos demais |
| Transação não pode ser negativa | Mesmo Value Object `Dinheiro` |
| Tipo: Deposit (Depósito) / Withdraw (Saque) | `TipoTransacao` + propriedade `Efeito` da transação |

Regras adicionais que o domínio impõe por consistência:

- Um saque não pode deixar a caixinha com saldo negativo.
- A data da transação pode ser hoje ou passada, nunca futura, e não pode ser anterior
  à criação da caixinha.
- Caixinha completa, pausada ou cancelada não aceita novas transações.
- Caixinha com saldo não pode ser removida (saque antes).
- Remover uma transação que deixaria o saldo negativo é bloqueado.
- A meta não pode ser reduzida para menos do que já foi guardado.
- O prazo, quando informado, tem que ser uma data futura.
- O status vira `Completo` sozinho ao bater a meta e volta a `EmProgresso` se o saldo
  cair (por exemplo após um saque ou aumento da meta).
- Capa aceita apenas jpg, jpeg, png, gif, webp e bmp, com até 5MB.

Nos níveis Intermediário e Avançado as regras vivem **dentro das entidades de
domínio**, não nos controllers: a entidade nunca entra em estado inválido.

### Sobre as duas casas decimais

A regra é verificada com `decimal.Round(valor, 2) != valor`, que compara **por
valor** e não por escala. Então `10.500` é aceito (vale 10,50) e `10.555` é recusado.
Nos níveis III e IV isso vive no Value Object `Dinheiro`: um valor inválido não
consegue ser construído, e as colunas são declaradas com `HasPrecision(18, 2)` para
que o banco carregue a mesma garantia.

### Sobre o `ValorTotal` (requisito PLUS)

O saldo **não** é recalculado somando as transações a cada consulta. Ele é uma coluna
persistida, atualizada pela entidade no momento em que a transação é registrada:

```csharp
_transacoes.Add(transacao);
ValorTotal = Dinheiro.Criar(saldoAnterior + transacao.Efeito, "saldo").Valor;
ReavaliarStatus(statusAnterior);
```

Isso cria um risco: se a transação for gravada mas o `ValorTotal` não, o saldo passa a
mentir. Seguindo a dica do enunciado, as duas gravações acontecem **dentro de uma
transação de banco**:

- **Intermediário:** `ObjetivoRepository.SalvarComTransacaoAsync` abre a transação,
  salva e dá commit; qualquer falha gera rollback.
- **Avançado:** `UnitOfWork.ExecutarEmTransacaoAsync` envolve o handler inteiro, e o
  `HostedService` ainda confere os saldos a cada 5 minutos como rede de segurança
  (`ObjetivoFinanceiro.ConferirValorTotal`).

### Sobre o soft delete

`EstaDeletado` nunca é filtrado manualmente nas queries. O EF Core recebe um
**query filter global**:

```csharp
builder.HasQueryFilter(o => !o.EstaDeletado);
builder.HasQueryFilter(t => !t.EstaDeletado);
```

Com isso, objetivos e transações removidos saem de todas as consultas e também dos
`Include`, sem nenhum `Where` espalhado pelo código. Remover uma caixinha marca as
transações dela em cascata.

## Entidades

```
ObjetivoFinanceiro   Id, Titulo, QuantidadeAlvo, Prazo?, QuantidadeIdealAporteMensal?,
                     Status, Transacoes, DataCriacao, EstaDeletado,
                     ValorTotal (PLUS), CapaObjetivo (PLUS)

Transacao            Id, Quantidade, Tipo, DataTransacao, DataCriacao,
                     EstaDeletado, ObjetivoFinanceiroId
```

`StatusObjetivo` cobre `EmProgresso`, `Completo`, `Cancelado` e `Pausado`.
`TipoTransacao` cobre `Deposito` e `Saque`. Os dois são persistidos como texto
(`HasConversion<string>()`) para que as tabelas fiquem legíveis.

> **Campos adicionados:** `ValorTotal` e `CapaObjetivo` no objetivo (os dois pedidos
> como PLUS) e `ObjetivoFinanceiroId` na transação, necessário para a chave estrangeira.

## Endpoints (níveis II, III e IV)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/objetivos?status=` | Lista as caixinhas ativas |
| GET | `/api/objetivos/{id}` | Detalhes com transações e resumo |
| POST | `/api/objetivos` | Cria uma caixinha |
| PUT | `/api/objetivos/{id}` | Atualiza uma caixinha |
| DELETE | `/api/objetivos/{id}` | Remoção lógica |
| PATCH | `/api/objetivos/{id}/pausar` | Pausa a caixinha |
| PATCH | `/api/objetivos/{id}/retomar` | Retoma a caixinha |
| PATCH | `/api/objetivos/{id}/cancelar` | Cancela a caixinha |
| POST | `/api/objetivos/{id}/capa` | Envia a imagem de capa |
| GET | `/api/objetivos/{id}/capa` | Devolve a imagem de capa |
| POST | `/api/objetivos/{id}/simular` | Simula a evolução com aportes e juros |
| GET | `/api/objetivos/{id}/transacoes` | Lista as transações |
| GET | `/api/objetivos/{id}/transacoes/{idT}` | Detalhes de uma transação |
| POST | `/api/objetivos/{id}/transacoes` | Cadastra uma transação |
| DELETE | `/api/objetivos/{id}/transacoes/{idT}` | Remoção lógica da transação |
| GET | `/api/objetivos/{id}/transacoes/extrato` | Extrato com saldo corrido |
| GET | `/api/relatorios/evolucao?mesesDeHistorico=6` | Relatório de evolução |

As rotas de capa, simulação, extrato e relatório existem a partir do nível
Intermediário.

## Arquitetura do nível Avançado

```
Caixinhas.Domain          Entidades, regras de negócio, eventos de domínio, o Value
                          Object Dinheiro e as interfaces dos repositórios. Não
                          referencia nenhum outro projeto nem biblioteca externa.

Caixinhas.Application     Commands e Queries (CQRS com MediatR), handlers dos eventos
                          de domínio, validadores e pipeline behaviors.
                          Referencia apenas o Domain.

Caixinhas.Infrastructure  EF Core, IEntityTypeConfiguration, repositórios, Unit of
                          Work com transação de banco, validação de imagem e o
                          HostedService.

Caixinhas.Api             Controllers finos, middleware de exceções e Swagger.
```

As dependências apontam sempre para dentro: a API conhece a infraestrutura, a
infraestrutura conhece a aplicação, e todas conhecem o domínio — mas o domínio não
conhece ninguém.

### Fluxo de uma transação

```
POST /api/objetivos/1/transacoes
  └─ CadastrarTransacaoCommand
       └─ ValidationBehavior                valida o command (FluentValidation)
       └─ CadastrarTransacaoCommandHandler
            └─ UnitOfWork.ExecutarEmTransacaoAsync   abre a transação de banco
                 └─ objetivo.RegistrarTransacao()
                      └─ new Transacao(...)          valida valor, tipo e data
                           └─ Dinheiro.CriarPositivo()  2 casas decimais, não negativo
                      └─ confere saldo para saque
                      └─ ValorTotal += Efeito        o saldo é atualizado AQUI
                      └─ dispara TransacaoRegistradaEvent
                      └─ dispara MetaAlcancadaEvent (se bateu a meta)
                 └─ UnitOfWork.CommitAsync()         salva e despacha os eventos
                      └─ TransacaoRegistradaEventHandler   confere a variação do saldo
                      └─ MetaAlcancadaEventHandler         notifica o usuário
                 └─ COMMIT                           transação e saldo entram juntos
```

Se qualquer etapa falhar, o rollback desfaz tudo — a transação não é gravada e o
saldo não muda.

### Testes

```bash
cd 04-Avancado-CleanArch
dotnet test
```

Cobrem as regras de negócio pedidas com os casos de borda:

- **Duas casas decimais:** `100`, `100.5`, `100.55` e `0.01` passam; `100.555`,
  `0.001` e `10.1234` falham; `10.500` passa (vale 10,50).
- **Não negativo:** `-0.01`, `-100` e `0` recusados em transação.
- **Depósito e saque:** efeito `+` e `−` no saldo, saque acima do saldo bloqueado,
  saque exato do saldo permitido.
- **`ValorTotal`:** confere que o valor incremental bate com a soma das transações.
- **Soft delete:** remover transação ajusta o saldo, o registro continua na coleção
  marcado, e remover um depósito que deixaria saldo negativo é bloqueado.
- **Simulação:** aritmética de juros compostos verificada mês a mês.
- **Handlers de CQRS:** com mocks dos repositórios e da Unit of Work, incluindo a
  verificação de que o cadastro roda dentro de `ExecutarEmTransacaoAsync`.

## Exemplos de requisição

Criar uma caixinha:

```bash
curl -X POST https://localhost:7001/api/objetivos \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Viagem para o Japão",
    "quantidadeAlvo": 20000.00,
    "prazo": "2028-05-01",
    "quantidadeIdealAporteMensal": 1200.00
  }'
```

Registrar um depósito:

```bash
curl -X POST https://localhost:7001/api/objetivos/1/transacoes \
  -H "Content-Type: application/json" \
  -d '{ "quantidade": 1500.50, "tipo": "Deposito", "dataTransacao": "2026-11-08" }'
```

Simular a evolução:

```bash
curl -X POST https://localhost:7001/api/objetivos/1/simular \
  -H "Content-Type: application/json" \
  -d '{ "aporteMensal": 1200.00, "quantidadeMeses": 18, "taxaJurosMensalPercentual": 0.80 }'
```

Resposta quando uma regra é violada:

```json
{
  "titulo": "Regra de negócio violada",
  "status": 400,
  "caminho": "/api/objetivos/1/transacoes",
  "erros": [
    "O valor da transação deve ter no máximo 2 casas decimais. Valor informado: 100.555."
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
