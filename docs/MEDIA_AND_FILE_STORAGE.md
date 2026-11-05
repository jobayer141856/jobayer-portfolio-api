# Media and File Storage

## 1. Storage Abstraction (`IFileStorageService`)
Decouple file persistence from controllers/endpoints behind an interface:
```csharp
public interface IFileStorageService
{
    Task<StoredFileResult> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct);
    Task DeleteAsync(string storageKey, CancellationToken ct);
}
```
- **Local Development:** `LocalFileStorageService` writes to a configured local directory served under `/uploads/*`.
- **Production:** `S3CompatibleFileStorageService` (supporting Cloudflare R2, AWS S3, DigitalOcean Spaces, or MinIO) or `AzureBlobStorageService`.

## 2. Upload Validation Rules
1. **File Size:** Max `5 MB` for images, max `10 MB` for PDF documents (configurable).
2. **File Signature (Magic Bytes):** Verify the stream header matches the declared extension and MIME type.
3. **Safe Naming:** Generate storage keys using `{folder}/{yyyy}/{MM}/{guid}.{ext}` so user-supplied filenames cannot cause path traversal or collisions.
4. **Image Metadata:** Extract pixel `Width` and `Height` on upload so the public Angular frontend can reserve aspect-ratio space and prevent Cumulative Layout Shift (CLS).

## 3. Referential Integrity
- `MediaAssets` referenced by `Profile`, `Projects`, `Experiences`, `Publications`, `Certifications`, or `BlogArticles` use `DeleteBehavior.Restrict`.
- Attempting to delete an in-use asset returns `409 Conflict` listing the entities that reference it.
