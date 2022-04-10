using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownshipTale.Api.Core
{
    public class Member
    {
        public int Id { get; }

        public MemberRole Role { get; }

        public bool IsApproved { get; }

        public void ChangeRole (MemberRole newRole)
        {
            throw new NotImplementedException ();
        }

        public void ApproveJoinRequest ()
        {
            throw new NotImplementedException();
        }

        public void Kick()
        {
            throw new NotImplementedException();
        }

        public void Ban()
        {
            throw new NotImplementedException();
        }
    }
}
