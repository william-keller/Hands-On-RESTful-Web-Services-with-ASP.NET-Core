using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VinylStore.Cart.Domain.Commands.Cart;
using VinylStore.Cart.Domain.Entities;
using VinylStore.Cart.Domain.Infrastructure.Repositories;
using VinylStore.Cart.Domain.Responses.Cart;
using VinylStore.Cart.Domain.Services;

namespace VinylStore.Cart.Domain.Handlers.Cart
{
    public class CreateCartHandler : IRequestHandler<CreateCartCommand, CartExtendedResponse>
    {
        private readonly ICatalogService _catalogService;
        private readonly IMapper _mapper;
        private readonly ICartRepository _repository;


        public CreateCartHandler(ICartRepository repository, IMapper mapper, ICatalogService catalogService)
        {
            _repository = repository;
            _mapper = mapper;
            _catalogService = catalogService;
        }

        public async Task<CartExtendedResponse> Handle(
            CreateCartCommand command,
            CancellationToken cancellationToken)
        {
            var entity = new Entities.Cart
            {
                Items = command.ItemsIds.Select(x => new CartItem { CartItemId = new Guid(x), Quantity = 1 }).ToList(),
                User = new CartUser { Email = command.UserEmail },
                ValidityDate = DateTimeOffset.Now.AddMonths(2),
                Id = Guid.NewGuid().ToString()
            };

            var result = await _repository.AddOrUpdateAsync(entity);

            var response = _mapper.Map<CartExtendedResponse>(result);

            var tasks = response.Items
                .Select(async x => await _catalogService.EnrichCartItem(x, cancellationToken));

            response.Items = await Task.WhenAll(tasks);

            return response;
        }
    }
}
