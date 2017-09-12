using BaseClassEnitiy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClassEnitiy
{
    //http://www.jigar.net/articles/viewhtmlcontent344.aspx
    public class BaseDbContext : DbContext
    {
        protected string currentUserIdentifier;
        public BaseDbContext(string connectionStringName,string currentUserIdentifier) : base(connectionStringName)
        {
            this.currentUserIdentifier = currentUserIdentifier;
        }
        public BaseDbContext(string currentUserIdentifier)  : base()
        {
            this.currentUserIdentifier = currentUserIdentifier;
        }
        public override int SaveChanges()
        {
            foreach (var auditableEntity in ChangeTracker.Entries<IEntity>())//http://www.entityframeworktutorial.net/change-tracking-in-entity-framework.aspx
            {
                if (auditableEntity.State == EntityState.Added ||
                    auditableEntity.State == EntityState.Modified)
                {
                    // implementation may change based on the useage scenario, this
                    // sample is for forma authentication.
                    //string currentUser = HttpContext.Current.User.Identity.Name;
                    string currentUser = "System";
                    if (!String.IsNullOrWhiteSpace(currentUserIdentifier))
                    {
                        currentUser = currentUserIdentifier;
                    }

                    // modify updated date and updated by column for 
                    // adds of updates.
                    DateTime now = DateTime.Now;
                    auditableEntity.Entity.UpdatedDate = now;
                    auditableEntity.Entity.UpdatedBy = currentUser;

                    // pupulate created date and created by columns for
                    // newly added record.
                    if (auditableEntity.State == EntityState.Added) //ถ้า add เติม create date/by
                    {
                        auditableEntity.Entity.CreatedDate = now;
                        auditableEntity.Entity.CreatedBy = currentUser;
                    }
                    else//ถ้าเป็น modify (update set) --> จะถือว่า createdate/by เป็น column ที่ไม่เปลี่ยนแปลง
                    {
                        // we also want to make sure that code is not inadvertly (ไม่ได้ตั้งใจ)
                        // modifying created date and created by columns 
                        auditableEntity.Property(p => p.CreatedDate).IsModified = false;
                        auditableEntity.Property(p => p.CreatedBy).IsModified = false;
                    }
                }
            }
            return base.SaveChanges();
        }
    }
}

#region Example
/*
public class APFormDbContext : BaseDbContext
{
    public APFormDbContext() : base(LoginSessionData.CurrentUser != null ? LoginSessionData.CurrentUser.UserName : String.Empty)
    {
        //Database.SetInitializer<MaleeSSODbContext>(new CreateDatabaseIfNotExists<MaleeSSODbContext>());
        //Database.SetInitializer<MaleeSSODbContext>(new DropCreateDatabaseIfModelChanges<MaleeSSODbContext>());
        //System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseAlways<APFormDbContext>());
        System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<APFormDbContext, Configuration>());
        //this.Configuration.LazyLoadingEnabled = false;
        //this.Configuration.ProxyCreationEnabled = false;
        //Database.SetInitializer<MaleeSSODbContext>(new MaleeSSODBInitializer());//http://www.entityframeworktutorial.net/code-first/seed-database-in-code-first.aspx
        //https://blog.oneunicorn.com/2013/05/28/database-initializer-and-migrations-seed-methods/

        //Disable initializer
        //Database.SetInitializer<SchoolDBContext>(null);
    }

    public APFormDbContext(string connectionStringName) : base(connectionStringName, LoginSessionData.CurrentUser != null ? LoginSessionData.CurrentUser.UserName : String.Empty)
    {
        //Database.SetInitializer<APFormDbContext>(new CreateDatabaseIfNotExists<APFormDbContext>());
    }
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserAction> UserActions { get; set; }
    public DbSet<Group> Groups { get; set; }
}
/*
Enable-migrations //You don't need this as you have already done it
add-migration initial -force
update-database -verbose
Add-migration Give_it_a_name
Update-database
Update-Database –TargetMigration: <name of last good migration>
Get-Migrations
https://msdn.microsoft.com/en-us/library/jj591621(v=vs.113).aspx
*/
/**
* 
* 
using (var dbContextTransaction = context.Database.BeginTransaction()) 
{ 
    try 
    { 
        context.Database.ExecuteSqlCommand( 
            @"UPDATE Blogs SET Rating = 5" + 
                " WHERE Name LIKE '%Entity Framework%'" 
            ); 

        var query = context.Posts.Where(p => p.Blog.Rating >= 5); 
        foreach (var post in query) 
        { 
            post.Title += "[Cool Blog]"; 
        } 

        context.SaveChanges(); 

        dbContextTransaction.Commit(); 
    } 
    catch (Exception) 
    { 
        dbContextTransaction.Rollback(); 
    } 
} 
*
*
* using (var conn = new SqlConnection("...")) //open connection on your own and pass thru connection to context
{ 
    conn.Open(); 
    using (var context = new BloggingContext(conn, contextOwnsConnection: false)) 
    { 
    } 
}
*
* 
using (var conn = new SqlConnection("...")) 
{ 
   conn.Open(); 

   using (var sqlTxn = conn.BeginTransaction(System.Data.IsolationLevel.Snapshot)) 
   { 
       try 
       { 
           var sqlCommand = new SqlCommand(); 
           sqlCommand.Connection = conn; 
           sqlCommand.Transaction = sqlTxn; 
           sqlCommand.CommandText = 
               @"UPDATE Blogs SET Rating = 5" + 
                " WHERE Name LIKE '%Entity Framework%'"; 
           sqlCommand.ExecuteNonQuery(); 

           using (var context =  
             new BloggingContext(conn, contextOwnsConnection: false)) 
            { 
                context.Database.UseTransaction(sqlTxn); 

                var query =  context.Posts.Where(p => p.Blog.Rating >= 5); 
                foreach (var post in query) 
                { 
                    post.Title += "[Cool Blog]"; 
                } 
               context.SaveChanges(); 
            } 

            sqlTxn.Commit(); 
        } 
        catch (Exception) 
        { 
            sqlTxn.Rollback(); 
        } 
    } 
} 
*
*/
#endregion