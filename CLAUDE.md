# GUSTO Convênio Web — Documentação Técnica

Portal web multi-tenant para empresas conveniadas fazerem seus pedidos diários de marmitas no GUSTO (Vila Branca, Jacareí-SP). Cada empresa é um tenant isolado: vê apenas seus próprios funcionários, pedidos e histórico. O cardápio é único e gerenciado pelo Admin GUSTO.

---

## Stack

| Camada | Tecnologia |
|---|---|
| UI | ASP.NET Core 8 Razor Pages |
| Backend | C# .NET 8 |
| ORM | Dapper + MySqlConnector |
| Banco | MySQL 8 (mesmo banco do gusto-agent) |
| Auth | Cookie customizado (sem Identity) — BCrypt.Net-Next |
| Hospedagem | SmartASP (IIS + ASP.NET Core 8) |

---

## Estrutura real da Solution

```
GustoConvenio.slnx
└── GustoConvenio.Web/
    ├── Program.cs                              → registra serviços, policies, cookie auth
    ├── appsettings.json                        → connection string MySQL
    ├── Auth/
    │   └── TenantService.cs                   → lê EmpresaId, EmpresaNome, IsAdmin do cookie
    ├── Data/
    │   ├── DbConnectionFactory.cs             → cria MySqlConnection a partir da string
    │   └── Repositories/
    │       ├── IEmpresaRepository / EmpresaRepository
    │       ├── IAdminUserRepository / AdminUserRepository
    │       ├── IFuncionarioRepository / FuncionarioRepository   (filtra por EmpresaId)
    │       ├── ICardapioRepository / CardapioRepository         (sem tenant)
    │       └── IPedidoRepository / PedidoRepository            (filtra por EmpresaId)
    ├── Models/
    │   ├── Empresa.cs
    │   ├── Funcionario.cs
    │   ├── CardapioItem.cs
    │   ├── AdminUser.cs
    │   └── ItemPedidoDto.cs                   → DTO de item; Tamanhos (preços hardcoded)
    ├── wwwroot/css/site.css                   → paleta amber, topbar, sidebar, grade, badges
    └── Pages/
        ├── Index.cshtml                       → redirect para /Login/Index
        ├── Login/
        │   ├── Index.cshtml(.cs)              → tenta AdminUser primeiro, depois Empresa
        │   └── Logout.cshtml(.cs)
        ├── Shared/
        │   ├── _Layout.cshtml                 → topbar + sidebar dinâmica (admin vs empresa)
        │   └── _LayoutLogin.cshtml
        ├── Funcionarios/
        │   ├── Index.cshtml(.cs)              → lista do tenant
        │   ├── Criar.cshtml(.cs)
        │   └── Editar.cshtml(.cs)             → edita nome + toggle ativo
        ├── Pedido/
        │   └── Index.cshtml(.cs)              → grade matricial + countdown + POST confirmar
        ├── Historico/
        │   ├── Index.cshtml(.cs)              → filtro mês/ano + totalizadores + tabela
        │   └── Detalhes.cshtml(.cs)           → item a item do pedido (isolado por tenant)
        └── Admin/
            ├── Empresas/
            │   ├── Index.cshtml(.cs)          → lista todas as empresas
            │   ├── Criar.cshtml(.cs)          → cria empresa + senha BCrypt
            │   └── Editar.cshtml(.cs)         → edita empresa; nova senha opcional
            ├── Cardapio/
            │   └── Index.cshtml(.cs)          → tabs por dia; adicionar/remover pratos e acomps
            └── Pedidos/
                ├── Index.cshtml(.cs)          → consolidado de hoje (todas as empresas)
                └── Detalhe.cshtml(.cs)        → detalhe por pedido (sem filtro tenant)
```

---

## Multi-Tenancy

**Tenant = Empresa conveniada.** O isolamento é feito por `EmpresaId`:

- Todo usuário autenticado carrega o `EmpresaId` como **claim** no cookie
- `TenantService` lê o claim via `IHttpContextAccessor` e é injetado nos Page Models
- Todos os repositórios recebem `EmpresaId` via `TenantService` — nunca de parâmetros externos
- O Admin GUSTO opera **fora do tenant** (sem filtro de `empresa_id`) e enxerga todos os dados

### Isolamento por tabela

| Tabela | Coluna tenant | Quem filtra |
|---|---|---|
| `funcionarios` | `empresa_id` | Repositório — sempre |
| `pedidos` | `empresa_id` | Repositório — sempre |
| `itens_pedido` | via `pedido_id` → `empresa_id` | JOIN com pedidos |
| `cardapio_web` | — (sem tenant) | Admin gerencia, todos leem |
| `empresas_convenio` | — (é a própria tabela de tenants) | Só Admin |

### Claims no cookie

```csharp
"EmpresaId"    // int como string — chave do tenant (ausente no cookie do admin)
"EmpresaNome"  // exibido na topbar
"IsAdmin"      // "true" para o Admin GUSTO
```

---

## Auth

- Login único para os dois perfis (`/Login/Index`)
- Fluxo: tenta `admin_users` primeiro (BCrypt); se não encontrar, tenta `empresas_convenio`
- Políticas: `"Admin"` (IsAdmin == true) e `"Empresa"` (EmpresaId presente)
- Cookie `GustoCookie`, sliding 8h
- Sem cadastro público — empresas criadas apenas pelo Admin

---

## Dois Perfis de Usuário

| Perfil | Policy | Sidebar | Descrição |
|---|---|---|---|
| **Empresa** | `"Empresa"` | Pedido / Funcionários / Histórico | Pede marmitas, gerencia seus funcionários |
| **Admin GUSTO** | `"Admin"` | Pedidos / Empresas / Cardápio | Gerencia tenants, cardápio e acompanha pedidos |

---

## Banco de Dados (MySQL — compartilhado com gusto-agent)

### Tabelas existentes (gusto-agent)

| Tabela | Uso no portal web |
|---|---|
| `pedidos` | Escrita do lote convênio (`tipo='convenio'`); leitura para histórico |
| `itens_pedido` | Detalhe de cada marmita por funcionário |
| `empresas_convenio` | Cadastro dos tenants (auth + configuração) |

### Tabelas novas (script `migration_gusto_web.sql`)

**`funcionarios`**
```sql
id          INT PK AUTO_INCREMENT
empresa_id  INT NOT NULL  -- FK → empresas_convenio
nome        VARCHAR(200) NOT NULL
ativo       TINYINT(1) DEFAULT 1
criado_em   DATETIME DEFAULT NOW()
```

**`cardapio_web`** *(sem tenant)*
```sql
id          INT PK AUTO_INCREMENT
dia_semana  TINYINT NOT NULL   -- 0=Seg … 5=Sab
tipo        VARCHAR(20)        -- 'prato' | 'acompanhamento'
nome        VARCHAR(200) NOT NULL
ativo       TINYINT(1) DEFAULT 1
ordem       INT DEFAULT 0
```

**`admin_users`**
```sql
id          INT PK AUTO_INCREMENT
nome        VARCHAR(200) NOT NULL
email       VARCHAR(150) NOT NULL UNIQUE
senha_hash  VARCHAR(256) NOT NULL
ativo       TINYINT(1) DEFAULT 1
criado_em   DATETIME DEFAULT NOW()
```

### Colunas adicionadas em `empresas_convenio`
```
email          VARCHAR(150)  NULL   -- login do portal
senha_hash     VARCHAR(256)  NULL   -- BCrypt
horario_corte  TIME          DEFAULT '10:00:00'
```

---

## Tela — Pedido do Dia (core)

Grade matricial: **colunas = funcionários ativos do tenant** × **linhas = cardápio do dia**

Seções:
- **Pratos** — exatamente 1 por funcionário
- **Tamanho** — Mini / Normal / Executiva / Churrasco (exatamente 1)
- **Acompanhamentos** — até 2 por funcionário

Preços dos tamanhos: hardcoded na classe estática `Tamanhos` em `Models/ItemPedidoDto.cs`.

```csharp
public static class Tamanhos
{
    public static readonly Dictionary<string, decimal> Precos = new()
    {
        ["Mini"]      = 21.90m,
        ["Normal"]    = 23.90m,
        ["Executiva"] = 24.90m,
        ["Churrasco"] = 27.90m,
    };
}
```

### Regras de negócio

- Horário de corte configurado **por tenant** pelo Admin (`empresas_convenio.horario_corte`)
- Banner com contagem regressiva em tempo real (JS, tick 1s)
- Após o corte: grade bloqueada no frontend + rejeição no backend
- Idempotência: 1 pedido por tenant por dia (`JaPediuHojeAsync`)
- POST serializa itens como JSON → hidden input → desserializa no backend

---

## Fluxo de Pedido Convênio

```
Empresa loga → claims EmpresaId + EmpresaNome injetados no cookie
  → /Pedido carrega grade filtrada por EmpresaId + cardápio do dia (dia_semana)
  → empresa monta grade → POST com PedidoJson (lista de ItemPedidoDto)
      → backend verifica horário de corte
      → backend verifica idempotência (JaPediuHojeAsync)
      → INSERT pedidos (tipo='convenio', empresa_id=X)
      → INSERT itens_pedido para cada funcionário marcado (transação)
      → redirect /Pedido com TempData["Sucesso"]
```

---

## Painel Admin

| Rota | Função |
|---|---|
| `/Admin/Pedidos` | Consolidado de pedidos de hoje (todas as empresas) + detalhe por pedido |
| `/Admin/Empresas` | CRUD de empresas: criar com senha BCrypt, editar, toggle ativo, horário de corte |
| `/Admin/Cardapio` | Gerenciar cardápio semanal por aba de dia; adicionar/remover pratos e acompanhamentos |

---

## Paleta Visual

```css
--gusto-dark:      #1a1a1a   /* topbar e sidebar */
--gusto-amber:     #e8a020   /* destaque, botões primários, item ativo */
--gusto-bg:        #f4f4f4   /* fundo da página */
--gusto-surface:   #ffffff   /* cards e content */
--gusto-text:      #1a1a1a
--gusto-muted:     #888888
```

---

## Integração com gusto-agent

- Banco MySQL compartilhado — pedidos do portal entram com `tipo='convenio'`
- O serviço Windows (`poller.py`) detecta pedidos novos e imprime automaticamente
- O portal **não** acessa Redis nem a API do agente
- Cardápio gerenciado exclusivamente pela tabela `cardapio_web` — Google Sheets descontinuado

---

## Decisões de Arquitetura

| # | Decisão | Escolha |
|---|---|---|
| 1 | Banco | **Compartilhado** com gusto-agent — pedidos chegam direto à impressora |
| 2 | Auth Admin | **Tabela `admin_users` separada** — isola o restaurante dos tenants |
| 3 | Cardápio | **Tabela `cardapio_web`** — planilha Google Sheets descontinuada |
| 4 | Notificação | **Nenhuma** — `poller.py` detecta e imprime automaticamente |
| 5 | Validação corte | **Frontend + backend** — countdown JS + rejeição no POST |
| 6 | ORM | **Dapper** (sem EF Core) — queries explícitas, sem migrations automáticas |

---

## Etapas de Desenvolvimento

| Etapa | Status | Conteúdo |
|---|---|---|
| 1 | ✅ | Análise do mockup, documentação, decisões de arquitetura, scripts SQL |
| 2 | ✅ | Solution, estrutura de projetos, Program.cs, DbConnectionFactory, Auth (cookie + TenantService + políticas), _Layout, _LayoutLogin, Login/Logout, site.css |
| 3 | ✅ | Módulo Funcionários — CRUD completo isolado por tenant |
| 4 | ✅ | Pedido do dia — grade matricial interativa, countdown, validação corte, POST com transação e idempotência |
| 5 | ✅ | Histórico — totalizadores do mês, tabela de pedidos e detalhes por pedido |
| 6 | ✅ | Painel Admin — Empresas (CRUD + BCrypt), Cardápio (tabs por dia), Pedidos (consolidado + detalhe cross-tenant) |
| 7 | ✅ | Deploy SmartASP — site `appfood` (egameiro-001-site6), Web Deploy via VS, Data Protection persistente, bugs de TimeSpan/TimeOnly e antiforgery resolvidos |
| 8 | ✅ | Multi-restaurante — tabela `restaurantes`, `restaurante_id` em todas as tabelas, perfil SuperAdmin, páginas `/SuperAdmin/Restaurantes` (Index+Criar+Editar), isolamento de cardápio/empresas por restaurante, webhook multi-tenant no gusto-agent via slug |

---

## Deploy (SmartASP) — Etapa 7

1. Executar `migration_gusto_web.sql` no banco via phpMyAdmin
2. Atualizar `appsettings.json` com connection string de produção
3. Publicar via Visual Studio → FTP → `/site/wwwroot`
4. Application Pool: No Managed Code | Integrated | 64-bit
5. Cheklist pós-deploy:
   - [ ] `/Login` carrega sem erro 500
   - [ ] Login como Admin GUSTO
   - [ ] Criar empresa + login como empresa
   - [ ] Cadastrar funcionários
   - [ ] Configurar cardápio do dia
   - [ ] Montar e confirmar pedido
   - [ ] Verificar histórico

---

## Pendências

### Técnicas
- [ ] **Tamanhos e preços do cardápio WhatsApp estão hardcoded** em `gusto-agent/services/cardapio.py` (Mini R$21,90, Normal R$23,90, Executiva R$24,90, Churrasco R$27,90). Precisa definir com o usuário como esses valores serão configurados — sugestão: nova tabela `restaurante_tamanhos` ou campo dedicado no painel Admin. Aguardando alinhamento com o usuário do restaurante.
- [x] ~~Migrar `gusto-agent/services/cardapio.py` para ler `cardapio_web` no MySQL~~ — implementado; Google Sheets removido
- [x] ~~Implementar webhook multi-tenant no gusto-agent~~ — `restaurante_id` propagado por toda a cadeia de handlers e cardápio
- [x] ~~Corrigir mapeamento `TimeSpan → TimeOnly`~~ — resolvido com alias `horario_corte AS horario_corte_raw` no `EmpresaRepository`
- [x] ~~Data Protection antiforgery token~~ — resolvido com `AddDataProtection().PersistKeysToFileSystem` + pacote `Microsoft.AspNetCore.DataProtection`
- [x] ~~Arquitetura multi-restaurante~~ — implementada na Etapa 8
- [x] ~~Domínio — `facerenew.app.br` sem www~~ — resolvido via configuração DNS/SmartASP

---

## Onboarding de Novo Restaurante

### Passo a passo para ativar um novo restaurante

**1. Portal Web (SuperAdmin)**
- Login em `/Login` com conta SuperAdmin
- `/SuperAdmin/Restaurantes/Criar` — preencher nome, slug (ex: `pizzaria-centro`), telefone, email
- Sistema cria automaticamente o usuário Admin do restaurante

**2. Banco de Dados**
- Executar `migration_cardapio_empresa_preco.sql` se ainda não aplicado
- Executar `migration_multi_restaurante.sql` se ainda não aplicado

**3. Admin do Restaurante**
- Login com as credenciais criadas pelo SuperAdmin
- `/Admin/Empresas/Criar` — cadastrar cada empresa conveniada (nome, email, senha inicial, horário de corte)
- `/Admin/Cardapio` → selecionar "📱 Cardápio WhatsApp" → cadastrar pratos e acompanhamentos por dia da semana
- `/Admin/Cardapio` → selecionar cada empresa conveniada → cadastrar cardápio e preços específicos

**4. gusto-agent**
- Criar instância UAZAPI para o número WhatsApp do restaurante
- Configurar webhook da instância apontando para:
  `https://<dominio-do-agent>/webhook/<slug>-<restaurante_id>`
  Exemplo: `https://agent.gusto.com.br/webhook/pizzaria-centro-2`
- O `restaurante_id` é o `id` gerado na tabela `restaurantes` (ver no phpMyAdmin)

**5. Checklist de validação**
- [ ] Bot responde no WhatsApp com cardápio do dia
- [ ] Empresa conveniada consegue logar no portal
- [ ] Empresa consegue montar e confirmar pedido
- [ ] Pedido aparece no painel Admin do restaurante
- [ ] Impressora recebe o pedido (se poller.py configurado)

---

## Arquitetura Multi-Restaurante (Etapa 8)

```
SuperAdmin (Eduardo — dono do SaaS)
  └── Restaurante (GUSTO, Restaurante X, Y...)  ← tabela `restaurantes`
        └── Empresa (conveniadas de cada restaurante)
              └── Funcionários / Pedidos / Cardápio
```

### Banco de dados
- Tabela `restaurantes` — id, nome, slug, telefone, email, endereco, ativo
- `restaurante_id` adicionado em: `admin_users`, `empresas_convenio`, `cardapio_web`, `funcionarios`
- `is_super_admin` adicionado em `admin_users`
- Script: `migration_multi_restaurante.sql`

### Perfis de acesso
| Perfil | Claim | Acesso |
|---|---|---|
| SuperAdmin | `IsSuperAdmin=true` | `/SuperAdmin/*` — vê todos os restaurantes |
| Admin | `IsAdmin=true` | `/Admin/*` — filtrado pelo seu `restaurante_id` |
| Empresa | `EmpresaId` | `/Pedido`, `/Funcionarios`, `/Historico` — isolado por empresa |

### Isolamento por restaurante
- `CardapioRepository` — todas as queries filtram por `restaurante_id`
- `EmpresaRepository.ListarTodasAsync(restauranteId)` — filtra por restaurante
- `TenantService` — expõe `RestauranteId` e `IsSuperAdmin` das claims do cookie
- Login injeta `RestauranteId` e `IsSuperAdmin` no cookie ao autenticar

---

## Integração gusto-agent × Multi-Restaurante

Cada restaurante tem sua própria instância UAZAPI com seu número de WhatsApp. O gusto-agent é **único e multi-tenant** — identifica o restaurante pelo slug na URL do webhook.

### Arquitetura de webhooks
```
UAZAPI instância GUSTO     → POST /webhook/gusto      → restaurante_id=1
UAZAPI instância RestX     → POST /webhook/restaurante-x → restaurante_id=2
```

### Implementação pendente no gusto-agent
1. Endpoint `/webhook/{slug}` — recebe mensagens de cada instância UAZAPI
2. Busca `restaurante_id` no banco via `SELECT id FROM restaurantes WHERE slug=?`
3. Todas as queries do agent passam a filtrar por `restaurante_id` (cardápio, pedidos, clientes)
4. Cardápio lido do MySQL (`cardapio_web WHERE restaurante_id=?`) em vez do Google Sheets

### Configuração UAZAPI por restaurante
Cada instância configura seu webhook apontando para:
```
https://<dominio-do-agent>/webhook/<slug-do-restaurante>
```
