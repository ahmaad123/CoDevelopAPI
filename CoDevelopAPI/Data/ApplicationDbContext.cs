using System;
using System.Collections.Generic;
using CoDevelopAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoDevelopAPI.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Rolepermission> Rolepermissions { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userrole> Userroles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_catalog", "adminpack");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(50)
                .HasColumnName("categoryname");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Clientid).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.HasIndex(e => e.Email, "clients_email_key").IsUnique();

            entity.HasIndex(e => e.Mobile, "clients_mobile_key").IsUnique();

            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Businessname)
                .HasMaxLength(150)
                .HasColumnName("businessname");
            entity.Property(e => e.Businesstype)
                .HasMaxLength(50)
                .HasColumnName("businesstype");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Mobile)
                .HasMaxLength(100)
                .HasColumnName("mobile");
            entity.Property(e => e.Monthlyprice)
                .HasPrecision(18, 2)
                .HasColumnName("monthlyprice");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Invoicenumber).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.Property(e => e.Invoicenumber).HasColumnName("invoicenumber");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Paymentstatus)
                .HasMaxLength(20)
                .HasColumnName("paymentstatus");
            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.Tax)
                .HasPrecision(18, 2)
                .HasColumnName("tax");
            entity.Property(e => e.Total)
                .HasPrecision(18, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("invoices_clientid_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Invoicenumber).HasColumnName("invoicenumber");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .HasColumnName("method");

            entity.HasOne(d => d.InvoicenumberNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Invoicenumber)
                .HasConstraintName("payments_invoicenumber_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Permid).HasName("permissions_pkey");

            entity.ToTable("permissions");

            entity.HasIndex(e => e.Permname, "permissions_permname_key").IsUnique();

            entity.Property(e => e.Permid).HasColumnName("permid");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Module)
                .HasMaxLength(100)
                .HasColumnName("module");
            entity.Property(e => e.Permname)
                .HasMaxLength(150)
                .HasColumnName("permname");
            entity.Property(e => e.Resource)
                .HasMaxLength(100)
                .HasColumnName("resource");

            entity.HasOne(d => d.Category).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("permissions_categoryid_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Projectid).HasName("projects_pkey");

            entity.ToTable("projects");

            entity.Property(e => e.Projectid).HasColumnName("projectid");
            entity.Property(e => e.Budget)
                .HasPrecision(18, 2)
                .HasColumnName("budget");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Developer)
                .HasMaxLength(100)
                .HasColumnName("developer");
            entity.Property(e => e.Manager)
                .HasMaxLength(100)
                .HasColumnName("manager");
            entity.Property(e => e.Progress).HasColumnName("progress");
            entity.Property(e => e.Projectname)
                .HasMaxLength(150)
                .HasColumnName("projectname");

            entity.HasOne(d => d.Client).WithMany(p => p.Projects)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("projects_clientid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Level)
                .HasDefaultValue(0)
                .HasColumnName("level");
            entity.Property(e => e.Rolecode)
                .HasMaxLength(100)
                .HasColumnName("rolecode");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Rolepermission>(entity =>
        {
            entity.HasKey(e => e.Rolepermid).HasName("rolepermissions_pkey");

            entity.ToTable("rolepermissions");

            entity.HasIndex(e => new { e.Roleid, e.Permissionid }, "unique_role_permission").IsUnique();

            entity.Property(e => e.Rolepermid).HasColumnName("rolepermid");
            entity.Property(e => e.Grantedby).HasColumnName("grantedby");
            entity.Property(e => e.Isallowed)
                .HasDefaultValue(true)
                .HasColumnName("isallowed");
            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.HasOne(d => d.GrantedbyNavigation).WithMany(p => p.Rolepermissions)
                .HasForeignKey(d => d.Grantedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rolepermissions_grantedby_fkey");

            entity.HasOne(d => d.Permission).WithMany(p => p.Rolepermissions)
                .HasForeignKey(d => d.Permissionid)
                .HasConstraintName("rolepermissions_permissionid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Rolepermissions)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("rolepermissions_roleid_fkey");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Ticketid).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.Property(e => e.Ticketid).HasColumnName("ticketid");
            entity.Property(e => e.Assigneeid).HasColumnName("assigneeid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Medium'::character varying")
                .HasColumnName("priority");
            entity.Property(e => e.Reporterid).HasColumnName("reporterid");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Open'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");

            entity.HasOne(d => d.Assignee).WithMany(p => p.TicketAssignees)
                .HasForeignKey(d => d.Assigneeid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_assigneeid_fkey");

            entity.HasOne(d => d.Client).WithMany(p => p.TicketClients)
                .HasForeignKey(d => d.Clientid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_clientid_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.TicketReporters)
                .HasForeignKey(d => d.Reporterid)
                .HasConstraintName("tickets_reporterid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<Userrole>(entity =>
        {
            entity.HasKey(e => e.Userroleid).HasName("userroles_pkey");

            entity.ToTable("userroles");

            entity.HasIndex(e => new { e.Userid, e.Roleid }, "unique_user_role").IsUnique();

            entity.Property(e => e.Userroleid).HasColumnName("userroleid");
            entity.Property(e => e.Assignedby).HasColumnName("assignedby");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.AssignedbyNavigation).WithMany(p => p.UserroleAssignedbyNavigations)
                .HasForeignKey(d => d.Assignedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("userroles_assignedby_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Userroles)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("userroles_roleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserroleUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("userroles_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
