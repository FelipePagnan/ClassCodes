# ECommerceModels

Projeto .NET 8 (Class Library, C#) com as entidades de domínio de um sistema
de e-commerce, geradas a partir do diagrama de classes `Projeto_Ecommerce_Start.drawio.png`.

## Sobre o diagrama

O diagrama original (feito no draw.io) descreve as entidades de um e-commerce,
cobrindo três áreas principais: **Catálogo de Produtos**, **Cliente/Endereço**
e **Pedido**. Abaixo está a transcrição das classes e seus atributos exatamente
como aparecem na imagem, seguida das decisões tomadas ao transformar isso em
código C#.

### Classes do diagrama

| Classe | Atributos |
|---|---|
| **Produto** | `Título: string`, `Descrição: string`, `Preço: decimal`, `Categoria: Categoria`, `Marca: string`, `Estoque: ProdutoEstoque[]`, `- Quantidade: int` (privado) |
| **ProdutoImagem** | `Identificador: string`, `Rota: string`, `EstaVisivel: boolean` |
| **Cliente** | `Nome: string`, `Sobrenome: string`, `Email: string`, `Telefone: string`, `Data de Nascimento: datetime`, `Documento: string`, `Endereços: Endereco` |
| **Endereço** | `Logradouro: string`, `Número: string`, `Complemento: string`, `CEP: string`, `Bairro: string`, `Cidade: string`, `Estado: string`, `País: string` |
| **Pedido** | `IdCliente`, `Items: PedidoItem[]`, `Data de Confirmação: datetime`, `Data de Envio: datetime`, `Status: int`, `Endereço de Entrega: Endereco`, `ValorFrete: decimal`, `ValorTotal` |
| **PedidoProduto** | `IdPedido`, `IdProduto`, `Quantidade`, `Preço` |
| **PedidoProdutoAvaliacao** | `IdPedidoProduto`, `Título: string`, `Descrição: string`, `Nota: int`, `IdCliente: int` |
| **PedidoPagamento** | `IdPagamentoExterno`, `Status: int`, `Data de Confirmação: datetime`, `IdPedido` |
| **PedidoAtualizacao** | `Descrição`, `Tipo`, `IdPedido` |

### Relacionamentos do diagrama

- **Produto → ProdutoImagem**: um produto possui várias imagens.
- **Produto → PedidoProduto**: um produto pode aparecer em vários itens de pedido.
- **PedidoProduto → PedidoProdutoAvaliacao**: um item de pedido pode ter avaliações.
- **Pedido → PedidoProduto**: um pedido possui vários itens.
- **Pedido → PedidoPagamento**: um pedido possui um registro de pagamento.
- **Pedido → PedidoAtualizacao**: um pedido possui um histórico de atualizações.
- **Pedido → Endereço**: o pedido referencia um endereço de entrega.
- **Pedido → Cliente**: o pedido pertence a um cliente.
- **Cliente (1) → Endereço (N)**: um cliente pode ter vários endereços cadastrados.

## Estrutura do projeto

```
ECommerceModels/
├── ECommerceModels.csproj
├── Enums/
│   ├── Categoria.cs
│   ├── StatusPedido.cs
│   ├── StatusPagamento.cs
│   └── TipoAtualizacaoPedido.cs
└── Models/
    ├── Produto.cs
    ├── ProdutoEstoque.cs
    ├── ProdutoImagem.cs
    ├── Cliente.cs
    ├── Endereco.cs
    ├── Pedido.cs
    ├── PedidoProduto.cs
    ├── PedidoProdutoAvaliacao.cs
    ├── PedidoPagamento.cs
    └── PedidoAtualizacao.cs
```

## Decisões de implementação

O diagrama não especifica tudo o que o código precisa (chaves primárias, tipos
concretos de enums, nulabilidade), então foram feitas as seguintes escolhas:

- **Id (chave primária)**: adicionado em todas as entidades, mesmo sem
  aparecer explicitamente no diagrama, por ser necessário para persistência
  (ex.: Entity Framework Core).
- **Enums fortemente tipados**: os campos `Status: int`, `Categoria: Categoria`
  e `Tipo` foram modelados como enums (`StatusPedido`, `StatusPagamento`,
  `Categoria`, `TipoAtualizacaoPedido`) em vez de `int` cru, para dar
  segurança de tipos.
- **`Produto.Quantidade`**: mantido como propriedade `private`, conforme o
  `-` do diagrama, com um método público `ObterQuantidadeTotal()` que soma as
  quantidades de `ProdutoEstoque`.
- **`ProdutoEstoque`**: o diagrama referencia o tipo (`Estoque: ProdutoEstoque[]`)
  mas não define a classe. Foi criada como uma posição/variação de estoque de
  um produto (`Id`, `Quantidade`, FK para `Produto`).
- **`PedidoItem` → `PedidoProduto`**: o diagrama usa `Items: PedidoItem[]` em
  `Pedido`, mas a classe detalhada no diagrama chama-se `PedidoProduto`.
  Foram tratadas como a mesma entidade (`Pedido.Items` é
  `ICollection<PedidoProduto>`).
- **Datas opcionais**: `DataDeConfirmacao`, `DataDeEnvio` e a data de
  confirmação do pagamento foram definidas como `DateTime?`, pois nem todo
  pedido/pagamento tem essas datas preenchidas em todos os estágios.
- **Coleções**: relações "muitos" foram modeladas com
  `ICollection<T>` inicializadas como `new List<T>()`, seguindo convenções
  comuns para entidades usadas com EF Core.
- **Navegação bidirecional**: entidades que participam de relações (como
  `Cliente` ↔ `Endereco`, `Pedido` ↔ `PedidoProduto`) possuem tanto a FK
  (`int`) quanto a propriedade de navegação (classe), facilitando o uso com
  ORMs.

## Requisitos

- .NET 8 SDK

## Como compilar

```bash
dotnet build
```

> Observação: este projeto foi gerado em um ambiente sem o .NET SDK
> disponível para validar a compilação localmente. Recomenda-se rodar
> `dotnet build` no seu ambiente antes de integrar o código.

## Próximos passos sugeridos

- Criar um `DbContext` (Entity Framework Core) com a configuração Fluent API
  das entidades acima.
- Adicionar validações (Data Annotations ou FluentValidation) nos campos
  obrigatórios.
- Criar DTOs/Requests separados das entidades de domínio, caso o projeto vá
  expor uma API.