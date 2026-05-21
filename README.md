# BoomTax API

REST API for electronic filing of IRS information returns. Create filings, submit forms, e-file to the IRS, and download copies — all through a simple HTTP interface.

> **IRIS-Ready:** The IRS is retiring the FIRE system on December 31, 2026. BoomTax already files through IRIS for all 1099 forms. If you use the BoomTax API, your integration doesn't change — we handle the FIRE-to-IRIS conversion automatically. [Learn more](https://www.boomtax.com/irs-iris)

**Base URL:** `https://api.boomtax.com`
**Swagger docs:** `https://api.boomtax.com/swagger`
**MCP Server:** [`@boomtax/mcp-server`](https://github.com/boomtax/mcp-server) — query your filing data from any AI assistant

## Supported Forms

| Form | Description | Filing System |
|------|-------------|---------------|
| **1042-S** | Foreign person's U.S. source income subject to withholding | IRIS |
| **1042-T** | Annual summary and transmittal of Forms 1042-S | IRIS |
| **1094-B / 1095-B** | ACA health coverage (insurers, government agencies) | AIR |
| **1094-C / 1095-C** | ACA employer-provided coverage (ALEs) | AIR |
| **1097-BTC** | Bond tax credit | IRIS |
| **1098** | Mortgage interest statement | IRIS |
| **1098-C** | Contributions of motor vehicles, boats, and airplanes | IRIS |
| **1098-E** | Student loan interest statement | IRIS |
| **1098-F** | Fines, penalties, and other amounts | IRIS |
| **1098-Q** | Qualifying longevity annuity contract information | IRIS |
| **1098-T** | Tuition statement | IRIS |
| **1099-A** | Acquisition or abandonment of secured property | IRIS |
| **1099-B** | Proceeds from broker and barter exchange transactions | IRIS |
| **1099-C** | Cancellation of debt | IRIS |
| **1099-CAP** | Changes in corporate control and capital structure | IRIS |
| **1099-DA** | Digital asset proceeds from broker transactions | IRIS |
| **1099-DIV** | Dividends and distributions | IRIS |
| **1099-G** | Certain government payments | IRIS |
| **1099-HC** | MA individual health coverage | PDR |
| **1099-INT** | Interest income | IRIS |
| **1099-K** | Payment card and third-party network transactions | IRIS |
| **1099-LS** | Reportable life insurance sale | IRIS |
| **1099-LTC** | Long-term care and accelerated death benefits | IRIS |
| **1099-MISC** | Miscellaneous information | IRIS |
| **1099-NEC** | Non-employee compensation | IRIS |
| **1099-OID** | Original issue discount | IRIS |
| **1099-PATR** | Taxable distributions from cooperatives | IRIS |
| **1099-Q** | Payments from qualified education programs | IRIS |
| **1099-QA** | Distributions from ABLE accounts | IRIS |
| **1099-R** | Distributions from pensions, annuities, IRAs | IRIS |
| **1099-S** | Proceeds from real estate transactions | IRIS |
| **1099-SA** | Distributions from HSA, Archer MSA, or Medicare Advantage MSA | IRIS |
| **1099-SB** | Seller's investment in life insurance contract | IRIS |
| **3921** | Exercise of an incentive stock option | IRIS |
| **3922** | Transfer of stock acquired through employee stock purchase plan | IRIS |
| **5498** | IRA contribution information | IRIS |
| **5498-ESA** | Coverdell ESA contribution information | IRIS |
| **5498-QA** | ABLE account contribution information | IRIS |
| **5498-SA** | HSA, Archer MSA, or Medicare Advantage MSA contributions | IRIS |
| **W-2** | Wage and tax statement | BSO |
| **W-2G** | Certain gambling winnings | IRIS |
| **W-3** | Transmittal of wage and tax statements | BSO |

> The live, machine-readable list is at `GET /FilingType`.

## Authentication

The API uses OAuth 2.0. Create API credentials at [`/Account/Api/Create`](https://www.boomtax.com/Account/Api/Create) in the BoomTax portal — the client secret is shown only once on the success page, so capture it then. Credentials are scoped to `read` (GET endpoints) or `write` (all endpoints).

### Client Credentials (server-to-server)

```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&client_id=<your-client-id>&client_secret=<your-client-secret>
```

Response:
```json
{
  "access_token": "...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

Include the token in subsequent requests:
```http
Authorization: Bearer <access_token>
```

Access tokens expire after one hour. Request a new token when the current one is close to expiring.

### Authorization Code (web apps and MCP clients)

For user-delegated access, use the standard OAuth 2.0 authorization code flow with PKCE:

- Authorization: `GET /oauth/authorize`
- Token exchange: `POST /oauth/token`
- Dynamic client registration: `POST /oauth/register`
- Discovery metadata: `GET /.well-known/oauth-protected-resource`

## Quick Start

```csharp
// 1. Authenticate (client credentials)
var credentials = new FormUrlEncodedContent(new Dictionary<string, string> {
    { "grant_type", "client_credentials" },
    { "client_id", "<your-client-id>" },
    { "client_secret", "<your-client-secret>" }
});
var response = await httpClient.PostAsync("https://api.boomtax.com/oauth/token", credentials);
var token = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
    .RootElement.GetProperty("access_token").GetString();

// 2. Set auth header
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

// 3. Create a filing
var filing = await httpClient.PostAsJsonAsync("https://api.boomtax.com/Filing",
    new { filingTypeId = 42, payerName = "ABC Corporation" });

// 4. Add forms (batch of up to 1,000)
var forms = new[] {
    new { FirstName = "Jane", LastName = "Doe", Ssn = "000-01-0001", /* ... */ }
};
await httpClient.PostAsJsonAsync($"https://api.boomtax.com/Form1099NEC/Batch?filingId={filingId}", forms);

// 5. E-file to the IRS
await httpClient.PostAsJsonAsync($"https://api.boomtax.com/EfileRequests?filingId={filingId}",
    new { email = "hr@company.com" });
```

## API Endpoints

### Filings

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/Filing` | List filings (paginated, filterable by tax year, form type, status) |
| `GET` | `/Filing?id={guid}` | Get filing details |
| `POST` | `/Filing` | Create a new filing |
| `DELETE` | `/Filing?id={guid}` | Delete a filing |

### Forms

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/Form/{id}` | Get any form by ID |
| `GET` | `/Filing/{id}/Forms` | List all forms in a filing (paginated) |
| `POST` | `/Form1099NEC?filingId={guid}` | Add a single form |
| `POST` | `/Form1099NEC/Batch?filingId={guid}` | Add up to 1,000 forms |
| `PUT` | `/Form1099NEC/{id}` | Update a form (full replacement) |
| `DELETE` | `/Form/{id}` | Delete a single form |
| `DELETE` | `/Form` | Batch delete (up to 200 form IDs) |

> Replace `Form1099NEC` with the appropriate form type for your filing (e.g., `Form1095B`, `FormW2`).

### E-Filing

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/EfileRequests?filingId={guid}` | Submit filing to the IRS |
| `GET` | `/EfileRequests?filingId={guid}` | Check e-file status |
| `GET` | `/EfileResponses?efileRequestId={guid}` | Get IRS response and errors |

> **Rate limit:** Do not poll a given e-file request more than once every 15 minutes.

### Other

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/FilingType` | List all available filing types (includes `filingSystem` field: IRIS, AIR, BSO, PDR) |
| `POST` | `/Download` | Request PDF download of a filing |

## FIRE to IRIS Migration

The IRS is permanently shutting down the FIRE system on **December 31, 2026**. All 1099 e-filing is moving to IRIS (Information Returns Intake System).

**If you integrate with the BoomTax API, no changes are required.** BoomTax handles the IRIS transition transparently:

- **Same endpoints** — the API doesn't change. You POST forms the same way.
- **Same file formats** — if you upload Pub. 1220 FIRE-format files, BoomTax converts them to IRIS XML automatically.
- **No TCC needed** — BoomTax files under its own IRS-authorized IRIS TCC.
- **Filing system visibility** — the `GET /FilingType` endpoint now includes a `filingSystem` field so you can see which IRS system each form type uses (IRIS, AIR, BSO, or PDR).

For more information, see our [complete IRIS migration guide](https://www.boomtax.com/irs-iris).

## Error Format

Errors are returned as [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "Filing not found."
}
```

## Sample Project

The [`BoomTax.Api.SampleProject`](BoomTax.Api.SampleProject/) in this repo demonstrates a complete filing lifecycle — authentication, filing creation, batch form submission, and e-filing — using C# and .NET 8.

To run it:

1. Create API credentials at [`/Account/Api/Create`](https://www.boomtax.com/Account/Api/Create). Choose `write` scope to exercise the full sample; `read` scope works for read-only scenarios. The client secret is shown only once — capture it then.
2. Clone this repo.
3. Set your credentials via [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):
   ```bash
   cd BoomTax.Api.SampleProject
   dotnet user-secrets set "ClientId" "<your-client-id>"
   dotnet user-secrets set "ClientSecret" "<your-client-secret>"
   ```
4. Run:
   ```bash
   dotnet run
   ```

## Getting Started

1. Sign up at [boomtax.com](https://www.boomtax.com).
2. Contact [support@boomtax.com](mailto:support@boomtax.com) to enable API access on your account.
3. Create API credentials at [`/Account/Api/Create`](https://www.boomtax.com/Account/Api/Create).
4. Use the [Swagger docs](https://api.boomtax.com/swagger) to explore endpoints interactively.

## License

Proprietary. Copyright BoomTax.
