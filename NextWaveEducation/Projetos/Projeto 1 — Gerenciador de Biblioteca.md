# Projeto 1 — Gerenciador de Biblioteca

## 1. Visão Geral

**Projeto:** Sistema de Gerenciamento de Livros / Biblioteca

Desenvolver um sistema de gerenciamento de biblioteca, permitindo cadastrar, consultar e remover livros, além de gerenciar usuários e empréstimos.

O projeto será desenvolvido em duas versões:

- **Versão 1:** Aplicação Console.
- **Versão 2:** API REST com ASP.NET Core e Entity Framework Core.

---

## 2. Requisitos Funcionais

### Gerenciamento de Livros

- [ ] Cadastrar um livro.
- [ ] Validar dados (PLUS 1).
- [ ] Consultar todos os livros.
- [ ] Consultar um livro específico.
- [ ] Remover um livro.

### Gerenciamento de Usuários (PLUS 1)

- [ ] Cadastrar um usuário.

### Gerenciamento de Empréstimos (PLUS 1)

- [ ] Cadastrar um empréstimo.
- [ ] Registrar data de empréstimo automaticamente.

### Gerenciamento de Devoluções (PLUS 2)

- [ ] Cadastrar data de devolução.
- [ ] Devolver um livro.
- [ ] Emitir mensagem informando a quantidade de dias de atraso, caso exista.
- [ ] Informar quando o livro foi devolvido dentro do prazo.

---

## 3. Entidades e Dados

### Livro

| Propriedade | Tipo |
|---|---|
| Id | int |
| Título | string |
| Autor | string |
| ISBN | string |
| Ano de Publicação | int |

### Usuário (PLUS)

| Propriedade | Tipo |
|---|---|
| Id | int |
| Nome | string |
| Email | string |

### Empréstimo (PLUS)

| Propriedade | Tipo |
|---|---|
| Id | int |
| IdUsuario | int |
| IdLivro | int |
| Data de Empréstimo | Hoje / Agora |

---

# 4. Versão 1 — Aplicação Console

## Objetivo

Desenvolver a primeira versão do sistema utilizando uma aplicação Console em C#.

## Cursos recomendados

Caso tenha dúvidas durante o desenvolvimento:

1. Masterclass LinkedIn e CV
2. Introdução a Programação
3. Fundamentos de C#
4. Programação Orientada a Objetos
5. C# e POO
6. Dominando o LINQ

## Dicas

- O Id pode ser definido pelo próprio usuário.
- Também é possível utilizar `Guid` como identificador.

## Passo a Passo

### 1. Criar o projeto

Criar um projeto do tipo Console.

Utilizando VS Code ou terminal:

```bash
dotnet new console -o GerenciadorLivro.AppConsole
```

### 2. Criar as classes das entidades

Criar as classes correspondentes às entidades do projeto.

Preferencialmente, utilizar arquivos separados para cada classe.

### 3. Criar o menu

Implementar um menu interativo que será exibido no terminal.

O sistema deverá aguardar o usuário selecionar uma opção.

### 4. Implementar as funcionalidades

**Opção 1 — Cadastrar Livro**

- Solicitar os dados do livro separadamente.
- Receber as informações do usuário.
- Instanciar o objeto Livro com os dados informados.
- Armazenar o livro.

**Opção 2 — Consultar Todos os Livros**

- Retornar todos os livros cadastrados.
- Exibir as informações na tela.

**Opção 3 — Consultar Um Livro**

- Solicitar o identificador do livro.
- Buscar o livro correspondente.
- Retornar e exibir suas informações.

**Opção 4 — Deletar Um Livro**

- Solicitar o identificador do livro.
- Buscar o livro correspondente.
- Remover o livro do sistema.

---

# 5. Versão 2 — API com ASP.NET Core e EF Core

## Objetivo

Evoluir o projeto Console para uma API REST utilizando ASP.NET Core e Entity Framework Core.

## Cursos recomendados

Caso tenha dúvidas durante o desenvolvimento:

1. Masterclass LinkedIn e CV
2. Introdução a Programação
3. Fundamentos de C#
4. Programação Orientada a Objetos
5. C# e POO
6. Jornada .NET Direto ao Ponto 9.0
7. Dominando o LINQ
8. Formação ASP.NET Core (PLUS 2)
9. Formação Arquitetura de Software (PLUS 2)

## Observações

- O Entity Framework Core poderá ser utilizado para acesso a dados, inclusive com banco de dados em memória.
- Utilizar SQL Server (PLUS 1).
- Aplicar o padrão Repository (PLUS 2).
- Aplicar Arquitetura Limpa (PLUS 2).

## Passo a Passo

### 1. Criar o projeto

Criar um projeto do tipo ASP.NET Core Web API.

Utilizando VS Code ou terminal:

```bash
dotnet new webapi -o GerenciadorLivro.API
```

### 2. Criar as classes das entidades

Criar as classes correspondentes às entidades do projeto.

Preferencialmente, utilizar arquivos separados para cada classe.

### 3. Criar o Controller

Criar o controller responsável pelo gerenciamento dos livros:

- `BooksController`
- Ou `LivrosController`

### 4. Criar as Actions

Implementar as Actions responsáveis por cada funcionalidade do sistema.

### 5. Criar o DbContext

Criar a classe `LivrosDbContext`, que será responsável pelo acesso aos dados utilizando Entity Framework Core.

### 6. Implementar acesso aos dados em memória

Inicialmente, implementar o acesso aos dados utilizando um banco de dados em memória.

### 7. Utilizar o acesso a dados nas Actions

Integrar o `LivrosDbContext` às Actions do Controller para realizar as operações de cadastro, consulta e remoção.

---

# 6. Publicação no LinkedIn

Criar uma publicação apresentando o início do projeto.

## Estrutura sugerida

1. Cumprimentar a rede.
2. Comunicar que está iniciando um projeto como parte do treinamento.
3. Marcar o responsável pelo treinamento: [Luis Deol](https://www.linkedin.com/in/luisdeol/).
4. Apresentar o projeto e explicar brevemente seus objetivos.
5. Concluir informando que publicará na rede as evoluções e novidades do desenvolvimento.

**Objetivo:** Compartilhar o aprendizado, documentar a evolução técnica e acompanhar o desenvolvimento do projeto publicamente.

---

# 7. Controle de Evolução

| Etapa | Descrição | Status |
|---|---|---|
| Versão 1 | Aplicação Console | ⬜ Pendente |
| Versão 1 | Cadastro e consulta de livros | ⬜ Pendente |
| Versão 1 | Remoção de livros | ⬜ Pendente |
| PLUS 1 | Validação de dados | ⬜ Pendente |
| PLUS 1 | Cadastro de usuários | ⬜ Pendente |
| PLUS 1 | Cadastro de empréstimos | ⬜ Pendente |
| PLUS 2 | Controle de devoluções | ⬜ Pendente |
| PLUS 2 | Cálculo de atrasos | ⬜ Pendente |
| Versão 2 | API ASP.NET Core | ⬜ Pendente |
| Versão 2 | Entity Framework Core | ⬜ Pendente |
| PLUS 1 | SQL Server | ⬜ Pendente |
| PLUS 2 | Repository Pattern | ⬜ Pendente |
| PLUS 2 | Arquitetura Limpa | ⬜ Pendente |
| LinkedIn | Publicação inicial | ⬜ Pendente |
