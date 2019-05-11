using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    //[Route("api/v{version:apiVersion}/[controller]")]  //Url versioning
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        //public async Task<IActionResult> Get()
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsAsync(includeTalks);
                //var models = mapper.Map<CampModel[]>(results);
                //return Ok(models);
                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                //return BadRequest("Database failure");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await campRepository.GetCampAsync(moniker);

                if (result is null)
                {
                    return NotFound();
                }
                else
                {
                    return mapper.Map<CampModel>(result);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> Get11(string moniker)
        {
            try
            {
                var result = await campRepository.GetCampAsync(moniker, true);

                if (result is null)
                {
                    return NotFound();
                }
                else
                {
                    return mapper.Map<CampModel>(result);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> CreateCamp([FromBody]CampModel model)
        {
            try
            {
                //Model validations
                if (!ModelState.IsValid)
                    return BadRequest();

                //Check camp exists
                var campExists = await campRepository.GetCampAsync(model.Moniker);
                if (campExists != null)
                {
                    return BadRequest("Camp already exists.");
                }

                var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });
                if (String.IsNullOrWhiteSpace(location)) //validation
                {
                    return BadRequest("Could not use current moniker");
                }

                //create a camp
                var camp = mapper.Map<Camp>(model);
                campRepository.Add<Camp>(camp);
                if (await campRepository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", mapper.Map<CampModel>(camp)); //return entity type object
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Could not create camp");
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Update(string moniker, [FromBody] CampModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var oldCamp = await campRepository.GetCampAsync(model.Moniker);
                if (oldCamp is null)
                {
                    return NotFound("Camp does not exists.");
                }

                mapper.Map(model, oldCamp);
                
                if (await campRepository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest($"Could not update camp {model.Name}");
        }

        [HttpDelete("{moniker}")]
        public async Task<ActionResult> Delete(string moniker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var camp = await campRepository.GetCampAsync(moniker, true);
                if(camp is null)
                {
                    return NotFound("Camp not found.");
                }

                //campRepository.Delete(camp.Talks);
                campRepository.Delete(camp);
                if(await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest($"Could not delete camp {moniker}");
        }
    }
}