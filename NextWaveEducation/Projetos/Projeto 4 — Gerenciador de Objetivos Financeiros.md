# Projeto 4 — Gerenciador de Objetivos Financeiros

## 1. Visão Geral

**Projeto:** Sistema de Gerenciamento de Objetivos Financeiros.

Desenvolver um sistema de gerenciamento de objetivos financeiros, baseado no conceito de caixinhas do Nubank, permitindo que usuários criem objetivos, definam valores-alvo, realizem depósitos e saques e acompanhem a evolução financeira de cada caixa.

O projeto poderá ser desenvolvido como uma API REST ou uma aplicação Full Stack.

O sistema deverá permitir o acompanhamento do saldo de cada objetivo, gerenciamento de transações e geração de relatórios sobre a evolução financeira.

---

## 2. Requisitos Funcionais

### Gerenciamento de Caixas / Objetivos Financeiros

- [ ] Cadastrar um objetivo financeiro.
- [ ] Atualizar informações de um objetivo.
- [ ] Remover um objetivo.
- [ ] Listar todos os objetivos.
- [ ] Consultar detalhes de um objetivo.
- [ ] Validar os dados informados.
- [ ] Criar um endpoint para simular a evolução de uma caixa, considerando a quantidade de aportes mensais e os rendimentos (PLUS).
- [ ] Criar um endpoint para upload de imagem de capa do objetivo/caixa (PLUS).
- [ ] Criar uma propriedade para armazenar o valor total da caixa, atualizada sempre que uma nova transação for cadastrada (PLUS).

**Observação:** O valor total da caixa deverá ser atualizado no momento do cadastro de novas transações, evitando a necessidade de somar todas as transações sempre que os detalhes do objetivo forem consultados.

**Dica:** Utilizar transações de banco de dados para garantir a consistência entre o registro da transação e a atualização do saldo.

### Gerenciamento de Transações

- [ ] Cadastrar uma transação.
- [ ] Remover uma transação.
- [ ] Listar transações.
- [ ] Consultar detalhes de uma transação.
- [ ] Validar os dados informados.
- [ ] Permitir depósitos (Deposit).
- [ ] Permitir saques (Withdraw).
- [ ] Atualizar automaticamente o saldo da caixa após cada transação.

### Relatórios

- [ ] Gerar relatório demonstrando a evolução financeira das caixas (PLUS).

---

## 3. Regras de Negócio

### Regras obrigatórias

- [ ] Uma transação deve possuir no máximo duas casas decimais de precisão.
- [ ] O valor de uma transação não pode ser negativo.
- [ ] Toda transação deve possuir um tipo válido.

### Tipos de Transação

| Tipo | Descrição |
|---|---|
| Deposit | Depósito |
| Withdraw | Saque |

### Regras complementares para implementação

- [ ] Não permitir saques superiores ao saldo disponível.
- [ ] Atualizar o saldo da caixa após cada depósito.
- [ ] Atualizar o saldo da caixa após cada saque.
- [ ] Garantir que o registro da transação e a atualização do saldo sejam realizados de forma consistente.
- [ ] Considerar transações removidas no cálculo do saldo, realizando o estorno correspondente quando necessário.

---

## 4. Entidades e Dados

### ObjetivoFinanceiro

| Propriedade | Tipo |
|---|---|
| Id | int / Guid |
| Titulo | string |
| QuantidadeAlvo | decimal |
| Prazo | DateTime (opcional) |
| QuantidadeIdealAporteMensal | decimal (opcional) |
| Status | Enum |
| Transacoes | Coleção de Transacao |
| DataCriacao | DateTime |
| EstaDeletado | bool |
| ValorTotal | decimal (PLUS) |
| CapaObjetivo | byte[] (PLUS) |

**Observações:**

- `QuantidadeAlvo`: valor financeiro que o usuário deseja alcançar.
- `Prazo`: data limite para alcançar o objetivo, sendo opcional.
- `QuantidadeIdealAporteMensal`: valor estimado de aporte mensal necessário para alcançar o objetivo.
- `Status`: representa a situação atual do objetivo.
- `Transacoes`: coleção de depósitos e saques vinculados ao objetivo.
- `DataCriacao`: data em que o objetivo foi criado.
- `EstaDeletado`: indica se o objetivo foi removido logicamente.
- `ValorTotal`: saldo atual da caixa, atualizado a cada nova transação (PLUS).
- `CapaObjetivo`: imagem de capa do objetivo financeiro (PLUS).

### Status do Objetivo Financeiro

| Status | Descrição |
|---|---|
| EmProgresso | Objetivo em andamento |
| Completo | Objetivo alcançado |
| Cancelado | Objetivo cancelado |
| Pausado | Objetivo temporariamente pausado |

### Transacao

| Propriedade | Tipo |
|---|---|
| Id | int / Guid |
| Quantidade | decimal |
| Tipo | Enum |
| DataTransacao | DateTime |
| DataCriacao | DateTime |
| EstaDeletado | bool |

**Observações:**

- `Quantidade`: valor financeiro da transação.
- `Tipo`: identifica se a operação é um depósito ou saque.
- `DataTransacao`: data em que a movimentação financeira ocorreu, podendo ser a data atual ou uma data passada.
- `DataCriacao`: data em que o registro foi criado no sistema.
- `EstaDeletado`: indica se a transação foi removida logicamente.

### Tipo de Transação

```csharp
public enum TipoTransacao
{
    Deposit = 1,
    Withdraw = 2
}
```

### Status do Objetivo

```csharp
public enum StatusObjetivo
{
    EmProgresso = 1,
    Completo = 2,
    Cancelado = 3,
    Pausado = 4
}
```

---

## 5. Stack Tecnológica e Padrões de Arquitetura

O projeto será dividido em três níveis de evolução.

### Nível Básico I — Aplicação Console

**Objetivo:** Desenvolver a lógica principal do sistema utilizando C# e Programação Orientada a Objetos.

- [ ] Criar aplicação Console.
- [ ] Criar as entidades.
- [ ] Implementar cadastro de objetivos financeiros.
- [ ] Implementar atualização e remoção de objetivos.
- [ ] Implementar listagem e consulta de objetivos.
- [ ] Implementar cadastro de transações.
- [ ] Implementar depósitos e saques.
- [ ] Implementar validações básicas.
- [ ] Implementar cálculo do saldo das caixas.

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
| 3 | Cadastro de objetivos financeiros | ⬜ Pendente |
| 4 | Atualização e remoção de objetivos | ⬜ Pendente |
| 5 | Listagem e consulta de objetivos | ⬜ Pendente |
| 6 | Cadastro de transações | ⬜ Pendente |
| 7 | Implementar depósitos e saques | ⬜ Pendente |
| 8 | Implementar validações | ⬜ Pendente |
| 9 | Implementar cálculo de saldo | ⬜ Pendente |

### Nível Básico II — API

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar API ASP.NET Core | ⬜ Pendente |
| 2 | Implementar Entity Framework Core | ⬜ Pendente |
| 3 | Configurar banco em memória | ⬜ Pendente |
| 4 | Implementar endpoints de objetivos | ⬜ Pendente |
| 5 | Implementar endpoints de transações | ⬜ Pendente |
| 6 | Integrar funcionalidades | ⬜ Pendente |

### Nível Intermediário

| Etapa | Descrição | Status |
|---|---|---|
| 1 | FluentValidation | ⬜ Pendente |
| 2 | Repository Pattern | ⬜ Pendente |
| 3 | Middleware de exceções | ⬜ Pendente |
| 4 | InputModel, ViewModel e DTOs | ⬜ Pendente |
| 5 | IEntityTypeConfiguration | ⬜ Pendente |
| 6 | SQL Server ou SQLite | ⬜ Pendente |
| 7 | Simulação de evolução financeira | ⬜ Pendente |
| 8 | Upload de imagem de capa | ⬜ Pendente |
| 9 | Atualização automática do saldo | ⬜ Pendente |
| 10 | Transações de banco de dados | ⬜ Pendente |
| 11 | Relatório de evolução das caixas | ⬜ Pendente |

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
- Permitir que cada usuário tenha seus próprios objetivos financeiros.
- Implementar histórico detalhado de movimentações.
- Criar gráficos de evolução do patrimônio.
- Implementar filtros por período e tipo de transação.
- Permitir transferências entre objetivos financeiros.
- Criar simulações considerando juros compostos e aportes recorrentes.
- Implementar notificações de objetivos próximos do prazo.
- Criar indicadores de progresso em relação ao valor-alvo.
- Implementar exclusão lógica com possibilidade de restauração.

Essas funcionalidades podem ser consideradas extensões opcionais, além dos requisitos originais do treinamento.

---

## 9. Cuidados Técnicos Importantes

- Utilizar `decimal` para valores financeiros, evitando `float` e `double`.
- Definir precisão e escala adequadas no mapeamento do Entity Framework Core.
- Garantir que depósitos e saques atualizem o saldo de forma atômica.
- Considerar o estorno do saldo ao remover uma transação.
- Diferenciar a data efetiva da movimentação (`DataTransacao`) da data de registro (`DataCriacao`).
- Definir claramente o comportamento de objetivos pausados, cancelados e completos.
- Garantir que o saldo não fique negativo.
- Ao simular rendimentos, deixar explícitos a taxa utilizada, o período e a periodicidade de capitalização.

