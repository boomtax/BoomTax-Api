# BoomTax API

REST API for electronic filing of IRS information returns. Create filings, submit forms, e-file to the IRS, and download copies — all through a simple HTTP interface.

**Base URL:** `https://api.boomtax.com`
**Swagger docs:** `https://api.boomtax.com/swagger`
**MCP Server:** [`@boomtax/mcp-server`](https://github.com/boomtax/mcp-server) — query your filing data from any AI assistant

## Supported Forms

| Form | Description |
|------|-------------|
| **1094-B / 1095-B** | ACA health coverage (insurers, government agencies) |
| **1094-C / 1095-C** | ACA employer-provided coverage (ALEs) |
| **1099-MISC** | Miscellaneous income |
| **1099-NEC** | Non-employee compensation |
| **1099-INT** | Interest income |
| **1099-DIV** | Dividends and distributions |
| **1099-K** | Payment card and third-party network transactions |
| **1099-R** | Distributions from pensions, annuities, IRAs |
| **1099-SA** | Distributions from HSA, Archer MSA, or Medicare Advantage MSA |
| **1099-C** | Cancellation of debt |
| **1099-HC** | MA individual health coverage |
| **W-2** | Wage and tax statement |
| **W-2G** | Gambling winnings |
| **5498-SA** | HSA, Archer MSA, or Medicare Advantage MSA contributions |

## Authentication

The API supports two authentication methods:

### Bearer Token (server-to-server)

```http
POST /Token
Content-Type: application/x-www-form-urlencoded

username=you@yourfirm.com&password=your-password&grant_type=password
```

Response:
```json
{
  "access_token": "eyJ...",
  "token_type": "bearer",
  "expires_in": 82800,
  "refresh_token": "abc123..."
}
```

Include the token in subsequent requests:
```http
Authorization: Bearer eyJ...
```

### OAuth 2.0 Authorization Code (web apps)

- Authorization: `GET /oauth/authorize`
- Token exchange: `POST /oauth/token`

## Quick Start

```csharp
// 1. Authenticate
var credentials = new FormUrlEncodedContent(new Dictionary<string, string> {
    { "username", "you@yourfirm.com" },
    { "password", "your-password" },
    { "grant_type", "password" }
});
var response = await httpClient.PostAsync("https://api.boomtax.com/Token", credentials);
var token = JToken.Parse(await response.Content.ReadAsStringAsync())["access_token"].ToString();

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
| `GET` | `/FilingType` | List all available filing types |
| `POST` | `/Download` | Request PDF download of a filing |

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

1. Clone this repo
2. Set your credentials via [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):
   ```bash
   cd BoomTax.Api.SampleProject
   dotnet user-secrets set "UserName" "you@yourfirm.com"
   dotnet user-secrets set "Password" "your-password"
   ```
3. Run:
   ```bash
   dotnet run
   ```

## Getting Started

1. Sign up at [boomtax.com](https://www.boomtax.com)
2. Contact [support@boomtax.com](mailto:support@boomtax.com) to enable API access on your account
3. Use the [Swagger docs](https://api.boomtax.com/swagger) to explore endpoints interactively

## License

Proprietary. Copyright BoomTax.
