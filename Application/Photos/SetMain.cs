using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class SetMain
    {
        public class Command : IRequest
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly ApplicationDbContext _db;
            private readonly IUserAccessor _userAccessor;
            public Handler(ApplicationDbContext db, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _db = db;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {

                var user = await _db.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetCurrentUserName());

                var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

                if (photo == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Photo = "Not Found" });

                var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

                currentMain.IsMain = false;
                photo.IsMain = true;

                var success = await _db.SaveChangesAsync() > 0;

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}