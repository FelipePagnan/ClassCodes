# Projeto 5 — Gerenciador de Clínica

## 1. Visão Geral

**Projeto:** Sistema de Gerenciamento de Clínica Médica.

Desenvolver um sistema de gerenciamento de clínica médica, permitindo administrar pacientes, médicos, atendimentos, serviços e agendamentos.

O sistema deverá centralizar as informações da clínica, facilitando o gerenciamento dos atendimentos, o acompanhamento dos pacientes e a organização da agenda dos profissionais.

O projeto poderá ser desenvolvido como uma API REST ou uma aplicação Full Stack.

---

## 2. Requisitos Funcionais

### Gerenciamento de Pacientes

- [ ] Cadastrar pacientes.
- [ ] Atualizar informações dos pacientes.
- [ ] Remover pacientes.
- [ ] Listar pacientes.
- [ ] Consultar detalhes de um paciente.
- [ ] Buscar pacientes por CPF.
- [ ] Buscar pacientes por número de celular.

### Gerenciamento de Médicos

- [ ] Cadastrar médicos.
- [ ] Atualizar informações dos médicos.
- [ ] Remover médicos.
- [ ] Listar médicos.
- [ ] Consultar detalhes de um médico.

### Gerenciamento de Atendimentos

- [ ] Cadastrar atendimentos.
- [ ] Atualizar informações dos atendimentos.
- [ ] Remover atendimentos.
- [ ] Listar atendimentos.
- [ ] Consultar detalhes de um atendimento.

### Gerenciamento de Serviços

- [ ] Cadastrar serviços.
- [ ] Atualizar informações dos serviços.
- [ ] Remover serviços.
- [ ] Listar serviços.
- [ ] Consultar detalhes de um serviço.

### Confirmação de Agendamentos (PLUS 2)

- [ ] Implementar confirmação de agendamentos.
- [ ] Enviar confirmações por e-mail.
- [ ] Enviar confirmações por SMS.
- [ ] Integrar os agendamentos com o Google Agenda.

### Notificações Automáticas (PLUS 2)

- [ ] Implementar um Background Service para processamento de notificações.
- [ ] Executar verificações de agendamentos.
- [ ] Enviar notificações aos pacientes no dia anterior ao agendamento.
- [ ] Automatizar o processo de lembretes sem necessidade de intervenção manual.

### Autenticação e Autorização (PLUS 2)

- [ ] Implementar autenticação de usuários.
- [ ] Implementar autorização baseada em perfis.
- [ ] Criar perfil de Médico.
- [ ] Criar perfil de Administrador.
- [ ] Criar perfil de Recepcionista.
- [ ] Restringir funcionalidades conforme as permissões de cada perfil.

### Gerenciamento de Anexos (PLUS 2)

Implementar CRUD de anexos associados aos pacientes e/ou atendimentos.

Tipos de anexos:

- [ ] Atestado.
- [ ] Receita.
- [ ] Evolução.

Funcionalidades:

- [ ] Cadastrar anexos.
- [ ] Atualizar informações dos anexos.
- [ ] Remover anexos.
- [ ] Listar anexos.
- [ ] Consultar detalhes de um anexo.
- [ ] Identificar o tipo de documento anexado.

---

## 3. Stack Tecnológica e Padrões de Arquitetura

O projeto será dividido em três níveis de evolução.

### Nível Básico I — Aplicação Console

**Objetivo:** Desenvolver a lógica principal do sistema utilizando C# e Programação Orientada a Objetos.

- [ ] Criar aplicação Console.
- [ ] Criar as classes das entidades.
- [ ] Implementar cadastro de pacientes.
- [ ] Implementar cadastro de médicos.
- [ ] Implementar cadastro de atendimentos.
- [ ] Implementar cadastro de serviços.
- [ ] Implementar consultas e listagens.
- [ ] Implementar atualizações e remoções.
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

## 4. Publicação no LinkedIn

Criar uma publicação apresentando o início do projeto.

### Estrutura sugerida

1. Cumprimentar a rede.
2. Comunicar que está iniciando um projeto como parte do treinamento.
3. Marcar o responsável pelo treinamento: [Luis Deol](https://www.linkedin.com/in/luisdeol/).
4. Apresentar o projeto e explicar brevemente seus objetivos.
5. Concluir informando que publicará na rede as evoluções e novidades do desenvolvimento.

**Objetivo:** Compartilhar o aprendizado, documentar a evolução técnica e acompanhar o desenvolvimento do projeto publicamente.

---

## 5. Controle de Evolução

### Nível Básico I — Console

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar aplicação Console | ⬜ Pendente |
| 2 | Criar classes das entidades | ⬜ Pendente |
| 3 | CRUD de pacientes | ⬜ Pendente |
| 4 | Busca por CPF e celular | ⬜ Pendente |
| 5 | CRUD de médicos | ⬜ Pendente |
| 6 | CRUD de atendimentos | ⬜ Pendente |
| 7 | CRUD de serviços | ⬜ Pendente |
| 8 | Implementar validações | ⬜ Pendente |

### Nível Básico II — API

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Criar API ASP.NET Core | ⬜ Pendente |
| 2 | Implementar Entity Framework Core | ⬜ Pendente |
| 3 | Configurar banco em memória | ⬜ Pendente |
| 4 | Implementar endpoints de pacientes | ⬜ Pendente |
| 5 | Implementar endpoints de médicos | ⬜ Pendente |
| 6 | Implementar endpoints de atendimentos | ⬜ Pendente |
| 7 | Implementar endpoints de serviços | ⬜ Pendente |
| 8 | Integrar funcionalidades | ⬜ Pendente |

### Nível Intermediário

| Etapa | Descrição | Status |
|---|---|---|
| 1 | FluentValidation | ⬜ Pendente |
| 2 | Repository Pattern | ⬜ Pendente |
| 3 | Middleware de exceções | ⬜ Pendente |
| 4 | InputModel, ViewModel e DTOs | ⬜ Pendente |
| 5 | IEntityTypeConfiguration | ⬜ Pendente |
| 6 | SQL Server ou SQLite | ⬜ Pendente |

### Nível Avançado

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Unit of Work | ⬜ Pendente |
| 2 | HostedService | ⬜ Pendente |
| 3 | Domain Events | ⬜ Pendente |
| 4 | CQRS | ⬜ Pendente |
| 5 | Testes Unitários | ⬜ Pendente |
| 6 | Arquitetura Limpa | ⬜ Pendente |

### Funcionalidades PLUS 2

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Confirmação de agendamentos por e-mail | ⬜ Pendente |
| 2 | Confirmação de agendamentos por SMS | ⬜ Pendente |
| 3 | Integração com Google Agenda | ⬜ Pendente |
| 4 | Background Service para notificações | ⬜ Pendente |
| 5 | Notificações no dia anterior | ⬜ Pendente |
| 6 | Autenticação de usuários | ⬜ Pendente |
| 7 | Autorização por perfil | ⬜ Pendente |
| 8 | Perfil Médico | ⬜ Pendente |
| 9 | Perfil Administrador | ⬜ Pendente |
| 10 | Perfil Recepcionista | ⬜ Pendente |
| 11 | CRUD de anexos | ⬜ Pendente |
| 12 | Suporte a atestados, receitas e evoluções | ⬜ Pendente |

### Divulgação

| Etapa | Descrição | Status |
|---|---|---|
| 1 | Publicação inicial no LinkedIn | ⬜ Pendente |
| 2 | Publicações sobre as evoluções | ⬜ Pendente |
| 3 | Publicação da conclusão do projeto | ⬜ Pendente |

---

## 6. Considerações para Evolução

Algumas possibilidades de evolução que podem enriquecer o projeto:

- Implementar controle de disponibilidade dos médicos.
- Criar calendário de agendamentos.
- Impedir conflitos de horários para médicos.
- Implementar cancelamento e reagendamento de consultas.
- Criar histórico de atendimentos dos pacientes.
- Implementar controle de permissões mais detalhado.
- Criar dashboard administrativo com indicadores da clínica.
- Implementar filtros por médico, paciente, serviço e período.
- Criar relatórios de atendimentos realizados e agendamentos futuros.
- Implementar armazenamento externo de documentos e anexos.
- Registrar auditoria das alterações realizadas pelos usuários.
- Implementar logs de acesso e operações importantes.

Essas funcionalidades podem ser consideradas extensões opcionais, além dos requisitos originais do treinamento.

---

## 7. Considerações Técnicas

- Utilizar validações para CPF, celular e e-mail.
- Avaliar a utilização de exclusão lógica para pacientes, médicos e atendimentos, preservando o histórico.
- Garantir que apenas usuários autorizados possam acessar informações e documentos.
- Utilizar armazenamento seguro para anexos médicos.
- Implementar tratamento de falhas nas integrações de e-mail, SMS e Google Agenda.
- Evitar o envio duplicado de notificações.
- Considerar fusos horários e horários de funcionamento da clínica.
- Utilizar processamento assíncrono para tarefas de notificação.
- Registrar logs e auditoria das operações realizadas.
- Proteger informações pessoais e médicas conforme a LGPD.

