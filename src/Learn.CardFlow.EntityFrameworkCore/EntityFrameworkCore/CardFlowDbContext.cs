using Learn.CardFlow.CardRequests;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Learn.CardFlow.Deliveries;
namespace Learn.CardFlow.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class CardFlowDbContext :
    AbpDbContext<CardFlowDbContext>,
    IIdentityDbContext
{
    public DbSet<EmployeeCardRequest> EmployeeCardRequests { get; set; }
    public DbSet<ProductionBatch> ProductionBatches { get; set; }
    public DbSet<CardQualityCheck> CardQualityChecks { get; set; }
    public DbSet<CardDelivery> CardDeliveries { get; set; }
    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    public CardFlowDbContext(DbContextOptions<CardFlowDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */
        builder.Entity<CardDelivery>(b =>
        {
            b.ToTable(CardFlowConsts.DbTablePrefix + "CardDeliveries", CardFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RecipientName).IsRequired().HasMaxLength(128);
            b.HasOne<EmployeeCardRequest>().WithMany().HasForeignKey(x => x.EmployeeCardRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<CardQualityCheck>().WithMany().HasForeignKey(x => x.CardQualityCheckId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.EmployeeCardRequestId).IsUnique();
            b.HasIndex(x => x.CardQualityCheckId).IsUnique();
        });

        builder.Entity<CardQualityCheck>(b =>
        {
            b.ToTable(CardFlowConsts.DbTablePrefix + "CardQualityChecks", CardFlowConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.FailureReason).HasMaxLength(500);
            b.HasOne<EmployeeCardRequest>().WithMany()
                .HasForeignKey(x => x.EmployeeCardRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProductionBatch>().WithMany()
                .HasForeignKey(x => x.ProductionBatchId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.EmployeeCardRequestId, x.ProductionBatchId }).IsUnique();
        });

        builder.Entity<EmployeeCardRequest>(b =>
        {
            b.ToTable(
                CardFlowConsts.DbTablePrefix + "EmployeeCardRequests",
                CardFlowConsts.DbSchema
            );

            b.ConfigureByConvention();

            b.Property(x => x.EmployeeName)
                .IsRequired()
                .HasMaxLength(128);

            b.Property(x => x.EmployeeNumber)
                .IsRequired()
                .HasMaxLength(32);

            b.Property(x => x.Status)
                .IsRequired();

            b.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            b.HasOne<ProductionBatch>()
              .WithMany()
              .HasForeignKey(x => x.ProductionBatchId)
              .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ProductionBatch>(b =>
{
    b.ToTable(
        CardFlowConsts.DbTablePrefix + "ProductionBatches",
        CardFlowConsts.DbSchema
    );

    b.ConfigureByConvention();

    b.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(128);

    b.Property(x => x.Status)
        .IsRequired();
});
    }

}
