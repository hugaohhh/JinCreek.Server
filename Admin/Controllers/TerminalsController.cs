using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Admin.Data;
using Admin.Models;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerminalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TerminalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Terminals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Terminal>>> GetTerminals()
        {
            return await _context.Terminals.ToListAsync();
        }

        // GET: api/Terminals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Terminal>> GetTerminal(string id)
        {
            var terminal = await _context.Terminals.FindAsync(id);

            if (terminal == null)
            {
                return NotFound();
            }

            return terminal;
        }

        // PUT: api/Terminals/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTerminal(string id, Terminal terminal)
        {
            if (id != terminal.Name)
            {
                return BadRequest();
            }

            _context.Entry(terminal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TerminalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Terminals
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Terminal>> PostTerminal(Terminal terminal)
        {
            _context.Terminals.Add(terminal);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TerminalExists(terminal.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTerminal", new { id = terminal.Name }, terminal);
        }

        // DELETE: api/Terminals/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Terminal>> DeleteTerminal(string id)
        {
            var terminal = await _context.Terminals.FindAsync(id);
            if (terminal == null)
            {
                return NotFound();
            }

            _context.Terminals.Remove(terminal);
            await _context.SaveChangesAsync();

            return terminal;
        }

        private bool TerminalExists(string id)
        {
            return _context.Terminals.Any(e => e.Name == id);
        }
    }
}
