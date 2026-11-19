using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// The pop-up to show today (the newest active one), if any.
public class PopupService
{
    private readonly SiteDbContext _db;
    public PopupService(SiteDbContext db) => _db = db;

    public Popup? Current()
    {
        try
        {
            var today = DateTime.Today;
            return _db.Popups.AsNoTracking()
                .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
                .OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
        }
        catch (Exception) { return null; }
    }
}
