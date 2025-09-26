using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace quan_ly_kho_hang.Models
{
   
    [CollectionName("Users")]
    public class AppUser : MongoIdentityUser<Guid>
    {
        
    }

    [CollectionName("Roles")]
    public class AppRole : MongoIdentityRole<Guid>
    {
        public AppRole() : base() { }
        public AppRole(string roleName) : base(roleName) { }
    }
    
}
