using APBD_Task5.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Task5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Room>> GetAll(
            [FromQuery] int? minCapacity,
            [FromQuery] bool? hasProjector,
            [FromQuery] bool? activeOnly)
        {
            IEnumerable<Room> rooms = DataStore.Rooms;

            if (minCapacity.HasValue)
            {
                rooms = rooms.Where(r => r.Capacity >= minCapacity.Value);
            }

            if (hasProjector.HasValue)
            {
                rooms = rooms.Where(r => r.HasProjector == hasProjector.Value);
            }

            if (activeOnly.HasValue && activeOnly.Value)
            {
                rooms = rooms.Where(r => r.IsActive);
            }

            return Ok(rooms);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Room> GetById(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        [HttpGet("building/{buildingCode}")]
        public ActionResult<IEnumerable<Room>> GetByBuildingCode(string buildingCode)
        {
            var rooms = DataStore.Rooms
                .Where(r => r.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(rooms);
        }

        [HttpPost]
        public ActionResult<Room> Create([FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            room.Id = DataStore.NextRoomId;
            DataStore.Rooms.Add(room);

            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpPut("{id:int}")]
        public ActionResult<Room> Update(int id, [FromBody] Room updatedRoom)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRoom = DataStore.Rooms.FirstOrDefault(r => r.Id == id);

            if (existingRoom == null)
            {
                return NotFound();
            }

            existingRoom.Name = updatedRoom.Name;
            existingRoom.BuildingCode = updatedRoom.BuildingCode;
            existingRoom.Floor = updatedRoom.Floor;
            existingRoom.Capacity = updatedRoom.Capacity;
            existingRoom.HasProjector = updatedRoom.HasProjector;
            existingRoom.IsActive = updatedRoom.IsActive;

            return Ok(existingRoom);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            bool hasRelatedReservations = DataStore.Reservations.Any(r => r.RoomId == id);
            if (hasRelatedReservations)
            {
                return Conflict(new { message = "Cannot delete room with related reservations." });
            }

            DataStore.Rooms.Remove(room);
            return NoContent();
        }
    }
}