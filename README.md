# OrderFlow API

O **OrderFlow** é uma API para gerenciamento de pedidos, desenvolvida com foco em **performance**, **escalabilidade** e **manutenibilidade** utilizando **.NET 10**.  
O projeto foi construído seguindo os princípios de **Clean Architecture**, **Domain-Driven Design (DDD)** e **CQRS**, promovendo alta coesão, baixo acoplamento e separação clara de responsabilidades.

---

# Tecnologias e Padrões

- **Framework:** .NET 10
- **Banco de Dados:** PostgreSQL 16
- **Arquitetura:** Clean Architecture, SOLID e DDD
- **Padrões e Bibliotecas:**
  - Minimal APIs
  - MediatR
  - FluentValidation
  - ASP.Versioning
  - Result Pattern
  - Smart Enums
- **Qualidade:** TDD (Test Driven Development)
- **Containerização:** Docker e Docker Compose

---

# Decisões Técnicas

## Minimal APIs
A API utiliza **Minimal APIs** para reduzir a verbosidade do código e melhorar a performance, mantendo a organização através de agrupamento de rotas, versionamento e filtros de validação.

## CQRS com MediatR
Foi adotado o padrão **CQRS (Command Query Responsibility Segregation)** para separar operações de leitura e escrita, facilitando escalabilidade e manutenção da aplicação.

## Result Pattern
Implementado para padronizar retornos da aplicação e evitar o uso excessivo de exceções como fluxo de controle.

## Smart Enums
Utilizados para representar regras de domínio de forma mais segura e expressiva, garantindo consistência nas chaves estrangeiras e melhor integração com migrations.

## Segurança
Autenticação baseada em **JWT Bearer Token** com expiração de 1 hora.  
A implementação foi simplificada para ambiente de demonstração, não contemplando blacklist de tokens ou controle distribuído via Redis.

## Nomenclatura
A tabela principal foi nomeada como `sales_order` para evitar conflitos com palavras reservadas em bancos SQL.

---

# Estrutura do Projeto

```text
OrderFlow/
├── OrderFlow.API/              # Entrypoint, Endpoints, Middlewares e Filtros
├── OrderFlow.Application/      # Casos de uso, Commands, Queries, Handlers e Validators
├── OrderFlow.Domain/           # Entidades, Regras de Negócio, Smart Enums e Interfaces
├── OrderFlow.Infrastructure/   # Persistência, DbContext, Repositórios e Migrations
├── OrderFlow.UnitTests/        # Testes unitários das camadas da aplicação
├── docker-compose.yml          # Orquestração dos containers
├── Dockerfile                  # Definição para a aplicação ser empacotada em uma imagem Docker
└── README.md                   # Documentação do projeto
```

---

# Como Executar o Projeto

## Subir o ambiente

```bash
docker compose up -d --build
```

## Parar os serviços

```bash
docker compose down
```

## Parar e remover volumes

```bash
docker compose down -v
```

## Remover imagem da API

```bash
docker rmi orderflow-order-api
```

---

# Documentação da API

Após subir o ambiente, a documentação Swagger estará disponível em:

```text
http://localhost:8080/swagger
```

---

# Endpoints

## Autenticação

### Login

```http
POST /api/login
```

Exemplo:

```text
http://localhost:8080/api/login
```

---

## Pedidos

### Criar Pedido

```http
POST /api/v1/orders
```

Exemplo:

```text
http://localhost:8080/api/v1/orders
```

Payload:

```json
{
  "customerId": 2,
  "currency": "BRL",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 2
    }
  ]
}
```
---

### Listar Pedidos

```http
GET /api/v1/orders
```

Parâmetros disponíveis:

| Parâmetro | Tipo | Descrição |
|---|---|---|
| Id | int | Id do pedido |
| CustomerId | int | Id do cliente |
| Status | int | Status do pedido |
| From | date | Data inicial |
| To | date | Data final |
| Page | int | Página atual |
| PageSize | int | Quantidade por página |

Exemplo:

```text
http://localhost:8080/api/v1/orders?Id=1&CustomerId=2&Status=1&From=2026-05-26&To=2026-05-26&Page=1&PageSize=10
```

---

### Buscar Pedido por Id

```http
GET /api/v1/orders/{id}
```

Exemplo:

```text
http://localhost:8080/api/v1/orders/3
```

---

### Confirmar Pedido

```http
POST /api/v1/orders/{id}/confirm
```

Exemplo:

```text
http://localhost:8080/api/v1/orders/1/confirm
```

---

### Cancelar Pedido

```http
POST /api/v1/orders/{id}/cancel
```

Exemplo:

```text
http://localhost:8080/api/v1/orders/1/cancel
```

---

# Testes

Para executar os testes unitários:

```bash
dotnet test
```

---

# Objetivo do Projeto

O objetivo deste projeto é demonstrar uma abordagem moderna para construção de APIs utilizando .NET, aplicando boas práticas de arquitetura, separação de responsabilidades, testes automatizados e organização de código voltada para ambientes escaláveis e de fácil manutenção.
