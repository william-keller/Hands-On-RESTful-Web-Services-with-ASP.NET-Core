﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VinylStore.Catalog.API.Infrastructure.Filters;
using VinylStore.Catalog.API.ResponseModels;
using VinylStore.Catalog.Domain.Commands.Genre;
using VinylStore.Catalog.Domain.Responses.Item;

namespace VinylStore.Catalog.API.Controllers
{
    [Route("api/genre")]
    [ApiController]
    [JsonException]
    public class GenreController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenreController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var result = await _mediator.Send(new GetGenresCommand());

            var totalItems = result.Count;

            var itemsOnPage = result
                .OrderBy(c => c.GenreDescription)
                .Skip(pageSize * pageIndex)
                .Take(pageSize);

            var model = new PaginatedItemResponseModel<GenreResponse>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetGenreCommand { Id = id });
            return Ok(result);
        }

        [HttpGet("{id:guid}/items")]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            var result = await _mediator.Send(new GetItemsByGenreCommand { Id = id });
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post(AddGenreCommand request)
        {
            var result = await _mediator.Send(request);
            return CreatedAtAction(nameof(GetById), new { id = result.GenreId }, null);
        }
    }
}
