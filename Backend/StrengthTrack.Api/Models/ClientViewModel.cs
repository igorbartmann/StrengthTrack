using System;
using StrengthTrack.Api.Entities;

namespace StrengthTrack.Api.Models
{
    public class ClientViewModel : ClientSimpleViewModel
    {
        public ClientViewModel(int id, string name, string cpf, IList<SessionViewModel> sessions) : base(id, name, cpf)
        {
            Sessions = sessions ?? new List<SessionViewModel>();
        }

        public IList<SessionViewModel> Sessions {get;set;}
    }
}