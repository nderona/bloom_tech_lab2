using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;
using BloomTech.Data;

namespace BloomTech.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext context;

        public ChatHub ( UserManager<IdentityUser> userManager, ApplicationDbContext context )
        {
            this.userManager = userManager;
            this.context = context;
        }


        public async Task SendMessage ( string user, string message, string connectionId )
        {
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            await Clients.Client( connectionId ).SendAsync( "ReceiveMessage", user, message, connectionId );

            var users = context.Users.ToList();

            foreach( var currentUser in users )
            {
                if( await userManager.IsInRoleAsync( currentUser, "Admin" ) )
                {
                    await Clients.User( currentUser.Id ).SendAsync( "ReceiveMessage", user, message, connectionId );
                }
            }

            //a7872d20-3c03-47eb-9ea4-c4f7687f1160

        }

        public string GetConnectionId ()
        {
            return Context.ConnectionId;
        }

    }
}
