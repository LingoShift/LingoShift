using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LingoShift.Application.ApplicationServices;
using LingoShift.Domain.ValueObjects;
using LingoShift.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LingoShift.Controller
{
    [ApiController]
    [Route("api")]
    public class SettingsController : ControllerBase
    {
        private readonly ISequenceConfigRepository _sequenceConfigRepository;
        private readonly TranslationApplicationService _translationService;

        public SettingsController(ISequenceConfigRepository sequenceConfigRepository, TranslationApplicationService translationService)
        {
            _sequenceConfigRepository = sequenceConfigRepository;
            _translationService = translationService;
        }

        [HttpGet("settings")]
        public async Task<ActionResult<IEnumerable<SequenceConfig>>> GetSettings()
        {
            try
            {
                var sequenceConfigs = await _sequenceConfigRepository.GetAllAsync();
                return Ok(sequenceConfigs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("settings")]
        public async Task<ActionResult> AddSequence([FromBody] SequenceConfig newSequence)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _sequenceConfigRepository.AddAsync(newSequence);
                await _translationService.RegisterSequenceAsync(newSequence);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("settings/{id}")]
        public async Task<ActionResult> UpdateSequence(string id, [FromBody] SequenceConfig updatedSequence)
        {
            if (id != updatedSequence.Id)
            {
                return BadRequest("ID in URL does not match the one in the body.");
            }

            try
            {
                var existingSequence = await _sequenceConfigRepository.GetByIdAsync(id);
                if (existingSequence == null)
                {
                    return NotFound();
                }

                var updated = await _sequenceConfigRepository.UpdateAsync(updatedSequence);
                if (!updated)
                {
                    return NotFound("Sequence config not found or not updated.");
                }

                await _translationService.RegisterSequenceAsync(updatedSequence);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("settings/{id}")]
        public async Task<ActionResult> RemoveSequence(string id)
        {
            try
            {
                var deleted = await _sequenceConfigRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound("Sequence config not found or not deleted.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("actions")]
        public ActionResult<IEnumerable<SequenceAction>> GetAvailableActions()
        {
            return Ok(SequenceAction.GetValues());
        }

        [HttpGet("languages")]
        public ActionResult<IEnumerable<Language>> GetAvailableLanguages()
        {
            return Ok(Language.GetValues());
        }
    }
}