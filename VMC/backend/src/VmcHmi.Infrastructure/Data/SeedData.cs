using Microsoft.EntityFrameworkCore;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Enums;

namespace VmcHmi.Infrastructure.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(HmiDbContext context, IPasswordHasher passwordHasher)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS hmi;");
        }
        catch
        {
            // Non-postgres provider or already existing
        }

        try
        {
            await context.Database.MigrateAsync();
        }
        catch
        {
            // Table(s) already exist or partial migration state from previous deployment
        }

        // Seed default operator user
        await SeedUserWithSessionAsync(context, passwordHasher, "operator", "Operator@123");

        // Seed HR user requested
        await SeedUserWithSessionAsync(context, passwordHasher, "Hr123@gmail.com", "Hr@123");
    }

    private static async Task SeedUserWithSessionAsync(
        HmiDbContext context,
        IPasswordHasher passwordHasher,
        string username,
        string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            user = new User(username, passwordHasher.Hash(password));
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        var hasSession = await context.MachineSessions.AnyAsync(s => s.UserId == user.Id);
        if (!hasSession)
        {
            var session = new MachineSession(user.Id);

            var machineChecks = new[]
            {
                ("E-stop released and functional", 0),
                ("Emergency stop button tested", 1),
                ("Machine guard in place", 2),
                ("Coolant levels adequate", 3),
                ("Air pressure at operating level", 4),
                ("Spindle runout within tolerance", 5)
            };

            var tools = new[]
            {
                ("T01", "Face Mill 50mm"),
                ("T02", "End Mill 10mm (4-flute)"),
                ("T03", "Drill 8mm")
            };

            var workpieceChecks = new[]
            {
                ("Workpiece material verified (Aluminum 6061-T6)", 0),
                ("Workpiece orientation correct (datum face against fixed jaw)", 1),
                ("Pocket face up", 2),
                ("Torque clamp to 25 Nm", 3),
                ("Zero gap at datum face confirmed", 4)
            };

            var toolCheckLabels = new[]
            {
                ("T01 Face Mill 50mm installed and secured", 0),
                ("T02 End Mill 10mm (4-flute) installed and secured", 1),
                ("T03 Drill 8mm installed and secured", 2),
                ("All tool offsets set (G43 applied)", 3),
                ("Tool setter measurement verified", 4)
            };

            foreach (var (label, order) in machineChecks)
                session.AddChecklistItem(ChecklistStage.MachineChecks, label, order);

            foreach (var (code, desc) in tools)
                session.AddTool(code, desc);

            foreach (var (label, order) in toolCheckLabels)
                session.AddChecklistItem(ChecklistStage.Tools, label, order);

            foreach (var (label, order) in workpieceChecks)
                session.AddChecklistItem(ChecklistStage.Workpiece, label, order);

            await context.MachineSessions.AddAsync(session);
            await context.SaveChangesAsync();
        }
    }
}

