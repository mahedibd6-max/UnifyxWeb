namespace UnifyxWeb;

// Saves pictures uploaded in Admin under wwwroot/uploads/site with a random name.
// Only JPG, PNG and WebP up to 3 MB, checked by the file's first bytes as well as its name.
public class ImageStore
{
    private const long MaxBytes = 3 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;
    public ImageStore(IWebHostEnvironment env) => _env = env;

    public async Task<(string? Path, string? Error)> SaveAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return (null, null);
        if (file.Length > MaxBytes) return (null, "The picture is larger than 3 MB.");

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) return (null, "Use a JPG, PNG or WebP picture.");

        var head = new byte[12];
        await using (var s = file.OpenReadStream()) { _ = await s.ReadAsync(head); }
        bool ok = (head[0] == 0xFF && head[1] == 0xD8)                                   // JPG
               || (head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47) // PNG
               || (head[0] == 0x52 && head[1] == 0x49 && head[8] == 0x57 && head[9] == 0x45); // WebP (RIFF....WEBP)
        if (!ok) return (null, "This file is not a real picture.");

        string folder = Path.Combine(_env.WebRootPath, "uploads", "site");
        Directory.CreateDirectory(folder);
        string name = Guid.NewGuid().ToString("N") + (ext == ".jpeg" ? ".jpg" : ext);
        await using (var outStream = File.Create(Path.Combine(folder, name)))
            await file.CopyToAsync(outStream);
        return ("/uploads/site/" + name, null);
    }

    // Removes an earlier upload (only files in our own folder).
    public void Delete(string? path)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/site/")) return;
        string full = Path.Combine(_env.WebRootPath, "uploads", "site", Path.GetFileName(path));
        if (File.Exists(full)) File.Delete(full);
    }
}
