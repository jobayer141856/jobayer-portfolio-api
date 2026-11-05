# API Endpoints and Contracts

**Base Path:** `/api/v1`  
**Content-Type:** `application/json` (except `multipart/form-data` on `/api/v1/admin/media/upload`)

## 1. Public Endpoints (Anonymous)

| Method | Path | Description | Cache Policy |
|---|---|---|---|
| `GET` | `/api/v1/profile` | Public profile, hero, social links, active résumé URL | OutputCache (tagged `profile`) |
| `GET` | `/api/v1/experiences` | Published work experiences ordered by `displayOrder` | OutputCache (tagged `experiences`) |
| `GET` | `/api/v1/projects` | Published projects (query: `featured`, `category`, `tag`) | OutputCache (tagged `projects`) |
| `GET` | `/api/v1/projects/{slug}` | Single published project case study by slug | OutputCache (tagged `projects`) |
| `GET` | `/api/v1/skills` | Visible skill categories and grouped skills | OutputCache (tagged `skills`) |
| `GET` | `/api/v1/educations` | Published education entries (respects `showResultPublicly`) | OutputCache (tagged `educations`) |
| `GET` | `/api/v1/publications` | Published research publications | OutputCache (tagged `publications`) |
| `GET` | `/api/v1/certifications` | Published certifications and achievements | OutputCache (tagged `certifications`) |
| `GET` | `/api/v1/blog` | Published blog posts (if blog enabled) | OutputCache (tagged `blog`) |
| `GET` | `/api/v1/blog/{slug}` | Single published blog post by slug | OutputCache (tagged `blog`) |
| `GET` | `/api/v1/seo/sitemap` | Published slugs and `updatedAt` timestamps for sitemap | OutputCache (5 min) |
| `POST` | `/api/v1/contact` | Submit visitor contact message (rate-limited) | No cache |

## 2. Authentication Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | Anonymous (rate-limited) | Validate credentials, return access token & refresh cookie |
| `POST` | `/api/v1/auth/refresh` | Refresh token | Rotate refresh token and issue new access token |
| `POST` | `/api/v1/auth/logout` | Authenticated | Revoke refresh token and clear cookie |
| `GET` | `/api/v1/auth/me` | Authenticated | Return current admin user profile |
| `POST` | `/api/v1/auth/change-password` | Authenticated | Verify current password and set new password |

## 3. Admin Endpoints (`[Authorize(Policy = "AdminOnly")]`)

| Resource | Endpoints |
|---|---|
| Dashboard | `GET /api/v1/admin/dashboard/summary` |
| Profile | `GET /api/v1/admin/profile`, `PUT /api/v1/admin/profile` |
| Experiences | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `PUT /reorder`, `DELETE /{id}` under `/api/v1/admin/experiences` |
| Projects | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `PUT /reorder`, `DELETE /{id}` under `/api/v1/admin/projects` |
| Skills | `GET`, `POST /categories`, `PUT /categories/{id}`, `DELETE /categories/{id}`, `POST /items`, `PUT /items/{id}`, `DELETE /items/{id}`, `PUT /reorder` under `/api/v1/admin/skills` |
| Educations | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `PUT /reorder`, `DELETE /{id}` under `/api/v1/admin/educations` |
| Publications | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `PUT /reorder`, `DELETE /{id}` under `/api/v1/admin/publications` |
| Certifications | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `PUT /reorder`, `DELETE /{id}` under `/api/v1/admin/certifications` |
| Blog | `GET`, `POST`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, `DELETE /{id}` under `/api/v1/admin/blog` |
| Messages | `GET`, `GET /{id}`, `PATCH /{id}/status`, `DELETE /{id}` under `/api/v1/admin/messages` |
| Media | `GET`, `POST /upload`, `PATCH /{id}`, `DELETE /{id}` under `/api/v1/admin/media` |

## 4. Standard Response Shapes

### Paginated List Response (Admin & Blog Lists)
```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

### Error Response (`ProblemDetails` / `ValidationProblemDetails`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "slug": ["A project with the slug 'erp-portal' already exists."],
    "title": ["Title is required."]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```
