# Gerenciador de Avaliação de Livros (Console)

Sistema de avaliação de livros (estilo GoodReads) desenvolvido em C# (.NET 8), aplicação console — Nível Básico I.

## Como executar

```bash
cd AvaliacaoLivros.AppConsole
dotnet run
```

> A opção "Cadastrar Livro via ISBN" usa a API pública e gratuita **Open Library**
> (não exige chave de API, mas precisa de internet). Se o ISBN não for encontrado ou
> não houver conexão, é só cadastrar manualmente pela opção 1.

## Funcionalidades implementadas

### Livros
- Cadastro, atualização, remoção, listagem e detalhes (com avaliações)
- Validação de dados (título, autor, ISBN único, ano, páginas)
- Cadastro de livro via consulta a **API externa por ISBN** (PLUS)
- Cadastro de capa do livro a partir de um arquivo de imagem, armazenado em `byte[]` (PLUS)
- Nota média recalculada automaticamente a cada nova avaliação (PLUS)

### Usuários
- Cadastro, atualização, remoção, listagem e detalhes (com avaliações feitas)
- Validação de dados (nome, e-mail único)

### Avaliações
- Cadastro e listagem de avaliações por livro
- Validações:
  - Nota deve ser entre 1 e 5
  - Data de início da leitura não pode ser maior que a data de fim

### Relatórios
- Relatório de livros lidos em um determinado ano, com nome do leitor e data de conclusão (PLUS)

## Estrutura

```
AvaliacaoLivros.AppConsole/
├── Models/
│   ├── Livro.cs
│   ├── Usuario.cs
│   ├── Avaliacao.cs
│   └── GeneroLivroEnum.cs
├── Services/
│   ├── AvaliacaoLivrosService.cs   (regra de negócio, dados em memória)
│   └── LivroExternoService.cs      (integração externa com Open Library)
├── Program.cs                      (menu e interação com o usuário)
└── AvaliacaoLivros.AppConsole.csproj
```

## Observações

- Os dados são mantidos em memória (listas); ao fechar o programa, tudo é perdido. Isso é esperado para o Nível Básico I.
- O enunciado cita a regra "data de início não pode ser maior que fim de leitura" na avaliação,
  mas a entidade `Avaliacao` original não trazia esses campos. Foram adicionados
  `DataInicioLeitura` e `DataFimLeitura` à entidade para viabilizar essa validação.
- A capa do livro é lida de um arquivo local (`File.ReadAllBytes`) e armazenada como `byte[]`
  diretamente no objeto `Livro`, conforme indicado no enunciado.
- Essa versão em memória pode servir de base para o Nível Básico II (ASP.NET Core API +
  Entity Framework Core), reaproveitando os Models e a lógica de `AvaliacaoLivrosService`.
