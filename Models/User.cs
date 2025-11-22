using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderSystem.Models
{
    public class User
    {
        public User()
        {
            Role = Role.Unassigned;
        }
        public User(int id, string username, string password, Role role, string email)
        {
            Id = id;
            Username = username;
            Password = password;
            Role = role;
            Email = email;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public string AuthenticationMethod { get; set; } = "Traditional";
        public string OAuthProvider { get; set; } = "";
    }
    public enum Role
    {
        Admin,
        Waiter,
        Cook,
        Unassigned
    }
    
}
