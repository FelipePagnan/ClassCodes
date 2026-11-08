# Projeto 3 — Gerenciador de Avaliação de Livros

## 1. Visão Geral

**Projeto:** Sistema de Gerenciamento e Avaliação de Livros.

Desenvolver um sistema de avaliação de livros, baseado na plataforma GoodReads, permitindo cadastrar livros, gerenciar usuários, registrar avaliações e consultar informações sobre as obras.

O projeto poderá ser desenvolvido como uma API REST ou uma aplicação Full Stack.

O sistema deverá permitir que usuários avaliem livros, atribuam notas, escrevam comentários e acompanhem suas avaliações.

---

## 2. Requisitos Funcionais

### Gerenciamento de Livros

- [ ] Cadastrar livros.
- [ ] Atualizar informações dos livros.
- [ ] Remover livros.
- [ ] Listar todos os livros.
- [ ] Consultar detalhes de um livro.
- [ ] Validar os dados informados.
- [ ] Criar um endpoint para integração com API externa de consulta de livros (PLUS).
- [ ] Criar um endpoint para upload de imagem da capa do livro (PLUS).

### Gerenciamento de Usuários

- [ ] Cadastrar usuários.
- [ ] Atualizar informações dos usuários.
- [ ] Remover usuários.
- [ ] Listar usuários.
- [ ] Consultar detalhes de um usuário.
- [ ] Consultar as avaliações realizadas por determinado usuário.
- [ ] Validar os dados informados.

### Gerenciamento de Avaliações

- [ ] Cadastrar uma avaliação para determinado livro.
- [ ] Listar as avaliações de determinado livro.
- [ ] Associar cada avaliação a um usuário.
- [ ] Associar cada avaliação a um livro.
- [ ] Validar os dados informados.
- [ ] Garantir que a nota esteja entre 1 e 5.

### Relatórios

- [ ] Gerar relatório com a quantidade de livros lidos durante determinado ano (PLUS).

---

## 3. Regras de Negócio

### Regras obrigatórias

- [ ] A data de início da leitura não pode ser maior que a data de fim da leitura durante o cadastro de uma avaliação.
- [ ] Não permitir cadastrar livros com o mesmo ISBN.
- [ ] A nota de uma avaliação deve estar entre 1 e 5.

---

## 4. Entidades e Dados

### Livro

| Propriedade | Tipo |
|---|---|
| Id | int |
| Titulo | string |
| Descricao | string |
| ISBN | string |
| Autor | string |
| Editora | string |
| Genero | Enum GeneroLivroEnum |
| AnoDePublicacao | int |
| QuantidadePaginas | int |
| DataCriacao | DateTime |
| NotaMedia | decimal (PLUS) |
| CapaLivro | byte[] (PLUS) |
| Avaliacoes | List<Avaliacao> |

**Observações:**

- `NotaMedia`: a cada cadastro de avaliação, poderá ser calculada a média das notas e o campo atualizado automaticamente.
- `CapaLivro`: a imagem poderá ser armazenada diretamente como uma sequência de bytes. Embora não seja a abordagem ideal para cenários reais de maior escala, atende perfeitamente à proposta deste projeto.

### Avaliacao

| Propriedade | Tipo |
|---|---|
| Id | int |
| Nota | int (1 a 5) |
| Descricao | string |
| IdUsuario | int |
| IdLivro | int |
| DataCriacao | DateTime |

### Usuario

| Propriedade | Tipo |
|---|---|
| Id | int |
| Email | string |
| Nome | string |
| Avaliacoes | List<Avaliacao> |

---

## 5. Stack Tecnológica e Padrões de Arquitetura

O projeto será dividido em três níveis de evolução.

### Nível Básico I — Aplicação Console

**Objetivo:** Desenvolver a lógica principal do sistema utilizando C# e Programação Orientada a Objetos.

- [ ] Criar aplicação Console.
- [ ] Criar as entidades.
- [ ] Implementar cadastro de livros.
- [ ] Implementar atualização e remoção de livros.
- [ ] Implementar listagem e consulta de livros.
- [ ] Implementar cadastro de usuários.
- [ ] Implementar cadastro de avaliações.
- [ ] Implementar validações básicas.

### Nível Básico II — API ASP.NET Core

**Objetivo:** Transformar a aplicação Console em uma API REST utilizando os recursos fundamentais do ecossistema .NET.

Tecnologias e conceitos:

- [ ] ASP.NET Core API.
- [ ] Entity Framework Core.
- [ ] Programação Orientada a Objetos (POO).
- [ ] Banco de dados em memória.

### Nível Intermediário

**Objetivo:** Melhorar a organização, manutenção, validação e persistência dos dados da aplicação.

Tecnologias e padrões:

- [ ] FluentValidation — Validação de dados.
- [ ] Repository Pattern — Padrão de repositório.
- [ ] Middleware — Tratamento global de exceções.
- [ ] InputModel — Modelos de entrada.
- [ ] ViewModel — Modelos de apresentação.
- [ ] DTOs — Objetos de transferência de dados.
- [ ] IEntityTypeConfiguration — Mapeamento individual das entidades.
- [ ] SQL Server ou SQLite — Banco de dados relacional.

### Nível Avançado

**Objetivo:** Evoluir a arquitetura da aplicação, implementando padrões avançados de desenvolvimento, processamento e testes.

Tecnologias e padrões:

- [ ] Unit of Work.
- [ ] HostedService.
- [ ] Domain Events.
- [ ] CQRS.
- [ ] Testes Unitários.
- [ ] Arquitetura Limpa (Clean Architecture).

---

## 6. Publicação no LinkedIn

Criar uma publicação apresentando o início do projeto.

### Estrutura sugerida

1. Cumprimentar a rede.
2. Comunicar que está iniciando um projeto como parte do treinamento.
3. Marcar o responsável pelo treinamento: [Luis Deol](https://www.linkedin.com/in/luisdeol/).
4. Apresentar o projeto e explicar brevemente seus objetivos.
5. Concluir informando que publicará na rede as evoluções e novidades do desenvolvimento.

**Objetivo:** Compartilhar o aprendizado, documentar a evolução técnica e acompanhar o desenvolvimento do projeto publicamente.

---

## 7. Controle de Evolução

### Nível Básico I — Console

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar aplicação Console | ⬜ Pendente |
| 2 | Criar entidades | ⬜ Pendente |
| 3 | Cadastro de livros | ⬜ Pendente |
| 4 | Atualização e remoção de livros | ⬜ Pendente |
| 5 | Listagem e consulta de livros | ⬜ Pendente |
| 6 | Cadastro de usuários | ⬜ Pendente |
| 7 | Cadastro de avaliações | ⬜ Pendente |
| 8 | Implementar validações | ⬜ Pendente |

### Nível Básico II — API

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar API ASP.NET Core | ⬜ Pendente |
| 2 | Implementar Entity Framework Core | ⬜ Pendente |
| 3 | Configurar banco em memória | ⬜ Pendente |
| 4 | Implementar endpoints de livros | ⬜ Pendente |
| 5 | Implementar endpoints de usuários | ⬜ Pendente |
| 6 | Implementar endpoints de avaliações | ⬜ Pendente |
| 7 | Integrar funcionalidades | ⬜ Pendente |

### Nível Intermediário

| Etapa | Descrição | Status |
|---|---|---|
| 1 | FluentValidation | ⬜ Pendente |
| 2 | Repository Pattern | ⬜ Pendente |
| 3 | Middleware de exceções | ⬜ Pendente |
| 4 | InputModel, ViewModel e DTOs | ⬜ Pendente |
| 5 | IEntityTypeConfiguration | ⬜ Pendente |
| 6 | SQL Server ou SQLite | ⬜ Pendente |
| 7 | Integração com API externa de livros | ⬜ Pendente |
| 8 | Upload de capa dos livros | ⬜ Pendente |
| 9 | Cálculo automático da nota média | ⬜ Pendente |
| 10 | Relatório de livros lidos por ano | ⬜ Pendente |

### Nível Avançado

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Unit of Work | ⬜ Pendente |
| 2 | HostedService | ⬜ Pendente |
| 3 | Domain Events | ⬜ Pendente |
| 4 | CQRS | ⬜ Pendente |
| 5 | Testes Unitários | ⬜ Pendente |
| 6 | Arquitetura Limpa | ⬜ Pendente |

### Divulgação

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Publicação inicial no LinkedIn | ⬜ Pendente |
| 2 | Publicações sobre as evoluções | ⬜ Pendente |
| 3 | Publicação da conclusão do projeto | ⬜ Pendente |

---

## 8. Considerações para Evolução

Algumas possibilidades de evolução que podem enriquecer o projeto:

- Implementar autenticação e autorização de usuários.
- Permitir que cada usuário mantenha seu próprio histórico de leitura.
- Impedir avaliações duplicadas do mesmo usuário para o mesmo livro, caso essa seja a regra definida.
- Implementar paginação, filtros e ordenação na listagem de livros.
- Criar ranking de livros mais bem avaliados.
- Criar ranking de usuários mais ativos.
- Permitir pesquisar livros por título, autor, gênero ou ISBN.
- Implementar uma interface Full Stack para consumir a API.

Essas funcionalidades podem ser consideradas extensões opcionais, além dos requisitos originais do treinamento.
