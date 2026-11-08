# Gerenciador de Avaliação de Livros

Sistema de avaliação de livros inspirado no [GoodReads](https://www.goodreads.com/),
desenvolvido em **.NET 8 / C#** e entregue em 4 níveis de evolução, do console até
Arquitetura Limpa com CQRS.

```
01-Basico-I-Console/     Aplicação Console
02-Basico-II-API/        ASP.NET Core API + EF Core (banco em memória)
03-Intermediario-API/    FluentValidation, Repository, Middleware, DTOs, SQLite,
                         Google Books API e upload de capa
04-Avancado-CleanArch/   Clean Architecture, CQRS, Unit of Work, Domain Events,
                         HostedService e testes unitários
```

## Como rodar

Requer o [SDK do .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) e conexão
com a internet na primeira execução (para restaurar os pacotes NuGet).

```bash
# Nível Básico I - Console
cd 01-Basico-I-Console
dotnet run --project src/Livros.Console

# Nível Básico II - API com banco em memória
cd 02-Basico-II-API
dotnet run --project src/Livros.Api

# Nível Intermediário - API com SQLite
cd 03-Intermediario-API
dotnet run --project src/Livros.Intermediario

# Nível Avançado - Clean Architecture
cd 04-Avancado-CleanArch
dotnet run --project src/Livros.Api
dotnet test                     # roda os testes unitários
```

O endereço aparece no console ao subir a aplicação. Nos níveis II, III e IV a raiz
(`/`) redireciona para o Swagger, onde dá para testar tudo pelo navegador.

> Os níveis III e IV criam o arquivo SQLite automaticamente na primeira execução
> (`EnsureCreated`) e já semeiam 3 livros, 2 usuários e 3 avaliações de exemplo.

## Funcionalidades

| Funcionalidade | Básico I | Básico II | Intermediário | Avançado |
|---|---|---|---|---|
| CRUD de livros com validação | ✅ | ✅ | ✅ | ✅ |
| CRUD de usuários com validação | ✅ | ✅ | ✅ | ✅ |
| Detalhes do usuário com suas avaliações | ✅ | ✅ | ✅ | ✅ |
| Cadastro e listagem de avaliações | ✅ | ✅ | ✅ | ✅ |
| Nota média recalculada a cada avaliação | ✅ | ✅ | ✅ | ✅ |
| Consulta em API externa (Google Books) | ✅ | — | ✅ | ✅ |
| Importar livro da API externa | ✅ | — | ✅ | ✅ |
| Upload de imagem de capa | ✅ | — | ✅ | ✅ |
| Relatório de livros lidos no ano | ✅ | ✅ | ✅ | ✅ |
| Distribuição de notas (1 a 5) por livro | — | — | ✅ | ✅ |
| Conferência periódica das notas médias | — | — | — | ✅ |

## Regras de negócio implementadas

| Regra | Onde fica |
|---|---|
| Data de início não pode ser maior que a de fim de leitura | Value Object `PeriodoLeitura` (Avançado); construtor de `Avaliacao` nos demais |
| Livros não podem ser cadastrados com o mesmo ISBN | Validação na aplicação + índice único no banco |
| Nota deve ser de 1 a 5 | Construtor de `Avaliacao` + FluentValidation |

Regras adicionais que o domínio impõe por consistência:

- Um usuário não avalia o mesmo livro duas vezes (índice único composto no banco).
- Um usuário com avaliações não é removido, para não apagar o histórico de notas.
- E-mail de usuário é único.
- A data de fim de leitura não pode estar no futuro.
- ISBN deve ter 10 ou 13 dígitos; `X` só é aceito como último dígito de um ISBN-10.
- Capa aceita apenas jpg, jpeg, png, gif, webp e bmp, com até 5MB.

Nos níveis Intermediário e Avançado as regras vivem **dentro das entidades de
domínio**, não nos controllers: a entidade nunca entra em estado inválido.

### Sobre o ISBN

O ISBN é comparado pela forma normalizada (só letras e dígitos, em maiúsculas), então
`978-0321125217` e `9780321125217` são detectados como o mesmo livro. No nível Avançado
essa forma normalizada é persistida em coluna própria com índice único, de modo que a
garantia vale no banco e não apenas no código.

## Entidades

```
Livro         Id, Titulo, Descricao, ISBN, Autor, Editora, Genero,
              AnoDePublicacao, QuantidadePaginas, DataCriacao,
              NotaMedia, CapaLivro (bytes), Avaliacoes

Avaliacao     Id, Nota, Descricao, IdUsuario, IdLivro, DataCriacao,
              DataInicioLeitura, DataFimLeitura

Usuario       Id, Email, Nome, Avaliacoes
```

`GeneroLivroEnum` cobre Romance, Ficção, Ficção Científica, Fantasia, Suspense, Terror,
Biografia, História, Técnico, Autoajuda, Infantil, Poesia e Outro. É persistido como
texto no banco (`HasConversion<string>()`) para que as tabelas fiquem legíveis.

> **Nota sobre as datas de leitura:** a especificação pede a regra "data de início não
> pode ser maior que fim de leitura", mas a entidade `Avaliacao` descrita não traz esses
> campos. Foram adicionados `DataInicioLeitura` e `DataFimLeitura` — sem eles a regra não
> teria onde ser aplicada, e o relatório de livros lidos no ano precisa saber quando cada
> leitura terminou.

## Endpoints (níveis II, III e IV)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/livros?busca=` | Lista livros, filtrando por título, autor ou editora |
| GET | `/api/livros/{id}` | Detalhes do livro com avaliações |
| POST | `/api/livros` | Cadastra um livro |
| PUT | `/api/livros/{id}` | Atualiza um livro |
| DELETE | `/api/livros/{id}` | Remove um livro |
| POST | `/api/livros/{id}/capa` | Envia a imagem de capa (multipart/form-data) |
| GET | `/api/livros/{id}/capa` | Devolve a imagem de capa |
| GET | `/api/livros/consulta-externa?isbn=` | Consulta o Google Books |
| POST | `/api/livros/importar` | Importa um livro do Google Books pelo ISBN |
| GET | `/api/usuarios` | Lista usuários |
| GET | `/api/usuarios/{id}` | Detalhes do usuário com as avaliações dele |
| POST | `/api/usuarios` | Cadastra um usuário |
| PUT | `/api/usuarios/{id}` | Atualiza um usuário |
| DELETE | `/api/usuarios/{id}` | Remove um usuário sem avaliações |
| GET | `/api/livros/{idLivro}/avaliacoes` | Lista as avaliações de um livro |
| POST | `/api/livros/{idLivro}/avaliacoes` | Cadastra uma avaliação |
| GET | `/api/relatorios/livros-lidos/{ano}` | Livros lidos no ano |
| GET | `/api/relatorios/livros-lidos` | Livros lidos no ano corrente |

As rotas de capa e de consulta externa existem a partir do nível Intermediário.

## Arquitetura do nível Avançado

```
Livros.Domain          Entidades, regras de negócio, eventos de domínio,
                       Value Objects (Isbn, Email, PeriodoLeitura) e as
                       interfaces dos repositórios. Não referencia nenhum
                       outro projeto nem biblioteca externa.

Livros.Application     Commands e Queries (CQRS com MediatR), handlers dos
                       eventos de domínio, validadores e pipeline behaviors.
                       Referencia apenas o Domain.

Livros.Infrastructure  EF Core, IEntityTypeConfiguration, repositórios,
                       Unit of Work, integração Google Books, validação de
                       imagem e o HostedService.

Livros.Api             Controllers finos, middleware de exceções e Swagger.
```

As dependências apontam sempre para dentro: a API conhece a infraestrutura, a
infraestrutura conhece a aplicação, e todas conhecem o domínio — mas o domínio não
conhece ninguém.

### Fluxo de uma avaliação

```
POST /api/livros/1/avaliacoes
  └─ CadastrarAvaliacaoCommand
       └─ ValidationBehavior              valida o command (FluentValidation)
       └─ CadastrarAvaliacaoCommandHandler
            └─ livro.AdicionarAvaliacao() aplica nota 1-5, período de leitura
                                          e avaliação duplicada
                 └─ PeriodoLeitura.Criar() valida início <= fim
                 └─ dispara AvaliacaoCadastradaEvent
            └─ UnitOfWork.CommitAsync()   salva e despacha os eventos
                 └─ AvaliacaoCadastradaEventHandler
                      └─ livro.RecalcularNotaMedia()
                           └─ dispara NotaMediaAtualizadaEvent (se a média mudou)
                                └─ NotaMediaAtualizadaEventHandler  registra a variação
```

O command de avaliação não calcula média, e o cálculo da média não sabe que existe um
endpoint de avaliação: a ligação é feita pelos eventos de domínio. O `HostedService`
varre o catálogo a cada 5 minutos e corrige qualquer livro cuja média persistida tenha
divergido das avaliações.

### Testes

```bash
cd 04-Avancado-CleanArch
dotnet test
```

Cobrem as três regras de negócio pedidas (nota 1 a 5, ISBN único, início ≤ fim) com os
casos de borda — nota 0 e 6, datas iguais, ISBN de 10 e 13 dígitos, `X` no lugar certo e
no lugar errado — além do recálculo da nota média, dos eventos de domínio e dos handlers
de CQRS com mocks dos repositórios e da Unit of Work.

## Exemplos de requisição

Cadastrar um livro:

```bash
curl -X POST https://localhost:7001/api/livros \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Domain-Driven Design",
    "descricao": "Tackling complexity in the heart of software.",
    "isbn": "978-0321125217",
    "autor": "Eric Evans",
    "editora": "Addison-Wesley",
    "genero": "Tecnico",
    "anoDePublicacao": 2003,
    "quantidadePaginas": 560
  }'
```

Cadastrar uma avaliação:

```bash
curl -X POST https://localhost:7001/api/livros/1/avaliacoes \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 1,
    "nota": 5,
    "descricao": "Mudou a forma como penso modelagem de software.",
    "dataInicioLeitura": "2026-09-01",
    "dataFimLeitura": "2026-10-15"
  }'
```

Enviar a imagem de capa:

```bash
curl -X POST https://localhost:7001/api/livros/1/capa \
  -F "arquivo=@capa.jpg"
```

Importar um livro do Google Books:

```bash
curl -X POST https://localhost:7001/api/livros/importar \
  -H "Content-Type: application/json" \
  -d '{ "isbn": "9780132350884" }'
```

Resposta quando uma regra é violada:

```json
{
  "titulo": "Regra de negócio violada",
  "status": 400,
  "caminho": "/api/livros/1/avaliacoes",
  "erros": [
    "A data de início da leitura (15/10/2026) não pode ser maior que a data de fim (01/10/2026)."
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
- API externa [Google Books](https://developers.google.com/books/docs/v1/using) para
  consulta de livros por ISBN ou termo
