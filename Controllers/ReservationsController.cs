using APBD_Task5.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Task5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Reservation>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] string? status,
            [FromQuery] int? roomId)
        {
            IEnumerable<Reservation> reservations = DataStore.Reservations;

            if (date.HasValue)
            {
                reservations = reservations.Where(r => r.Date == date.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                reservations = reservations.Where(r =>
                    r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (roomId.HasValue)
            {
                reservations = reservations.Where(r => r.RoomId == roomId.Value);
            }

            return Ok(reservations);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Reservation> GetById(int id)
        {
            var reservation = DataStore.Reservations.FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        [HttpPost]
        public ActionResult<Reservation> Create([FromBody] Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == reservation.RoomId);

            if (room == null)
            {
                return BadRequest(new { message = "Room does not exist." });
            }

            if (!room.IsActive)
            {
                return BadRequest(new { message = "Cannot create reservation for an inactive room." });
            }

            bool overlaps = DataStore.Reservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                r.Date == reservation.Date &&
                reservation.StartTime < r.EndTime &&
                reservation.EndTime > r.StartTime);

            if (overlaps)
            {
                return Conflict(new { message = "Reservation overlaps with an existing reservation for the same room." });
            }

            reservation.Id = DataStore.NextReservationId;
            DataStore.Reservations.Add(reservation);

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }

        [HttpPut("{id:int}")]
        public ActionResult<Reservation> Update(int id, [FromBody] Reservation updatedReservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingReservation = DataStore.Reservations.FirstOrDefault(r => r.Id == id);

            if (existingReservation == null)
            {
                return NotFound();
            }

            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == updatedReservation.RoomId);

            if (room == null)
            {
                return BadRequest(new { message = "Room does not exist." });
            }

            if (!room.IsActive)
            {
                return BadRequest(new { message = "Cannot assign reservation to an inactive room." });
            }

            bool overlaps = DataStore.Reservations.Any(r =>
                r.Id != id &&
                r.RoomId == updatedReservation.RoomId &&
                r.Date == updatedReservation.Date &&
                updatedReservation.StartTime < r.EndTime &&
                updatedReservation.EndTime > r.StartTime);

            if (overlaps)
            {
                return Conflict(new { message = "Reservation overlaps with an existing reservation for the same room." });
            }

            existingReservation.RoomId = updatedReservation.RoomId;
            existingReservation.OrganizerName = updatedReservation.OrganizerName;
            existingReservation.Topic = updatedReservation.Topic;
            existingReservation.Date = updatedReservation.Date;
            existingReservation.StartTime = updatedReservation.StartTime;
            existingReservation.EndTime = updatedReservation.EndTime;
            existingReservation.Status = updatedReservation.Status;

            return Ok(existingReservation);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var reservation = DataStore.Reservations.FirstOrDefault(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            DataStore.Reservations.Remove(reservation);
            return NoContent();
        }
    }
}