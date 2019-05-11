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
    [Route("api/camps/{moniker}/talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await campRepository.GetTalksByMonikerAsync(moniker);
                return mapper.Map<TalkModel[]>(talks);                
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, id);
                if (talk is null) return NotFound();

                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talk");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel talkModel)
        {
            try
            {
                //Gets the camp using moniker
                var camp = await campRepository.GetCampAsync(moniker, true);
                if(camp is null)
                {
                    return BadRequest("Camp doesnot exists");
                }

                //converts talkmodel to talk entity
                var talk = mapper.Map<Talk>(talkModel);
                //maps to camp
                talk.Camp = camp;

                if(talkModel.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }
                //gets speaker
                var speaker = await campRepository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("Speaker doesnot exists");
                }

                //maps speaker
                talk.Speaker = speaker;
                //adds talk entity
                campRepository.Add(talk);

                if(await campRepository.SaveChangesAsync())
                {
                    //gets the url after talk add
                    var url = linkGenerator.GetPathByAction(HttpContext,
                        "Get", values: new { moniker, id = talk.TalkId });

                    return Created(url, mapper.Map<TalkModel>(talk));
                }

                return BadRequest("Talk cannot be saved");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create talk");
            }
        }
    }
}