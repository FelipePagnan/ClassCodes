# Projeto 2 — Sistema de Gerenciamento de Doação de Sangue

## 1. Visão Geral

**Projeto:** Sistema de Gerenciamento de Banco de Sangue.

Desenvolver um sistema para gerenciamento de doadores, registro de doações e controle de estoque de sangue, permitindo acompanhar a disponibilidade por tipo sanguíneo e fator Rh.

O projeto será desenvolvido em diferentes níveis de complexidade, evoluindo desde uma aplicação Console até uma arquitetura mais robusta utilizando recursos avançados do ecossistema .NET.

---

## 2. Requisitos Funcionais

### Cadastro de Doadores

- [ ] Cadastrar doadores.
- [ ] Validar os dados informados.
- [ ] Não permitir cadastro de doadores com e-mail duplicado.
- [ ] Permitir o cadastro de menores de idade, mesmo que não possam doar.
- [ ] Validar peso mínimo de 50 kg.
- [ ] Integrar uma API externa para consulta de CEP no cadastro de endereço (PLUS).

### Controle de Estoque de Sangue

- [ ] Controlar o estoque de sangue.
- [ ] Separar o estoque por tipo sanguíneo e fator Rh.
- [ ] Atualizar o estoque sempre que uma doação for registrada.
- [ ] Avisar quando o estoque atingir a quantidade mínima definida (PLUS).

### Registro de Doações

- [ ] Registrar doações realizadas.
- [ ] Associar cada doação a um doador.
- [ ] Registrar a data da doação.
- [ ] Registrar a quantidade de sangue doada.
- [ ] Atualizar automaticamente o estoque de sangue após cada doação.

### Consulta de Doadores

- [ ] Consultar doadores cadastrados.
- [ ] Consultar informações individuais de um doador.
- [ ] Consultar o histórico de doações de um doador.

### Relatórios

- [ ] Gerar relatório com a quantidade total de sangue disponível por tipo sanguíneo e fator Rh (PLUS).
- [ ] Gerar relatório de doações realizadas nos últimos 30 dias, incluindo informações dos doadores (PLUS).

---

## 3. Regras de Negócio

### Regras obrigatórias

- [ ] Não permitir cadastrar dois doadores com o mesmo e-mail.
- [ ] Menores de idade podem possuir cadastro, mas não podem realizar doações.
- [ ] O doador precisa pesar no mínimo 50 kg.

### Regras adicionais (PLUS)

- [ ] Mulheres só podem realizar uma nova doação após 90 dias da última doação.
- [ ] Homens só podem realizar uma nova doação após 60 dias da última doação.
- [ ] A quantidade de sangue doada deve estar entre 420 ml e 470 ml.

**Atenção:** As regras de elegibilidade devem ser validadas no momento do registro da doação, considerando também o histórico de doações do doador.

---

## 4. Entidades e Dados

### Doador

| Propriedade | Tipo |
|---|---|
| Id | int |
| NomeCompleto | string |
| Email | string |
| DataNascimento | DateTime |
| Genero | string |
| Peso | double |
| TipoSanguineo | string |
| FatorRh | string |
| Doacoes | List<Doacao> |
| Endereco | Endereco (PLUS) |

### Endereco (PLUS)

| Propriedade | Tipo |
|---|---|
| Id | int |
| Logradouro | string |
| Cidade | string |
| Estado | string |
| CEP | string |
| Doador | Doador |

### Doacao

| Propriedade | Tipo |
|---|---|
| Id | int |
| DoadorId | int |
| DataDoacao | DateTime |
| QuantidadeML | int |
| Doador | Doador |

### EstoqueSangue

| Propriedade | Tipo |
|---|---|
| Id | int |
| TipoSanguineo | string |
| FatorRh | string |
| QuantidadeML | int |

---

# 5. Stack Tecnológica e Padrões de Arquitetura

O projeto será dividido em três níveis de evolução.

## Nível Básico I — Aplicação Console

**Objetivo:** Desenvolver a lógica principal do sistema utilizando C# e Programação Orientada a Objetos.

- [ ] Criar aplicação Console.
- [ ] Implementar as entidades.
- [ ] Implementar cadastro de doadores.
- [ ] Implementar validações básicas.
- [ ] Implementar registro de doações.
- [ ] Implementar controle de estoque.
- [ ] Implementar consultas.

## Nível Básico II — API ASP.NET Core

**Objetivo:** Transformar a aplicação Console em uma API REST utilizando os recursos fundamentais do ecossistema .NET.

Tecnologias e conceitos:

- [ ] ASP.NET Core API.
- [ ] Entity Framework Core.
- [ ] Programação Orientada a Objetos (POO).
- [ ] Banco de dados em memória.

## Nível Intermediário

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

## Nível Avançado

**Objetivo:** Evoluir a arquitetura da aplicação, implementando padrões avançados de desenvolvimento, processamento e testes.

Tecnologias e padrões:

- [ ] Unit of Work.
- [ ] HostedService.
- [ ] Domain Events.
- [ ] CQRS.
- [ ] Testes Unitários.
- [ ] Arquitetura Limpa (Clean Architecture).

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

## Nível Básico I — Console

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar aplicação Console | ⬜ Pendente |
| 2 | Criar entidades | ⬜ Pendente |
| 3 | Cadastro de doadores | ⬜ Pendente |
| 4 | Validações básicas | ⬜ Pendente |
| 5 | Registro de doações | ⬜ Pendente |
| 6 | Controle de estoque | ⬜ Pendente |
| 7 | Consultas | ⬜ Pendente |

## Nível Básico II — API

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar API ASP.NET Core | ⬜ Pendente |
| 2 | Implementar Entity Framework Core | ⬜ Pendente |
| 3 | Configurar banco em memória | ⬜ Pendente |
| 4 | Implementar endpoints | ⬜ Pendente |
| 5 | Integrar funcionalidades | ⬜ Pendente |

## Nível Intermediário

| Etapa | Descrição | Status |
|---|---|---|
| 1 | FluentValidation | ⬜ Pendente |
| 2 | Repository Pattern | ⬜ Pendente |
| 3 | Middleware de exceções | ⬜ Pendente |
| 4 | InputModel, ViewModel e DTOs | ⬜ Pendente |
| 5 | IEntityTypeConfiguration | ⬜ Pendente |
| 6 | SQL Server ou SQLite | ⬜ Pendente |
| 7 | Integração com API de CEP | ⬜ Pendente |
| 8 | Controle de estoque mínimo | ⬜ Pendente |
| 9 | Regras avançadas de doação | ⬜ Pendente |
| 10 | Relatórios | ⬜ Pendente |

## Nível Avançado

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Unit of Work | ⬜ Pendente |
| 2 | HostedService | ⬜ Pendente |
| 3 | Domain Events | ⬜ Pendente |
| 4 | CQRS | ⬜ Pendente |
| 5 | Testes Unitários | ⬜ Pendente |
| 6 | Arquitetura Limpa | ⬜ Pendente |

## Divulgação

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Publicação inicial no LinkedIn | ⬜ Pendente |
| 2 | Publicações sobre as evoluções | ⬜ Pendente |
| 3 | Publicação da conclusão do projeto | ⬜ Pendente |
